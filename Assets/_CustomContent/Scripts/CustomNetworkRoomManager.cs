using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using System;

public class CustomNetworkRoomManager : NetworkRoomManager
{
        private ApplicationInitializer m_AppInitializer;

        public int SelectedModelID=-1;
        public bool IsDesktopEnabled = false;
        public XRInitialization.XRType LocalSelectedXRType;
        private int m_PlayersInGame;
        private int m_PlayersConnectedToSceneCount;

        public int GetPlayersInGameCount(){
            return m_PlayersConnectedToSceneCount;
        }

        // public static System.Action OnClientDisconnection;
        private string SelectedModelPath="";

        // In case of scene reload, we have to get the reference to the InitScene ApplicationInitializer
        private ApplicationInitializer AppInitializer(){
            if(m_AppInitializer == null){
                m_AppInitializer = GameObject.FindObjectOfType<ApplicationInitializer>(); 
            }

            return m_AppInitializer;
        }

        public void AfterAllPlayersLoadedDissection(){
            GameObject.FindObjectOfType<MorgueController>().Init();
        }

        public string GetSelectedModelPath()
        {
            if (SelectedModelPath == null || SelectedModelPath.Length == 0) SetSelectedModel(0);
            return SelectedModelPath;
        }
        public void SetSelectedModel(int id){
            SelectedModelID = id;
/*        Debug.Log("#################");
        Debug.Log(id);
        Debug.Log(AppInitializer());
        Debug.Log(AppInitializer().CanvasController);
        Debug.Log(AppInitializer().CanvasController.ResourcesModelIDs);
        Debug.Log(AppInitializer().CanvasController.ResourcesModelIDs[id]);*/
        InitSceneCanvasController.ResourcesModelData data = AppInitializer().CanvasController.ResourcesModelIDs[id];

            // AppInitializer.CanvasController.UpdateRoomModelData();
            if(AppInitializer())AppInitializer().CanvasController.UpdateTitlesNameModel(data.Name);
            SelectedModelPath = data.Path;
        }

        public void SetDesktopMode(bool desktop){
            IsDesktopEnabled = desktop;            
        }

        public override void Start() {
            base.Start();
            CustomNetworkRoomPlayer.OnReadyStateChanged += OnChangedReadyState;
            InitSceneUserReadyIcon.OnChangeAdminTablet += OnChangeAdminTablet;
            InitSceneUserReadyIcon.OnChangeUserAvatar += OnChangeUserAvatar;
            
        }

        public override void OnDestroy() {
            base.OnDestroy();
            CustomNetworkRoomPlayer.OnReadyStateChanged -= OnChangedReadyState;
            InitSceneUserReadyIcon.OnChangeAdminTablet -= OnChangeAdminTablet;
            InitSceneUserReadyIcon.OnChangeUserAvatar -= OnChangeUserAvatar;
        }

        private void OnChangedReadyState(int netId, bool newState){
            ApplicationInitializer appInitializer = AppInitializer();
            if(appInitializer != null){
                appInitializer.CanvasController.SetReadyUser(netId, newState); 
            }
        }


        private void OnChangeAdminTablet(int netId, bool adminTabletEnabled){
            Debug.Log("ChangeAdminTablet netId "+netId+ " to "+adminTabletEnabled);
            Debug.Log("RoomSlots: "+ roomSlots.Count);
            NetworkRoomPlayer player = roomSlots[netId];
            if(player != null){
                ((CustomNetworkRoomPlayer)player).isAdminTabletEnabled = adminTabletEnabled;
            }
        }

        private void OnChangeUserAvatar(int netId, int avatarId){
            
            Debug.Log("OnChangeUserAvatar netId "+netId+ " to "+avatarId);
            Debug.Log("RoomSlots: "+ roomSlots.Count);
            NetworkRoomPlayer player = roomSlots[netId];
            if(player != null){
                ((CustomNetworkRoomPlayer)player).avatarId = avatarId;
            }
        }


        #region room server virtuals

