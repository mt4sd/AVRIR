// using System.Collections;
// using System.Collections.Generic;
// using System.Linq;
// using Sirenix.OdinInspector;
// using UnityEngine;

// public class MorguePlayerController : MonoBehaviour
// {
//     // Common
//     public bool IsForVR;
//     public CharacterController CharacterController;
//     public MorgueController Morgue;


//     // VR Content
//     [ShowIf("IsForVR")]
//     public Transform CameraRigParent;
//     [ShowIf("IsForVR")]
//     public float CameraRigMoveSpeed = 1;
//     [ShowIf("IsForVR")]
//     public Vector2 CameraRigHeightMinMax;
//     [ShowIf("IsForVR")]
//     public float SpeedVerticalMove = 100;
//     [ShowIf("IsForVR")]
//     public GameObject FloorPositionMarker;
//     [ShowIf("IsForVR")]
//     public LineRenderer FloorPositionRay;
//     [ShowIf("IsForVR")]
//     public VRFloatingNameModel FloatingNameModel;
//     [ShowIf("IsForVR")]
//     public float VRDistanceGrabObject = 1;
//     [ShowIf("IsForVR")]
//     public Transform VRHand;
//     [ShowIf("IsForVR")]
//     public OVRInput.Button ClickButton;

//     // NO VR
//     // public Camera SceneCamera;
//     [HideIf("IsForVR")]
//     public Camera NoVRCamera;
//     [HideIf("IsForVR")]
//     public float GrabAnimTime = 0.25f;
//     [HideIf("IsForVR")]
//     public Transform MarkerGrabObjects;
//     [HideIf("IsForVR")]
//     public float minFov = 35f;
//     [HideIf("IsForVR")]
//     public float maxFov = 100f;
//     [HideIf("IsForVR")]
//     public Vector2 MinMaxGrabbingDistance;
//     [HideIf("IsForVR")]
//     public float ExtraRaycastForwardDistance = 0.1f;
//     [HideIf("IsForVR")]
//     public GameObject Marker3DPrefab;

//     // Private
//     private MeshInteractiveController m_LastPointedModel;
//     private Vector3 m_GrabbedModelLastPosition;
//     private MeshInteractiveController GrabbedModel;
//     private bool m_isGrabbing;
//     private Vector3 m_GrabbedModelForward;
//     float sensitivity = 17;


//     private bool m_ShowNameParent=true;
//     private GameObject m_OpenCloseBodySphere;
//     private Vector3 m_UpVector;
//     private float m_LastHandMeshDistance;
//     // public List<OVRInput.Button> ClickButtons;

//     private bool GetAllButtons_Up(){
//         bool isTrue = false;
//         // foreach(OVRInput.Button button in ClickButtons){
//             if(OVRInput.GetUp(ClickButton, OVRInput.Controller.RTouch)){
//                 isTrue = true;
//             }
//         // }
//         return isTrue;
//     }

//     private bool GetAllButtons_CheckPerFrame(){
//         bool isTrue = false;
//         // foreach(OVRInput.Button button in ClickButtons){
//             if(OVRInput.Get(ClickButton, OVRInput.Controller.RTouch)){
//                 isTrue = true;
//             }
//         // }
//         return isTrue;
//     }
//     private bool GetAllButtons_Down(){
//         bool isTrue = false;
//         // foreach(OVRInput.Button button in ClickButtons){
//             if(OVRInput.GetDown(ClickButton, OVRInput.Controller.RTouch)){
//                 isTrue = true;
//             }
//         // }
//         return isTrue;
//     }

//     void Awake()
//     {
//         OVRGrabber.TriggerEnterEvent += OnTriggerEnterEvent;
//         OVRGrabber.TriggerExitEvent += OnTriggerExitEvent;
//     }
//     void OnDestroy()
//     {
//         OVRGrabber.TriggerEnterEvent -= OnTriggerEnterEvent;
//         OVRGrabber.TriggerExitEvent -= OnTriggerExitEvent;
//     }

