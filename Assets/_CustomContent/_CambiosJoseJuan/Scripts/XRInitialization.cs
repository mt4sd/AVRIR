using Mirror;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SpatialTracking;
using static PanelInArmController;

public class XRInitialization : NetworkBehaviour
{
    public bool AutoStart;

    [System.Serializable]
    public class SyncUserData{
        [SerializeField]
        [SyncVar]
        public string UserName;
        [SerializeField]
        [SyncVar]
        public int UserDataID = -1;
        [SerializeField]
        [SyncVar]
        public bool EnableTablet;
        [SerializeField]
        [SyncVar]
        public int PlayerID = -1;

        [SerializeField]
        [SyncVar]
        public int AvatarId;
    }
    [SyncVar(hook = nameof(StartAfterChangeUserData))]
    public SyncUserData UserDataSynchronized = new SyncUserData();


    // [SyncVar]
    public int UserDataID
    {
        get { return UserDataSynchronized.UserDataID; }   // get method
        set { UserDataSynchronized.UserDataID = value; }  // set method
    }

    public bool EnableTablet
    {
        get { return UserDataSynchronized.EnableTablet; }   // get method
        set { UserDataSynchronized.EnableTablet = value; }  // set method
    }

    // [SyncVar(hook = nameof(StartAfterChangePlayerID))]
    public int PlayerID
    {
        get { return UserDataSynchronized.PlayerID; }   // get method
        set { UserDataSynchronized.PlayerID = value; }  // set method
    }



    public enum XRType { Controller, Manus, OculusHand, Desktop }

    public XRType xrType;

    public bool forceXRType = false;

    public GameObject LoadingPanel;
    public Transform HiddenTabletParent; // To hide PanelInMorgueController

    public GameObject VRParent;
    public GameObject DesktopParent;
    public Transform DesktopDummyObject;
    public Transform OffsetTransform;
    public List<GameObject> DesktopContent;
    public List<GameObject> ManusVRContent;
    public List<GameObject> ControllerVRContent;
    public List<GameObject> OHandVRContent;
    public List<MonoBehaviour> DesktopComponent;
    public List<MonoBehaviour> ManusVRComponent;
    public List<MonoBehaviour> ControllerVRComponent;
    public List<MonoBehaviour> OHandVRComponent;

    public TrackedPoseDriverDissection trackedPoseDriverR;
    public TrackedPoseDriverDissection trackedPoseDriverL;

    public Camera XRCamera;
    public Transform currentRightHand;
    public Transform currentLeftHand;
    public DesktopPlayerController DesktopPlayer;
    public List<Transform> FakeHandsList;
    public PanelInArmController PanelInArm;


    private NetworkIdentity netIdentityLocal;
    public NetworkRoomPlayer roomPlayer;

    public Material HandsMaterial;
    public Material FakeHandsMaterial;
    public List<UserHead> UserHeadList;
    public UserHead head;
    public Color PlayerColor;
    public Transform ShoesParent;
    public float OffsetShoesBack=0.25f;
    public List<SkinnedMeshRenderer> HandsMeshList;
    public List<SkinnedMeshRenderer> FakeHandsMeshList;
    public List<MeshRenderer> ShoesMeshList;


    [Header("Fingers UI")]
    public List<Transform> FakeRightIndexFingersList;
    public List<Transform> FakeLeftIndexFingersList;
    public PlayerFingerUIInteractor FingerUIInteractorRight;
    public PlayerFingerUIInteractor FingerUIInteractorLeft;

    public float TimeChangePosition = 1;

    public StationController currentStation;
    public List<VRGrabbing> VRGrabbingList;

    private bool m_IsLoading;
    private int XRCameraCullingMask;
    private int DesktopCameraCullingMask;
    private CustomPhysicsHand[] CustomPhysicsHandsArray;
    public bool m_IsLocalPlayer;
    // private Vector3 m_StartXRCameraLocalPosition;

