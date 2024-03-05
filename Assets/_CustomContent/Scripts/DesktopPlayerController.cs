using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;

// #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
// using System.Runtime.InteropServices;
// #endif

public class DesktopPlayerController:MonoBehaviour {
    
    // #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    //     [DllImport("user32.dll")]
    //     static extern bool SetCursorPos(int X, int Y);
    // #endif
    
    public GameObject UI_SelectedObjectMenuParent;
    public List<TMPro.TextMeshProUGUI> UI_SelectedObjectMenuTitles;

    public Transform StaticCameraMarkersParent;
    public List<Transform> m_StaticCameraMarkersList;

    public float StaticIconsSelectedGrowSize = 2;
    public GameObject StaticCameraIconPrefab;
    public Transform StaticCameraIconsParent;

    public GameObject CanvasUI;
    public Vector3 InitSceneStartEulerAngles = new Vector3(35, -90, 0);
    public Camera PlayerCamera;
    public Transform RealObjectsParent;
    public GameObject SelectedObjectToolPanel;


    public DesktopPlayerCamPreview CamPreviewPrefab;
    public Transform DesktopPlayerCamPanelParent;

    public Vector2 CameraRotationSpeed;
    public GameObject SelectedObjectToolPanel_SelectedObject;
    public GameObject SelectedObjectToolPanel_NoSelectedObject;
    public Vector2 ZoomFOVValueMinMax;
    public float ZoomFOVChangeStep=0.1f;

    public GameObject MaximizedUserPanel;
    public DesktopPlayerCamPreview MaximizedCamPreview;
    public GameObject ButtonsParent;
    public float SpeedPushPull = 10f;
    public float SpeedRotation = 30f;
    public float SpeedGrab = 10;
    public GameObject Selector3D;
    private float m_CurrentZoomValue = 0.5f;
    private int m_CameraId;

    private List<GameObject> m_StaticCameraIcons;
    private ZAnatomy.MeshInteractiveController m_LastPointedObject;
    private float yaw = 0.0f;
    private float pitch = 0.0f;
    private ZAnatomy.MeshInteractiveController m_SelectedObject;
    private ZAnatomy.MeshInteractiveController pointedObject;
    private bool m_IsLoading;
    private List<Camera> PlayersCameraPreview;
    private float m_GrabbingDistance;

    public VRGrabbingDesktop grabbingDesktop;
    public float DragCameraSpeed = 2;
    public float OrbitCameraSpeed = 10;
    public MeshRenderer CameraBoundary;

    public Transform ParentPivot;
    public Transform CameraParent;


    public enum Panel{
        UserCameras,
        DesktopTools,
        MoveTable,
        Interactions,
        TabletHost,
        Help
    }
    public List<GameObject> PanelEnablers;
    public GameObject InteractionButtonBar;
    private Vector3 m_PlayerCameraOrbitOrigin;
    private Vector3 m_DragOrigin;
    private Vector3 m_PlayerCameraDragOrigin;
    private Vector3 m_SelectedObjectGrabPoint;
    private bool m_IsDissectionScene;
    public NamePanelGroup UserNameGroup;

    private void EnablePanel(Panel selectedPanel, bool enable){
        GameObject panelEnabler = PanelEnablers[(int)selectedPanel];

        if(enable){
            switch(selectedPanel){
                case Panel.TabletHost:
                    EnablePanel(Panel.DesktopTools, false);
                    EnablePanel(Panel.Interactions, false);
                    EnablePanel(Panel.MoveTable, false);
                    break;
                case Panel.MoveTable:
                    EnablePanel(Panel.TabletHost, false);
                    EnablePanel(Panel.DesktopTools, false);
                    EnablePanel(Panel.Interactions, false);
                    break;
                case Panel.Interactions:
                    EnablePanel(Panel.TabletHost, false);
                    EnablePanel(Panel.DesktopTools, false);
                    EnablePanel(Panel.MoveTable, false);
                    break;
                case Panel.DesktopTools:
                    EnablePanel(Panel.TabletHost, false);
                    EnablePanel(Panel.Interactions, false);
                    EnablePanel(Panel.MoveTable, false);
                    break;
            }
        }

        panelEnabler.SetActive(enable);
    }
    private bool IsEnabledPanel(Panel panel){
        GameObject panelGameObject = PanelEnablers[(int)panel];
        return panelGameObject.activeInHierarchy;
    }