        /// <summary>
        /// This is called on the host when a host is started.
        /// </summary>
        public override void OnRoomStartHost() {
            Debug.Log("OnRoomStartHost");
        }

        /// <summary>
        /// This is called on the host when the host is stopped.
        /// </summary>
        public override void OnRoomStopHost() {
            Debug.Log("OnRoomStopHost - Client reloading from InitScene");
        }

        /// <summary>
        /// This is called on the server when the server is started - including when a host is started.
        /// </summary>
        public override void OnRoomStartServer() {
            Debug.Log("OnRoomStartServer");}

        /// <summary>
        /// This is called on the server when the server is started - including when a host is stopped.
        /// </summary>
        public override void OnRoomStopServer() {
            Debug.Log("OnRoomStopServer");}

        /// <summary>
        /// This is called on the server when a new client connects to the server.
        /// </summary>
        /// <param name="conn">The new connection.</param>
        public override void OnRoomServerConnect(NetworkConnectionToClient conn) {
            // Debug.Log("OnRoomServerConnect - New player "+conn.identity.netId);
            
        }

        /// <summary>
        /// This is called on the server when a client disconnects.
        /// </summary>
        /// <param name="conn">The connection that disconnected.</param>
        public override void OnRoomServerDisconnect(NetworkConnectionToClient conn) {
            Debug.Log("OnRoomServerDisconnect");    
            // NetworkRoomPlayer player = roomSlots.Find((x)=>x.connectionToClient == conn);
            if(AppInitializer())AppInitializer().CanvasController.RemovePlayer(conn);//player.netId); 
        }

        /// <summary>
        /// This is called on the server when a networked scene finishes loading.
        /// </summary>
        /// <param name="sceneName">Name of the new scene.</param>
        public override void OnRoomServerSceneChanged(string sceneName) {
            Debug.Log("OnRoomServerSceneChanged");
            
            //(AppInitializer.GetRoomPlayer() as CustomNetworkRoomPlayer).CmdInitMorgue();
        }

        /// <summary>
        /// This allows customization of the creation of the room-player object on the server.
        /// <para>By default the roomPlayerPrefab is used to create the room-player, but this function allows that behaviour to be customized.</para>
        /// </summary>
        /// <param name="conn">The connection the player object is for.</param>
        /// <returns>The new room-player object.</returns>
        public override GameObject OnRoomServerCreateRoomPlayer(NetworkConnectionToClient conn)
        {
            Debug.Log("OnRoomServerCreateRoomPlayer");
            int netId = roomSlots.Count;
            if(AppInitializer())AppInitializer().CanvasController.AddNewPlayer(conn, netId, 1, true,netId==0, false, false); 
            return null;
        }

        /// <summary>
        /// This allows customization of the creation of the GamePlayer object on the server.
        /// <para>By default the gamePlayerPrefab is used to create the game-player, but this function allows that behaviour to be customized. The object returned from the function will be used to replace the room-player on the connection.</para>
        /// </summary>
        /// <param name="conn">The connection the player object is for.</param>
        /// <param name="roomPlayer">The room player object for this connection.</param>
        /// <returns>A new GamePlayer object.</returns>
        public override GameObject OnRoomServerCreateGamePlayer(NetworkConnectionToClient conn, GameObject roomPlayer)
        {
            Debug.Log("OnRoomServerCreateGamePlayer");
            return null;
        }

        /// <summary>
        /// This allows customization of the creation of the GamePlayer object on the server.
        /// <para>This is only called for subsequent GamePlay scenes after the first one.</para>
        /// <para>See <see cref="OnRoomServerCreateGamePlayer(NetworkConnectionToClient, GameObject)">OnRoomServerCreateGamePlayer(NetworkConnection, GameObject)</see> to customize the player object for the initial GamePlay scene.</para>
        /// </summary>
        /// <param name="conn">The connection the player object is for.</param>
        public override void OnRoomServerAddPlayer(NetworkConnectionToClient conn)
        {
            base.OnServerAddPlayer(conn);
            Debug.Log("OnRoomServerAddPlayer");
        }