    public bool m_hola;
    void OnDestroy()
    {
        MorgueController.OnZAnatomyLoadedFinished -= OnZAnatomyLoadedFinished;
        MorgueController.OnZAnatomyLoadStarted -= OnZAnatomyLoadStarted;
    }
    void Awake()
    {
        // m_StartXRCameraLocalPosition = XRCamera.transform.localPosition;

        MorgueController.OnZAnatomyLoadedFinished += OnZAnatomyLoadedFinished;
        MorgueController.OnZAnatomyLoadStarted += OnZAnatomyLoadStarted;

        // Force disable all XR Content at beggining to avoid Nulls
        CustomPhysicsHandsArray = gameObject.GetComponentsInChildren<CustomPhysicsHand>(true);
        CheckOrDisableObjectsAndComponents(true);

        m_IsLocalPlayer = true;
    }
    public bool IsNetworkActive(){
        // Debug.Log("IsNetworkActive - connectionServerReady: "+ connectionToServer.isReady + " connectionToClient: "+ connectionToClient.isReady );
        return connectionToServer.isReady;
    }

    private void CheckOrDisableObjectsAndComponents(bool forceDisable=false){
        Debug.Log("CheckOrDisableObjectsAndComponents ID: "+PlayerID+" IsLocal? "+m_IsLocalPlayer);
        // if(!m_IsLocalPlayer) return;

        foreach (GameObject content in DesktopContent)
            if (content != null) content.SetActive(forceDisable?false:xrType == XRType.Desktop);
        foreach (GameObject content in ManusVRContent)
            if (content != null) content.SetActive(forceDisable?false:xrType == XRType.Manus);
        foreach (GameObject content in ControllerVRContent)
            if (content != null) content.SetActive(forceDisable?false:xrType == XRType.Controller);
        foreach (GameObject content in OHandVRContent)
            if (content != null) content.SetActive(forceDisable?false:xrType == XRType.OculusHand);


        // foreach (MonoBehaviour content in DesktopComponent)
        //     content.enabled = (forceDisable?false:xrType == XRType.Desktop);
        foreach (MonoBehaviour content in ManusVRComponent)
            if (content != null) content.enabled = (forceDisable?false:xrType == XRType.Manus);
        foreach (MonoBehaviour content in ControllerVRComponent)
            if (content != null) content.enabled = (forceDisable?false:xrType == XRType.Controller);
        foreach (MonoBehaviour content in OHandVRComponent)
            if (content != null) content.enabled = (forceDisable?false:xrType == XRType.OculusHand);

        foreach(CustomPhysicsHand content in CustomPhysicsHandsArray){
            if (content != null) if (content != null)content.enabled = (forceDisable?false:xrType != XRType.Desktop);
        }

        
        
        if(m_IsLocalPlayer){
            XRCamera.transform.localPosition = new Vector3(0, 0.25f, -0.25f);//Vector3.zero;// m_StartXRCameraLocalPosition;
            XRCamera.transform.localEulerAngles = Vector3.zero;
        } else
        {
            foreach(var hand in this.GetComponentsInChildren<OVRHand>(true))
            {
                hand.enabled = false;
            }
        }
    }

    public void Start()
    {
#if !UNITY_EDITOR
    forceXRType = false;
#endif
        
        if(AutoStart){
            DelayedStart();
        }
    }

    public bool IsLocalPlayer(){
        return m_IsLocalPlayer;
    }

    public void SetPlayerID(int id){
        PlayerID = id;
    }

    public void SetUserDataID(int userDataId){
        UserDataID = userDataId;
    }

    public void SetData(int userDataId, int playerId, int avatarId, bool enableTablet, string name){
        SyncUserData newData = new SyncUserData();
        newData.PlayerID = playerId;
        newData.UserDataID = userDataId;
        newData.EnableTablet = enableTablet;
        newData.UserName = name;
        newData.AvatarId = avatarId;

        UserDataSynchronized = newData;
    }

