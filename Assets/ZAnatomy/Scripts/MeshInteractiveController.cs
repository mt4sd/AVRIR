using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
// using Sirenix.OdinInspector;

namespace ZAnatomy{
    public class MeshInteractiveController : MonoBehaviour//, IPointerClickHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler
{
    public string Path;
    public string BodyType;
    public ZAnatomyController.OrganType BodyPart;
    public MeshCollider Collider;
    public ZAnatomyController ZAnatomyController;
    public Material[] CustomSelectedMaterials;
    public Material CustomNotSelectedMaterial;
    public Color CustomColor;
    public DividedOrganController DividedPrefab;
    public string DividedPrefabRelativePath;
    // [FoldoutGroup("RelationsBetweenModels")]
    public List<MeshInteractiveController> ModelsOverList;
    // [FoldoutGroup("RelationsBetweenModels")]
    public List<MeshInteractiveController> ModelsBelowList;
    
    // [FoldoutGroup("RelationsBetweenModels")]
    // [ShowInInspector]
    public List<DataList.DataModel> m_ConnectedModelsList;
    // [FoldoutGroup("RelationsBetweenModels")]
    // [ShowInInspector]
    public List<Collider> m_ConnectedColliders_Over;
    // [FoldoutGroup("RelationsBetweenModels")]
    // [ShowInInspector]
    public List<Collider> m_ConnectedColliders_Below;
    public bool CanBeGrabbed;

    private Vector3 MorgueMarkerScale;

    private MeshRenderer mesh;
    private Color SelectedColor;
    private Material DefaultMaterial;
    private Material NotSelectedMaterial;
    private Material SelectedMaterial;
    private int ID;
    private Vector3 m_LocalPosition;
    private Vector3 m_Position;
    private Vector3 m_LocalEulerAngles;
    private Vector3 m_LocalExpandedPosition;
    private Vector3 m_LastPosition;
    private Quaternion m_StartRotation;
    private Transform ParentModel;
    private bool isExpanded;
    private MeshKDTree MeshKDTree;
    private Rigidbody Rigidbody;
    
    private Vector3 MorgueOriginalPosition;
    private Vector3 MorgueDeltaPosition;
    private Transform MorgueParent;
    public bool MorgueIsInPlace = true;
    private GameObject MorgueMarker;
    private float MaxTransparencyValue;

    private bool m_PuzzleIsGreen;
    // [ShowInInspector]
    private bool IsOnMorgue;
    
    void Start()
    {
        ZAnatomyController = ZAnatomyController.zAnatomy;

        if(ZAnatomyController == null){
            return;
        }

        InitOrganInPlace();
    }

    public void Init(int id, Color color/*, ZAnatomyController zAnatomy*/, string path, bool useMeshKDTree = true){
        ID = id;
        Path = path;
        ZAnatomyController = ZAnatomyController.zAnatomy; //ZAnatomyController = zAnatomy;

        mesh = GetComponent<MeshRenderer>();
        mesh.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mesh.receiveShadows = false;
        MaxTransparencyValue = mesh.sharedMaterial.color.a;

        if(Collider == null){
            Collider = gameObject.AddComponent<MeshCollider>();
        }
        if(useMeshKDTree){
            if(MeshKDTree == null){
                MeshKDTree = gameObject.AddComponent<MeshKDTree>();
            }
        }
        SelectedColor = color;
        m_LocalPosition = transform.localPosition;
        m_Position = transform.position;

        m_LocalEulerAngles = transform.localEulerAngles;
        ParentModel = transform.parent;

        Select(false);
    }