    public void TogglePanel(int PanelID){
        Panel selectedPanel = (Panel)PanelID;
        EnablePanel(selectedPanel, !IsEnabledPanel(selectedPanel));
    }

    void Start()
    {
        UserInitializerController.OnPlayersChanged += UpdateCameraPreviews;
        AnalyticsManager.OnPlayersChanged += UpdateCameraPreviews;
        // CustomNetworkRoomManager.OnClientDisconnection += UpdateCameraPreviews;
        // Mirror.NetworkServer.OnDisconnectedEvent += OnDisconnectionEvent;
        
        Debug.Log("Start DesktopPlayer");
        Init();
    }

    void OnDestroy()
    {
        UserInitializerController.OnPlayersChanged -= UpdateCameraPreviews;
        AnalyticsManager.OnPlayersChanged -= UpdateCameraPreviews;
        // Mirror.NetworkServer.OnDisconnectedEvent -= OnDisconnectionEvent;
        // CustomNetworkRoomManager.OnClientDisconnection -= UpdateCameraPreviews;
    }

    // private void OnDisconnectionEvent(Mirror.NetworkConnectionToClient client){
    //     UpdateCameraPreviews();
    // }

    private bool CanInteractWithPieces(){
        return IsEnabledPanel(Panel.Interactions);
    }

    public void Init(){
        m_CameraId = 0;

        Selector3D.SetActive(false);
        
        SelectObject(null);

        // CheckZoomCamera(true);

        UpdateCameraPreviews();

        InitStaticCameras();
    }

    private void ResetEnablers(){
        foreach(GameObject panel in PanelEnablers){
            panel.SetActive(false);
        }
    }

    public void SetDissectionScene(){
        m_IsDissectionScene = true;
        Debug.Log("Set Dissection Scene on Desktop");
        
        if(gameObject.activeInHierarchy){
            ResetEnablers();
            ResetMenuTitles();
            StartCoroutine(WaitToInitFirstCamera());
        }
            // OnClickChangeCamera(0);
    }

    private System.Collections.IEnumerator WaitToInitFirstCamera(){
        yield return new WaitForSeconds(0.1f);
        OnClickSelectCamera((int)FloatingCameraPositions.LeftInferior);
    }

    public void UpdateCameraPreviews(){
        Utils.ClearChilds(DesktopPlayerCamPanelParent, new List<string>{ "BG" });
        PlayersCameraPreview= new List<Camera>();

        if(UserInitializerController.Instance != null){
            UserInitializerController.Instance.IterateUsers((UserInitializerController.UserData userData)=>{
                XRInitialization parentXR = gameObject.GetComponentInParent<XRInitialization>();
                // bool isCurrentPlayer = userData.XRObject.gameObject != parentXR.gameObject;

                if(userData.IsConnected && !userData.IsLocalPlayer){
                    DesktopPlayerCamPreview camPreview = Instantiate(CamPreviewPrefab,DesktopPlayerCamPanelParent);

                    string finalName = Utils.GetNameFromAnalytics(userData);

                    camPreview.Init(finalName, userData.Color, userData.RenderTexture, userData, this);
                    // userData.XRObject.XRCamera.enabled = true;
                    PlayersCameraPreview.Add(userData.PlayerCamera);
                }
            });
        }

        MaximizedUserPanel.gameObject.SetActive(false);
    }

    private void CheckCameraPreviews(){
        // To allow enable preview cams only when cams panel is actived
        foreach(Camera cam in PlayersCameraPreview){
            // if(cam == null){
            //     UpdateCameraPreviews();
            //     return;
            // }

            if(DesktopPlayerCamPanelParent.gameObject.activeInHierarchy && !cam.enabled){
                cam.enabled = true;
            }
        }
    }

    // void OnApplicationQuit()
    // {
    //     Utils.SaveSessionFile();
    // }

