using kcp2k;
using Mirror;
using Mirror.Discovery;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

public class ApplicationInitializer : MonoBehaviour
{
    public InitSceneCanvasController CanvasController;
    public CustomNetworkRoomManager manager;
    public XRInitialization xr;
    public Camera cam;

    [Header("Canvas")]
    public NetworkRoomPlayer roomPlayer;
    public TMPro.TMP_Text text;
    public GameObject optionsCanvas;
    public GameObject cancelCanvas;
    public GameObject roomCanvas;

    public bool NetworkClientConnected;
    public bool NetworkClientActive;
    public bool NetworkClientIsConnecting;
    private int HostSelectedModelID;

    public GameObject onlineUtilities;

    readonly Dictionary<long, ServerResponse> discoveredServers = new Dictionary<long, ServerResponse>();

    private static LinkedList<System.Uri> serverUri;

    public void Start()
    {
        LeanTween.init();
        xr.gameObject.SetActive(true);
        cam.gameObject.SetActive(true);
#if UNITY_EDITOR
        DebugManager.instance.enableRuntimeUI = false; // Unity Bug fix: https://forum.unity.com/threads/errors-with-the-urp-debug-manager.987795/
        Selection.activeGameObject = this.gameObject;
#endif
        CanvasController.Init();

        if (serverUri == null) serverUri = new LinkedList<System.Uri>();
        GetManager().GetComponent<NetworkDiscovery>().StartDiscovery();
    }

    public CustomNetworkRoomManager GetManager(){
        if(manager == null){
            manager = GameObject.FindObjectOfType<CustomNetworkRoomManager>();
        }
        return manager;
    }

    public void Update()
    {

        if(NetworkClientConnected != NetworkClient.isConnected){
            CanvasController.OnNetwork_Connected(NetworkClient.isConnected);
        }

        if(NetworkClientActive != NetworkClient.active){
            CanvasController.OnNetwork_ClientActive(NetworkClient.active);
        }

        if(NetworkClientIsConnecting != NetworkClient.isConnecting){
            CanvasController.OnNetwork_IsConnecting(NetworkClient.isConnecting);
        }

        NetworkClientConnected = NetworkClient.isConnected;
        NetworkClientActive = NetworkClient.active;
        NetworkClientIsConnecting = NetworkClient.isConnecting;

        roomCanvas.SetActive(NetworkClientConnected);
        optionsCanvas.SetActive(!NetworkClientActive);
        cancelCanvas.SetActive(NetworkClientIsConnecting);

        if (roomPlayer)
        {
            if (roomPlayer.readyToBegin)
                text.text = "Listo";
            else
                text.text = "No listo";
        }
    }
    
    [Button(size: ButtonSizes.Gigantic)]
    public void Canvas_StartHost(){
        InitSceneCanvasController canvasController = GameObject.FindObjectOfType<InitSceneCanvasController>();
        canvasController.OnClickStartHost();
        canvasController.OnClickLoadSelectedModel();
    }

    [Button(size: ButtonSizes.Gigantic)]
    public void Canvas_StartClient(){
        InitSceneCanvasController canvasController = GameObject.FindObjectOfType<InitSceneCanvasController>();
        canvasController.OnClickStartClient();
    }
    
    [Button(size: ButtonSizes.Gigantic)]
    public void Canvas_ClientReady()
    {

        GameObject.FindObjectOfType<CustomNetworkRoomManager>().SetPlayers(GameObject.FindObjectOfType<InitSceneCanvasController>().InstantiatedUserReadyIcons.Count);
        InitSceneCanvasController canvasController = GameObject.FindObjectOfType<InitSceneCanvasController>();
        canvasController.OnClickReadyClient();
    }

    public NetworkRoomPlayer GetRoomPlayer(){
        // if (roomPlayer == null) 
            // roomPlayer = Object.FindObjectOfType<NetworkRoomPlayer>();
            foreach (CustomNetworkRoomPlayer player in (CustomNetworkRoomPlayer[])Object.FindObjectsOfType(typeof(CustomNetworkRoomPlayer)))
                if (player.hasAuthority)
                {
                    roomPlayer = player;
                    break;
                }

        return roomPlayer;
    }

    [HorizontalGroup("LocalButtons")]
    [Button(size: ButtonSizes.Gigantic)]
    public void StartLocal(){
        // SceneManager.LoadSceneAsync("Dissection", LoadSceneMode.Single);
        
        StartHost();
        StartCoroutine(ImitateClientSide());
    }
    [HorizontalGroup("LocalButtons")]
    [Button(size: ButtonSizes.Gigantic)]
    public void StartLocalAsVR(){
        // SceneManager.LoadSceneAsync("Dissection", LoadSceneMode.Single);
        CanvasInitSceneOnlyForDesktop canvasInit = GameObject.FindObjectOfType<CanvasInitSceneOnlyForDesktop>();
        
        if(canvasInit != null){
            canvasInit.OnClickButtonSetVR();
        }

        Invoke("StartLocal", 0.5f);
    }