    private void StartAfterChangeUserData(SyncUserData oldValue, SyncUserData newValue){
        // Only allow one Desktop (The host)

        Debug.Log("StartAfterChangePlayerID newVal: "+newValue);
        if(newValue.PlayerID > 0){
            xrType=XRInitialization.XRType.Controller; 
        }else{
            if(newValue.PlayerID == 0 /*&& IsPCorEditor()*/){ // Si se descomenta esto, aparece el espiritu santo azul detrás de los jugadores VR. Hay que ver por qué estaba puesto

                GetRoomPlayerReference();
                // xrType = ((CustomNetworkRoomPlayer)roomPlayer).isDesktopEnabled?XRType.Desktop:XRType.Controller;
                
                // Find roomPlayer with ID 0
                foreach (NetworkRoomPlayer player in (NetworkRoomPlayer[])UnityEngine.Object.FindObjectsOfType(typeof(NetworkRoomPlayer)))
                    if (player.isServer && player.index == newValue.PlayerID)
                    {
                        xrType = ((CustomNetworkRoomPlayer)player).isDesktopEnabled?XRType.Desktop:XRType.Controller;
                        // roomPlayer = player;
                        // PlayerID = player.index;
                        break;
                    }


                // CustomNetworkRoomManager roomManager = GameObject.FindObjectOfType<CustomNetworkRoomManager>();
                // if(roomManager != null){
                //     xrType = roomManager.LocalSelectedXRType;// XRInitialization.XRType.Desktop;
                // }
            }
        }
        Debug.Log("xrType: "+xrType);
        
        // if(xrType == XRType.Desktop){
        //     MorgueGrabUtilities.morgue.DesktopXR = this;
        // }
        
        #if UNITY_EDITOR
            forceXRType = true; // To force VR on Unity Clients 
#endif

        DelayedStart();
    }

    private void GetRoomPlayerReference(){
        if(roomPlayer==null){
            foreach (NetworkRoomPlayer player in (NetworkRoomPlayer[])UnityEngine.Object.FindObjectsOfType(typeof(NetworkRoomPlayer)))
                if (player.hasAuthority)
                {
                    roomPlayer = player;
                    // PlayerID = player.index;
                    break;
                }
        }
    }

    public void DelayedStart(){

        // Save camera culling masks
        XRCameraCullingMask = XRCamera.cullingMask;
        DesktopCameraCullingMask = XRCamera.cullingMask;

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 72; // Quest 2 native FPS

        netIdentityLocal = this.GetComponent<NetworkIdentity>();
        m_IsLocalPlayer = false;

        if (NetworkClient.active && netIdentityLocal.hasAuthority)
        {
            GetRoomPlayerReference();
        }
        // isLocalPlayer = true;


        foreach (NetworkRoomPlayer player in (NetworkRoomPlayer[])UnityEngine.Object.FindObjectsOfType(typeof(NetworkRoomPlayer)))
        {
            if(player.index == PlayerID && player.hasAuthority){
                m_IsLocalPlayer = true;
                break;
            }
        }

        if(PlayerID==-1){
            m_IsLocalPlayer = true;
        }
        // if(NetworkServer.active){
        // isLocalPlayer = netIdentityLocal.isLocalPlayer;
        // }

        // if(!NetworkClient.active){ // In case we are in local, force PlayerID = 0
        //     // PlayerID = (int)netIdentity.netId % 4;
        //     PlayerID = 0;
        // }
        // PlayerID = 4; // Reorder
        if (NetworkClient.isConnected){
            if(xrType != XRType.Desktop){
                PlayerColor = UserInitializerController.Instance.RegisterNewPlayer(this);
            }else{
                PlayerColor = Color.black;
            }

            MorgueController morgue = GameObject.FindObjectOfType<MorgueController>();
            if (morgue != null){
                SetLoading(morgue.IsLoadingZAnatomy());
            }else{
                SetLoading(true);
            }
        }
        else{
            SetLoading(false);
        }

        // if(UserInitializerController.Instance!= null){
        //     UserInitializerController.Instance.SetNetIDOnUserData(UserDataID,(int)netIdentity.netId);
        // }

        
        // if (PlayerID == 0) PlayerColor = FeedBackUtilities.colorSelected;
        // if (PlayerID == 1) PlayerColor = FeedBackUtilities.colorNear;
        // if (PlayerID == 2) PlayerColor = Color.white;

        // if (PlayerID == 3) PlayerColor = Color.black;
        UpdateGameObjectName();
        Init();

        // CheckMultiplayer();
    }