    private void InitStaticCameras(){
        GameObject adminPos = GameObject.Find("Admin_Pos");
        if(adminPos == null) { // Init scene 
            m_IsDissectionScene = false;
            CanvasUI.SetActive(false);
            pitch = InitSceneStartEulerAngles.x;
            yaw = InitSceneStartEulerAngles.y;
            PlayerCamera.transform.localEulerAngles =InitSceneStartEulerAngles;
            PlayerCamera.transform.localPosition = Vector3.zero;
            GameObject.Find("Canvas_InitGame").GetComponent<Canvas>().worldCamera = PlayerCamera;
            return;
        }

        // Load cameras
        m_StaticCameraMarkersList= new List<Transform>();
        m_StaticCameraMarkersList.Add(adminPos.transform);
        foreach(Transform child in StaticCameraMarkersParent){
            m_StaticCameraMarkersList.Add(child);
        }

        // Init icons
        Utils.ClearChilds(StaticCameraIconsParent);

        foreach(Transform camera in m_StaticCameraMarkersList){
            GameObject newIcon = Instantiate(StaticCameraIconPrefab, StaticCameraIconsParent);
            newIcon.transform.localScale = Vector3.one;
        }
    }

    private void UpdateCameraIcons(){
        for (int i = 0; i < m_StaticCameraMarkersList.Count; i++)
        {
            Transform camera = m_StaticCameraMarkersList[i];
            Transform newIcon = StaticCameraIconsParent.GetChild(i);
            newIcon.GetComponent<Image>().color = m_CameraId == i?Color.green:Color.white;
            // newIcon.localScale = Vector3.one*(m_CameraId == i?StaticIconsSelectedGrowSize:StaticIconsSelectedGrowSize*0.25f);
        }
    }

    public void SetLoading(bool loading){
        if (CanvasUI!=null)
        {
            CanvasUI.SetActive(!loading);
            m_IsLoading = loading;
        }
    }

    private void CheckCameraRotation(int buttonId){
        if(Input.GetMouseButton(buttonId) && (m_IsLoading || !LeanTween.isTweening(PlayerCamera.gameObject))) {
            if(!EventSystem.current.IsPointerOverGameObject()){
                Transform camera = PlayerCamera.transform;
                // camera.Rotate(new Vector3(0, -Input.GetAxis("Mouse X") * CameraRotationSpeed, -Input.GetAxis("Mouse Y") * CameraRotationSpeed));
                // float X = camera.rotation.eulerAngles.y;
                // float Y = camera.rotation.eulerAngles.z;
                // // float Z = RealObjectsParent.rotation.eulerAngles.x;
                // camera.rotation = Quaternion.Euler(0, X, Y);


                // camera.Rotate(new Vector3(-Input.GetAxis("Mouse Y") * CameraRotationSpeed, Input.GetAxis("Mouse X")*CameraRotationSpeed, 0),0, Space.World);
            
                    
                yaw -= CameraRotationSpeed.x * Input.GetAxis("Mouse X");
                pitch += CameraRotationSpeed.y * Input.GetAxis("Mouse Y");

                camera.eulerAngles = new Vector3(pitch, yaw, 0.0f);
                    
            }
        }
    }