//     private void OnTriggerEnterEvent(GameObject grabbable){
//         SelectPointedModel(grabbable.GetComponent<MeshInteractiveController>());
//     }
//     private void OnTriggerExitEvent(GameObject grabbable){
//         if(m_LastPointedModel == grabbable.GetComponent<MeshInteractiveController>()){
//             SelectPointedModel(null);
//         }
//     }

//     private void UpdateCameraRigHeight(int direction = 0){
//         if(!IsForVR){
//             return;
//         }

//         // Debug.Log("UpdateCameraRigHeight dir: "+direction);
//         Vector3 newPosition = CameraRigParent.transform.position;
//         // Debug.Log("--- oldPos = "+newPosition);
//         if(direction != 0){
//             newPosition.y += direction * CameraRigMoveSpeed * Time.deltaTime;
//         }else{
//             newPosition.y = (CameraRigHeightMinMax.x + CameraRigHeightMinMax.y) *0.5f;
//         }
//         newPosition.y = Mathf.Clamp(newPosition.y, CameraRigHeightMinMax.x, CameraRigHeightMinMax.y);

//         CameraRigParent.transform.position = newPosition;
//         // Debug.Log("--- newPos = "+newPosition);
//     }

//     public void Init(){
//         // SceneCamera.cullingMask |= 1 << LayerMask.NameToLayer("Mask");
//         // SceneCamera.clearFlags = CameraClearFlags.Skybox;
//         m_LastHandMeshDistance = VRDistanceGrabObject;

//         FloatingNameModel.Show(false);

//         UpdateCameraRigHeight();
//     }

//     public void Enable(bool enable){
//         gameObject.SetActive(IsForVR || enable);

//         if(!enable){
//             Ungrab();
//         }
//     }

//     public void SetPosition(Vector3 newPos){
//         CharacterController.enabled = false;
//         CharacterController.transform.position = new Vector3(newPos.x, newPos.y, newPos.z);
//         CharacterController.enabled = true;
//     }

//     private void LateUpdate() {
//         // Zoom
//         if(!IsForVR){
//             float fov = NoVRCamera.fieldOfView;
//             fov += Input.GetAxis("Mouse ScrollWheel") * -sensitivity;
//             fov = Mathf.Clamp(fov, minFov, maxFov);
//             NoVRCamera.fieldOfView = fov;
//             // SceneCamera.fieldOfView = fov;
//         }

//         if(!m_isGrabbing){
//                 if(Morgue.Menu.IsOnVR()){
//                     MeshInteractiveController pointedModel=m_LastPointedModel;
//                     m_LastHandMeshDistance = VRDistanceGrabObject;
//                     float maxDistance = VRDistanceGrabObject;

//                     List<DataList.DataModel> activeModels = Morgue.CurrentZAnatomyController.Data.GetActiveModels()?.FindAll((x)=>x.Model.Collider.enabled);

//                     if(activeModels == null){
//                         return;
//                     }

//                     foreach (DataList.DataModel model in activeModels)
//                     {
//                         MeshInteractiveController item = model.Model;
//                         if(!item.CanBeGrabbed)
//                             continue;
                            
//                         Vector3 referencePoint = item.GetMeshKDTree().ClosestPointOnSurface(VRHand.position);//item.GetCenterPosition();// item.Collider.ClosestPoint(VRHand.position);
//                         Vector3 surfaceNormal = Vector3.zero;
//                         //if(CheckSphereExtra(item.Collider, VRHand, out referencePoint, out surfaceNormal)){
//                             float distanceHandMesh = Vector3.Distance(referencePoint, VRHand.transform.position);

//                             if(distanceHandMesh <= VRDistanceGrabObject){
//                                 if(distanceHandMesh <= m_LastHandMeshDistance){
//                                     pointedModel = item;
                                    