    private void UpdateGameObjectName(){
        this.name = "XR Rig[PID:" + PlayerID + "][uID:"+ UserDataID +"]-"+xrType+(m_IsLocalPlayer?"-LocalPlayer":""); 
    }

    bool zAnatomyLoad = false;
    bool allPlayerConnected = false;

    private void OnZAnatomyLoadedFinished(){
        Debug.Log("OnZAnatomyLoadedFinished");
        OnlineUtilities.GetOnlineUtilities().InitZAnatomyState_Event();
    }

    public void OnAllPlayerLoadedZAnatomy() {
        zAnatomyLoad = true;

        CheckInit();
    }

    public void OnAllPlayersConnected(){
        allPlayerConnected = true;

        CheckInit();
    }
    private void CheckInit()
    {
        if (allPlayerConnected && zAnatomyLoad) {
            InitOnlineData();
            if (isServer)
                AnalyticsManager.GetInstance().InitDefaultChallenge();
        }

    }

    private void InitOnlineData(){
        SetLoading(false);
        Debug.Log("InitOnlineData "+gameObject.name);

        if(PlayerID == 0 && isServer){
            CustomNetworkRoomManager roomManager = GameObject.FindObjectOfType<CustomNetworkRoomManager>();
            if(roomManager != null){
                xrType = roomManager.LocalSelectedXRType;// XRInitialization.XRType.Desktop;
            }

            DesktopPlayer.SetDissectionScene();
            OnlineUtilities.GetOnlineUtilities().ChangePlayerXRType_Event(PlayerID/* PlayerID*/, (int)xrType);
        }
        
        // Find Tablet in game and destroy it if is not host
        PanelInMorgueController[] tabletsInGame = GameObject.FindObjectsOfType<PanelInMorgueController>();
        foreach(PanelInMorgueController panelTablet in tabletsInGame){
            if(panelTablet!= null && panelTablet.gameObject != DesktopParent.gameObject){
                if(/*!isServer ||*/ (isServer && xrType == XRType.Desktop)){
                    Destroy(panelTablet.gameObject);
                    Debug.Log("Destroyed InGame Tablet of Host");
                }
            }
        }

        
        if (m_IsLocalPlayer && AnalyticsManager.GetInstance() != null) 
            AnalyticsManager.GetInstance().Init();

        // if(PanelInMorgueController.Instance == null){
        //     MorgueController Morgue = GameObject.Find("_MorgueController")?.GetComponent<MorgueController>();
        //     if(Morgue!=null){
        //         GameObject tablet = Morgue.InstantiateLocalTablet(HiddenTabletParent);
        //         tablet.transform.GetChild(0).GetComponent<PanelInMorgueController>().ForceAsGlobalInstance();
        //         tablet.SetActive(false);
        //     }
        // }

        // if(tabletsInGame != null && tabletsInGame.gameObject != DesktopParent.gameObject){
        //     if(!isServer || IsPCorEditor()){
        //         Destroy(tabletsInGame.gameObject);
        //         Debug.Log("Destroyed InGame Tablet of Host");
        //     }
        // }

        Init();
    }
    private void OnZAnatomyLoadStarted(){
        Debug.Log("OnZAnatomyLoadStarted");
        SetLoading(true);
    }