    void Update() {
        if(!this.gameObject.activeInHierarchy){
            return;
        }

        if(!m_IsDissectionScene){ // Init scene
            CheckCameraRotation(0);
            return;
        }

        // CheckZoomCamera();
        CheckForwardMovementCamera();

        CheckDragCameraOnButton(1);
        CheckCameraOrbitOnButton(2);

        CheckCameraPreviews();

        if(!CanInteractWithPieces()){
            CheckCameraRotation(0);
            if(m_SelectedObject != null){
                SelectObject(null);
                UpdatePointedObject(true);
            }
            return;
        }

        bool isSelectedObject = IsObjectSelected();
        if(!isSelectedObject){

            CheckCameraRotation(0);

            UpdatePointedObject();

        }else{

            // if(Input.GetMouseButtonDown(0)){
            //     OnStartGrabbing();
            // }

            Vector3 directionForward = m_SelectedObject!=null?m_SelectedObject.GetCenterPosition() - PlayerCamera.transform.position:Vector3.zero;
            Vector3 newPosition = m_SelectedObject!=null?m_SelectedObject.transform.position:Vector3.zero;
            switch(m_CurrentModelAction){
                case ModelAction.Grab:
                    // SelectedObjectToolPanel.transform.position = Input.mousePosition;
                    // Vector3 mousePos = Input.mousePosition;
                    // mousePos.z = /*directionForward.magnitude;*/m_GrabbingDistance;
                    // newPosition = PlayerCamera.ScreenToWorldPoint(mousePos, Camera.MonoOrStereoscopicEye.Mono)+Utils.GetDifferenceFromMeshCenter(m_SelectedObject); // Move to mouse position

                    Vector3 verticalMovement = PlayerCamera.transform.up * Input.GetAxis("Mouse Y") * Time.deltaTime * SpeedGrab;
                    Vector3 horizontalMovement = PlayerCamera.transform.right * Input.GetAxis("Mouse X") * Time.deltaTime * SpeedGrab;
                    newPosition = m_SelectedObject.transform.position + verticalMovement + horizontalMovement;
                    
                    // newPosition = PlayerCamera.ScreenToWorldPoint(mousePos, Camera.MonoOrStereoscopicEye.Mono) +Utils.GetDifferenceFromMeshCenter(m_SelectedObject) + (m_SelectedObjectGrabPoint-m_SelectedObject.GetCenterPosition());

                    // if(CheckPositionInsideBoundaries(newPosition)){
                        m_SelectedObject.transform.position = LimitPositionInsideBoundaries(newPosition);
                    // }

                    //  Move cursor to screenposition and drag speed relative to move

                    // Cursor position
                    // #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN  
                    //     Vector2 cursorPos = PlayerCamera.WorldToScreenPoint(m_SelectedObject.GetCenterPosition());
                    //     SetCursorPos((int)cursorPos.x,(int)cursorPos.y);//Call this when you want to set the mouse position
                    // #endif

                    Selector3D.transform.position = m_SelectedObject.GetCenterPosition();// m_SelectedObject!=null?m_SelectedObject.GetCenterPosition():Vector3.zero;   
                    
                    if(Input.GetMouseButtonUp(0)){
                        OnStopGrabbing();
                    }
                    break;
                case ModelAction.Rotate:
                    // m_SelectedObject.transform.Rotate(new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0) * Time.deltaTime * SpeedRotation);
                    m_SelectedObject.transform.RotateAround(m_SelectedObject.GetCenterPosition(), PlayerCamera.transform.right /*new Vector3(1, 0, 0)*/, -Input.GetAxis("Mouse Y") * Time.deltaTime * SpeedRotation);
                    m_SelectedObject.transform.RotateAround(m_SelectedObject.GetCenterPosition(), PlayerCamera.transform.up /*new Vector3(0, 1, 0)*/, Input.GetAxis("Mouse X") * Time.deltaTime * SpeedRotation);

                    Selector3D.transform.position = m_SelectedObject.GetCenterPosition();

                    if(Input.GetMouseButtonUp(0)){
                        OnStopRotate();
                    }

                    FeedBackUtilities.SetMaterialsColor(m_SelectedObject.gameObject, null, FeedBackUtilities.ActionState.None);

                    OnlineUtilities.GetOnlineUtilities().RotateMeshInteractive_Event(m_SelectedObject.GetComponent<MeshInteractableOnline>().ID, m_SelectedObject.transform.position, m_SelectedObject.transform.eulerAngles);
                    break;
                case ModelAction.Push:
                    grabbingDesktop.MoveObject( 1*SpeedPushPull*Time.deltaTime * directionForward);

                    if(LimitPositionInsideBoundaries(m_SelectedObject.transform.position) != m_SelectedObject.transform.position){ // Has choked against something
                        m_SelectedObject.transform.position = newPosition; // Back to last position
                    }

                    Selector3D.transform.position = m_SelectedObject.GetCenterPosition();

                    if(Input.GetMouseButtonUp(0)){
                        OnStopPushPull();
                    }
                    break;
                case ModelAction.Pull:
                    
                    grabbingDesktop.MoveObject(-1*SpeedPushPull*Time.deltaTime * directionForward);

                    if(LimitPositionInsideBoundaries(m_SelectedObject.transform.position) != m_SelectedObject.transform.position){ // Has choked against something
                        m_SelectedObject.transform.position = newPosition; // Back to last position
                    }
                    Selector3D.transform.position = m_SelectedObject.GetCenterPosition();
                    
                    if(Input.GetMouseButtonUp(0)){
                        OnStopPushPull();
                    }
                    break;

                case ModelAction.None:
                    // Click outside deselects
                    if(!EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonDown(0)){
                        SelectObject(null);
                    }
                    // RightClick deselects
                    if(Input.GetMouseButtonDown(1)){
                        SelectObject(null);
                    }
                    break;
            }    
    
        }

        if(m_SelectedObject!=null){
            SelectedObjectToolPanel.transform.position = PlayerCamera.WorldToScreenPoint(m_SelectedObject.GetCenterPosition());
        }
        bool showSelectedPanel = isSelectedObject && (grabbingDesktop.GetBestCandidate() == pointedObject);
        SelectedObjectToolPanel_SelectedObject.gameObject.SetActive(showSelectedPanel);
        SelectedObjectToolPanel_NoSelectedObject.gameObject.SetActive(!showSelectedPanel);

    }