    public void InitOrganInPlace(){
        string nameSource = ZAnatomyUtils.GetIDModelFromPath(Path).Trim();
        DataList.DataModel data = ZAnatomyController.GetDataModel(nameSource);
        if(data != null){
            ZAnatomyController.OrganPrefab organ = ZAnatomyController.GetOrganPrefab(nameSource);

            data.Model = this;
            data.Model.gameObject.SetActive(ZAnatomyController.OrgansTypeInfo.Find((x) => x.Type == data.BodyPart).Enabled);
            data.Model.transform.localPosition = organ.LocalPosition;
            data.Model.transform.localEulerAngles = organ.LocalEulerAngles;

            MorgueIsInPlace = true;
            Init(data.ID, data.Model.CustomColor, /*this, */data.Path);// Utils.ColorFromHex(data.Color));
            Morgue_Init();
            SetCollider(true);
            CanBeGrabbed = true;
            // Morgue_CalculateModelsOver(Data.GetInteractiveMeshList());
        }
    }

    public MeshKDTree GetMeshKDTree(){
        if(MeshKDTree == null)
            MeshKDTree = GetComponent<MeshKDTree>();

        if(MeshKDTree == null)
            MeshKDTree = gameObject.AddComponent<MeshKDTree>();

        return MeshKDTree;
    }

    public float GetVolumeSize(){
        return mesh.bounds.size.sqrMagnitude;
    }

    public void AtlasInit(){
        IsOnMorgue = false;
    }

    public int GetID(){
        return ID;
    }

    public string GetPublicName(){
        //Debug.Log("Getting public name of " + Path);
        string finalName = "";
        string name = "";
        string side = "";
        if(!string.IsNullOrEmpty(Path)){
            name = ZAnatomyUtils.GetIDModelFromPath(Path);
        }else{
            name = gameObject.name;
        }

        string[] splittedName = name.Split(".");

        if(splittedName.Length > 1){
            name = splittedName[0].ToLower();
            side = splittedName[1];
            if(side.Contains("r")){
                side = "Right ";
            }
            if(side.Contains("l")){
                side = "Left ";
            }
            if(side.Contains("a")){
                side = "Anterior ";
            }
            if(side.Contains("p")){
                side = "Posterior ";
            }
        }else{
            name = ZAnatomyUtils.UpperFirstChar(name);
        }

        finalName = side+name;
        //Debug.Log("Final name: " + finalName);
        return finalName;
    }

    public void AddRigidbody(bool add){
        if(add){
            if(Rigidbody == null){
                Rigidbody = gameObject.AddComponent<Rigidbody>();
                Rigidbody.constraints = RigidbodyConstraints.FreezeAll;
                Rigidbody.useGravity = false;
                Rigidbody.drag = 1;
                foreach(MeshCollider mesh in Rigidbody.GetComponentsInChildren<MeshCollider>()){
                    mesh.convex = true;
                }
            }
        }else{
            if(Rigidbody != null){
                foreach(MeshCollider mesh in Rigidbody.GetComponentsInChildren<MeshCollider>()){
                    mesh.convex = false;
                }
                Destroy(Rigidbody);
                Rigidbody = null;
            }
        }
    }
    
    public Rigidbody GetRigidbody(){
        return Rigidbody;
    }

    public Vector3 GetModelsOverValue(){
        if(GetMeshKDTree()==null){
            return Collider.ClosestPoint(GetCenterPosition() + Vector3.up);
        }
        // return Collider.ClosestPointOnBounds(GetCenterPosition());
        return GetMeshKDTree().ClosestPointOnSurface(GetCenterPosition()+Vector3.up);
    }

    public Vector3 GetMorgueOriginalPosition(){
        return MorgueOriginalPosition + MorgueDeltaPosition;
    }

    public void UpdateMorgueOriginalPosition(Vector3 deltaMove){
        MorgueDeltaPosition = deltaMove;
        // MorgueOriginalPosition += deltaMove * Vector3.forward;
    }
    
    // void Update()
    // {
    //     Debug.DrawLine(GetMorgueOriginalPosition(), transform.position, Color.red);
    // }

