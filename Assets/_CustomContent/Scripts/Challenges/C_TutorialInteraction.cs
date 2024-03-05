using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ZAnatomy;
using static UserAnalytics;

public class C_TutorialInteraction : Challenge
{
    private int[] CurrentState = { 0, 0 };
    private float[] timeState = { 0, 0 };

    private int maxNumOfObjectsPerColor = 2;
    public GameObject[] myMeshInteractableToGrab;
    public Vector3[] myMeshInteractableToGrabInitPosition;
    private bool[] moveObject;
    public GameObject[] myMeshInteractableToPlace;
    public int nextUserID;
    public bool[] objectsState;
    private FeedBackUtilities.ActionState lastActionState = FeedBackUtilities.ActionState.None;

    public int directionOfObjectMovement = -1;
    
    private ZAnatomy.ZAnatomyController.OrganType organType = ZAnatomy.ZAnatomyController.OrganType.Skeletal;

    private List<string> ComponentsToColor = new List<string>(){
        "Mandible-Unified(Clone)",
        "Frontal bone-Unified(Clone)",
        "Parietal bone.r-Unified(Clone)",
        "Parietal bone.l-Unified(Clone)",
        "Temporal bone.l-Unified(Clone)",
        "Maxilla.r-Unified(Clone)",
        "Temporal bone.r-Unified(Clone)",
        "Maxilla.l-Unified(Clone)"
    };

    private XRInitialization myUser;
    private VRGrabbing leftHand;
    private VRGrabbing rightHand;

    public void Start()
    {
        challengeID = 2;
    }

    public override void Init()
    {
        base.Init();
        for (int i = 0; i < CurrentState.Length; i++)
            CurrentState[i] = 0;
        for (int i = 0; i < timeState.Length; i++)
            timeState[i] = 0;
            
        myMeshInteractableToGrab = new GameObject[maxNumOfObjectsPerColor];
        myMeshInteractableToGrabInitPosition = new Vector3[maxNumOfObjectsPerColor];
        myMeshInteractableToPlace = new GameObject[maxNumOfObjectsPerColor];

        objectsState = new bool[maxNumOfObjectsPerColor];
        moveObject = new bool[maxNumOfObjectsPerColor];
        for (int i = 0; i < objectsState.Length; i++) {
            objectsState[i] = false;
            moveObject[i] = false;
        }

        stepFunctions.Add(new UnityEvent());
        stepFunctions[0].AddListener(InitInitialColors);

        FeedBackUtilities.ChangeObjectsColor = false; // syncro

        if (Mirror.NetworkClient.isHostClient)
            OnlineUtilities.GetOnlineUtilities().SetOrganType_Event((int) organType);

        myUser = UserInitializerController.Instance.GetMainUser();


        if (UserInitializerController.Instance.GetMainUser().xrType == XRInitialization.XRType.Desktop) return;
        leftHand = myUser.currentLeftHand.GetComponent<VRGrabbing>();
        rightHand = myUser.currentRightHand.GetComponent<VRGrabbing>();

        
    }

    public void InitInitialColors()
    {
        List<MeshInteractableOnline> meshes = OnlineUtilities.GetOnlineUtilities().meshesOnline;
        Snapshot.Save(meshes, UserInitializerController.Instance.DissectionTable.transform.position);

        int playerIndex = 0;
        int currentNumberOfObjects = 0;
        // for (int i = 0; i < meshes.Count; i++)
        for (int j = 0; j<ComponentsToColor.Count && playerIndex <= UserInitializerController.Instance.UsersData.Count - 2; j++)
            for (int i = 0; i < meshes.Count; i++)
            {
                MeshInteractiveController mesh = meshes[i].GetComponent<MeshInteractiveController>();

                // Relocate on original position
                // Utils.MeshInteractiveMorgueSetOnPositionByLeanTween(mesh, true);
                mesh.MorgueSetOnPosition(true);

                if (mesh.BodyPart == organType)
                {
                    if (ComponentsToColor[j].Equals(meshes[i].name))
                    {
                        if (currentNumberOfObjects >= maxNumOfObjectsPerColor)
                        {
                            currentNumberOfObjects = 0;
                            playerIndex++;
                            if (playerIndex >= UserInitializerController.Instance.UsersData.Count - 2) break;
                        }

                        if (UserInitializerController.Instance.UsersData[playerIndex].IsLocalPlayer)
                        {
                            myMeshInteractableToGrab[currentNumberOfObjects] = meshes[i].gameObject;
                            myMeshInteractableToGrabInitPosition[currentNumberOfObjects] = meshes[i].transform.position;
                        }
                        if (UserInitializerController.Instance.UsersData[playerIndex].IsConnected)
                            foreach (Material material in meshes[i].GetComponent<Renderer>().materials)
                                material.color = UserInitializerController.Instance.UsersData[playerIndex].Color;
                        currentNumberOfObjects++;
                    }
                }
            }
    }