    private Vector3 LimitPositionInsideBoundaries(Vector3 newPosition){
        if(newPosition.z > CameraBoundary.bounds.max.z)newPosition.z = CameraBoundary.bounds.max.z;
        if(newPosition.y > CameraBoundary.bounds.max.y)newPosition.y = CameraBoundary.bounds.max.y;
        if(newPosition.x > CameraBoundary.bounds.max.x)newPosition.x = CameraBoundary.bounds.max.x;
        
        if(newPosition.z < CameraBoundary.bounds.min.z)newPosition.z = CameraBoundary.bounds.min.z;
        if(newPosition.y < CameraBoundary.bounds.min.y)newPosition.y = CameraBoundary.bounds.min.y;
        if(newPosition.x < CameraBoundary.bounds.min.x)newPosition.x = CameraBoundary.bounds.min.x;

        return newPosition;
    }

    private void CheckCameraOrbitOnButton(int buttonId){
        if (Input.GetMouseButtonUp(buttonId))
        {
            // Selector3D.SetActive(false);

            // Add Camera as children with world position
            PlayerCamera.transform.SetParent(CameraParent, true);

            yaw = PlayerCamera.transform.eulerAngles.y;
            pitch = PlayerCamera.transform.eulerAngles.x;
            return;
        }

        if (Input.GetMouseButtonDown(buttonId))
        {
            SelectObject(null);
            // Raycast to obtain pivot point
            Vector3 raycastPoint = ParentPivot.transform.position;

            // RaycastHit hit;
            // // Ray ray = PlayerCamera.ScreenPointToRay(Input.mousePosition);
            // Ray ray = PlayerCamera.ViewportPointToRay(new Vector2(0.5f, 0.5f)); // Center of screen

            // // Does the ray intersect any objects excluding the player layer
            // if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            // {
            //     raycastPoint = hit.point;
            // }

            // Selector3D.SetActive(true);

            // ParentPivot.transform.position = raycastPoint;
            Selector3D.transform.position = raycastPoint;

            // Add Camera as children with world position
            PlayerCamera.transform.SetParent(ParentPivot, true);
            // PlayerCamera.transform.LookAt(raycastPoint);

            m_DragOrigin = Input.mousePosition;
            m_PlayerCameraOrbitOrigin = ParentPivot.transform.eulerAngles;
            
            return;
        }

        if(Input.GetMouseButton(buttonId)){   
            if (!Input.GetMouseButton(buttonId)) return;
    
            Vector3 pos = PlayerCamera.ScreenToViewportPoint(Input.mousePosition - m_DragOrigin);
            Vector3 move = new Vector3(-pos.x * OrbitCameraSpeed, -pos.y * OrbitCameraSpeed, 0);
    
            Vector3 forwardMovement = PlayerCamera.transform.right * move.y;// (PlayerCamera.transform.position - ParentPivot.transform.position).normalized * move.y;
            Vector3 upMovement = -ParentPivot.transform.up * move.x;
            // PlayerCamera.transform.Translate(move, Space.Self);  

            // PlayerCamera.transform.localPosition = m_PlayerCameraDragOrigin + PlayerCamera.transform.InverseTransformDirection(move);// Translate(move, Space.Self);  
            // PlayerCamera.transform.position = m_PlayerCameraDragOrigin + upMovement + forwardMovement;
            ParentPivot.eulerAngles = m_PlayerCameraOrbitOrigin + upMovement/* + forwardMovement*/;
        }
    }