//                                     if(distanceHandMesh <= m_LastHandMeshDistance){
//                                         m_LastHandMeshDistance = distanceHandMesh;
//                                     }

//                                     //Debug.DrawLine(referencePoint, VRHand.transform.position, Color.green);
//                                 }else{
//                                     //Debug.DrawLine(referencePoint, VRHand.transform.position, Color.yellow);
//                                 }
//                             }else{
//                                 if(item == m_LastPointedModel){
//                                     pointedModel = null;
//                                 }
//                                 //Debug.DrawLine(referencePoint, VRHand.transform.position, Color.red);
//                             }
//                         // }else{
//                         //     if(item == m_LastPointedModel){
//                         //         pointedModel = null;
//                         //     }
//                         //     Debug.DrawLine(referencePoint, VRHand.transform.position, Color.red);
//                         // }
//                     }

//                     if(pointedModel != null){
//                         if(pointedModel != m_LastPointedModel){
//                             SelectPointedModel(pointedModel);
//                         }
//                     }else{
//                         SelectPointedModel(null);
//                         m_LastHandMeshDistance = VRDistanceGrabObject;
//                     }

//                 }else{
//                     RaycastHit hit;
//                     LayerMask layerMask = LayerMask.GetMask("Mask");

//                     Vector3 startPos = NoVRCamera.transform.position;
//                     Vector3 forward = NoVRCamera.transform.forward;
//                     Debug.DrawRay(startPos,forward,Color.green);
//                     if(Physics.Raycast(startPos, forward, out hit, MinMaxGrabbingDistance.y,layerMask)){
//                         MeshInteractiveController pointedModel = hit.collider.gameObject.GetComponent<MeshInteractiveController>();
                        
//                         if(pointedModel != m_LastPointedModel){
//                             if(pointedModel != null && pointedModel.CanBeGrabbed){
//                                 SelectPointedModel(pointedModel);
//                             }else{
//                                 SelectPointedModel(null);
//                             }
//                         }

//                     }else{
//                         SelectPointedModel(null);
//                     }
//                 }
//         }

//         if(Input.GetMouseButtonDown(0) || GetAllButtons_Down()){//OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch)){
//             TryGrabLastPointed();
//         }
        
//         if(Input.GetMouseButtonUp(0) || GetAllButtons_Up()){//OVRInput.GetUp(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch)){
//             Ungrab();
//         }


//         m_isGrabbing = Input.GetMouseButton(0) || GetAllButtons_CheckPerFrame();// OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch);

//         if(m_isGrabbing && GrabbedModel != null){
//             if(Input.GetKey(KeyCode.E)){
//                 // Forward
//                 Vector3 direction = -NoVRCamera.transform.forward * Time.deltaTime * SpeedVerticalMove;
//                 Vector3 nextPosition = GrabbedModel.transform.position + direction;
//                 if(Vector3.Distance(NoVRCamera.transform.position, nextPosition) > MinMaxGrabbingDistance.x){
//                     GrabbedModel.transform.Translate(direction, Space.World);
//                 }
//             }
//             if(Input.GetKey(KeyCode.Q)){
//                 // Backward
//                 Vector3 direction = NoVRCamera.transform.forward * Time.deltaTime * SpeedVerticalMove;
//                 Vector3 nextPosition = GrabbedModel.transform.position + direction;
//                 if(Vector3.Distance(NoVRCamera.transform.position, nextPosition) < MinMaxGrabbingDistance.y){
//                     GrabbedModel.transform.Translate(direction, Space.World);
//                 }
//             }

//             // Check Raycast max distance to grab
//             RaycastHit hit;
//             LayerMask layerMask = ~LayerMask.GetMask(new string[]{"Mask","Ignore Raycast"});