    public override void End()
    {
        base.End();
        FeedBackUtilities.ChangeObjectsColor = true;

        Snapshot.RecoverZAnatomy(UserInitializerController.Instance.DissectionTable.transform.position);
    }

    internal void ChangeIDColors(int userDataID, int index)
    {
        Debug.Log("Change ID Colors  " + userDataID + ",  " + nextUserID);
        List<MeshInteractableOnline> meshes = OnlineUtilities.GetOnlineUtilities().meshesOnline;

        nextUserID = UserInitializerController.Instance.GetNextUser(userDataID, directionOfObjectMovement);
        int currentNumberOfObjects = 0;
        for (int j = userDataID * maxNumOfObjectsPerColor; j < userDataID * maxNumOfObjectsPerColor + maxNumOfObjectsPerColor && currentNumberOfObjects < maxNumOfObjectsPerColor; j++)
            for (int i = 0; i < meshes.Count && currentNumberOfObjects <maxNumOfObjectsPerColor; i++)
            {
                MeshInteractiveController mesh = meshes[i].GetComponent<MeshInteractiveController>();

                if (mesh.BodyPart == organType)
                {
                    if (ComponentsToColor[j].Equals(meshes[i].name))
                    {
                        if (currentNumberOfObjects==index)
                        {
                            if (UserInitializerController.Instance.UsersData[nextUserID].IsLocalPlayer)
                            {
                                myMeshInteractableToPlace[currentNumberOfObjects] = meshes[i].gameObject;
                            }
                            if (UserInitializerController.Instance.UsersData[userDataID].IsConnected)
                                foreach (Material material in meshes[i].GetComponent<Renderer>().materials)
                                    material.color = UserInitializerController.Instance.UsersData[nextUserID].Color;
                        }
                        currentNumberOfObjects++;
                    }
                }
            }

    }

    public override void InitStats()
    {
        // Primero agarramos los dos objetos que estan de nuestro color y los ponemos cerca nuestra
        // Despu�s ponemos eso al lado de nuestro compa�ero
        // Colocamos el que nos de nuestro compa�ero
        challengeStats.stats = new ChallengeValueFloat[]
           {
            new ChallengeValueFloat("CurrentStep_1",0), // 0 para agarrar modelo 1, 1 pasar el modelo 1 a otro y 2 volver a colocar el modelo 1 en su sitio
            new ChallengeValueFloat("CurrentStep_2",0), // 0 para agarrar modelo 2, 1 pasar el modelo 2 a otro y 2 volver a colocar el modelo 2 en su sitio
            new ChallengeValueFloat("NumOfObjectsPerUser",0),
            new ChallengeValueFloat("Time_Catch_Models_1",0), // Time grab model 1
            new ChallengeValueFloat("Time_Catch_Models_2",0), // Time grab model 2
            new ChallengeValueFloat("Time_Transfer_Models_1",0), // Time transferencia del modelo 1
            new ChallengeValueFloat("Time_Transfer_Models_2",0), // Time transferencia del modelo 2
            new ChallengeValueFloat("Time_Place_Models_1",0), // Time colocar el modelo 1
            new ChallengeValueFloat("Time_Place_Models_2",0), // Time colocar el modelo 2
            new ChallengeValueFloat("Red",0), // Numero de veces mano roja
            new ChallengeValueFloat("Yellow",0), // Numero de veces mano amarilla
            new ChallengeValueFloat("Green",0), // Numero de veces mano verde
           };

        ChallengeStats.SetValue(challengeStats, "NumOfObjectsPerUser", maxNumOfObjectsPerColor);
    }