    public void MorgueSetOnPosition(bool setInPlace, bool backToLastPos=false){
        if(setInPlace){
            MorgueShowMarker(false);
            Vector3 originalPos = GetMorgueOriginalPosition();

            // LeanTween.cancel(gameObject);
            // LeanTween.move(gameObject, originalPos, 1).setEaseOutCirc().setOnComplete(()=>{
            //     transform.position = originalPos;
            // });
            // LeanTween.rotate(gameObject, m_StartRotation.eulerAngles, 1).setEaseOutCirc().setOnComplete(()=>{
            //     transform.rotation = m_StartRotation;
            // });

            transform.position = originalPos;
            transform.rotation = m_StartRotation;

            m_LastPosition = transform.position;
            MorgueIsInPlace = true;
        }else{
            if(backToLastPos){
                // LeanTween.cancel(gameObject);
                // LeanTween.move(gameObject, m_LastPosition, 1).setEaseOutCirc().setOnComplete(()=>{
                //     transform.position = m_LastPosition;
                //     transform.rotation = m_StartRotation;
                // });

                transform.position = m_LastPosition;
                transform.rotation = m_StartRotation;
            }else{
                m_LastPosition = transform.position;
            }
            MorgueShowMarker(true);
            // transform.SetParent(MorgueParent, true);
            MorgueIsInPlace = false;
        }
        
    }

    public void Morgue_SetDependenciesAsBelow(){
        
        foreach (MeshInteractiveController item in ModelsOverList)
        {
            //Debug.DrawLine(item.Model.GetModelsOverValue(), GetModelsOverValue(), Color.red, Mathf.Infinity);
            item.Morgue_RegisterDependency(this);
        }
    }

    public void Morgue_RegisterDependency(MeshInteractiveController model){
        if(!ModelsBelowList.Contains(model)){
            ModelsBelowList.Add(model);
        }
    }

    public void Morgue_Init(){
        MorgueParent = transform.parent;
        MorgueOriginalPosition = transform.position;
        m_LastPosition = MorgueOriginalPosition;
        m_StartRotation = transform.rotation;
        IsOnMorgue = true;
    }


