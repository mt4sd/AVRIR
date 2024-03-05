using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using Sirenix.OdinInspector;

public class InitSceneCanvasController : MonoBehaviour
{
    public enum InitSceneState{
        Home,
        Room_Host,
        Room_Client,
        Host_SelectModel,
    }

    [System.Serializable]
    public class CustomColorsClass{
        public Color Button_Default;
        public Color Button_Ready;
        public Color Button_NotReady;
    }
    public CustomColorsClass CustomColors;
    public ApplicationInitializer AppInitializer;
    public List<GameObject> PanelPerStates;
    public InitSceneUserReadyIcon UserInRoomPrefab;
    
    public List<Color> ColorsList= new List<Color>();
    public List<TMPro.TextMeshProUGUI> NameModelTitles;
    [Header("Start Panel")]
    public Button ButtonStartClient;

    // [Header("Home")]
    // public GameObject Panel_Home;
    [Header("Room Host")]
    public Button ButtonHostStart;
    public TMPro.TextMeshProUGUI ButtonHostStartLabel;
    private bool m_HostReady;

    [Header("Room Client")]
    public Transform UsersInRoomClientParent;
    public Button ButtonClientReady;
    public TMPro.TextMeshProUGUI ButtonClientReadyLabel;
    private bool m_ClientReady;
    
    [Header("Loading")]
    public GameObject LoadingContent;
    public TMPro.TextMeshProUGUI LoadingContentText;

    private InitSceneState m_CurrentState;
    private Coroutine m_LoadingCoroutine;
    [Header("Debug publics")]
    public List<InitSceneUserReadyIcon> InstantiatedUserReadyIcons;
    private InitSceneState m_StateBeforeLoading;

    private void CloseAllPanels(){
        LoadingContent.SetActive(false);

        foreach(GameObject panel in PanelPerStates){
            panel.SetActive(false);
        }
    }

    private void ShowLoading(bool show, string showMessage=""){
        if(!show){
            NewState(m_StateBeforeLoading);
        }else{
            CloseAllPanels();
            m_StateBeforeLoading = m_CurrentState;
        }
        LoadingContent.SetActive(show);
        LoadingContentText.text = showMessage;
    }

    public void Init(){
        InstantiatedUserReadyIcons = new List<InitSceneUserReadyIcon>();

        Utils.ClearChilds(UsersInRoomClientParent);
        CloseAllPanels();
        NewState(InitSceneState.Home);
    }

    public void NewState(InitSceneState state){
        CloseAllPanels();
        m_CurrentState=state;
        PanelPerStates[(int)m_CurrentState].SetActive(true);


        switch(state){
            case InitSceneState.Home:
                UsersInRoomClientParent.gameObject.SetActive(false);
                break;
            case InitSceneState.Room_Host:
                // UpdateRoomModelData();
                CheckReadyClientButton();
                UsersInRoomClientParent.gameObject.SetActive(true);
                ButtonHostStart.interactable = false;
                break;
            case InitSceneState.Room_Client:
                CheckReadyClientButton();
                UsersInRoomClientParent.gameObject.SetActive(true);
                break;
            case InitSceneState.Host_SelectModel:
                UsersInRoomClientParent.gameObject.SetActive(false);
                InstantiateLoadModelButtons();
                break;
        }
    }

    // public void UpdateRoomModelData(){
    //     CustomNetworkRoomPlayer hostPlayer = AppInitializer.GetManager().roomSlots.Find((x)=>x.index == 0) as CustomNetworkRoomPlayer;

    //     ResourcesModelData data = AppInitializer.CanvasController.ResourcesModelIDs[hostPlayer.SelectedModelID];
    //     UpdateTitlesNameModel(data.Name);
    // }

    #region  LOAD MODEL SELECTOR
    [System.Serializable]
    public class ResourcesModelData{
        public string Name;
        public string Path;
        public Sprite Icon;
    }
    public List<ResourcesModelData> ResourcesModelIDs;
    public InitSceneLoadModelButton LoadModelButtonPrefab;
    public ScrollRect LoadModelHorizontalScroll;
    public Transform LoadModelButtonsParent;
    public Button LoadModelButton;
    public float LoadModelScrollNormalizedChangeOnClick;
    private InitSceneLoadModelButton m_SelectedModelButton;
    private bool m_StartLocalScene;

    private void InstantiateLoadModelButtons(){
        Utils.ClearChilds(LoadModelButtonsParent);
        for (int i = 0; i < ResourcesModelIDs.Count; i++)
        {
            ResourcesModelData data = ResourcesModelIDs[i];
            InitSceneLoadModelButton newButton = Instantiate(LoadModelButtonPrefab, LoadModelButtonsParent);
            newButton.Init(this,i, data);
        }

        LoadModelButton.interactable = false;
    }

    public void OnClickBackFromSelectModel(){
        NewState(InitSceneState.Home);
    }

    private void OpenPanelSelectModel(){
        NewState(InitSceneState.Host_SelectModel);
    }

    public void OnClickChangeModelButtonScroll(int direction){
        LoadModelHorizontalScroll.horizontalNormalizedPosition += direction * LoadModelScrollNormalizedChangeOnClick;
    }