        // for users to apply settings from their room player object to their in-game player object
        /// <summary>
        /// This is called on the server when it is told that a client has finished switching from the room scene to a game player scene.
        /// <para>When switching from the room, the room-player is replaced with a game-player object. This callback function gives an opportunity to apply state from the room-player to the game-player object.</para>
        /// </summary>
        /// <param name="conn">The connection of the player</param>
        /// <param name="roomPlayer">The room player object.</param>
        /// <param name="gamePlayer">The game player object.</param>
        /// <returns>False to not allow this player to replace the room player.</returns>
        public override bool OnRoomServerSceneLoadedForPlayer(NetworkConnectionToClient conn, GameObject roomPlayer, GameObject gamePlayer)
        {
            XRInitialization xrInit = gamePlayer.GetComponent<XRInitialization>();
            CustomNetworkRoomPlayer RoomPlayer = roomPlayer.GetComponent<CustomNetworkRoomPlayer>();
            // bool isDesktopEnabled = (roomSlots.Find((x)=>x.index == 0) as CustomNetworkRoomPlayer).GetComponent.isDesktopEnabled;
            
            int playerId = RoomPlayer.index;
            
            // if(IsDesktopEnabled && playerId==0){
            //     UserInitializerController.Instance.DesktopXR = xrInit;
            // }

            if(IsDesktopEnabled && playerId > 0)
                playerId--;

            // xrInit.SetUserDataID(playerId);
            // xrInit.SetPlayerID(RoomPlayer.index);
            
            bool enableTablet = RoomPlayer.isAdminTabletEnabled;

            // ApplicationInitializer appInitializer = AppInitializer();
            // // if(appInitializer!=null){
            //     enableTablet = appInitializer.CanvasController.IsEnabledTabletForPlayer(conn);
            // // }

            string name = "User "+ (playerId+1);
            xrInit.SetData(playerId, RoomPlayer.index, RoomPlayer.avatarId,enableTablet,name);

            UserInitializerController.Instance.SetNetIDOnUserData(playerId, conn);

            
            Debug.Log("OnRoomServerSceneLoadedForPlayer ID: "+playerId);
            m_PlayersConnectedToSceneCount++;

            Debug.Log("Count of playeeers: " + m_PlayersConnectedToSceneCount);
            if(RoomPlayer.isServer){
                Debug.Log("m_PlayersInGame == "+m_PlayersInGame);
                Debug.Log("m_PlayersConnectedToSceneCount == "+m_PlayersConnectedToSceneCount);
                if(m_PlayersInGame == m_PlayersConnectedToSceneCount){
                    StartCoroutine(WaitToInitOnlineDataOnAllClients());
                    // OnlineUtilities.GetOnlineUtilities().InitOnlineData_Event();
                }
            }

            return true;
        }

        private IEnumerator WaitToInitOnlineDataOnAllClients(){
            yield return new WaitForSeconds(1);
            OnlineUtilities.GetOnlineUtilities().InitOnlineData_Event();
        }

        /// <summary>
        /// This is called on the server when all the players in the room are ready.
        /// <para>The default implementation of this function uses ServerChangeScene() to switch to the game player scene. By implementing this callback you can customize what happens when all the players in the room are ready, such as adding a countdown or a confirmation for a group leader.</para>
        /// </summary>
        public override void OnRoomServerPlayersReady()
        {
            Debug.Log("OnRoomServerPlayersReady");
            // all players are readyToBegin, start the game
            ServerChangeScene(GameplayScene);
        }

        /// <summary>
        /// This is called on the server when CheckReadyToBegin finds that players are not ready
        /// <para>May be called multiple times while not ready players are joining</para>
        /// </summary>
        public override void OnRoomServerPlayersNotReady() {
            Debug.Log("OnRoomServerPlayersNotReady");
            // AppInitializer.CanvasController.CheckReadyUsers(); 
        }

        #endregion

        #region room client virtuals

        /// <summary>
        /// This is a hook to allow custom behaviour when the game client enters the room.
        /// </summary>
        public override void OnRoomClientEnter() {
            Debug.Log("OnRoomClientEnter");
            UpdateClientIcons();
        }