    // [FoldoutGroup("Debug Buttons")]
    // [Button]
    public void Morgue_SetConnectedModels(){
        m_ConnectedModelsList = new List<DataList.DataModel>();
        if(ZAnatomyController == null){
            ZAnatomyController = transform.GetComponentInParent<ZAnatomyController>();
        }
        List<DataList.DataModel> fullList = ZAnatomyController.Data.DataModelsList;

        // Prueba con un raycast por cada vértice
        RaycastHit hit;
        ModelsOverList = new List<MeshInteractiveController>();
        ModelsBelowList = new List<MeshInteractiveController>();
        m_ConnectedColliders_Over = new List<Collider>();
        m_ConnectedColliders_Below = new List<Collider>();
        Vector3[] vertices = Collider.sharedMesh.vertices;//GetComponent<MeshFilter>().sharedMesh.vertices;//Collider.sharedMesh.vertices;
        Vector3[] normals = Collider.sharedMesh.normals;//GetComponent<MeshFilter>().sharedMesh.normals;
        // Dictionary<string, int> RaycastCounts = new Dictionary<string, int>();
        // int minCountToAdd = 1;
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 vertex = vertices[i];
            Vector3 normal = transform.TransformDirection(normals[i]);
            // float scaleAmount = 0.95f;
            // vertex = new Vector3(vertex.x *scaleAmount, vertex.y * scaleAmount, vertex.z);
            vertex = transform.TransformPoint(vertex);
            float normalTolerance = ZAnatomyController.CalculateModelOver_NormalTolerance;// 0.95f;//0.9f;
            bool isLookingUp = ZAnatomyController.CalculateModelOver_UseLookUp && Vector3.Dot(normal, Vector3.up) > normalTolerance;
            bool isLookingDown = ZAnatomyController.CalculateModelOver_UseLookDown && Vector3.Dot(normal, -Vector3.up) > normalTolerance;

            float minDistanceToCount = ZAnatomyController.CalculateModelOver_MinDistanceToCount;//0.1f;

            if(isLookingUp){
                if (Physics.Raycast(vertex, Vector3.up, out hit, Mathf.Infinity/*, LayerMask.GetMask("Mask")*/)){
                    //Debug.Log("Raycasted: "+ hit.collider.gameObject.name);
                    if(hit.collider.gameObject != gameObject && !m_ConnectedColliders_Over.Contains(hit.collider) && !m_ConnectedColliders_Below.Contains(hit.collider)){
                        // string id = hit.collider.gameObject.name;

                        // int count = 1;
                        // if(RaycastCounts.ContainsKey(id)){
                        //     count = RaycastCounts[id];
                        //     count++;
                        //     RaycastCounts[id] = count;
                        // }else{
                        //     RaycastCounts.Add(id,count);
                        // }

                        // if(count>=minCountToAdd){
                            if(Vector3.Distance(hit.point, vertex)>minDistanceToCount){

                                m_ConnectedColliders_Over.Add(hit.collider);
                                ModelsBelowList.Add(hit.collider.GetComponent<MeshInteractiveController>());
                            }
                            
                        // }
                    }
                }
                // Debug.Log(vertex);     
                //Debug.Log("Vertex #"+i+" = " + vertices[i].ToString("F7"));
                Debug.DrawRay(vertex, Vector3.up * 10, Color.red, 5);
            }

            if(isLookingDown){
                // RaycastCounts = new Dictionary<string, int>();
                if(Physics.Raycast(vertex, -Vector3.up, out hit, Mathf.Infinity/*, LayerMask.GetMask("Mask")*/)){
                    if(hit.collider.gameObject != gameObject && !m_ConnectedColliders_Below.Contains(hit.collider) && !m_ConnectedColliders_Over.Contains(hit.collider)){
                        // string id = hit.collider.gameObject.name;

                        // int count = 1;
                        // if(RaycastCounts.ContainsKey(id)){
                        //     count = RaycastCounts[id];
                        //     count++;
                        //     RaycastCounts[id] = count;
                        // }else{
                        //     RaycastCounts.Add(id,count);
                        // }

                        // if(count>=minCountToAdd){
                            if(Vector3.Distance(hit.point, vertex)>minDistanceToCount){
                                m_ConnectedColliders_Below.Add(hit.collider);
                                ModelsOverList.Add(hit.collider.GetComponent<MeshInteractiveController>());
                            }
                        // }
                    }
                }
                Debug.DrawRay(vertex, -Vector3.up * 10, Color.yellow, 5);
            }
        }

        DataList.DataModel currentDataModel = ZAnatomyController.Data.DataModelsList.Find((x)=>x.NameSource.Equals(ZAnatomyUtils.GetIDModelFromPath(Path)));
        currentDataModel.ConnectedModelIDs_Anterior = new List<string>();
        currentDataModel.ConnectedModelIDs_Posterior = new List<string>();

        m_ConnectedModelsList = new List<DataList.DataModel>();
        foreach (Collider collider in m_ConnectedColliders_Over)
        {
            DataList.DataModel dataModel = fullList.Find((x)=>x.Model.gameObject == collider.gameObject);
            if(dataModel != null){
                string IDConnectedModel = ZAnatomyUtils.GetIDModelFromPath(dataModel.Path);
                currentDataModel.ConnectedModelIDs_Anterior.Add(IDConnectedModel);
                m_ConnectedModelsList.Add(dataModel);
            }
        }
        foreach (Collider collider in m_ConnectedColliders_Below)
        {
            DataList.DataModel dataModel = fullList.Find((x)=>x.Model.gameObject == collider.gameObject);
            if(dataModel != null){
                string IDConnectedModel = ZAnatomyUtils.GetIDModelFromPath(dataModel.Path);
                currentDataModel.ConnectedModelIDs_Posterior.Add(IDConnectedModel);
                m_ConnectedModelsList.Add(dataModel);
            }
        }
    }