    public void SetLoading(bool loading)
    {
        if (!m_IsLocalPlayer)
        {
            loading = false;
        }
        Debug.Log("SetLoading "+loading);
        m_IsLoading = loading;
        LoadingPanel.SetActive(loading);
        CheckOrDisableObjectsAndComponents(loading);
        if ((xrType == XRType.Desktop)) DesktopPlayer.SetLoading(loading);

        if(loading){
            int loadingCullingMask = (1 << LayerMask.NameToLayer("Loading"));
            XRCamera.cullingMask = loadingCullingMask;
            if ((xrType == XRType.Desktop)) DesktopPlayer.PlayerCamera.cullingMask = loadingCullingMask;
        }else{
            // if (m_IsLocalPlayer && AnalyticsManager.GetInstance() != null) AnalyticsManager.GetInstance().Init();

            // Load camera culling masks
            XRCamera.cullingMask = XRCameraCullingMask;
            if ((xrType == XRType.Desktop)) DesktopPlayer.PlayerCamera.cullingMask = DesktopCameraCullingMask;
        }

        CheckLoading();
    }
    private void CheckLoading(){
        if(m_IsLoading){
            Transform camera = (xrType==XRType.Desktop?DesktopPlayer.PlayerCamera:XRCamera).transform;
            LoadingPanel.transform.position = camera.position;
        }
    }

    private void CheckMultiplayer()
    {
        Debug.Log("Check Multiplayer Player: "+PlayerID);

        if (NetworkClient.isConnected && !m_IsLocalPlayer){// !(netIdentityLocal.hasAuthority)) {
            // Network actived and not local player
            Debug.Log(" ---- Network Actived and not local");

            Camera cam = this.GetComponentInChildren<Camera>(true);

            foreach(UserHead head in UserHeadList){
                head.gameObject.SetActive(false); // Hide all others
            }

            head = UserHeadList[UserDataSynchronized.AvatarId];
            head.gameObject.SetActive(true);

            // Destroy(cam.gameObject.GetComponent<UniversalAdditionalCameraData>());
            Destroy(cam.gameObject.GetComponent<AudioListener>());
            Destroy(cam.gameObject.GetComponent<TrackedPoseDriver>());
            // cam.cullingMask = ~(1 << LayerMask.NameToLayer("DissapearOnLocal"));
            // Destroy(cam); // Set camera render texture for PlayerID

            // foreach (var component in this.GetComponentsInChildren<Oculus.Interaction.Input.OVRCameraRigRef>(true)) Destroy(component);
            foreach (var component in this.GetComponentsInChildren<OVRManager>(true)) Destroy(component);

            foreach (var component in this.GetComponentsInChildren<HandAnimationController>(true)) Destroy(component);
            foreach (var component in this.GetComponentsInChildren<CustomPhysicsHand>(true))
            {
                if(component != null){
                    if(component.FollowRenderer != null){
                        component.FollowRenderer.enabled = false;
                    }
                    component.enabled = false; //Destroy(component);
                }
            }
            foreach (var component in this.GetComponentsInChildren<TrackedPoseDriver>(true)) Destroy(component);
            foreach (var component in this.GetComponentsInChildren<Camera>(true)) component.enabled = false;
        } else 
        {
            Debug.Log(" ---- Network disabled and LOCAL");
            if (!NetworkClient.isConnected)// Not connected
            {
                foreach (NetworkBehaviour i in this.GetComponentsInChildren<NetworkBehaviour>())
                {
                    if (i == this) return;
                    GameObject tmp = i.gameObject;
                    Destroy(i);
                    tmp.gameObject.SetActive(true);
                }
                foreach (NetworkIdentity i in this.GetComponentsInChildren<NetworkIdentity>())
                {
                    GameObject tmp = i.gameObject;
                    Destroy(i);
                    tmp.gameObject.SetActive(true);
                }
            }else{
                Debug.Log("Checking for nulls ----- TEST to find how to avoid them");
                
                // foreach (var component in this.GetComponentsInChildren<Oculus.Interaction.Input.OVRCameraRigRef>(true)) Destroy(component);
                // foreach (var component in this.GetComponentsInChildren<Oculus.Interaction.Input.FromOVRHandDataSource>(true)) Destroy(component);
            }
        }
    }