    private void CheckDragCameraOnButton(int buttonId){ 
        if (Input.GetMouseButtonDown(buttonId))
        {
            SelectObject(null);
            m_DragOrigin = Input.mousePosition;
            m_PlayerCameraDragOrigin = PlayerCamera.transform.position;
            return;
        }

        if(Input.GetMouseButton(buttonId)){   
            if (!Input.GetMouseButton(buttonId)) return;
    
            Vector3 pos = PlayerCamera.ScreenToViewportPoint(Input.mousePosition - m_DragOrigin);
            Vector3 move = new Vector3(-pos.x * DragCameraSpeed, -pos.y * DragCameraSpeed, 0);
    
            Vector3 forwardMovement = PlayerCamera.transform.right * move.x;
            Vector3 upMovement = PlayerCamera.transform.up * move.y;
            // PlayerCamera.transform.Translate(move, Space.Self);  

            // PlayerCamera.transform.localPosition = m_PlayerCameraDragOrigin + PlayerCamera.transform.InverseTransformDirection(move);// Translate(move, Space.Self);  
            Vector3 newPosition = m_PlayerCameraDragOrigin + upMovement + forwardMovement;
            PlayerCamera.transform.position = LimitPositionInsideBoundaries(newPosition);
        }
    }

    public enum ModelAction{
        None,
        Grab,
        Rotate,
        Push,
        Pull
    }
    private ModelAction m_CurrentModelAction;
    

    private void CheckZoomCamera(bool force=false){
        if(force || Input.mouseScrollDelta.y != 0){
            m_CurrentZoomValue += Input.mouseScrollDelta.y * ZoomFOVChangeStep * Time.deltaTime;
            m_CurrentZoomValue = Mathf.Clamp01(m_CurrentZoomValue);
            PlayerCamera.fieldOfView = Mathf.Lerp(ZoomFOVValueMinMax.x, ZoomFOVValueMinMax.y, m_CurrentZoomValue);
        }
    }

    private void CheckForwardMovementCamera(bool force=false){
        if(force || Input.mouseScrollDelta.y != 0){
            Vector3 newPosition = PlayerCamera.transform.position + PlayerCamera.transform.forward * Input.mouseScrollDelta.y * DragCameraSpeed * Time.deltaTime;

            PlayerCamera.transform.position = LimitPositionInsideBoundaries(newPosition);
        }
    }

    private void UpdatePointedObject(bool forceUpdate = false){
        if(!forceUpdate){
            if(EventSystem.current.IsPointerOverGameObject())
                return;
        }
        
        pointedObject = null;
        Transform camera = PlayerCamera.transform;

        // // Bit shift the index of the layer (8) to get a bit mask
        int layerMask = 1 << LayerMask.NameToLayer("Mask");

        // // This would cast rays only against colliders in layer 8.
        // // But instead we want to collide against everything except layer 8. The ~ operator does this, it inverts a bitmask.
        // layerMask = ~layerMask;

        RaycastHit hit;
        Ray ray = PlayerCamera.ScreenPointToRay(Input.mousePosition);

        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(ray,/*camera.position, camera.TransformDirection(Vector3.forward),*/ out hit, Mathf.Infinity, layerMask))///*, layerMask*/))
        {
            pointedObject = hit.collider.gameObject.GetComponent<ZAnatomy.MeshInteractiveController>();
            // Debug.DrawRay(camera.position, camera.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
            // Debug.Log("Did Hit");
            
            // SelectedObjectToolPanel.transform.position = Input.mousePosition;

            if(Input.GetMouseButtonDown(0)){
                m_SelectedObjectGrabPoint = hit.point;//PlayerCamera.WorldToScreenPoint(pointedObject.GetCenterPosition());// hit.point;
                if(pointedObject!=null){
                    SelectedObjectToolPanel.transform.position = PlayerCamera.WorldToScreenPoint(pointedObject.GetCenterPosition());
                }
                SelectObject(pointedObject);
            }else{
                SelectedObjectToolPanel.transform.position = Input.mousePosition;
            }
        }
        else
        {
            // Debug.DrawRay(camera.position, camera.TransformDirection(Vector3.forward) * 1000, Color.white);
            // Debug.Log("Did not Hit");
        }

        bool isPointingObject = /*m_CameraId == 0 &&*/ pointedObject != null;

        // UI_SelectedObjectMenuParent.SetActive(isPointingObject); // This is contextual menus (Not Selected and buttons)

