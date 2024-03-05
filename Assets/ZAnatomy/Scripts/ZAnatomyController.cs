using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

#if MIRROR
using Mirror;
#endif

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace ZAnatomy{
    public class ZAnatomyController : MonoBehaviour
    {

        #if ODIN_INSPECTOR
        [Button]
        #endif
        public void EnableOrganType(OrganType type, bool enable){        
            IterateDataListWithFunction((data)=>{
                if(data.BodyPart == type){
                    data.Model.gameObject.SetActive(enable);
                }else{
                    //Debug.Log("Not found body part: "+ data.BodyPart + " not equal to " +type);
                }
            });
        }
        public void SetOneOrganType(OrganType type)
        {
            IterateDataListWithFunction((data) => {
                if (data.BodyPart == type)
                {
                    data.Model.gameObject.SetActive(true);
                }
                else
                {
                    data.Model.gameObject.SetActive(false);
                }
            });
        }

        public enum OrganType{
            Skeletal,
            Nervous,
            Joints,
            Cardiovascular,
            Visceral,
            Muscular,
            Body,
            NotRecognized
        }

        [System.Serializable]
        public class OrganPrefab{
            public string ID;
            public string Path;
            public OrganType OrganType;
            public MeshInteractiveController UnifiedPrefab;
            public string UnifiedPrefabRelativePath;
            public Vector3 LocalPosition;
            public Vector3 LocalEulerAngles;
        }

        [System.Serializable]
        public class OrganTypeInfo{
            public OrganType Type;
            public string Name;
            public Color Color;
            public bool Enabled;
        }

        public bool AutoinitOnStart;
        public bool UseOrganPrefabs;
        #if ODIN_INSPECTOR
        [ShowIf("UseOrganPrefabs")]
        #endif
        public List<OrganPrefab> OrganPrefabsList;
        #if ODIN_INSPECTOR
        [ShowIf("UseOrganPrefabs")]
        #endif
        public Transform OrgansPrefabsParent;
        
        #if UNITY_EDITOR
            #if ODIN_INSPECTOR
            [ShowIf("UseOrganPrefabs")]
            [Button]
            [ButtonGroup("Prefabs to Paths")]
            #endif
            public void UpdatePrefabsPathList(){
                foreach(OrganPrefab prefab in OrganPrefabsList){
                    MeshInteractiveController meshInteractivePrefab = null;

                    if(prefab.UnifiedPrefab !=null){
                        string path = AssetDatabase.GetAssetPath(prefab.UnifiedPrefab.gameObject.GetInstanceID());
                        path = path.Substring(path.IndexOf("Prefabs"));
                        path = path.Replace(".prefab", "");
                        prefab.UnifiedPrefabRelativePath = path;
                        meshInteractivePrefab = prefab.UnifiedPrefab;
                    }else{
                        Debug.LogError("UnifiedPrefab "+prefab.ID + " is null - Loading prefab from path");
                        
                        string path = GetMainPrefabPath()+prefab.UnifiedPrefabRelativePath+".prefab";
                        Debug.Log("--- Loading unified meshInteractive prefab in "+path);
                        meshInteractivePrefab = AssetDatabase.LoadAssetAtPath<ZAnatomy.MeshInteractiveController>(path);
                    }

                    UpdateMeshInteractiveDividedPrefabsPath(meshInteractivePrefab);
                }
            }
            #if ODIN_INSPECTOR
            [ShowIf("UseOrganPrefabs")]
            [Button]
            [ButtonGroup("Prefabs to Paths")]
            #endif
            public void RemovePrefabsReferences(){
                LoadPrefabsReferences();

                foreach(OrganPrefab prefab in OrganPrefabsList){
                    RemovePrefabReferenceFromDividedPrefab(prefab.UnifiedPrefab);
                    prefab.UnifiedPrefab = null;
                }
            }
            #if ODIN_INSPECTOR
            [ShowIf("UseOrganPrefabs")]
            [Button]
            [ButtonGroup("Prefabs to Paths")]
            #endif
            public void LoadPrefabsReferences(){
                foreach(OrganPrefab prefab in OrganPrefabsList){
                    if(string.IsNullOrEmpty(prefab.UnifiedPrefabRelativePath)){
                        Debug.LogError("--- Prefab called "+prefab.ID+ " has no RelativePath to load");
                    }else{
                        // path = "/Assets/ZAnatomy/CreatedModels/"+gameObject.name+path;
                        // string[] splittedPath = path.Split("/");
                        
                        // path = path.Substring(0, path.Split("/")[path.Split("/").Length-1].Length);
                        // GetParent(path);
                        
                        string path = GetMainPrefabPath()+"/"+prefab.UnifiedPrefabRelativePath+".prefab";
                        path = path.Replace("\\","/");
                        prefab.UnifiedPrefab = AssetDatabase.LoadAssetAtPath<MeshInteractiveController>(path);
                        Debug.Log("--- Loading path: "+path);
                        LoadMeshInteractiveDividedPiecesReferences(prefab.UnifiedPrefab);
                    }
                }

                Debug.Log("Finished load prefab references");
            }

            private void LoadMeshInteractiveDividedPiecesReferences(MeshInteractiveController unifiedMeshInteractive){
                string path = GetMainPrefabPath()+"/"+unifiedMeshInteractive.DividedPrefabRelativePath+".prefab";
                Debug.Log("--- Loading divided organ controller prefab in "+path);
                unifiedMeshInteractive.DividedPrefab = AssetDatabase.LoadAssetAtPath<DividedOrganController>(path);
            }

            private void RemovePrefabReferenceFromDividedPrefab(MeshInteractiveController unifiedMeshInteractive){
                UpdateMeshInteractiveDividedPrefabsPath(unifiedMeshInteractive);

                unifiedMeshInteractive.DividedPrefab = null;
            }

            private void UpdateMeshInteractiveDividedPrefabsPath(MeshInteractiveController unifiedMeshInteractive){
                if(unifiedMeshInteractive.DividedPrefab != null){
                    string path = AssetDatabase.GetAssetPath(unifiedMeshInteractive.DividedPrefab.gameObject.GetInstanceID());
                    path = path.Substring(path.IndexOf("Prefabs"));
                    path = path.Replace(".prefab", "");
                    unifiedMeshInteractive.DividedPrefabRelativePath = path;
                }else{
                    Debug.LogError("MeshInteractive "+unifiedMeshInteractive.Path+ " has null DividedPrefab");
                }
            }

            private string GetMainPrefabPath(){
                string path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(this);
                path = Path.GetDirectoryName(path);
                return path;
            }
        #endif

        public List<MeshRenderer> ExcludedMeshRendererList;

        public DataList Data;
        public List<OrganTypeInfo> OrgansTypeInfo;

        [HideInInspector]
        public string DataJson;
        
        public int HorizontalDistanceExpandRegions = 1;
        public List<Material> OriginalMaterialsList;
        public Material TransparentSelectedMaterial;
        public BoxCollider BoxRegionDropObjects;
        private int AutoIDCount = 0;

        [Header("Calculate Models Over")]
        
        #if ODIN_INSPECTOR
        [FoldoutGroup("Calculate Models Over")]
        #endif
        public bool Gizmo_ShowConnected_Over = true;
        #if ODIN_INSPECTOR
        [FoldoutGroup("Calculate Models Over")]
        #endif
        public bool Gizmo_ShowConnected_Below = true;
        #if ODIN_INSPECTOR
        [FoldoutGroup("Calculate Models Over")]
        #endif
        public float CalculateModelOver_NormalTolerance = 0.95f;
        #if ODIN_INSPECTOR
        [FoldoutGroup("Calculate Models Over")]
        #endif
        public bool CalculateModelOver_UseLookUp = true;
        #if ODIN_INSPECTOR
        [FoldoutGroup("Calculate Models Over")]
        #endif
        public bool CalculateModelOver_UseLookDown;
        #if ODIN_INSPECTOR
        [FoldoutGroup("Calculate Models Over")]
        #endif
        public float CalculateModelOver_MinDistanceToCount = 0.1f;

        private System.Action m_LoadFinishedCallback;
        public static ZAnatomyController zAnatomy;

        void Awake()
        {
            zAnatomy = this;
        }

        void Start()
        {
            if(AutoinitOnStart){
                Init();
            }
        }

        public void ImportOrgansTypeInfo(List<OrganTypeInfo> importedOrgansTypeInfo){
            OrgansTypeInfo = new List<OrganTypeInfo>(importedOrgansTypeInfo);
        }

        public void Init(System.Action loadFinishedCallback = null){
            m_LoadFinishedCallback = loadFinishedCallback;

            if(UseOrganPrefabs){
                StartCoroutine(InstantiateAllOrgansAndInit());
            }else{
                // InitOrgans();
                if(m_LoadFinishedCallback != null){
                    m_LoadFinishedCallback();
                }
            }
        }

        private static int OrderOnType(DataList.DataModel item)
        {
            if(item.BodyPart is OrganType.Body)
                return 0;
            if(item.BodyPart is OrganType.Muscular)
                return 1;
            if(item.BodyPart is OrganType.Skeletal)
                return 2;
            if(item.BodyPart is OrganType.Visceral)
                return 3;
            return 4;
        }

        private IEnumerator InstantiateAllOrgansAndInit(){
            List<DataList.DataModel> orderedByRegionList = new List<DataList.DataModel>(Data.DataModelsList);
            orderedByRegionList.Sort((x, y) => OrderOnType(x).CompareTo(OrderOnType(y)));

            foreach(DataList.DataModel data in orderedByRegionList){
                OrganPrefab organ = OrganPrefabsList.Find((x)=>x.ID.Equals(data.NameSource));
                
                if(organ == null){
                    Debug.LogError("There is no organ with path "+organ.Path + " in ZAnatomyController.DataList");
                    data.Model = null;
                }else{
                    // Get Prefab direct reference
                    MeshInteractiveController prefabReference = organ.UnifiedPrefab;

                    // Try to load from Resources
                    if(prefabReference == null){
                        // prefabReference = Resources.Load<ZAnatomy.MeshInteractiveController>(organ.UnifiedPrefabRelativePath);
                        
                        ResourceRequest resourceRequest = Resources.LoadAsync<ZAnatomy.MeshInteractiveController>(organ.UnifiedPrefabRelativePath);
                        yield return resourceRequest;
                        prefabReference = resourceRequest.asset as ZAnatomy.MeshInteractiveController;
                    }

                    // // Try to load from streamingAssets
                    // if(prefabReference == null){
                    //     string path = Application.streamingAssetsPath + GetMainPrefabName() + organ.UnifiedPrefabRelativePath;
                    //     Debug.Log("Loading organ prefab: "+path);
                    //     prefabReference = AssetDatabase.LoadAssetAtPath<MeshInteractiveController>(path);
                    // }

                    // // Try to load from asset path 
                    // if(prefabReference == null){
                    //     string path = GetMainPrefabPath()+organ.UnifiedPrefabRelativePath;
                    //     Debug.Log("Loading organ prefab: "+path);
                    //     prefabReference = AssetDatabase.LoadAssetAtPath<MeshInteractiveController>(path);
                    // }

                    if(prefabReference != null){
#if MIRROR
                        if (NetworkClient.isHostClient && prefabReference.gameObject.GetComponent<NetworkIdentity>())
                        {
                            data.Model = Instantiate(prefabReference, OrgansPrefabsParent);
                            // data.Model.ZAnatomyController = this;
                            // data.Model.gameObject.SetActive(OrgansTypeInfo.Find((x) => x.Type == data.BodyPart).Enabled);
                            // data.Model.transform.localPosition = organ.LocalPosition;
                            // data.Model.transform.localEulerAngles = organ.LocalEulerAngles;
                            NetworkServer.Spawn(data.Model.gameObject);
                        } else if (prefabReference.gameObject.GetComponent<NetworkIdentity>()==null)
                        {
                            data.Model = Instantiate(prefabReference, OrgansPrefabsParent);
                            // data.Model.ZAnatomyController = this;
                            // data.Model.gameObject.SetActive(OrgansTypeInfo.Find((x) => x.Type == data.BodyPart).Enabled);
                            // data.Model.transform.localPosition = organ.LocalPosition;
                            // data.Model.transform.localEulerAngles = organ.LocalEulerAngles;
                        }
                        MeshInteractableOnline meshOnline = data.Model.GetComponent<MeshInteractableOnline>();
                        if (meshOnline == null) meshOnline = data.Model.gameObject.AddComponent<MeshInteractableOnline>();
                        meshOnline.SetID(OnlineUtilities.GetOnlineUtilities().meshesOnline.Count);
                        OnlineUtilities.GetOnlineUtilities().meshesOnline.Add(data.Model.GetComponent<MeshInteractableOnline>());
#else
                            data.Model = Instantiate(prefabReference, OrgansPrefabsParent);
                            // data.Model.ZAnatomyController = this;
                            // data.Model.gameObject.SetActive(OrgansTypeInfo.Find((x) => x.Type == data.BodyPart).Enabled);
                            // data.Model.transform.localPosition = organ.LocalPosition;
                            // data.Model.transform.localEulerAngles = organ.LocalEulerAngles;
#endif
                        // data.Model = Instantiate(prefabReference, OrgansPrefabsParent);
                        // data.Model.gameObject.SetActive(OrgansTypeInfo.Find((x)=>x.Type== data.BodyPart).Enabled);
                        // data.Model.transform.localPosition = organ.LocalPosition;
                        // data.Model.transform.localEulerAngles = organ.LocalEulerAngles;
                    }
                    else{
                        Debug.LogError("Prefab reference of "+data.NameSource+" could not be found: "+organ.UnifiedPrefabRelativePath);
                    }
                }

                yield return null;
            }
            
            // InitOrgans();

            if(m_LoadFinishedCallback != null){
                m_LoadFinishedCallback();
            }
        }

        // private void InitOrgans(){
        //     IterateDataListWithFunction((data)=>{
        //         data.Model.InitOrganInPlace();
        //         // data.Model.MorgueIsInPlace = true;
        //         // data.Model.Init(data.ID, data.Model.CustomColor, this, data.Path);// Utils.ColorFromHex(data.Color));
        //         // data.Model.Morgue_Init();
        //         // data.Model.Morgue_CalculateModelsOver(Data.GetInteractiveMeshList());

        //         // EnableMeshColliders(true);
        //     });

        //     if(m_LoadFinishedCallback != null){
        //         m_LoadFinishedCallback();
        //     }
        // }

        /// <summary>
        /// Iterate all DataModels in DataList and execute parametrized function in every member
        /// </summary>
        /// <param name="iterationFunction"></param>
        public void IterateDataListWithFunction(System.Action<DataList.DataModel> iterationFunction){
            foreach(DataList.DataModel data in Data.DataModelsList){
                iterationFunction(data);
            };
        }

        public DataList.DataModel GetDataModel(string nameSource){
            DataList.DataModel dataModel = Data.DataModelsList.Find((x)=>nameSource.Equals(x.NameSource.Trim()));

            if(dataModel== null){
                Debug.Log("zAnatomyController cannot find DataModel with ID "+nameSource);
            }

            return dataModel;
        }

        public OrganPrefab GetOrganPrefab(string nameSource){
            OrganPrefab organ = OrganPrefabsList.Find((x)=>x.ID.Equals(nameSource));
            
            if(organ == null){
                Debug.Log("zAnatomyController cannot find OrganPrefab with ID "+nameSource);
            }

            return organ;
        }

        #if ODIN_INSPECTOR
        [Button]
        #endif
        public void UpdateDataList(){
            UpdateExcludedMeshes();
            AutoIDCount=0;
            Data = new DataList();
            Data.DataModelsList = new List<DataList.DataModel>();
            UpdateDataListRecursive(transform);
            DataJson = DataList.ToJSON(Data);
            Debug.Log("Updated Data List");
        }

        #if ODIN_INSPECTOR
        [FoldoutGroup("Advanced Debug Buttons")]
        [Button]
        #endif
        public void UpdateExcludedMeshes(){
            ExcludedMeshRendererList = new List<MeshRenderer>(transform.FindChildRecursiveInChildren("_ExcludeObjectsParent").GetComponentsInChildren<MeshRenderer>());
            Debug.Log("Refreshed Excluded Meshes");
        }

        #if ODIN_INSPECTOR
        [Button]
        #endif
        public void UpdateMeshInteractiveControllers(){
            IterateDataListWithFunction((data)=>{
                data.Model.ZAnatomyController = this;
                data.Model.CustomColor = data.Model.GetComponent<MeshRenderer>().sharedMaterial.color;
            });
            Debug.Log("Updated Mesh Interactive Controllers");
        }

        #if ODIN_INSPECTOR
        [Button]
        #endif
        private void DestroyAllTMProObjects(){
            foreach(TMPro.TextMeshPro title in transform.GetComponentsInChildren<TMPro.TextMeshPro>(true)){
                Debug.Log("Deleting "+ title.gameObject.name);
                GameObject.DestroyImmediate(title.gameObject);
            
            }
            Debug.Log("Destroyed All TMProObjects");
        }

        #if ODIN_INSPECTOR
        [Button]
        #endif
        private void UpdateOrganTypes(){
            IterateDataListWithFunction((data)=>{
                data.Model.BodyPart = ZAnatomyUtils.GetOrganTypeFromPart(data.Model.BodyType);
            });
        }

        private void UpdateDataListRecursive(Transform parent){
            foreach(Transform child in parent){
                MeshInteractiveController interactiveMesh = child.GetComponent<MeshInteractiveController>();

                if(interactiveMesh != null){
                    DataList.DataModel newModel = new DataList.DataModel();
                    newModel.ID = AutoIDCount++;

                    string cleanName = interactiveMesh.gameObject.name;
                    if(cleanName.Contains(".")){
                        cleanName = cleanName.Split('.')[0];
                    }
                    newModel.Name = cleanName;
                    newModel.NameSource = interactiveMesh.gameObject.name;
                    
                    newModel.Path = interactiveMesh.Path;
                    newModel.Type = interactiveMesh.BodyType;
                    newModel.BodyPart = interactiveMesh.BodyPart;
                    // if(newModel.BodyPart == OrganType.NotRecognized){
                    //     newModel.BodyPart = OrganType.Visceral;
                    // }
                    newModel.DividedModelPrefab = interactiveMesh.DividedPrefab;

                    newModel.Color = "#"+ColorUtility.ToHtmlStringRGB(interactiveMesh.GetComponent<MeshRenderer>().sharedMaterial.color);
                    newModel.Model = interactiveMesh;

                    Data.DataModelsList.Add(newModel);
                }

                if(child.childCount > 0){
                    UpdateDataListRecursive(child);
                }
            }
        }

        
        #if ODIN_INSPECTOR
        [FoldoutGroup("Advanced Debug Buttons")]
        [Button]
        #endif
        public void EnableAllMeshColliders(){
            EnableMeshColliders(true);
        }
        
        #if ODIN_INSPECTOR
        [FoldoutGroup("Advanced Debug Buttons")]
        [Button]
        #endif
        public void DisableAllMeshColliders(){
            EnableMeshColliders(false);
        }

        #if ODIN_INSPECTOR
        [FoldoutGroup("Advanced Debug Buttons")]
        [Button]
        #endif
        public void UpdateCustomMaterials(){
            IterateDataListWithFunction((data)=>{
                data.Model.UpdateCustomMaterials(TransparentSelectedMaterial);
            });
        }

        public void EnableMeshColliders(bool enable, int depth=-1){
            IterateDataListWithFunction((data)=>{
                data.Model.SetCollider(enable);
                data.Model.CanBeGrabbed = true;
            });
        }

        public void EnableAllMeshGameObjects(bool enable){
            IterateDataListWithFunction((data)=>{
                data.Model.gameObject.SetActive(enable);
            });
        }

        #if ODIN_INSPECTOR
        [FoldoutGroup("Calculate Models Over")]
        [Button]
        #endif
        private void LoadAllConnectedObjectsInMesh(){
            IterateDataListWithFunction((data)=>{
                data.Model.Morgue_SetConnectedModels();
                data.Model.Morgue_ResetModelsBelow();
            });

            IterateDataListWithFunction((data)=>{
                data.Model.Morgue_UpdateModelsBelow();
            });

            Debug.Log("LOADED ALL CONNECTED MESH");
        }
    }
}