    // [FoldoutGroup("Debug Buttons")]
    // [Button]
    public void Morgue_ResetModelsBelow(){
        ModelsBelowList.Clear();
        m_ConnectedColliders_Over.Clear();
    }

    // [FoldoutGroup("Debug Buttons")]
    // [Button]
    public void Morgue_UpdateModelsBelow(MeshInteractiveController belowObject = null, DataList.DataModel data=null){
        if(belowObject == null){
            belowObject = this;
        }else{
            if(belowObject == this){
                return;
            }

            if(!ModelsBelowList.Contains(belowObject)){
                ModelsBelowList.Add(belowObject);
            }
            else{
                return;
            }

            if(!m_ConnectedColliders_Over.Contains(belowObject.Collider)){
                m_ConnectedColliders_Over.Add(belowObject.Collider);
                m_ConnectedModelsList.Add(data);
            }
        }

        for (int i = 0; i < m_ConnectedModelsList.Count; i++)
        {
            DataList.DataModel dataModel = m_ConnectedModelsList[i];
            MeshInteractiveController meshInteractive = dataModel.Model;
            // MeshInteractiveController meshInteractive = collider.GetComponent<MeshInteractiveController>();
            if(meshInteractive == this || meshInteractive == null)
                continue;

            // if(!meshInteractive.ModelsBelowList.Contains(belowObject))
            //     meshInteractive.ModelsBelowList.Add(belowObject);

            // if(!meshInteractive.m_ConnectedColliders_Over.Contains(belowObject.Collider)){
            //     meshInteractive.m_ConnectedColliders_Over.Add(belowObject.Collider);
            //     meshInteractive.m_ConnectedModelsList.Add(dataModel);
            // }else{
            //     continue;
            // }

            meshInteractive.Morgue_UpdateModelsBelow(belowObject, dataModel);
            // ModelsBelowList.Add();
        }
        // HAZ ESTO
    }

// #if UNITY_EDITOR
//     void OnDrawGizmos()
//     {
//         // if (UnityEditor.Selection.activeGameObject != this.gameObject) return;
        
//         // Gizmos.color = Color.green;
//         // Gizmos.DrawWireCube(Collider.bounds.center, Collider.bounds.size);

//         // foreach(DataList.DataModel data in m_ConnectedModelsList){
//         //     // if(data.Model.Collider.ClosestPoint(data.Model.Collider.bounds.center + Vector3.up).y > Collider.ClosestPoint(Collider.bounds.center - Vector3.up).y){
//         //     if(m_ConnectedColliders_Over != null && m_ConnectedColliders_Over.Contains(data.Model.Collider)){
//         //         Gizmos.color = Color.red;
//         //         if(ZAnatomyController.Gizmo_ShowConnected_Over)
//         //             Gizmos.DrawWireCube(data.Model.Collider.bounds.center, data.Model.Collider.bounds.size);
//         //     }

//         //     if(m_ConnectedColliders_Below != null && m_ConnectedColliders_Below.Contains(data.Model.Collider)){
//         //         Gizmos.color = Color.yellow;
//         //         if(ZAnatomyController.Gizmo_ShowConnected_Below)
//         //             Gizmos.DrawWireCube(data.Model.Collider.bounds.center, data.Model.Collider.bounds.size);
//         //     }
//         // }
//     }
// #endif