    private IEnumerator ImitateClientSide(){
        yield return new WaitForSeconds(0.1f);
        // StartClient();
        yield return new WaitForSeconds(0.1f);
        ReadyClient();
    }

    [Button(size: ButtonSizes.Gigantic)]
    public void StartHost()
    {
        discoveredServers.Clear();
        NetworkManager.singleton.StopHost();
        
        NetworkManager.singleton.StartHost();
        GetManager().GetComponent<NetworkDiscovery>().AdvertiseServer();
        StartCoroutine(StartHostCoroutine());

        NetworkServer.Spawn(Instantiate(onlineUtilities));
    }

    private IEnumerator StartHostCoroutine(){
        AsyncOperation loadSceneOperation = SceneManager.LoadSceneAsync("EmptyScene", LoadSceneMode.Additive);
        
        yield return new WaitUntil(()=>loadSceneOperation.isDone);

        roomPlayer = GetRoomPlayer();
        
        (roomPlayer as CustomNetworkRoomPlayer).CmdSetNewModelID(HostSelectedModelID);
        bool isDesktopEnabled = GameObject.FindObjectOfType<XRInitialization>().xrType == XRInitialization.XRType.Desktop;

        (roomPlayer as CustomNetworkRoomPlayer).CmdSetDesktop(isDesktopEnabled);
    }

    [Button(size: ButtonSizes.Gigantic)]
    public void StopHost(){
        // SceneManager.UnloadSceneAsync("EmptyScene");
        GetManager().StopHost();
        // NetworkManager.singleton.StopHost();
        // NetworkManager.singleton.StopServer();
    }

    // [Button(size: ButtonSizes.Gigantic)]
    private Coroutine WaitForServerUriCoroutine;
    public void StartClient()
    {
        discoveredServers.Clear();

        // Avoid null and only startClient when condition is ready
        if(WaitForServerUriCoroutine != null){
            StopCoroutine(WaitForServerUriCoroutine);
        }
        WaitForServerUriCoroutine = StartCoroutine(WaitForServerUri());

        // if (serverUri!=null && serverUri.Count>0){
        //     NetworkManager.singleton.StartClient(serverUri.First.Value);
        // }

        // GetManager().GetComponent<NetworkDiscovery>().StartDiscovery();
    }

    private IEnumerator WaitForServerUri(){
        yield return new WaitUntil(()=> serverUri!=null && serverUri.Count>0);
        
        NetworkManager.singleton.StartClient(serverUri.First.Value);
    }

    // [Button(size: ButtonSizes.Gigantic)]
    public void ReadyClient()
    {
        roomPlayer = GetRoomPlayer();
        // if (roomPlayer == null) 
        //     // roomPlayer = Object.FindObjectOfType<NetworkRoomPlayer>();
        //     foreach (NetworkRoomPlayer player in (NetworkRoomPlayer[])Object.FindObjectsOfType(typeof(NetworkRoomPlayer)))
        //         if (player.hasAuthority)
        //         {
        //             roomPlayer = player;
        //             break;
        //         }

        if(roomPlayer == null){
            return;
        }

        roomPlayer.CmdChangeReadyState(!roomPlayer.readyToBegin);

        if (roomPlayer.readyToBegin)
            text.text = "Listo";
        else
            text.text = "No listo";
    }


    public void SetNewModelID(int id){
        HostSelectedModelID = id;
    }


    [Button(size: ButtonSizes.Gigantic)]
    public void CancelConnection()
    {
        GetManager().StopClient();
    }
    public void StopServer()
    {
        GetManager().StopServer();
    }


    public void OnDiscoveredServer(ServerResponse info)
    {
        // Note that you can check the versioning to decide if you can connect to the server or not using this method
        // discoveredServers[info.serverId] = info;


        // foreach (ServerResponse info in discoveredServers.Values)
        //  if (GUILayout.Button(info.EndPoint.Address.ToString()))
        // {
        //   Connect(info);
        // break; // Por ahora nos conectamos con el primero
        //}
        Connect(info);
    }

    void Connect(ServerResponse info)
    {
        // GetManager().GetComponent<NetworkDiscovery>().StopDiscovery();
        serverUri.AddFirst(info.uri);
    }

    // [Button(size: ButtonSizes.Gigantic)]
    public void XR_SetAsDesktop(){
        XRInitialization initPlayer = GameObject.FindObjectOfType<XRInitialization>();
        
        if(initPlayer!= null)
            initPlayer.ForceDesktop();
    }

}