        if(isPointingObject){
            string system = MorgueGrabUtilities.morgue.GetOrganType(pointedObject.BodyPart).Name;
            foreach(TMPro.TextMeshProUGUI title in UI_SelectedObjectMenuTitles){
                title.text = system+" | "+pointedObject.GetPublicName();
                title.color = Color.black;
                if(m_SelectedObject != pointedObject)title.text += "\n<size=50%><color=red>Click to select";
                // title.color = MorgueGrabUtilities.morgue.GetOrganType(pointedObject.BodyPart).Color;
            }
            // FeedBackUtilities.SetMaterialsColor(pointedObject.GetRenderer(), FeedBackUtilities.ActionState.Selected);
        }else{
            ResetMenuTitles();
        }

        if(pointedObject != m_LastPointedObject && m_LastPointedObject != null){
            FeedBackUtilities.SetMaterialsColor(m_LastPointedObject.gameObject, null, FeedBackUtilities.ActionState.None);
        }

        m_LastPointedObject = pointedObject;
    }

    private void ResetMenuTitles(){
        foreach(TMPro.TextMeshProUGUI title in UI_SelectedObjectMenuTitles){
            title.text = "Select an organ";
            title.color = Color.red;
        }
    }


    private void SelectObject(ZAnatomy.MeshInteractiveController pointedObject){
        m_SelectedObject = pointedObject;

        InteractionButtonBar.SetActive(m_SelectedObject != null);
        Selector3D.SetActive(m_SelectedObject != null);

        if(m_SelectedObject != null){
            Selector3D.transform.position = m_SelectedObject.GetCenterPosition();
        }
    }
    public bool IsObjectSelected(){
        return m_SelectedObject != null;
    }

    public ZAnatomy.MeshInteractiveController GetPointed()
    {
        return pointedObject;
    }

    public void OnClickHighlightPointedObject(){

    }

    public void OnClickButtonTablet(bool enable){
        
        
        if(enable){

        }

    }

    public void OnStartGrabbing(){
        m_CurrentModelAction = ModelAction.Grab;
        m_GrabbingDistance = Vector3.Distance(m_SelectedObjectGrabPoint /* m_SelectedObject.GetCenterPosition()*/ /*-Utils.GetDifferenceFromMeshCenter(m_SelectedObject)*/, PlayerCamera.transform.position);
        ButtonsParent.SetActive(false);
        // Selector3D.SetActive(true);

        Debug.Log("Start Grabbing - Add here Dummy hands movement");

        grabbingDesktop.ChangeGrabState();
    }

    private void OnStopGrabbing(){
        m_CurrentModelAction = ModelAction.None;
        grabbingDesktop.ChangeGrabState();
        ButtonsParent.SetActive(true);
        // Selector3D.SetActive(false);
    }


    public void OnStartRotate(){
        m_CurrentModelAction = ModelAction.Rotate;
        ButtonsParent.SetActive(false);
        // grabbingDesktop.ChangeGrabState();
        // grabbingDesktop.RotateObject(true);
        // Debug.Log("Start Rotate");
    }
    public void OnStopRotate(){
        m_CurrentModelAction = ModelAction.None;
        ButtonsParent.SetActive(true);
        // grabbingDesktop.ChangeGrabState();
        // grabbingDesktop.RotateObject(false);
        // Debug.Log("Stop Rotate");
    }

    public void OnClickResetSelectedModel(){
        // Debug.Log("Reset model to original position and rotation");
        OnlineUtilities.GetOnlineUtilities().ResetMeshInteractive_Event(m_SelectedObject.GetComponent<MeshInteractableOnline>().ID);

        SelectObject(null);
    }

    public void OnRepeatPushPull(int direction){
        ButtonsParent.SetActive(false);
        // Selector3D.SetActive(true);
        if(direction >0){
            // Debug.Log("Pushing Object");
            m_CurrentModelAction = ModelAction.Push;
        }else{
            // Debug.Log("Pulling Object");
            m_CurrentModelAction = ModelAction.Pull;
        }
    }

    private void OnStopPushPull(){
        m_SelectedObjectGrabPoint = m_SelectedObject.GetCenterPosition();
        ButtonsParent.SetActive(true);
        // Selector3D.SetActive(false);
        m_CurrentModelAction = ModelAction.None;
    }

    public void OnClickMaximizeUser(DesktopPlayerCamPreview selectedCam){
        // Debug.Log("OnClickMaximizedUser ");
        MaximizedUserPanel.gameObject.SetActive(true);

        MaximizedCamPreview.Init(selectedCam.TextName.text, selectedCam.BGColor.color, (RenderTexture)selectedCam.CameraImage.texture, selectedCam.GetUserData());

        OnlineUtilities.GetOnlineUtilities().EnableLocalRenderTexture_Event(true, selectedCam.GetUserData().XRObject.UserDataID);
    }

    public void OnClickCloseMaximize(){
        MaximizedUserPanel.gameObject.SetActive(false);
        OnlineUtilities.GetOnlineUtilities().EnableLocalRenderTexture_Event(false, -1);
    }

    public void OnClickChangeCamera(int direction)
    {
        if (m_StaticCameraMarkersList.Count == 0)
        {
            return;
        }

        int newId = m_CameraId + direction;
        if (newId < 0)
            newId = m_StaticCameraMarkersList.Count - 1;

        if (newId > (m_StaticCameraMarkersList.Count - 1))
            newId = 0;


        m_CameraId = newId;
        Transform cameraMarker = m_StaticCameraMarkersList[m_CameraId];

        // PlayerCamera.transform.position = cameraMarker.position;
        // PlayerCamera.transform.rotation = cameraMarker.rotation;

        AnimateCameraTo(cameraMarker);

        UpdateCameraIcons();
    }

    public enum FloatingCameraPositions{
        Right,
        RightSuperior,
        RightInferior,
        Left,
        LeftSuperior,
        LeftInferior,
        Superior,
        Inferior,
        Anterior
    }

    void AnimateCameraTo(Transform cameraMarker)
    {
        float timeAnim = 0.5f;
        LeanTween.cancel(PlayerCamera.gameObject);
        LeanTween.move(PlayerCamera.gameObject, cameraMarker.position, timeAnim).setEaseOutCirc();
        LeanTween.rotate(PlayerCamera.gameObject, cameraMarker.eulerAngles, timeAnim).setEaseOutCirc().setOnComplete(() =>
        {
            yaw = PlayerCamera.transform.eulerAngles.y;
            pitch = PlayerCamera.transform.eulerAngles.x;
        });
    }

    [System.Serializable]
    public class FloatingCameraData{
        public FloatingCameraPositions PositionName;
        public Transform Position;
    }

    public List<FloatingCameraData> CameraPositions;

    public void OnClickSelectCamera(int newCameraID){
        FloatingCameraData cameraData = CameraPositions.Find((X)=>X.PositionName == (FloatingCameraPositions)newCameraID);
        if(cameraData!=null){
            AnimateCameraTo(cameraData.Position);
        }else{
            Debug.LogError("Cannot find camera with id "+newCameraID);
        }
    }

    public void OnClickTakeScreenshot(){
        Debug.Log("Take Screenshot");

        ResetEnablers();

        StartCoroutine(WaitToTakeScreenshot());
    }

    private IEnumerator WaitToTakeScreenshot(){
        yield return new WaitForSeconds(0.2f);
        
        // string now = DateTime.Now.strftime("%Y-%m-%d_%H%M%S")  # 2020-08-23_211709
        string datetime = DateTime.Now.ToString("yyyy-MM-dd-HH_mm_ss");
        string fileName = "Screenshot_"+ datetime+".png";
        
        string finalFile = Utils.GetProjectDataFolder(fileName);

        Debug.Log("Taking screenshot on "+finalFile);
        // string folder = "SessionXX";
        ScreenCapture.CaptureScreenshot(finalFile);
    }

    public void OnClickConfigUsers(){
        AskUserName(false);
    }

    public void OnClickOpenDataFolder(){
        string finalURL = Utils.GetProjectDataFolder("");

        if(Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor){
            finalURL = "file://"+finalURL;
        }

        if(Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor){
            finalURL = "file://"+Uri.EscapeUriString(finalURL);
        }

        Debug.Log("Opening URL:" +finalURL);
        Application.OpenURL(finalURL);
    }

    internal void AskUserName(bool inChallenge)
    {
        UserNameGroup.gameObject.SetActive(true);
        UserNameGroup.Init(inChallenge);
    }
}