    public void Morgue_CalculateModelsOver(List<MeshInteractiveController> list){
        // //ModelsOverList= list.FindAll((x)=>x.GetModelsOverValue().y > GetModelsOverValue().y);
        // //return;

        // Vector3 positionOver = transform.position + Vector3.up * 100;
        // List<ResourcesDataProfile.DataList.DataModel> connectedModels = m_ConnectedModelsList.FindAll(
        //     (x)=>x.Model.GetModelsOverValue().y > GetModelsOverValue().y);// GetCenterPosition().y > GetCenterPosition().y);//x.Model.GetMeshKDTree().ClosestPointOnSurface(positionOver).y>GetMeshKDTree().ClosestPointOnSurface(positionOver).y);//x.Model.GetCenterPosition().y > GetCenterPosition().y);//
        // ModelsOverList = new List<MeshInteractiveController>();
        // foreach (ResourcesDataProfile.DataList.DataModel item in connectedModels)
        // {
        //     //Debug.DrawLine(item.Model.GetModelsOverValue(), GetModelsOverValue(), Color.red, Mathf.Infinity);
        //     ModelsOverList.Add(item.Model);
        // }


        // ModelsBelowList = new List<MeshInteractiveController>();
        // // List<ResourcesDataProfile.DataList.DataModel> modelsBelow = m_ConnectedModelsList.FindAll(
        // //     (x)=>!ModelsOverList.Contains(x.Model));// GetCenterPosition().y > GetCenterPosition().y);//x.Model.GetMeshKDTree().ClosestPointOnSurface(positionOver).y>GetMeshKDTree().ClosestPointOnSurface(positionOver).y);//x.Model.GetCenterPosition().y > GetCenterPosition().y);//
        // // foreach (ResourcesDataProfile.DataList.DataModel item in modelsBelow)
        // // {
        // //     //Debug.DrawLine(item.Model.GetModelsOverValue(), GetModelsOverValue(), Color.red, Mathf.Infinity);
        // //     ModelsBelowList.Add(item.Model);
        // // }
    }

    public List<MeshInteractiveController> GetModelsOver(List<MeshInteractiveController> currentList=null){
        List<MeshInteractiveController> modelsAnterior = new List<MeshInteractiveController>();

        if(string.IsNullOrEmpty(Path)){
            Debug.Log("MeshInteractiveController has no Path");
            return new List<MeshInteractiveController>();
        }
        
        Debug.Log("Getting models over of "+ZAnatomyUtils.GetIDModelFromPath(Path));
        DataList.DataModel currentDataModel = ZAnatomyController.Data.DataModelsList.Find((x)=>x.NameSource.Equals(ZAnatomyUtils.GetIDModelFromPath(Path)));
        foreach(string model in currentDataModel.ConnectedModelIDs_Anterior){
            DataList.DataModel dataModelOver = ZAnatomyController.Data.DataModelsList.Find((x)=>x.NameSource.Equals(ZAnatomyUtils.GetIDModelFromPath(model)));
            modelsAnterior.Add(dataModelOver.Model);
        }
        return modelsAnterior;

        // return ModelsOverList;

        // if(currentList == null){
        //     currentList = new List<MeshInteractiveController>();
        // }
        // currentList.AddRange(ModelsOverList);
        // foreach(MeshInteractiveController interactiveMesh in ModelsOverList){
        //     interactiveMesh.GetModelsOver(currentList);
        // }
        // return currentList;
        // // return ModelsOverList;
    }

    public List<MeshInteractiveController> GetModelsBelow(List<MeshInteractiveController> currentList=null){
        List<MeshInteractiveController> modelsPosterior = new List<MeshInteractiveController>();

        DataList.DataModel currentDataModel = ZAnatomyController.Data.DataModelsList.Find((x)=>x.NameSource.Equals(ZAnatomyUtils.GetIDModelFromPath(Path)));
        foreach(string model in currentDataModel.ConnectedModelIDs_Posterior){
            DataList.DataModel dataModelOver = ZAnatomyController.Data.DataModelsList.Find((x)=>x.NameSource.Equals(ZAnatomyUtils.GetIDModelFromPath(model)));
            modelsPosterior.Add(dataModelOver.Model);
        }
        return modelsPosterior;
        //return ModelsBelowList;
        // //return new List<MeshInteractiveController>();// ModelsBelowList;

        // // List<MeshInteractiveController> modelsList = new List<MeshInteractiveController>(ModelsBelowList);
        // if(currentList == null){
        //     currentList = new List<MeshInteractiveController>();
        // }
        // currentList.AddRange(ModelsBelowList);
        // foreach(MeshInteractiveController interactiveMesh in ModelsBelowList){
        //     interactiveMesh.GetModelsBelow(currentList);
        // }
        // return currentList;
        // // return ModelsBelowList;
    }