        public void UpdateClientIcons(){
            ApplicationInitializer appInitializer = AppInitializer();
            if(appInitializer != null){
                appInitializer.CanvasController.ClearClientIcons();
                for (int i = 0; i < roomSlots.Count; i++)
                {
                    CustomNetworkRoomPlayer player = (CustomNetworkRoomPlayer)roomSlots[i];
                    appInitializer.CanvasController.AddNewPlayer(player.connectionToClient, i, player.avatarId, false,roomSlots[i].isLocalPlayer, roomSlots[i].readyToBegin, ((CustomNetworkRoomPlayer)player).isAdminTabletEnabled); 
                }
            }
        }

        /// <summary>
        /// This is a hook to allow custom behaviour when the game client exits the room.
        /// </summary>
        public override void OnRoomClientExit() {
            Debug.Log("OnRoomClientExit");
            UpdateClientIcons();
        }

        /// <summary>
        /// This is called on the client when it connects to server.
        /// </summary>
        public override void OnRoomClientConnect() {
            Debug.Log("OnRoomClientConnect");
            // AppInitializer.CanvasController.UpdateRoomModelData();
        }

        /// <summary>
        /// This is called on the client when disconnected from a server.
        /// </summary>
        public override void OnRoomClientDisconnect() {
            Debug.Log("OnRoomClientDisconnect");}

        /// <summary>
        /// This is called on the client when a client is started.
        /// </summary>
        public override void OnRoomStartClient() {
            Debug.Log("OnRoomStartClient");}

        /// <summary>
        /// This is called on the client when the client stops.
        /// </summary>
        public override void OnRoomStopClient() {
            Debug.Log("OnRoomStopClient");
            SceneManager.LoadScene("InitScene", LoadSceneMode.Single);
        }

        /// <summary>
        /// This is called on the client when the client is finished loading a new networked scene.
        /// </summary>
        public override void OnRoomClientSceneChanged() {
            Debug.Log("OnRoomClientSceneChanged");}

        /// <summary>
        /// Called on the client when adding a player to the room fails.
        /// <para>This could be because the room is full, or the connection is not allowed to have more players.</para>
        /// </summary>
        public override void OnRoomClientAddPlayerFailed() {
            Debug.Log("OnRoomClientAddPlayerFailed");}

    #endregion

    // public override void OnClientDisconnect(){
    //     base.OnClientDisconnect();

    //     Debug.Log("OnClientDisconnect");
    //     if(OnClientDisconnection!=null){
    //         OnClientDisconnection(/*NetworkClient.connection.connectionId*/);
    //     }
    // }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        Debug.Log("OnServerDisconnect: Client disconnected.");
        base.OnServerDisconnect(conn);
        // Application.Quit();
    }

    public override void OnClientDisconnect()
    {
        Debug.Log("OnClientDisconnect: Client disconnected.");
        base.OnClientDisconnect();
        Application.Quit();
    }

    public void SetPlayers(int playersInGame){
        m_PlayersConnectedToSceneCount = 0;
        m_PlayersInGame = playersInGame;
    }
    
    public void SetReadyHost(int playersInGame)
    {
        SetPlayers(playersInGame);
        OnRoomServerPlayersReady();
    }
    public override void ClientChangeScene(string newSceneName, SceneOperation sceneOperation = SceneOperation.Normal, bool customHandling = false)
    {
        base.ClientChangeScene(newSceneName, sceneOperation, customHandling);

        SetPlayers(GameObject.FindObjectOfType<InitSceneCanvasController>().InstantiatedUserReadyIcons.Count);

        if (NetworkClient.isLoadingScene)
        {
            ApplicationInitializer tmp = (ApplicationInitializer)GameObject.FindObjectOfType(typeof(ApplicationInitializer));

            Debug.Log("Client Change Scene:    " + tmp + ",  " + newSceneName);
            if (tmp != null)
            {
                tmp.xr.SetLoading(true);
                tmp.xr.GetComponentInChildren<DesktopPlayerController>().SetLoading(true);
            }
        }
    }
}