//             if(Physics.Raycast(NoVRCamera.transform.position + NoVRCamera.transform.forward * MinMaxGrabbingDistance.x, NoVRCamera.transform.forward, out hit, MinMaxGrabbingDistance.y,layerMask)){
//                 Vector3 hitPoint = hit.point + NoVRCamera.transform.forward*ExtraRaycastForwardDistance;
//                 if(Vector3.Distance(GrabbedModel.transform.position, NoVRCamera.transform.position) > Vector3.Distance(hitPoint, NoVRCamera.transform.position)){
//                     GrabbedModel.transform.position = hitPoint;
//                 }
//             }

//             // if(Input.GetMouseButton(1)){
//             //     GrabbedModel.transform.Rotate(Vector3.up*Input.GetAxis("Horizontal"), Space.World);
//             //     GrabbedModel.transform.Rotate(Vector3.right*Input.GetAxis("Vertical"), Space.World);
//             // }

//             if(CheckGrabbedModelDistance()){
//                 Color selectedColor = Color.green;
//                 //selectedColor.a = 0.1f;
//                 //GrabbedModel.GetRenderer().material.color = selectedColor;
//                 SetMaterialsColor(GrabbedModel.GetRenderer(), selectedColor);
//             }else{
//                 //if(!GrabbedModel.MorgueIsInPlace){
//                     Color selectedColor = CheckGrabbedModelInsideHole()?Color.magenta:Color.yellow;
//                     //selectedColor.a = 0.1f;
//                     //GrabbedModel.GetRenderer().material.color = selectedColor;
//                     SetMaterialsColor(GrabbedModel.GetRenderer(), selectedColor);
//                 //}
//             }

//             if(!Morgue.Menu.IsOnVR()){
//                 GrabbedModel.transform.rotation = Quaternion.LookRotation(m_GrabbedModelForward, m_UpVector);//GrabbedModel.transform.up);
//             }
//             //GrabbedModel.transform.up = m_GrabbedModelUp;
//             //FloatingNameModel.SetPosition(GrabbedModel.GetCenterPosition());
//             //Debug.DrawLine(GrabbedModel.transform.position, GrabbedModel.transform.position + GrabbedModel.transform.up, Color.green);
//         }

//         if(Input.GetKeyDown(KeyCode.R)){
//             m_ShowNameParent = !m_ShowNameParent;
//         }   

//         UpdateNameLabel();
        
//         CheckVerticalCameraPosition();
//     }

//     private void CheckVerticalCameraPosition(){
//         if (OVRInput.Get(OVRInput.Button.Two, OVRInput.Controller.LTouch)) // UP
//         {
//             UpdateCameraRigHeight(1);
//         }
//         if (OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.LTouch)) // DOWN
//         {
//             UpdateCameraRigHeight(-1);
//         }
//     }

//     public void SelectPointedModel(MeshInteractiveController pointedModel){
//         if(pointedModel == null){
//             //Debug.Log("DESELECTED ON EXIT");
//             DeselectLastPointed();
//             // CheckFloorPreview(true);
//             return;
//         }

//         //Debug.Log(pointedModel.gameObject.name);
//         DeselectLastPointed();
//         // ModelsOnHumanController human = pointedModel.GetComponentInParent<ModelsOnHumanController>();
//         //Debug.Log(pointedModel.MorgueIsInPlace + " " +human.IsBodyOpened());

//         if(!pointedModel.MorgueIsInPlace || (pointedModel.MorgueIsInPlace)){
//             //Debug.Log("--- Painting YELLOW");
//             Color selectedColor = Color.yellow;
//             SetNameText(pointedModel.GetID());
//             //selectedColor.a = 0.1f;
//             //pointedModel.GetRenderer().material.color = selectedColor;
//             SetMaterialsColor(pointedModel.GetRenderer(), selectedColor);
            
//             m_LastPointedModel = pointedModel.Morgue_GetParentObjectOrSelf();
//         }
//     }