    private Vector3 GetDifferenceFromCenter(){
        return transform.position - mesh.bounds.center;
    }

    public Vector3 GetCenterPosition(){
        if(mesh == null)
            mesh = GetComponent<MeshRenderer>();
        return mesh.bounds.center;
    }

    public Renderer GetRenderer(){
        if(mesh == null)
            mesh = GetComponent<MeshRenderer>();
        return mesh;
    }

    public void SetNotSelected(bool isDefault = true){
        if(mesh == null)
            return;
            
        mesh.materials = CustomSelectedMaterials;
    }

    // [Button]
    public void UpdateCustomMaterials(Material TransparentSelectedMaterial){
        CustomSelectedMaterials = GetRenderer().sharedMaterials;
        CustomNotSelectedMaterial = TransparentSelectedMaterial;
        CustomColor = CustomSelectedMaterials[0].color;
    }

    // [Button]
    private void RecalculateNormals(){
        GetComponent<MeshFilter>().sharedMesh.RecalculateNormals();
    }

    private void UpdateMaterialsColors(Color newColor){
        List<Material> materialsList = new List<Material>();
        for (int i = 0; i < mesh.materials.Length; i++)
        {
            Material newMat = mesh.materials[i];
            newMat.renderQueue = 3100-i;
            newMat.color = newColor;
            materialsList.Add(newMat);
        }

        mesh.materials = materialsList.ToArray();
    }

    public void Select(bool select){
        if(select){
            UpdateMaterialsColors(Color.yellow);
        }
        else{
            if(mesh==null)
                return;
            
            SetNotSelected();
        }
    }

    public void MorgueAddMarker(GameObject prefabMarker){
        if(MorgueMarker == null){
            MorgueMarker = Instantiate(prefabMarker);
            MorgueMarker.transform.SetParent(transform, true);
            MorgueMarker.transform.position = GetCenterPosition();
            MorgueMarkerScale = MorgueMarker.transform.localScale;
        }
    }

    public void MorgueShowMarker(bool show){
        if(MorgueMarker != null){
            MorgueMarker.gameObject.SetActive(show);
            if(show){
                MorgueMarker.transform.localScale = Vector3.zero;

                // LeanTween.cancel(MorgueMarker.gameObject);
                // LeanTween.scale(MorgueMarker.gameObject, MorgueMarkerScale, 1);

                MorgueMarker.transform.localScale = MorgueMarkerScale;
            }
        }
    }
    
    public void UpdateCollider(){
        if(Collider==null)
            Collider = GetComponent<MeshCollider>();
    }

    public void SetCollider(bool enabled){
        UpdateCollider();

        Collider.enabled = enabled;
    }

    public void SetID(int id){
        ID = id;
    }

    void OnTriggerEnter(Collider other)
    {
        if(IsOnMorgue){
            // if(other.CompareTag("PlayerHand") && other.GetComponentInParent<MorguePlayerController>())
            // {
            //     other.GetComponentInParent<MorguePlayerController>().OnVRHandTriggerEnter(this);
            // }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(IsOnMorgue){
            // if(other.CompareTag("PlayerHand") && other.GetComponentInParent<MorguePlayerController>())
            // {
            //     other.GetComponentInParent<MorguePlayerController>().OnVRHandTriggerExit(this);
            // }
        }
    }
}
}