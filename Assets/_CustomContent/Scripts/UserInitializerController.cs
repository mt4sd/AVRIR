using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserInitializerController : MonoBehaviour
{
    private static UserInitializerController _instance;

    public static UserInitializerController Instance { get { 
        
        if(_instance==null) 
        _instance = GameObject.FindObjectOfType<UserInitializerController>(true); 

        return _instance; 
    } }


    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }
    }

    [System.Serializable]
    public class UserData{
        public int UniqueID;
        public string ColorName;
        public string UserName;
        public string UserSurname;
        public Color Color;
        public RenderTexture RenderTexture;

        // Real time references
        public Camera PlayerCamera;
        public Transform Position;
        public XRInitialization XRObject;
        public bool IsConnected;
        public bool IsLocalPlayer;
        public NetworkConnectionToClient ConnectionID;

        public void Reset(){
            PlayerCamera = null;
            Position = null;
            XRObject = null;
            IsConnected = false;
            IsLocalPlayer = false;
            ConnectionID = null;
        }
    }

    public List<UserData> UsersData;
    public XRInitialization DesktopXR;

    public bool EditorUseVR;
    public GameObject UserPrefab;
    private XRInitialization mainUser;
    public XRInitialization[] listOfUsers;
    public static Action OnPlayersChanged;
    public GameObject DissectionTable;
    public LocalRenderTextureController LocalRenderTexture;
    // public bool IsDesktopInGame;
    private Vector3 m_DissectionTableStartPosition;
    public Vector2 DissectionTableMoveMinMax;
    private Vector3 m_DissectionTableStartLocalPos;
    private CustomNetworkRoomManager NetworkRoomManager;

    // Start is called before the first frame update
    void Start()
    {
        // XRInitialization user = Instantiate(userPrefab).GetComponent<XRInitialization>();
        // listOfUsers.Add(user);
        // mainUser = user;
        // user.
        // listOfUsers = (XRInitialization[]) Object.FindObjectsOfType(typeof(XRInitialization));
        
        if (!NetworkClient.isConnected)
        {
            XRInitialization user = Instantiate(UserPrefab).GetComponent<XRInitialization>();
            user.xrType = !EditorUseVR?XRInitialization.XRType.Desktop:XRInitialization.XRType.Controller;
            mainUser = user;
            listOfUsers = (XRInitialization[]) UnityEngine.Object.FindObjectsOfType(typeof(XRInitialization));
        }
        // mainUser = listOfUsers[0];
        m_DissectionTableStartLocalPos = DissectionTable.transform.localPosition;
        m_DissectionTableStartPosition = DissectionTable.transform.position;

        LocalRenderTexture.Hide();

        Mirror.NetworkServer.OnDisconnectedEvent += OnDisconnectedPlayer;
    }

    void OnDestroy()
    {        
        Mirror.NetworkServer.OnDisconnectedEvent -= OnDisconnectedPlayer;
    }

    private void UpdateListOfUsers(){

        if (listOfUsers == null || listOfUsers.Length == 0)
            listOfUsers = (XRInitialization[])UnityEngine.Object.FindObjectsOfType(typeof(XRInitialization));
    }

    public void IterateUsers(Action<UserInitializerController.UserData> iteratorFunction){
        
        foreach(UserInitializerController.UserData userData in UsersData){
            iteratorFunction(userData);
        }
    }

    // public XRInitialization GetXRUserPerID(int PlayerID){
    //     Debug.Log("Getting XR user per ID: "+PlayerID);
    //     return listOfUsers[PlayerID];
    // }
    private void OnDisconnectedPlayer(Mirror.NetworkConnectionToClient client){
        Debug.Log("OnDisconnectedPlayer "+client.clientOwnedObjects.Count);

        IterateUsers((UserData data)=>{
            if(data.ConnectionID != null && data.ConnectionID.connectionId == client.connectionId){
                // Remove position
                RemovePlayer(client);//.identity.GetComponent<XRInitialization>().UserDataID);
            }
        });

        XRInitialization mainUser = GetMainUser();

        if(mainUser != null && mainUser.DesktopPlayer != null){
        // if(DesktopXR != null) DesktopXR
            mainUser.DesktopPlayer.UpdateCameraPreviews();
        }
    }

    internal int GetNextUser(int userDataID, int directionOfObjectMovement)
    {
        int nextID = userDataID;
        while (true)
        {
            nextID = (nextID + directionOfObjectMovement) % (UsersData.Count - 1);
            if (nextID < 0) nextID = UsersData.Count - 2;
            if (UsersData[nextID].IsConnected)
                return nextID;
        }
    }

    public XRInitialization GetDesktopUser(){
        List<XRInitialization> players = new List<XRInitialization>(GameObject.FindObjectsOfType<XRInitialization>());

        foreach(XRInitialization player in players){
            if(!ExistsUserData(player)){
                return player;
            }            
        }

        return null;
    }

    private bool ExistsUserData(XRInitialization xrPlayer){
        bool exists = false;
        foreach(UserData data in UsersData){
            if(data.XRObject == xrPlayer){
                exists = true;
            }
        }
        return exists;
    }

    public UserData GetPlayerData(int playerID){
        UserData foundData = UsersData.Find((x)=> x.XRObject != null && x.XRObject.PlayerID == playerID);
        // Debug.Log("Getting PlayerData for player "+playerID); 
        // Debug.Log(foundData);

        return foundData;
    }

    public void UpdateHeight(float heightMove)
    {
        // UpdateListOfUsers();

        // foreach ( XRInitialization user in listOfUsers)
        {
            OnlineUtilities.GetOnlineUtilities().HeightEvent(heightMove);
            // Transform offset = GetMainUser().OffsetTransform;// user.transform.GetChild(0);

            // LeanTween.cancel(offset.gameObject);
            // LeanTween.move(offset.gameObject, offset.transform.position + Vector3.up*heightMove, 1).setEaseOutCirc();
        }
    }

    public void UpdateTablePosition(Vector2 distanceMove){
        float newPositionX = DissectionTable.transform.localPosition.x + distanceMove.x * 100;
        float newPositionY = DissectionTable.transform.localPosition.y + distanceMove.y * 100;
        // Debug.Log("New Table Z position: "+newPosition);

        if(distanceMove == Vector2.zero){
            newPositionX = 0;
            newPositionY = 0;
        }
        // newPosition = Mathf.Clamp(newPosition, DissectionTableMoveMinMax.x, DissectionTableMoveMinMax.y);
        // Debug.Log("---- After clamp: "+newPosition);

        OnlineUtilities.GetOnlineUtilities().TableMove_Event(new Vector2(newPositionX, newPositionY));
    }

    public void EnableLocalRenderTexture(bool enable, int playerUserDataID=-1){
        if(enable){
            if(GetMainUser().UserDataID != playerUserDataID && GetMainUser().xrType != XRInitialization.XRType.Desktop){
                LocalRenderTexture.InitData(UsersData.Find((x)=>x.XRObject.UserDataID == playerUserDataID));
            }
        }else{
            LocalRenderTexture.Hide();
        }
    }

    public void /*Vector3*/ SetTablePosition(Vector2 newPosition){        
        if (DissectionTable!=null)
        {
            Vector3 localPos = DissectionTable.transform.localPosition;
            if(newPosition == Vector2.zero){
                localPos = m_DissectionTableStartLocalPos;
            }else{
                localPos = newPosition;
            }

            DissectionTable.transform.localPosition = localPos;
            // LeanTween.cancel(DissectionTable);
            // LeanTween.moveLocal(DissectionTable, localPos, 1).setEaseOutCirc();
            // return DissectionTable.transform.parent.TransformPoint(localPos);
        }
        // return Vector3.zero;
    }

    public Vector3 GetTableStartPosition(){
        return m_DissectionTableStartPosition;
    }

    public XRInitialization GetMainUser()
    {
        if (mainUser == null)
        {
            UpdateListOfUsers();

            foreach (XRInitialization user in listOfUsers)
            {
                if (user.TryGetComponent(out NetworkIdentity identity))
                {
                    if (identity.hasAuthority) mainUser = user;
                }
            }
        }

        return mainUser;
    }

    public void RemovePlayerOnClients(int uniqueId){
        UserData userData = UsersData.Find((x)=>x.UniqueID == uniqueId);

        // SET ONLINE CALLS asd
        Debug.Log("Removing player from UserData Initializer - Is Connected should be false for player with uniqueId = "+uniqueId);
        userData.Reset();
        if(OnPlayersChanged != null){
            OnPlayersChanged();
        }
    }

    private void RemovePlayer(NetworkConnectionToClient connectionId){
        UserData userData = UsersData.Find((x)=>x.ConnectionID != null && x.ConnectionID.connectionId == connectionId.connectionId);// UsersData[connectionId];
        if(userData != null){
            Debug.Log("Removing player with connectionId = " + connectionId);

            if(GetMainUser() != null && GetMainUser().IsNetworkActive() ){
                // Only send event if host is still connected
                OnlineUtilities.GetOnlineUtilities().RemovePlayer_Event(userData.UniqueID);
            }
        }else{
            Debug.LogError("NOT FOUND - Cannot remove player with connectionID = "+connectionId);
        }
    }

    public void SetNetIDOnUserData(int userDataID, NetworkConnectionToClient connectionId){
        UserData userData = UsersData[userDataID];
        userData.ConnectionID = connectionId;
        // userData.UniqueID = userDataID;
    }

    public Color RegisterNewPlayer(XRInitialization newPlayer){
        // if(newPlayer.xrType == XRInitialization.XRType.Desktop)
        //     return Color.black;
        
        Color playerColor = Color.white;
        int playerId = newPlayer.UserDataID;
        // if(IsDesktopInGame){
        //     playerId--;
        // }

        if(playerId > UsersData.Count-1){
            Debug.LogError("There is no UserData for playerID: "+ playerId);
        }

        Debug.Log("Registering new player " + playerId);
        UserData userData = UsersData[playerId];
        userData.PlayerCamera = newPlayer.GetComponentInChildren<Camera>();

        userData.IsLocalPlayer = newPlayer.IsLocalPlayer();// GetMainUser()==newPlayer;

        if(!userData.IsLocalPlayer){
            userData.PlayerCamera.targetTexture = userData.RenderTexture;
        }
        userData.XRObject = newPlayer;
        userData.IsConnected = true;

        playerColor = userData.Color;
        
        if(OnPlayersChanged != null){
            OnPlayersChanged();
        }

        return playerColor;
    }


}