//     void Update()
//     {
//         if(GrabbedModel != null || m_LastPointedModel != null){
//             FloatingNameModel.SetPosition(GrabbedModel != null?GrabbedModel.GetCenterPosition():m_LastPointedModel.GetCenterPosition());
//         }
//     }

//     private void SetMaterialsColor(Renderer renderer, Color selectedColor){
//         MeshRenderer meshRenderer = (MeshRenderer)renderer;
//         for(int i = 0;i<meshRenderer.materials.Length;i++){
//             meshRenderer.materials[i].color = selectedColor;
//         }
//     }

//     public bool IsPointingGrabbableModel(){
//         if(m_LastPointedModel ==null)
//             return true;

//         if(!m_LastPointedModel.MorgueIsInPlace){
//             return true;
//         } 

//         bool canBeGrabbed = true;
//         //Debug.Log(" CLICK --------- ");
//         foreach(MeshInteractiveController model in m_LastPointedModel.GetModelsOver()){
//             if(model != m_LastPointedModel && model.MorgueIsInPlace && model.CanBeGrabbed){
//                 canBeGrabbed = false;
//                 //Debug.Log(" OBJECT OVER IT: "+model.name);
//             }
//         }

//         return canBeGrabbed;
//     }

//     private void TryGrabLastPointed(){
//         if(m_LastPointedModel != null){
//             bool canBeGrabbed = IsPointingGrabbableModel();

//             if(canBeGrabbed){
//                 GrabbedModel = m_LastPointedModel;
//                 m_GrabbedModelLastPosition = GrabbedModel.transform.position;
//                 GrabbedModel.transform.SetParent(Morgue.Menu.IsOnVR()?VRHand.transform:MarkerGrabObjects, true);
//                 m_UpVector = GrabbedModel.transform.up;
//                 m_GrabbedModelForward = GrabbedModel.transform.forward;
//                 //Debug.DrawLine(GrabbedModel.transform.position, GrabbedModel.transform.position + m_GrabbedModelUp, Color.red, 10);

//                 SetNameText(GrabbedModel.GetID());

//                 Color selectedColor = Color.green;
//                 SetMaterialsColor(GrabbedModel.GetRenderer(), selectedColor);
//                 GrabbedModel.SetCollider(false);
//                 GrabbedModel.MorgueAddMarker(Marker3DPrefab);
//                 GrabbedModel.MorgueShowMarker(false);
//             }else{
//                 Color selectedColor = Color.red;

//                 SetMaterialsColor(m_LastPointedModel.GetRenderer(), selectedColor);

//                 float amount = -0.35f;
//                 selectedColor = new Color(selectedColor.r + amount, selectedColor.g + amount, selectedColor.b + amount);
//                 foreach(MeshInteractiveController model in m_LastPointedModel.GetModelsOver()){
//                     if(model.MorgueIsInPlace && model.CanBeGrabbed){
                        
//                         //model.GetRenderer().material.color = selectedColor;
//                         SetMaterialsColor(model.GetRenderer(), selectedColor);
//                         LeanTween.cancel(model.gameObject);
//                         LeanTween.value(model.gameObject, 0, 1, 1f).setOnComplete(()=>{
//                             model.SetNotSelected(true);
//                         });
//                     }
//                 }
//             }
//         }
//     }

//     private void SetNameText(int id){
//         //Debug.Log("SetNameText ----- "+id);
//         DataList.DataModel grabbedData = Morgue.CurrentZAnatomyController.Data.DataModelsList.Find((x)=>x.ID==id);// GetActiveModels().Find((x)=>x.ID==id);
//         string region = "REGION";
//         string name = "Name";
//         if(grabbedData != null){
//             region = grabbedData.BodyRegion.ToString();
//             name = Utils.UpperFirstChar(grabbedData.Name);
//             string text = "<size=50%>"+region+"</size>\n"+name;
//             Color color = Morgue.Menu.Importer.ZAnatomyMale.BodyData.Find((x)=>x.Region == grabbedData.BodyRegion).Color;//Color.white;//Utils.ColorFromHex(grabbedData.Color);
//             FloatingNameModel.SetName(text, color);
//         }else{
//             Debug.LogError("SetNameText: Cannot find DataModel for id: "+id);
//         }
//     }

