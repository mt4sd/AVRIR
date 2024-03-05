using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AnalyticsManager : MonoBehaviour
{
    [System.Serializable]
    public class DataStructureToJson{
        [System.Serializable]
        public class ChallengeData{
            public string DateTime;

            public string ChallengeName;
            public int ChallengeID;

            public string UnitName;
            public int UnitID;

            // public List<UserAnalytics> ListUserAnalytics = new List<UserAnalytics>();
            // public List<UserAnalytics.ChallengeValue[]> ChallengeInfo = new List<UserAnalytics.ChallengeValue[]>();
            // public List<UserAnalytics.ChallengeValueFloat[]> ChallengeInfo = new List<UserAnalytics.ChallengeValueFloat[]>();
            public List<UserDataInfo> UsersData = new List<UserDataInfo>();
        }

        [System.Serializable]
        public class UserDataInfo{
            [System.Serializable]
            public class Stat{
                public string Name;
                public string Value;
                public Stat(string name, string value)
                {
                    Name = name;
                    Value = value;
                }
                public Stat()
                {
                    Name = "";
                    Value = "0";
                }
            }

            public int ID;
            public string ColorName;
            public string UserName;
            public string UserSurname;
            public List<Stat> Data = new List<Stat>();
        }

        public string DateTime;
        public List<ChallengeData> ListHistoricalChallenges;

        public DataStructureToJson(){
            DateTime = Utils.GetNowDateTimeString();
            ListHistoricalChallenges = new List<ChallengeData>();
            Debug.Log("New challenge historical data: "+DateTime);
        }

        public void AddChallengeData(Challenge challenge, List<UserAnalytics> userAnalytics){
            ChallengeData newData = new ChallengeData();
            newData.DateTime = Utils.GetNowDateTimeString();

            newData.UnitName = AnalyticsManager.currentUnitText;
            newData.UnitID = AnalyticsManager.unitID;

            if (challenge != null && challenge.ChallengeData != null){
                newData.ChallengeName = challenge.ChallengeData.Name;
                newData.ChallengeID = AnalyticsManager.GetInstance().lastChallenge.challengeID;
            }else{
                newData.ChallengeName = "Not available challenge data";
                newData.ChallengeID = -1;
            }

            // newData.ListUserAnalytics = new List<UserAnalytics>(list);
            // newData.ChallengeInfo = new List<UserAnalytics.ChallengeValue[]>(challenge.ShowStats());
            // newData.ChallengeInfo = challenge.ShowStatsToSave();

            newData.UsersData = new List<UserDataInfo>();
            List<UserAnalytics.ChallengeValueFloat[]> list1 = challenge.ShowStatsToSave();
            for (int i = 0; i < list1.Count; i++)
            {
                UserAnalytics.ChallengeValueFloat[] playerStat = list1[i];
                UserDataInfo user = new UserDataInfo();
                user.ID = i;
                user.ColorName = userAnalytics[i].colorName;
                user.UserName = userAnalytics[i].userName;
                user.UserSurname = userAnalytics[i].userSurname;

                user.Data = new List<UserDataInfo.Stat>();

                if(playerStat == null){
                    continue;
                }

                for (int i1 = 0; i1 < playerStat.Length; i1++)
                {
                    UserAnalytics.ChallengeValueFloat stat = playerStat[i1];
                    // UserAnalytics.ChallengeValueFloat floatedStat = (UserAnalytics.ChallengeValueFloat) stat;
                    UserDataInfo.Stat newJsonStat = null;
                    if(stat != null){
                        newJsonStat = new UserDataInfo.Stat(stat.statName, stat.statValue.ToString());
        }
                    // else{
                    //     UserAnalytics.ChallengeValueString stringedStat = (UserAnalytics.ChallengeValueString) stat;
                    //     if(stringedStat != null){
                    //         newJsonStat = new UserDataInfo.Stat(stringedStat.statName, stringedStat.statString);
                    //     }
                    // }

                    if(newJsonStat != null){
                        user.Data.Add(newJsonStat);
                    }
                }

                newData.UsersData.Add(user);
            }

            Debug.Log("Adding challenge data to historical list: "+newData.DateTime);
            ListHistoricalChallenges.Add(newData);
        }

        public static string ToJson(DataStructureToJson data){
            return JsonUtility.ToJson(data, true);
        }
    }

    internal Challenge GetChallengeToCheck(int unitID, int challengeID)
    {
        foreach (Challenge c in this.GetComponentsInChildren<Challenge>())
            if (c.challengeID == challengeID) return c;
        return null;
    }

    private DataStructureToJson CurrentData;

    public void SetDataFromServer(DataStructureToJson Data)
    {
        if (!NetworkClient.isHostClient)
        {
            CurrentData = Data;
        }
    }
    public DataStructureToJson GetDataToServer()
    {
        if (NetworkClient.isHostClient)
        {
            return CurrentData;
        }
        return null;
    }

    public List<DataStructureToJson.ChallengeData> GetHistorialChallenges()
    {
        return CurrentData.ListHistoricalChallenges;
    }

    public string GetDataJson(){
        DataStructureToJson data = CurrentData;//new DataStructureToJson(userAnalytics);
        return DataStructureToJson.ToJson(data);
    }

    public string GetDateTimeStart(){
        return CurrentData.DateTime;
    }

    public void InitHistoricalData(){
        CurrentData = new DataStructureToJson();
    }

    public void SaveChallengeData(List<UserAnalytics> list){
        // Debug.Log("SaveChallengeData -- ");
        if(currentChallenge != null){
            CurrentData.AddChallengeData(currentChallenge, list);
            Utils.SaveSessionFile();
        }
    }

    public List<UserAnalytics> userAnalytics;

    public UserAnalytics myAnalytics;

    public List<AnalyticPanel> infoPanels;

    private XRInitialization myUser;
    private int currentShownUser = 0;

    // Check Positions
    private Quaternion lastCameraRotation;
    private Vector3 lastRightHandPosition;
    private Vector3 lastLeftHandPosition;

    private static AnalyticsManager analyticsManager;
    private static float informationPeriodTime = 1f;
    public static Action OnPlayersChanged;
    private List<Challenge> ChallengesList = new List<Challenge>();

    [SerializeField]
    private Challenge currentChallenge;
    public Challenge lastChallenge;
    public Challenge defaultChallenge;

    public static string currentUnitText = "Basic Skills";
    public static int unitID = 0;

    public Challenge GetCurrentChallenge() { return currentChallenge; }
    public void SetCurrentChallenge(Challenge challenge)
    {
        if(currentChallenge != null && challenge == null){ 
            if(myUser!=null && myUser.isServer){
                // Saving on challenge finished. Called in all clients and the Windows versions will save data
                SaveChallengeData(userAnalytics);
            }
        }

        currentChallenge = challenge;
        if (challenge != null)
            lastChallenge = challenge;
    }

    public void Awake()
    {
        analyticsManager = this;
    }

    public void Start()
    {
        foreach(Transform child in transform){
            ChallengesList.Add(child.GetComponent<Challenge>());
        }

        if (NetworkClient.isHostClient)
        {
            StartCoroutine(SendInformationToClients(informationPeriodTime + informationPeriodTime / 2.0f));
        }

        InitHistoricalData();
    }

    public Challenge GetChallenge(ScriptableChallenge scriptableChallenge){
        return ChallengesList.Find((x)=> x.ChallengeData == scriptableChallenge);
    }

    public void StartChallenge(ScriptableChallenge scriptable){
        AnalyticsManager.GetInstance().GetChallenge(scriptable).SetChallenge();
        
        // ShowChallengeUI(true, scriptable.Description);
    }

    public static AnalyticsManager GetInstance()
    {
        return analyticsManager;
    }

    public void ResetAnalytic()
    {
        Init(); 
        /*
        myAnalytics = new UserAnalytics();

        myAnalytics.appID = myUser.UserDataID;//myUser.PlayerID;
        myAnalytics.name = "User " + myUser.PlayerID;
        */
    }
    public void ForceEnd(){        
        // SaveChallengeData(userAnalytics);

        OnlineUtilities.GetOnlineUtilities().SetChallenge_Event(-1);
    }

    public void Init()
    {
        if (myUser != null) return;

        myUser = UserInitializerController.Instance.GetMainUser();

        Debug.Log("Initting Analytics from local player "+ myUser.xrType);

        if (myUser.xrType == XRInitialization.XRType.Desktop) return;
        myAnalytics = new UserAnalytics();

        myAnalytics.appID = myUser.UserDataID;//myUser.PlayerID;
        myAnalytics.colorName = UserInitializerController.Instance.UsersData[UserInitializerController.Instance.GetMainUser().UserDataID].ColorName;

        StartCoroutine(SendInformationToServer());

        ResetCheckPosition();
    }

    private IEnumerator SendInformationToServer()
    {
        OnlineUtilities.GetOnlineUtilities().SendLocalAnalytics(myAnalytics);

        yield return new WaitForSeconds(informationPeriodTime);

        myAnalytics.userSurname = UserInitializerController.Instance.UsersData[myUser.UserDataID].UserSurname;
        myAnalytics.userName = UserInitializerController.Instance.UsersData[myUser.UserDataID].UserName;

        StartCoroutine(SendInformationToServer());
    }

    private IEnumerator SendInformationToClients(float timeInterval)
    {
        OnlineUtilities.GetOnlineUtilities().SendListAnalytics(userAnalytics);

        yield return new WaitForSeconds(timeInterval);

        StartCoroutine(SendInformationToClients(informationPeriodTime));
    }

    public void Update()
    {
        if (UpdateValues()) return;

        myAnalytics.headMovement += Quaternion.Angle(lastCameraRotation, myUser.XRCamera.transform.localRotation);
        myAnalytics.leftHandMovement += Vector3.Distance(lastLeftHandPosition, myUser.trackedPoseDriverL.transform.localPosition);
        myAnalytics.rightHandMovement += Vector3.Distance(lastRightHandPosition, myUser.trackedPoseDriverR.transform.localPosition);

        myAnalytics.timeInApplication += Time.deltaTime;

        ResetCheckPosition();
    }
    
    private bool UpdateValues()
    {
        return (myUser == null ||
               myUser.xrType == XRInitialization.XRType.Desktop ||
               myUser.trackedPoseDriverL == null || myUser.trackedPoseDriverR == null);
    }

    public void ResetCheckPosition()
    {
        if (UpdateValues()) return;
        lastCameraRotation = myUser.XRCamera.transform.localRotation;
        lastLeftHandPosition = myUser.trackedPoseDriverL.transform.localPosition;
        lastRightHandPosition = myUser.trackedPoseDriverR.transform.localPosition;
    }

    internal void UpdateAnalytic(UserAnalytics myAnalytics)
    {
        for(int i = 0; i<userAnalytics.Count; i++)
        {
            if (userAnalytics[i].appID == myAnalytics.appID)
            {
                userAnalytics[i] = myAnalytics;
                return;
            }
        }

        // Not found
        userAnalytics.Add(myAnalytics);
        userAnalytics = userAnalytics.OrderBy(o => o.appID).ToList();
        
        if(OnPlayersChanged!=null){
            OnPlayersChanged();
        }
    }

    private bool infoShow = false;
    private int infoUnitID = -1;
    private int infoChallengeID = -1;
    private int infoStatIndex = -1;

    internal void SetInformationPanelState(bool show, int unitID, int challengeID, int statIndex)
    {
        infoShow = show;
        infoUnitID = unitID;
        infoChallengeID = challengeID;
        infoStatIndex = statIndex;
    }

    internal void UpdateAnalyticList(List<UserAnalytics> analyticList)
    {
        userAnalytics = analyticList;

        foreach (AnalyticPanel infoPanel in infoPanels)
        {
            infoPanel.gameObject.SetActive(true);
            infoPanel.SetInfoShow(infoShow);

            if (infoShow)
            {
                if (infoPanel.gameObject.activeInHierarchy)
                    infoPanel.UpdateInformation(currentShownUser, infoUnitID, infoChallengeID, infoStatIndex);
            }
        }
    }

    public void SetCurrentShownUser(int user)
    {
        currentShownUser = user;
    }

    public void ShowChallengeUI(bool enable, ScriptableChallenge challenge = null){
        foreach (AnalyticPanel infoPanel in infoPanels)
        {
            infoPanel.ShowChallenge(enable, challenge);
        }
    }

    internal void InitDefaultChallenge()
    {
        defaultChallenge.SetChallenge();
    }
}