    internal void OnMove(PanelInArmController.ButtonAction action, bool isInit = false)
    {
        MorgueController Morgue = GameObject.Find("_MorgueController")?.GetComponent<MorgueController>();
        if (Morgue)
        {
            MorgueController.ReferencePlayerPosition newReferencePosition = Morgue.RequestPlayerNewPos(this.PlayerID, action);
            // MorgueController.ReferencePlayerPosition newReferencePosition = Morgue.RequestFirstEmptyPlayerPos(this.PlayerID);
            
            // // Change color for new Position

            // // ESTO HAY QUE ENVIARLO POR RED PARA ACTUALIZARLO EN TODOS

            int idUserData = newReferencePosition.ID-1;
            if(idUserData >= 0){
                PlayerColor = UserInitializerController.Instance.UsersData[idUserData].Color;
                OnlineUtilities.GetOnlineUtilities().SetPlayerColor(this, PlayerColor);
                // UpdatePlayerColor();
            }

            this.MovePlayerToPosition(newReferencePosition.GetPosition(), newReferencePosition.GetRotation(), newReferencePosition.Station, isInit);

            bool serverInVR = (isServer && xrType != XRType.Desktop);
            if(isLocalPlayer && (this.EnableTablet || serverInVR)){
                GameObject tablet = Morgue.InstantiateLocalTablet(newReferencePosition.TabletMarker);
                tablet.transform.GetChild(0).GetComponent<PanelInMorgueController>().ForceAsGlobalInstance();
            }
            
            if(PanelInMorgueController.Instance == null && isLocalPlayer){
                GameObject tablet = Morgue.InstantiateLocalTablet(HiddenTabletParent);
                tablet.transform.GetChild(0).GetComponent<PanelInMorgueController>().ForceAsGlobalInstance();
                
                tablet.SetActive(this.EnableTablet);
            }
        }
    }

    public void UpdatePlayerColor(){
        foreach (SkinnedMeshRenderer handMesh in FakeHandsMeshList)
        {
            handMesh.material = FakeHandsMaterial;
            handMesh.material.color = PlayerColor;
        }

        foreach(SkinnedMeshRenderer handMesh in HandsMeshList){
            handMesh.material = HandsMaterial;
            handMesh.material.color = PlayerColor;
        }
        
        foreach(UserHead head in UserHeadList){
            head.RecolorMeshes(PlayerColor);
        }

        foreach(MeshRenderer shoes in ShoesMeshList){
            shoes.material = FakeHandsMaterial;
            shoes.material.color = PlayerColor;
        }
    }