//     private bool CheckGrabbedModelDistance(){
//         return /*!GrabbedModel.MorgueIsInPlace && */Vector3.Distance(GrabbedModel.GetMorgueOriginalPosition(), GrabbedModel.transform.position) < Morgue.DistanceToInsertPiece;
//     }

//     private bool CheckGrabbedModelInsideHole(){
//         ModelsOnHumanController.ModelZone hole = Morgue.GetCurrentHuman().GetModelZone();
//         if(hole == null){
//             //Debug.Log("Hole is null");
//             return Morgue.GetCurrentHuman().IsModelInsideActiveCollider(GrabbedModel.GetCenterPosition());
//         }
//         return Vector3.Distance(hole.HoleInBody.transform.position, GrabbedModel.transform.position) < hole.GetHoleRadius();
//     }

//     public void Ungrab(){ 
//         if(GrabbedModel != null){

//             bool canBePlaced = true;
//             //Debug.Log(" CLICK --------- ");
//             List<MeshInteractiveController> modelsBelow = new List<MeshInteractiveController>();// GrabbedModel.GetModelsBelow();
//             // foreach(MeshInteractiveController model in modelsBelow){
//             //     if(model != GrabbedModel && !model.MorgueIsInPlace){
//             //         canBePlaced = false;
//             //         //Debug.Log(" OBJECT OVER IT: "+model.name);
//             //     }
//             // }

//             bool isInsideHole = CheckGrabbedModelInsideHole();
//             bool stayOnOriginalPos = GrabbedModel.MorgueIsInPlace && isInsideHole;

//             if(stayOnOriginalPos){
//                 GrabbedModel.MorgueSetOnPosition(true);
//             }else if(CheckGrabbedModelDistance()){
//                 if(canBePlaced){
//                     GrabbedModel.MorgueSetOnPosition(true);
//                 }else{
//                     GrabbedModel.MorgueSetOnPosition(false, true);

//                     Color selectedColor = Color.red;

//                     SetMaterialsColor(GrabbedModel.GetRenderer(), selectedColor);
                    
//                     foreach(MeshInteractiveController model in modelsBelow){
//                         if(!model.MorgueIsInPlace){
//                             SetMaterialsColor(model.GetRenderer(), selectedColor);
                            
//                             LeanTween.value(model.gameObject, 0, 1, 1f).setOnComplete(()=>{
//                                 model.SetNotSelected(true);
//                             });
//                         }
//                     }
//                 }
//             }
//             else{
//                 GrabbedModel.MorgueSetOnPosition(false, !GrabbedModel.MorgueIsInPlace && isInsideHole);
//             }
            
//             GrabbedModel.SetCollider(true);
            
//             GrabbedModel = null;
//             DeselectLastPointed();
//         }
//     }

//     private void UpdateNameLabel(){
//         FloatingNameModel.Show(m_ShowNameParent && ((m_isGrabbing && GrabbedModel != null) || (!m_isGrabbing && m_LastPointedModel != null && !m_LastPointedModel.MorgueIsInPlace)));
//     }

//     private void DeselectLastPointed(){
//         if(m_LastPointedModel != null){
//             m_LastPointedModel.SetNotSelected(true);
//         }
//         m_LastPointedModel = null;
//     }

//     public void OnVRHandTriggerEnter(MeshInteractiveController pointedModel){
//         if(pointedModel != m_LastPointedModel){
//             SelectPointedModel(pointedModel);
//         }
//     }

//     public void OnVRHandTriggerExit(MeshInteractiveController pointedModel){
//         if(pointedModel == m_LastPointedModel){
//             SelectPointedModel(null);
//         }
//     }
// }