    public void OnSelectModelButton(InitSceneLoadModelButton button){
        if(m_SelectedModelButton != button){
            if(m_SelectedModelButton != null){
                m_SelectedModelButton.SetInteractable(true);
            }
        }

        m_SelectedModelButton = button;
        if(m_SelectedModelButton != null){
            m_SelectedModelButton.SetInteractable(false);
        }

        LoadModelButton.interactable = m_SelectedModelButton != null;
    }

    [Button(ButtonSizes.Gigantic)]
    public void OnClickLoadSelectedModel(){
        if(m_SelectedModelButton == null){
            m_SelectedModelButton = LoadModelButtonsParent.GetChild(0).GetComponent<InitSceneLoadModelButton>();
        }
        
        // AppInitializer.SetNewModelID(m_SelectedModelButton.GetData().Path);
        AppInitializer.SetNewModelID(m_SelectedModelButton.GetDataID());

        // AppInitializer.manager.SelectedModelPath = m_SelectedModelButton.GetPath();
        StartHost();
    }
    #endregion

    public void UpdateTitlesNameModel(string name){
        foreach(TMPro.TextMeshProUGUI title in NameModelTitles){
            title.text = name;
        }
    }

    public void OnNetwork_Connected(bool isConnected){
        Debug.Log("OnNetwork_Connected: "+isConnected);

        if(!NetworkClient.isConnected){
            NewState(InitSceneCanvasController.InitSceneState.Home);
        }
    }
    public void OnNetwork_ClientActive(bool active){
        Debug.Log("OnNetwork_ClientActive: "+active); 
    }
    public void OnNetwork_IsConnecting(bool isConnecting){
        Debug.Log("OnNetwork_IsConnecting: "+isConnecting);
        
    }

    public void OnClickCancelLoad(){
        AppInitializer.CancelConnection();
        ShowLoading(false);
    }

    [Button(ButtonSizes.Gigantic)]
    public void OnClickStartLocal(){
        m_StartLocalScene = true;
        GameObject.FindObjectOfType<CustomNetworkRoomManager>().SetPlayers(1);
        OpenPanelSelectModel();
    }

    private void StopLoadingCoroutine(){
        if(m_LoadingCoroutine!=null){
            StopCoroutine(m_LoadingCoroutine);
        }
    }

    [Button(ButtonSizes.Gigantic)]
    public void OnClickStartHost(){
        m_StartLocalScene = false;
        OpenPanelSelectModel();
    }

    public void StartHost(){
        if(m_StartLocalScene){
            ShowLoading(true,"Starting local dissection...");
            AppInitializer.StartLocal();
        }else{
            m_StartLocalScene = false;

            AppInitializer.StartHost();

            ShowLoading(true, "Creating new room in local network...");
            StopLoadingCoroutine();
            m_LoadingCoroutine = StartCoroutine(WaitToNetworkClientConnected(OnHostConnectedToServer));
        }
    }

    public void OnHostConnectedToServer(){
        NewState(InitSceneState.Room_Host);
    }
    
    [Button(ButtonSizes.Gigantic)]
    public void OnClickStartClient(){
        m_StartLocalScene = false;

        AppInitializer.StartClient();
        ShowLoading(true, "Searching for rooms in local network...");
        
        StopLoadingCoroutine();
        m_LoadingCoroutine = StartCoroutine(WaitToNetworkClientConnected(OnClientConnectedToServer));
    }

    private IEnumerator WaitToNetworkClientConnected(System.Action callback){
        yield return new WaitUntil(()=>AppInitializer.NetworkClientConnected);
        
        ShowLoading(false);

        if(callback!=null){
            callback();
        }
    }

    public void OnClientConnectedToServer(){
        NewState(InitSceneState.Room_Client);
        StartCoroutine(WaitToSetReadyClient());
    }

    private IEnumerator WaitToSetReadyClient(){
        yield return new WaitForSeconds(0.5f);
        OnClickReadyClient();
    }

    public void OnClickClientCancel(){
        AppInitializer.CancelConnection();
        NewState(InitSceneState.Home);
    }

    public void OnClickHostCancel(){
        // Debug.Log("Cancel Connection");
        // AppInitializer.CancelConnection();
        // Debug.Log("Stopping host");
        // AppInitializer.StopHost();
        
        // AppInitializer.CancelConnection();

        AppInitializer.StopHost();
        
        Debug.Log("Change State to Home");
        AppInitializer.xr.gameObject.SetActive(true);
        NewState(InitSceneState.Home);
    }

    public void OnClickReadyClient(){
        AppInitializer.ReadyClient();
        //CheckReadyClientButton();
    }

    public void OnClickReadyHost(){
        ButtonHostStart.interactable = false;
        OnClickReadyClient();
    }