    [ContextMenu("ReInit")]
    public void Init()
    {
        Debug.Log("ReInit player");
        VRGrabbingList = new List<VRGrabbing>();

        UpdatePlayerColor();
        
        bool showDesktop = xrType == XRType.Desktop && m_IsLocalPlayer; // Only on local Player

        if (VRParent) VRParent.SetActive(xrType != XRType.Desktop && !showDesktop);
        if (DesktopParent) DesktopParent.SetActive(showDesktop);
        
        if(xrType != XRType.Desktop){
            PanelInMorgueController panelInMorgue = DesktopParent.GetComponent<PanelInMorgueController>();
            if(panelInMorgue != null){
                GameObject.Destroy(panelInMorgue);
            }
        }

        CheckOrDisableObjectsAndComponents(m_IsLoading);
            
        // foreach (GameObject content in DesktopContent)
        //     content.SetActive(xrType == XRType.Desktop);
        // foreach (MonoBehaviour content in DesktopComponent)
        //     content.enabled = (xrType == XRType.Desktop);

        // foreach (GameObject content in ManusVRContent)
        //     content.SetActive(xrType == XRType.Manus);
        // foreach (GameObject content in ControllerVRContent)
        //     content.SetActive(xrType == XRType.Controller);
        // foreach (GameObject content in OHandVRContent)
        //     content.SetActive(xrType == XRType.OculusHand);            
        // foreach (MonoBehaviour content in ManusVRComponent)
        //     content.enabled = (xrType == XRType.Manus);
        // foreach (MonoBehaviour content in ControllerVRComponent)
        //     content.enabled = (xrType == XRType.Controller);
        // foreach (MonoBehaviour content in OHandVRComponent)
        //     content.enabled = (xrType == XRType.OculusHand);

        DesktopDummyObject.gameObject.SetActive(xrType == XRType.Desktop && !m_IsLocalPlayer);//!m_IsLocalPlayer);
        Debug.Log("XRInitialization: "+xrType);
        if(xrType != XRType.Desktop){
            if (!NetworkClient.active || netIdentityLocal == null || netIdentityLocal.hasAuthority)
            {
                UpdateFingerUI();

                OnMove((ButtonAction)this.UserDataID, true);
                PanelInArm?.Init(this, XRCamera, GetCurrentFakeHandTransform());
            }
        }else{
            // if(!m_IsLocalPlayer){
                MorgueController Morgue = GameObject.Find("_MorgueController")?.GetComponent<MorgueController>();
                if (Morgue)
                {
                    DesktopDummyObject.SetParent(Morgue.DesktopDummyTransformParent);
                    DesktopDummyObject.localPosition = Vector3.zero;
                    DesktopDummyObject.localEulerAngles = Vector3.zero;
                    currentStation = Morgue.ReferencePositions[0].Station;

                    // MorgueController.ReferencePlayerPosition newReferencePosition = Morgue.RequestPlayerNewPos(this.PlayerID, (ButtonAction)this.PlayerID);

                    // DesktopDummyObject.position = newReferencePosition.GetPosition();
                    // DesktopDummyObject.rotation = newReferencePosition.GetRotation();
                    // currentStation = newReferencePosition.Station;
                }else{
                    DesktopDummyObject.gameObject.SetActive(false);//!m_IsLocalPlayer);
                }
            // }
        }
        
        CheckMultiplayer();
        
        UpdateGameObjectName();
    }
    
    private void UpdateFingerUI(){
        // FAKE PARENT 
            // RIGHT
        foreach(Transform finger in FakeRightIndexFingersList){
            if(finger.gameObject.activeInHierarchy){
                FingerUIInteractorRight.transform.SetParent(finger);
                FingerUIInteractorRight.transform.localPosition = Vector3.zero;

                currentRightHand = finger.GetComponentInParent<CustomPhysicsHand>().transform;
            }
        }
            // LEFT
        foreach(Transform finger in FakeLeftIndexFingersList){
            if(finger.gameObject.activeInHierarchy){
                FingerUIInteractorLeft.transform.SetParent(finger);
                FingerUIInteractorLeft.transform.localPosition = Vector3.zero;

                currentLeftHand = finger.GetComponentInParent<CustomPhysicsHand>().transform;
            }
        }

        // DEVICE INPUT
        List<XRDeviceInput> inputDevices = new List<XRDeviceInput>(this.GetComponentsInChildren<XRDeviceInput>(false));
        FingerUIInteractorRight.UpdateDeviceInput(inputDevices.Find((x)=> x.controllerCharacteristics.HasFlag(FingerUIInteractorRight.Hand)));
        FingerUIInteractorLeft.UpdateDeviceInput(inputDevices.Find((x)=> x.controllerCharacteristics.HasFlag(FingerUIInteractorLeft.Hand)));
    }

    private Transform GetCurrentFakeHandTransform(){
        Transform hand = null;

        hand = FakeHandsList[(int)xrType];

        return hand;
    }

    private void Update()
    {
        CheckLoading();

        // Debug.Log("FPS: " + 1.0f / Time.deltaTime);
        if(xrType == XRType.Desktop)
            return;

        if (!NetworkClient.active || netIdentityLocal == null || netIdentityLocal.hasAuthority)
            ChangeXRType();
        // Debug.Log(xrType);
        //Debug.Log(Camera.allCamerasCount);

        CheckShoes();
    }


