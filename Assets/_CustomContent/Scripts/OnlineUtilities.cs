using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZAnatomy;
using static AnalyticsManager;

public class OnlineUtilities : NetworkBehaviour
{
    private const int GRAB_STATE = 1;
    private const int XR_STATE = 2;
    private static OnlineUtilities onlineUtilities;
    public List<MeshInteractableOnline> meshesOnline;

    public void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        onlineUtilities = this;
    }

    public bool pruebas;
    public void Update()
    {
        if (pruebas)
        {
            SendP();
            Debug.Log("Server");
        }
    }

    [Command(requiresAuthority = false)]
    private void SendP()
    {
        SetP();
    }

    [ClientRpc]
    private void SetP()
    {
        if (!pruebas)
            Debug.Log("Client");
    }


    #region MeshInteractableShare
    // Movement
    internal void ShareTransform(int ID)
    {

        if (meshesOnline.Count > ID)
            SendTransformRequestToServer(ID, meshesOnline[ID].transform.position, meshesOnline[ID].transform.rotation);
    }

    [Command(requiresAuthority = false)]
    private void SendTransformRequestToServer(int ID, Vector3 pos, Quaternion rot)
    {
        if (meshesOnline.Count > ID)
            SetTransformClient(ID, pos, rot);
    }


    [ClientRpc]
    private void SetTransformClient(int ID, Vector3 pos, Quaternion rot)
    {
        if (meshesOnline.Count > ID && !meshesOnline[ID].isGrabbedLocal)
        {
            meshesOnline[ID].transform.position = pos;
            meshesOnline[ID].transform.rotation = rot;
        }
    }

    internal void SetGrabState(int ID, bool isGrabbed, int playerID)
    {
        SendGrabToServer(ID, isGrabbed, playerID);
    }


    [Command(requiresAuthority = false)]
    private void SendGrabToServer(int ID, bool isGrabbed, int playerID)
    {
        SetGrabInClient(ID, isGrabbed, playerID);
    }

    [ClientRpc]
    private void SetGrabInClient(int ID, bool isGrabbed, int playerID)
    {
        if (meshesOnline.Count > ID && playerID!= UserInitializerController.Instance.GetMainUser().PlayerID)
        {
            meshesOnline[ID].isGrabbed = isGrabbed;
            if (isGrabbed)
                meshesOnline[ID].GetComponent<MeshInteractiveController>().MorgueIsInPlace = false;
        }
    }

    internal void MeshInteractiveSetPosition(int iD, int playerID, bool setInPlace, bool backToLastPos = false)
    {
        SendMeshInteractiveSetPositionToServer(iD, playerID, setInPlace, backToLastPos);
    }

    [Command(requiresAuthority = false)]
    private void SendMeshInteractiveSetPositionToServer(int iD, int playerID, bool setInPlace, bool backToLastPos)
    {
        SetMeshInteractiveSetPositionInClient(iD, playerID, setInPlace, backToLastPos);
    }

    [ClientRpc]
    private void SetMeshInteractiveSetPositionInClient(int iD, int playerID, bool setInPlace, bool backToLastPos)
    {
        if (meshesOnline.Count > iD && playerID!=UserInitializerController.Instance.GetMainUser().PlayerID)
        {
            if (setInPlace)
            {
                Utils.MeshInteractiveMorgueSetOnPositionByLeanTween(meshesOnline[iD].GetComponent<MeshInteractiveController>(), true);
            }
            else
            {
                // Utils.MeshInteractiveMorgueSetOnPositionByLeanTween(meshesOnline[iD].GetComponent<MeshInteractiveController>(), false, backToLastPos);
            }
        }
    }

    #endregion

    internal void C_TutorialInteractionChangeObjectState(int userDataID, int index)
    {
        C_TutorialInteractionChangeObjectState_ToServer(userDataID, index);
    }
    [Command(requiresAuthority = false)]
    private void C_TutorialInteractionChangeObjectState_ToServer(int userDataID, int index)
    {
        C_TutorialInteractionChangeObjectState_Client(userDataID, index);
    }

    [ClientRpc]
    private void C_TutorialInteractionChangeObjectState_Client(int userDataID, int index)
    {
        (AnalyticsManager.GetInstance().GetCurrentChallenge() as C_TutorialInteraction).ChangeIDColors(userDataID, index);
    }

    internal void SendObjectColor(int ID, Color color, bool paintColor = true)
    {
        SendObjectColorToServer(ID, color, paintColor);
    }

    [Command(requiresAuthority = false)]
    private void SendObjectColorToServer(int ID, Color color, bool paintColor)
    {
        SendObjectColorInClient(ID, color, paintColor);
    }

    [ClientRpc]
    private void SendObjectColorInClient(int ID, Color color, bool paintColor)
    {
        if (!paintColor)
        {
            meshesOnline[ID].GetComponent<MeshInteractiveController>().SetNotSelected(true);
        }        
    }


    internal void SendHandFeedBackState(CustomPhysicsHand physicsHand, FeedBackUtilities.ActionState colorState)
    {
        SendHandFeedBackStateToServer(UserInitializerController.Instance.GetMainUser().PlayerID, physicsHand.handType, colorState);
    }

    [Command(requiresAuthority = false)]
    private void SendHandFeedBackStateToServer(int iD, CustomPhysicsHand.HandType handType, FeedBackUtilities.ActionState colorState)
    { 
        SetHandFeedBackStateInClient(iD, handType, colorState);
    }

    [ClientRpc]
    private void SetHandFeedBackStateInClient(int iD, CustomPhysicsHand.HandType handType, FeedBackUtilities.ActionState colorState)
    {
        if (iD != UserInitializerController.Instance.GetMainUser().PlayerID)
        {
            XRInitialization player = UserInitializerController.Instance.GetPlayerData(iD).XRObject;

            foreach (CustomPhysicsHand hand in player.GetComponentsInChildren<CustomPhysicsHand>(true))
            {
                if (hand.gameObject.activeSelf && handType == hand.handType)
                    FeedBackUtilities.SetHandColor(hand, colorState, iD);
            }
        }
    }

    internal void SetInformationInPanelState(bool show, int unitID, int challengeID, int statIndex, DataStructureToJson data)
    {
        SetInformationInPanelStateToServer(show, unitID, challengeID, statIndex, data);
    }

    [Command(requiresAuthority = false)]
    private void SetInformationInPanelStateToServer(bool show, int unitID, int challengeID, int statIndex, DataStructureToJson data)
    {
        SetInformationInPanelStateInClient(show, unitID, challengeID, statIndex, data);
        AnalyticsManager.GetInstance().SetInformationPanelState(show, unitID, challengeID, statIndex);
    }

    [ClientRpc]
    private void SetInformationInPanelStateInClient(bool show, int unitID, int challengeID, int statIndex, DataStructureToJson data)
    {
        if (!NetworkClient.isHostClient)
        {
            Challenge.lastChallengeIndexData = statIndex;
            if (data != null) AnalyticsManager.GetInstance().SetDataFromServer(data);
            AnalyticsManager.GetInstance().SetInformationPanelState(show, unitID, challengeID, statIndex);
        }
    }

    #region Event - Reset MeshInteractive
    public void ResetMeshInteractive_Event(int ID)
        {
            ResetMeshInteractive_SendToServer(ID);
        }

        [Command(requiresAuthority = false)]
        private void ResetMeshInteractive_SendToServer(int ID)
        {
            ResetMeshInteractive_SetInClient(ID);
        }

        [ClientRpc]
        private void ResetMeshInteractive_SetInClient(int ID)
        {
            Utils.MeshInteractiveMorgueSetOnPositionByLeanTween(meshesOnline[ID].GetComponent<MeshInteractiveController>(), true);
        }
    #endregion

    #region Event - Rotate MeshInteractive
        public void RotateMeshInteractive_Event(int ID, Vector3 position, Vector3 eulerAngles)
        {
            RotateMeshInteractive_SendToServer(ID,position, eulerAngles);
        }

        [Command(requiresAuthority = false)]
        private void RotateMeshInteractive_SendToServer(int ID, Vector3 position,  Vector3 eulerAngles)
        {
            RotateMeshInteractive_SetInClient(ID,position, eulerAngles);
        }

        [ClientRpc]
        private void RotateMeshInteractive_SetInClient(int ID, Vector3 position, Vector3 eulerAngles)
        {
            meshesOnline[ID].transform.eulerAngles = eulerAngles;
            meshesOnline[ID].transform.position = position;
        }
    #endregion
    


    #region Event - Height of Players
    // HEIGHT OF PLAYERS
    public void HeightEvent(float heightMove)
        {
            SendHeightToServer(heightMove);
        }

        [Command(requiresAuthority = false)]
        private void SendHeightToServer(float heightMove)
        {
            SetHeightInClient(heightMove);
        }

        [ClientRpc]
        private void SetHeightInClient(float heightMove)
        {
            Transform offset = UserInitializerController.Instance.GetMainUser()?.OffsetTransform;// user.transform.GetChild(0);
            
            if (offset!=null)
            {
                LeanTween.cancel(offset.gameObject);
                LeanTween.move(offset.gameObject, offset.transform.position + Vector3.up * heightMove, 1).setEaseOutCirc();
            }
        }

    #endregion

    #region Event - Move dissection table
    // MOVE TABLE EVENT
    public void TableMove_Event(Vector2 newPosition)
        {
            TableMove_SendToServer(newPosition);
        }

        [Command(requiresAuthority = false)]
        private void TableMove_SendToServer(Vector2 newPosition)
        {
            TableMove_SetInClient(newPosition);
        }

        [ClientRpc]
        private void TableMove_SetInClient(Vector2 newPosition)
        {
            List<Vector3> meshLastPositions = new List<Vector3>();
            MorgueGrabUtilities.morgue.GetZAnatomyController().IterateDataListWithFunction((DataList.DataModel data)=>{
                meshLastPositions.Add(data.Model.transform.position);
            });

            Vector3 startTablePosition = UserInitializerController.Instance.GetTableStartPosition();
            UserInitializerController.Instance.SetTablePosition(newPosition);
             
            Vector3 newTablePosition = UserInitializerController.Instance.DissectionTable.transform.position;//UserInitializerController.Instance.SetTablePosition(newPosition);
            Vector3 delta = newTablePosition - startTablePosition;
            
            // Debug.Log("TableMove_SetInClient - Delta: "+delta + " start: "+startTablePosition + " new: "+newTablePosition);
            int iterator = 0;
            MorgueGrabUtilities.morgue.GetZAnatomyController().IterateDataListWithFunction((DataList.DataModel data)=>{
                data.Model.UpdateMorgueOriginalPosition(delta);// Vector3.Distance(UserInitializerController.Instance.DissectionTable.transform.position,oldPosition));
                if(!data.Model.MorgueIsInPlace){
                    data.Model.transform.position = meshLastPositions[iterator];
                }
                iterator++;
            });

            // float oldPosition = UserInitializerController.Instance.DissectionTable.transform.position.x;
            // // float newGlobalPositionX = UserInitializerController.Instance.SetTablePosition(newPosition).x; 
            // float delta = -(newGlobalPositionX - oldPosition);
            // Debug.Log("TableMove_SetInClient - Delta: "+delta + " old: "+oldPosition + " newGlobal: "+newGlobalPositionX);
            // // float newGlobalPositionX = UserInitializerController.Instance.DissectionTable.transform.parent.TransformPoint(newPosition);
            // // Vector3 deltaVector = UserInitializerController.Instance.DissectionTable.transform.position - oldPosition;
            // MorgueGrabUtilities.morgue.GetZAnatomyController().IterateDataListWithFunction((DataList.DataModel data)=>{
            //     data.Model.UpdateMorgueOriginalPosition(delta);// Vector3.Distance(UserInitializerController.Instance.DissectionTable.transform.position,oldPosition));
            // });
        }
    #endregion

    #region Enable/Disable Local Render Texture
        public void EnableLocalRenderTexture_Event(bool enable, int playerUserData){
            EnableLocalRenderTexture_SendToServer(enable, playerUserData);
        }

        [Command(requiresAuthority = false)]
        private void EnableLocalRenderTexture_SendToServer(bool enable, int playerUserData)
        {
            EnableLocalRenderTexture_SetInClient(enable,playerUserData);
        }

        [ClientRpc]
        private void EnableLocalRenderTexture_SetInClient(bool enable, int playerUserData)
        {
            UserInitializerController.Instance.EnableLocalRenderTexture(enable, playerUserData);
        }
    #endregion

    #region Event - Sync Anatomic Systems
        // Switch organ toggle type
        public void EnableOrganType_Event(int idOrganType,bool enable){
            EnableOrganType_SendToServer(idOrganType, enable);
        }

        [Command(requiresAuthority = false)]
        private void EnableOrganType_SendToServer(int idOrganType,bool enable)
        {
            EnableOrganType_SetInClient(idOrganType,enable);
        }

        [ClientRpc]
        private void EnableOrganType_SetInClient(int idOrganType,bool enable)
        {
            GameObject.Find("_MorgueController").GetComponent<MorgueController>().OnToggleOrganType((ZAnatomy.ZAnatomyController.OrganType)idOrganType, enable);
        }

    #endregion

    #region Challenges

    // Set active one type of organ
    public void SetOrganType_Event(int idOrganType)
    {
        SetOrganType_SendToServer(idOrganType);
    }

    [Command(requiresAuthority = false)]
    private void SetOrganType_SendToServer(int idOrganType)
    {
        SetOrganType_SetInClient(idOrganType);
    }

    [ClientRpc]
    private void SetOrganType_SetInClient(int idOrganType)
    {
        GameObject.Find("_MorgueController").GetComponent<MorgueController>().OnSetOrganType((ZAnatomy.ZAnatomyController.OrganType)idOrganType);
        AnalyticsManager.GetInstance().GetCurrentChallenge().NextFunctionStep();
    }

    internal void SetChallenge_Event(int challengeID)
    {
        SetChallenge_SendToServer(challengeID);
    }

    [Command(requiresAuthority = false)]
    private void SetChallenge_SendToServer(int challengeID)
    {
        SetChallenge_SetInClient(challengeID);
    }

    [ClientRpc]
    private void SetChallenge_SetInClient(int challengeID)
    {
        Debug.Log("Setting challenge id: "+challengeID);

        if (challengeID==-1)
        {
            
            if (AnalyticsManager.GetInstance().GetCurrentChallenge() != null)
            {
                AnalyticsManager.GetInstance().GetCurrentChallenge().End();
                // AnalyticsManager.GetInstance().SetCurrentChallenge(null);
            }

            
            AnalyticsManager.GetInstance().ShowChallengeUI(false);
            GameObject.Find("_MorgueController").GetComponent<MorgueController>().ReloadLastSelectedOrganTypes();
        } else
        {
            GameObject.Find("_MorgueController").GetComponent<MorgueController>().SaveLastSelectedOrganTypes();
            AnalyticsManager.GetInstance().SetCurrentChallenge(AnalyticsManager.GetInstance().transform.GetChild(challengeID).GetComponent<Challenge>());
            AnalyticsManager.GetInstance().GetCurrentChallenge().Init();

            //AnalyticsManager.GetInstance().ShowChallengeUI(true, AnalyticsManager.GetInstance().GetCurrentChallenge().ChallengeData.Description);
            //AnalyticsManager.GetInstance().ShowChallengeUI(true, AnalyticsManager.GetInstance().currentChallenge.ChallengeData.Description);
            AnalyticsManager.GetInstance().ShowChallengeUI(true, AnalyticsManager.GetInstance().GetCurrentChallenge().ChallengeData);
        }        
    }
    #endregion

    #region Event - Reset morgue
    // Reset Morgue
    internal void ResetMorgue()
        {
            SendResetMorgueToServer();
        }

        [Command(requiresAuthority = false)]
        private void SendResetMorgueToServer()
        {
            SetResetMorgueInClient();
        }

        [ClientRpc]
        private void SetResetMorgueInClient()
        {
            // PanelInMorgueController.Instance.ResetDissection();

            GameObject.Find("_MorgueController").GetComponent<MorgueController>().OnClickResetMorgue();
        }
    #endregion

    #region Tablet Configuration
        internal void MorgueColor(bool state)
        {
            Debug.Log("Color Request 2");
            SendMorgueColorToServer(state);
        }

        [Command(requiresAuthority = false)]
        private void SendMorgueColorToServer(bool state)
        {
            Debug.Log("Color Request3");
            SetMorgueColorInClient(state);
        }

        [ClientRpc]
        private void SetMorgueColorInClient(bool state)
        {
            FeedBackUtilities.ShowColorFeedback = state;
            if (!state)
                foreach (VRGrabbing grabbing in UserInitializerController.Instance.GetMainUser().GetComponentsInChildren<VRGrabbing>())
                {
                    if (grabbing.GetGrabbedObject() != null)
                        UserInitializerController.Instance.GetMainUser().currentStation.DeselectLastPointed(grabbing.GetGrabbedObject(), null);
                }
        }
        internal void MorgueInformation(bool state)
        {
            SendMorgueInformationToServer(state);
        }

        [Command(requiresAuthority = false)]
        private void SendMorgueInformationToServer(bool state)
        {
            SetMorgueInformationInClient(state);
        }

        [ClientRpc]
        private void SetMorgueInformationInClient(bool state)
        {
            if (UserInitializerController.Instance == null)
            {
                return;
            }

            VRFloatingNameModel.FloatingInfo = state;
            foreach (VRFloatingNameModel panel in UserInitializerController.Instance.GetMainUser().GetComponentsInChildren<VRFloatingNameModel>())
            {
                // panel.Show(enabled);
                panel.gameObject.SetActive(state);
            }
        }
    #endregion

    #region Event - Remove Player from clients
        // Send Desktop Player XRType
        public void RemovePlayer_Event(int uniqueId){
            RemovePlayer_SendToServer(uniqueId);
        } 

        [Command(requiresAuthority = false)]
        private void RemovePlayer_SendToServer(int uniqueId)
        {
            RemovePlayer_SetInClient(uniqueId);
        }

        [ClientRpc]
        private void RemovePlayer_SetInClient(int uniqueId)
        {
            UserInitializerController.Instance.RemovePlayerOnClients(uniqueId);
        }
    #endregion

    #region Event - Send Desktop Player XRType
        // Send Desktop Player XRType
        
        public void ChangePlayerXRType_Event(int playerID, int xrTypeInt){
            ChangePlayerXRType_SendToServer(playerID, xrTypeInt);
        } 

        [Command(requiresAuthority = false)]
        private void ChangePlayerXRType_SendToServer(int playerID, int xrTypeInt)
        {
            ChangePlayerXRType_SetInClient(playerID, xrTypeInt);
        }

        [ClientRpc]
        private void ChangePlayerXRType_SetInClient(int playerID, int xrTypeInt)
        {
            if(xrTypeInt == (int)XRInitialization.XRType.Desktop){
                // UserInitializerController.Instance.IsDesktopInGame = true;
                UserInitializerController.UserData playerData = UserInitializerController.Instance.GetPlayerData(playerID); 
                
                if(playerData == null){
                    XRInitialization xrDesktop = UserInitializerController.Instance.GetDesktopUser();// GetXRUserPerID(playerID);

                    if(xrDesktop != null){
                        xrDesktop.xrType = XRInitialization.XRType.Desktop;
                        xrDesktop.Init();
                    }else{
                        Debug.LogError("Cannot find playerData with PlayerID: "+playerID);
                    }                    
                }else{
                    playerData.XRObject.xrType = XRInitialization.XRType.Desktop;
                    playerData.XRObject.Init();

                    Debug.Log("ChangePlayerXRType_SetInClient for player "+playerID+ " to xrType "+(XRInitialization.XRType)xrTypeInt);
                }
            }
        }
    #endregion

    #region Users Analytics

    internal void SetCurrentShownUser(int userID)
    {
        SetCurrentShownUserServer(userID);
    }

    [Command(requiresAuthority = false)]
    public void SetCurrentShownUserServer(int userID)
    {
        SetCurrentShownUserClient(userID);
    }

    [ClientRpc(includeOwner = false)]
    private void SetCurrentShownUserClient(int userID)
    {
        AnalyticsManager.GetInstance().SetCurrentShownUser(userID);
    }

    public void SendLocalAnalytics(UserAnalytics myAnalytics)
    {
        SendLocalAnalytics_ToServer(myAnalytics);
    }

    [Command(requiresAuthority = false)]
    private void SendLocalAnalytics_ToServer(UserAnalytics myAnalytics)
    {
        AnalyticsManager.GetInstance().UpdateAnalytic(myAnalytics);
    }

    public void SendListAnalytics(List<UserAnalytics> analyticList)
    {
        SendListAnalytics_ToClient(analyticList);
    }

    [ClientRpc]
    private void SendListAnalytics_ToClient(List<UserAnalytics> analyticList)
    {
        AnalyticsManager.GetInstance().UpdateAnalyticList(analyticList);
    }

    #endregion
    public static OnlineUtilities GetOnlineUtilities()
    {
        return onlineUtilities;
    }

    [Command(requiresAuthority = false)]
    public void SetPlayerStartPosition(int playerID, int idPosition)
    {
        MorgueController.ReferencePlayerPosition position = MorgueGrabUtilities.morgue.ReferencePositions[idPosition];
        position.OcuppedByPlayerID = playerID;
        position.Station = idPosition == -1 ? MorgueGrabUtilities.morgue.reorder : MorgueGrabUtilities.morgue.morgue;
        SendPlayerStartPosition(playerID, idPosition);
    }

    [ClientRpc(includeOwner = false)]
    private void SendPlayerStartPosition(int playerID, int idPosition)
    {
        MorgueController.ReferencePlayerPosition position = MorgueGrabUtilities.morgue.ReferencePositions[idPosition];
        position.OcuppedByPlayerID = playerID;
        position.Station = idPosition == -1 ? MorgueGrabUtilities.morgue.reorder : MorgueGrabUtilities.morgue.morgue;
    }

    [Command(requiresAuthority = false)]
    public void SetPlayerColor(XRInitialization xRInitialization, Color newColor)
    {
        xRInitialization.PlayerColor = newColor;
        SendPlayerColor(xRInitialization, newColor);
        xRInitialization.UpdatePlayerColor();
    }

    [ClientRpc(includeOwner = false)]
    private void SendPlayerColor(XRInitialization xRInitialization, Color newColor)
    {
        xRInitialization.PlayerColor = newColor;
        xRInitialization.UpdatePlayerColor();
    }


    [Command(requiresAuthority = false)]
    public void SetPlayerID(XRInitialization xRInitialization, int ID)
    {
        xRInitialization.PlayerID = ID;
        SendPlayerID(xRInitialization, ID);
        xRInitialization.Init();

    }

    [ClientRpc(includeOwner = false)]
    private void SendPlayerID(XRInitialization xRInitialization, int ID)
    {
        xRInitialization.PlayerID = ID;
        xRInitialization.Init();
    }

    internal void SetXRtypeState(int playerID, XRInitialization.XRType type)
    {
        Debug.Log("Send  : " + playerID + "  :  " + type);
        SetXRtypeStateToServer(playerID, type);
    }
    [Command(requiresAuthority = false)]
    internal void SetXRtypeStateToServer(int playerID, XRInitialization.XRType type)
    {
        SetXRtypeInClient(playerID, type);
    }

    [ClientRpc]
    private void SetXRtypeInClient(int playerID, XRInitialization.XRType type)
    {
        if (playerID != UserInitializerController.Instance.GetMainUser().PlayerID)
        {
            XRInitialization xr = UserInitializerController.Instance.GetPlayerData(playerID).XRObject;
            xr.xrType = type;
            xr.Init();
            Debug.Log("Modify  : " + playerID + "  :  " + type); 
        }
    }

    #region Event - UpdateOnlineData
        // Send Desktop Player XRType
        
        public void InitOnlineData_Event(){
            InitOnlineData_SendToServer();
        } 

        [Command(requiresAuthority = false)]
        private void InitOnlineData_SendToServer()
        {
            InitOnlineData_SetInClient();
        }

        [ClientRpc]
        private void InitOnlineData_SetInClient()
        {
            
            Debug.Log("InitOnlineData_SetInClient");
            XRInitialization[] xrPlayers = FindObjectsOfType<XRInitialization>(true);

            foreach(XRInitialization player in xrPlayers){
                player.OnAllPlayersConnected();
            }
        }

        int countZAnatomyLoaded=0;
        public void InitZAnatomyState_Event(){
            InitZAnatomyState_SendToServer();
        } 

        [Command(requiresAuthority = false)]
        private void InitZAnatomyState_SendToServer()
        {
            countZAnatomyLoaded++;
            InitZAnatomyState_SetInClient(countZAnatomyLoaded == FindObjectOfType<CustomNetworkRoomManager>().GetPlayersInGameCount());
        }

        [ClientRpc]
        private void InitZAnatomyState_SetInClient(bool init)
        {
            if (init) {
                XRInitialization[] xrPlayers = FindObjectsOfType<XRInitialization>(true);

                foreach(XRInitialization player in xrPlayers){
                    player.OnAllPlayerLoadedZAnatomy();
                }
            }
        }

        internal void SendUserNames()
        {
            List<UserInitializerController.UserData> users = UserInitializerController.Instance.UsersData; 

            string[] names = new string[users.Count];
            string[] surnames = new string[users.Count];

            for (int i = 0; i<users.Count; i++) {
                names[i] = users[i].UserName;
                surnames[i] = users[i].UserSurname;
            }

            SendUserNames_SendToServer(names, surnames);
        }

        [Command(requiresAuthority = false)]
        private void SendUserNames_SendToServer(string[] names, string[] surnames)
        {
            SendUserNames_SetInClient(names, surnames);
        }

        [ClientRpc]
        private void SendUserNames_SetInClient(string[] names, string[] surnames)
        {
            List<UserInitializerController.UserData> users = UserInitializerController.Instance.UsersData; 

            for (int i = 0; i<users.Count; i++) {
                users[i].UserName = names[i];
                users[i].UserSurname = surnames[i];
            }
        }
    #endregion
}