    private void CheckReadyClientButton(){
        // if(AppInitializer.GetRoomPlayer() != null){
        //     Debug.Log("CheckReadyClientButton width ready: "+ AppInitializer.GetRoomPlayer().readyToBegin);
        // }

        if (AppInitializer.GetRoomPlayer() != null && AppInitializer.GetRoomPlayer().readyToBegin){
            ButtonClientReadyLabel.text = "Ready to start";
            ButtonClientReady.image.color = CustomColors.Button_Ready;
        }else{
            ButtonClientReadyLabel.text = "Not ready<size=50%>\n(Tap to ready)";
            ButtonClientReady.image.color = CustomColors.Button_NotReady;
        }

        // CheckButtonHostStart();
    }

    private void CheckButtonHostStart(){
        int pendingPlayersCount = AppInitializer.GetManager().roomSlots.FindAll((x)=>!x.readyToBegin).Count;
        // Debug.Log("CheckButtonHostStart: "+pendingPlayersCount);

        bool loadingGame = pendingPlayersCount == 0;
        bool enoughPlayers = AppInitializer.GetManager().numPlayers > 1;
        bool interactable = enoughPlayers/* && pendingPlayersCount == 1*/;
        ButtonHostStart.interactable = interactable; // Only left Host to start
        string reasonNoInteractable = !enoughPlayers?"Need to be more than 1 player in the room to start":("Wait for users to be ready\nPending users: "+(pendingPlayersCount-1));
        if(!loadingGame){
            ButtonHostStartLabel.text = interactable?"Start dissection":"<size=50%>"+reasonNoInteractable;
            ButtonHostStart.image.color = interactable?CustomColors.Button_Ready:CustomColors.Button_NotReady;
        }else{
            ButtonHostStartLabel.text = "Starting...";
            ButtonHostStart.image.color = CustomColors.Button_Default;
        }
    }

    public void AddNewPlayer(NetworkConnectionToClient conn, int netId, int avatarId,bool isHost, bool isLocalPlayer, bool isReady, bool enableAdminTablet){
        if(UsersInRoomClientParent == null){
            Debug.Log("UsersInRoomClientParent = null - Avoiding AddNewPlayer Icon on Room");
            return;
        }

        // uint netId = conn.identity.netId;

        InitSceneUserReadyIcon newUser = Instantiate(UserInRoomPrefab, UsersInRoomClientParent);
        Color playerColor = ColorsList[netId];

        Debug.Log("Add new player "+netId);
        CustomNetworkRoomManager room = NetworkManager.singleton as CustomNetworkRoomManager;
        if (room != null && !room.IsDesktopEnabled)
        {
            Debug.Log("--- No desktop enabled");
            playerColor = ColorsList[netId+1];
        }else{
            Debug.Log("--- Desktop enabled");
        }
       
        // bool isLocalUser = conn.identity.isLocalPlayer;
        newUser.Init(netId, avatarId, conn, playerColor, isLocalPlayer, NetworkClient.isHostClient, enableAdminTablet);
        newUser.SetReady(isReady);

        // Debug.Log("Canvas - AddNewPlayer - "+netId);
        
        // CustomNetworkRoomPlayer roomPlayer = AppInitializer.manager.roomSlots[netId].GetComponent<CustomNetworkRoomPlayer>();
        // roomPlayer.PlayerInRoomIcon = newUser;
         
        InstantiatedUserReadyIcons.Add(newUser);

        CheckButtonHostStart();
    }


    public void RemovePlayer(NetworkConnectionToClient conn){//uint netId){//
        // uint netId = conn.identity.netId;
        int removePlayerIndex = InstantiatedUserReadyIcons.FindIndex((x)=>x.PlayerConnection == conn);
        Debug.Log("Remove player with found index = "+removePlayerIndex);
        if(removePlayerIndex != -1){
            GameObject.Destroy(InstantiatedUserReadyIcons[removePlayerIndex].gameObject);
            InstantiatedUserReadyIcons.RemoveAt(removePlayerIndex);
        }
    }

    // public void CheckReadyUsers(){
    //     foreach(InitSceneUserReadyIcon user in InstantiatedUserReadyIcons){
    //         SetReadyUser(user.GetNetId(), user.PlayerConnection.isReady);
    //     }
    // }

    public void ClearClientIcons(){
        InstantiatedUserReadyIcons.Clear();
        if(UsersInRoomClientParent!=null){
            Utils.ClearChilds(UsersInRoomClientParent);
        }
    }

    public void SetReadyUser(int id, bool ready){
        if(InstantiatedUserReadyIcons == null || InstantiatedUserReadyIcons.Count == 0 || id > InstantiatedUserReadyIcons.Count-1){
            return;
        }

        InitSceneUserReadyIcon selectedIcon = InstantiatedUserReadyIcons[id];//.Find( (x) => x.GetNetId()==id );

        if(selectedIcon != null){
            selectedIcon.SetReady(ready);
        }else{
            Debug.LogError("SetReadyUser failed - There is no user with netId: "+id);
        }

        CheckReadyClientButton();
        CheckButtonHostStart();
    }

    public bool IsEnabledTabletForPlayer(NetworkConnectionToClient playerConnection){
        Debug.Log("IsEnabledTablet on player " + playerConnection.identity.netId);
        return InstantiatedUserReadyIcons.Find((x)=>x.PlayerConnection.identity.netId == playerConnection.identity.netId).IsTabletEnabled();
    }
}