    private void CheckShoes(){
        // for (int i = 0; i < ShoesMeshList.Count; i++)
        // {
            // Vector3 headPlane = Vector3.ProjectOnPlane(head.transform.forward, Vector3.up);

            Vector3 newPosition = head.transform.position;// - headPlane.normalized * OffsetShoesBack;
            newPosition.y = 0;

            ShoesParent.transform.eulerAngles = Vector3.up * head.transform.eulerAngles.y;
            ShoesParent.transform.GetChild(0).transform.localPosition = new Vector3(0,0,-OffsetShoesBack);

            ShoesParent.transform.position = Vector3.Lerp(ShoesParent.transform.position, newPosition, Time.deltaTime*4); 
        // }
    }

    [ContextMenu("ForceDesktop")]
    public void ForceDesktop(){
        forceXRType = true;
        xrType = XRType.Desktop;
        Init();
        Debug.Log("ChangedXRType to "+xrType);
    }

    public void ForceVR(){
        forceXRType = true;
        xrType = XRType.Controller;
        Init();
        Debug.Log("ChangedXRType to "+xrType);
    }

    private bool IsPCorEditor(){
        return Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WebGLPlayer;
    }

    private void ChangeXRType()
    {
        bool forceType = Application.platform == RuntimePlatform.WindowsEditor && forceXRType;
        if (xrType == XRType.Manus || forceType) return;

        XRType OldType = xrType;

        if (IsPCorEditor())
        {
            // xrType = XRType.Desktop;
        }else{
            if (OVRInput.IsControllerConnected(OVRInput.Controller.Hands) || (!trackedPoseDriverR.dataAvailable && !trackedPoseDriverL.dataAvailable))
                // xrType = XRType.OculusHand;
                xrType = XRType.Controller;
            else
                xrType = XRType.Controller;
        }

        if (OldType != xrType)
        {
            Init();
            if (NetworkClient.active && netIdentity.hasAuthority)
                OnlineUtilities.GetOnlineUtilities().SetXRtypeState(PlayerID, xrType); // Init();
        }
    }

    public void MovePlayerToPosition(Vector3 newPosition, Quaternion newRotation, StationController station, bool isInit)
    {
        Transform offset = OffsetTransform;
        Vector3 nextPosition = new Vector3(newPosition.x, offset.position.y, newPosition.z); 
        
        LeanTween.cancel(offset.gameObject);
        if (!isInit)
        {
            LeanTween.move(offset.gameObject, nextPosition, TimeChangePosition).setEaseOutCirc();
            LeanTween.rotate(offset.gameObject, newRotation.eulerAngles, TimeChangePosition).setEaseOutCirc();
        }
        else
        {
            offset.position = nextPosition;
            offset.rotation = newRotation;
        }

        UpdateStation(station);
    }

    public void RegisterVRGrabbing(VRGrabbing vrGrabbing){
        VRGrabbingList.Add(vrGrabbing);
    }

    public void UpdateStation(StationController station){
        currentStation = station;

        foreach(VRGrabbing grabbing in VRGrabbingList){
            grabbing.station = currentStation;
        }
    }

    [ContextMenu("Controller")]
    public void SETCONTROLLER()
    {
        xrType = XRType.Controller;
        Init();
        if (NetworkClient.active && netIdentity.hasAuthority)
            OnlineUtilities.GetOnlineUtilities().SetXRtypeState(PlayerID, xrType); // Init();
    }

    [ContextMenu("OculusHand")]
    public void SETOculusHand()
    {
        xrType = XRType.OculusHand;
        Init();
        if (NetworkClient.active && netIdentity.hasAuthority)
            OnlineUtilities.GetOnlineUtilities().SetXRtypeState(PlayerID, xrType); // Init();
    }
}