    public override void EvaluateStats()
    {
        for (int i = 0; i < CurrentState.Length; i++)
            if (CurrentState[i]==0) {
                if (moveObject[i]==false && myMeshInteractableToGrabInitPosition[i]==myMeshInteractableToGrab[i].transform.position) {
                    timeState[i] = 0;
                } else {
                    timeState[i] += Time.deltaTime;
                    moveObject[i] = true;
                }
            } else {
                timeState[i] += Time.deltaTime;
            }

        CheckHandColorState();
        for (int i = 0; i < CurrentState.Length; i++)
            EvaluateState(i);
    }

    private void EvaluateState(int index)
    {
        if (CurrentState[index] == 0)
        {
            ChallengeStats.SetValue(challengeStats, "Time_Catch_Models_" + (index+1), timeState[index]);

            if (!objectsState[index]) // Represent Grabbing
            {
                objectsState[index] = (leftHand.GetGrabbedGroup() == myMeshInteractableToGrab[index]
                    || rightHand.GetGrabbedGroup() == myMeshInteractableToGrab[index]) && CheckAllNear(UserInitializerController.Instance.GetMainUser().UserDataID, myMeshInteractableToGrab[index]);

                if (objectsState[index])
                {
                    OnlineUtilities.GetOnlineUtilities().C_TutorialInteractionChangeObjectState(UserInitializerController.Instance.GetMainUser().UserDataID, index);
                }
            }
            
            if (CheckIfAllInCorrectState(index) && CheckAllNear(UserInitializerController.Instance.GetMainUser().UserDataID, myMeshInteractableToGrab[index]))
                ChangeState(index);
        }
        else if (CurrentState[index] == 1)
        {
            ChallengeStats.SetValue(challengeStats, "Time_Transfer_Models_" + (index + 1), timeState[index]);

            nextUserID = UserInitializerController.Instance.GetNextUser(myUser.UserDataID, directionOfObjectMovement);
            if (CheckAllNear(nextUserID, myMeshInteractableToGrab[index]))
                ChangeState(index);
            // OnlineUtilities.GetOnlineUtilities().SendObjectColor(myMeshInteractableToGrab[i].GetComponent<MeshInteractableOnline>().ID, Color.white, false);
        }
        else if (CurrentState[index] == 2)
        {
            ChallengeStats.SetValue(challengeStats, "Time_Place_Models_" + (index + 1), timeState[index]);
            if (!objectsState[index]) // Represent in place
            {
                if (myMeshInteractableToPlace[index] == null)
                {
                    // El otro usuario no nos ha dado nuestro objeto
                    timeState[index] = 0;
                    return;
                }
                MeshInteractiveController mesh = myMeshInteractableToPlace[index].GetComponent<ZAnatomy.MeshInteractiveController>();
                objectsState[index] = mesh.MorgueIsInPlace;

                if (mesh.MorgueIsInPlace)
                {
                    OnlineUtilities.GetOnlineUtilities().SendObjectColor(myMeshInteractableToPlace[index].GetComponent<MeshInteractableOnline>().ID, Color.white, false);

                    if (CheckIfAllInCorrectState(index))
                        ChangeState(index);
                }
            }
            
        }
    }

    private void CheckHandColorState()
    {
        if (lastActionState != FeedBackUtilities.lastActionState)
        {
            if (FeedBackUtilities.lastActionState == FeedBackUtilities.ActionState.Correct)
                ChallengeStats.SetValue(challengeStats, "Green", ChallengeStats.GetValue(challengeStats, "Green") +1);
            else if (FeedBackUtilities.lastActionState == FeedBackUtilities.ActionState.Near)
                ChallengeStats.SetValue(challengeStats, "Yellow", ChallengeStats.GetValue(challengeStats, "Yellow") +1);
            else if (FeedBackUtilities.lastActionState == FeedBackUtilities.ActionState.Wrong)
                ChallengeStats.SetValue(challengeStats, "Red", ChallengeStats.GetValue(challengeStats, "Red") +1);
        }
        lastActionState = FeedBackUtilities.lastActionState;
    }

    private bool CheckAllNear(int userDataID, GameObject myMeshInteractableToGrab)
    {
        MorgueController.ReferencePlayerPosition position = MorgueGrabUtilities.morgue.ReferencePositions[userDataID];

        Vector2 referencePos = new Vector2(position.PositionMarker.transform.position.x, position.PositionMarker.transform.position.z);
;
        float minDistance = 0.5f;
        if (myMeshInteractableToGrab == null) return false;

        Vector2 meshPos = new Vector2(myMeshInteractableToGrab.transform.position.x, myMeshInteractableToGrab.transform.position.z);
        if (Vector2.Distance(referencePos, meshPos) > minDistance ) return false;
        
        return true;
    }

    private void ChangeState(int index)
    {
        for (int i = 0; i < objectsState.Length; i++)
            objectsState[i] = false;

        if (CurrentState[index]==0)
        {
            // OnlineUtilities.GetOnlineUtilities().SendObjectColor(myMeshInteractableToGrab[i].GetComponent<MeshInteractableOnline>().ID, Color.white, false);
            // OnlineUtilities.GetOnlineUtilities().C_TutorialInteractionChangeObjectState(UserInitializerController.Instance.GetMainUser().UserDataID, index);
        }
        else if (CurrentState[index] == 1)
        {
        }
        else if (CurrentState[index] == 2)
        {
            challengeRunning = false;
            challengeFinish = true;
        }

        CurrentState[index]++;
        ChallengeStats.SetValue(challengeStats, "CurrentStep_"+(index+1), CurrentState[index]);
        timeState[index] = 0;
    }

    public bool CheckIfAllInCorrectState(int index)
    {
        return objectsState[index];
    }

    public override ChallengeValueFloatSlider[] ShowStats(int userID, int unitID, int challengeID, int infoStatIndex)
    {
        List<ChallengeValueFloat> challengeStat = GetUserAnalytic(userID, unitID, challengeID, infoStatIndex);

        if (challengeStat == null)
            return null;

        return new ChallengeValueFloatSlider[]
        {
            new ChallengeValueFloatSlider("Finish", ChallengeStats.GetValue(challengeStat, "CurrentStep_1") >=3 &&
                                                    ChallengeStats.GetValue(challengeStat, "CurrentStep_2") >= 3 ? 1 : 0),  // 1 finish, 0 not finish
            new ChallengeValueFloatSlider("Catching time (s)", (float) Math.Round(ChallengeStats.GetValue(challengeStat, "Time_Catch_Models_1") +
                                                        ChallengeStats.GetValue(challengeStat, "Time_Catch_Models_2") / maxNumOfObjectsPerColor),
                new float[] {20, 40}),
            new ChallengeValueFloatSlider("Transfering time (s)", (float) Math.Round(ChallengeStats.GetValue(challengeStat, "Time_Transfer_Models_1") +
                                                        ChallengeStats.GetValue(challengeStat, "Time_Transfer_Models_2"))/2, 
                new float[] {20, 40}),
            new ChallengeValueFloatSlider("Placing back time (s)", (float) Math.Round(ChallengeStats.GetValue(challengeStat, "Time_Place_Models_1") +
                                                         ChallengeStats.GetValue(challengeStat, "Time_Place_Models_2"))/2, 
                new float[] {20, 40}),
            new ChallengeValueFloatSlider("Red hand", ChallengeStats.GetValue(challengeStat, "Red"),
                new float[] {3, 5}),
            new ChallengeValueFloatSlider("Yellow hand", ChallengeStats.GetValue(challengeStat, "Yellow"),
                new float[] {10, 20}),
            new ChallengeValueFloatSlider("Green hand", ChallengeStats.GetValue(challengeStat, "Green"),
                new float[] {10, 20})
        };
    }
}
