using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static UserAnalytics;

public abstract class Challenge : MonoBehaviour
{
    public bool challengeFinish = false;
    public bool challengeRunning = false;
    public ScriptableChallenge ChallengeData;
    public static ScriptableChallenge CurrentChallengeData;
    private int functionIndex = -1;
    public int challengeID;
    public int unitID=0;
    protected UserAnalytics.ChallengeStats challengeStats;

    protected List<UnityEvent> stepFunctions;

    public void Update()
    {
        if (!challengeRunning) return;

        if (UserInitializerController.Instance.GetMainUser().xrType == XRInitialization.XRType.Desktop) return;

        EvaluateStats();
    }

    [ContextMenu("SetChallenge")]
    public void SetChallenge()
    {
        Debug.Log("SetChallenge");

        OnlineUtilities.GetOnlineUtilities().SetChallenge_Event(challengeID);

        this.GetComponentInParent<AnalyticsManager>().SetCurrentChallenge(this);
    }

    [ContextMenu("Init")]
    public virtual void Init()
    {
        challengeFinish = false;
        functionIndex = -1;
        challengeRunning = true;
        StartStats();
        stepFunctions = new List<UnityEvent>();
        EvaluateStatsInit();

        CurrentChallengeData = ChallengeData;
    }

    [ContextMenu("End")]
    public virtual void End()
    {
        EvaluateStatsEnd();
        challengeRunning = false;

        this.GetComponentInParent<AnalyticsManager>().SetCurrentChallenge(null);
        Debug.Log("END CHALLENGE");
        
        CurrentChallengeData = null;
    }
    public abstract void InitStats();
    public abstract void EvaluateStats();

    public abstract ChallengeValueFloatSlider[] ShowStats(int userID, int unitID, int challengeID, int infoStatIndex);
    public virtual List<ChallengeValueFloat[]> ShowStatsToSave()
    {
        List<ChallengeValueFloat[]> allValues = new List<ChallengeValueFloat[]>();


        for (int i = 0; i < AnalyticsManager.GetInstance().userAnalytics.Count; i++)
        {
            ChallengeStats challengeStat = GetUserAnalyticToSave(AnalyticsManager.GetInstance().userAnalytics[i]);

            List<ChallengeValueFloat> list = new List<ChallengeValueFloat>();

            if (challengeStat == null || challengeStat.stats == null)
                allValues.Add(null);
            else {
                foreach (ChallengeValueFloat stat in challengeStat.stats)
                    list.Add(new ChallengeValueFloat(stat.statName, stat.statValue));

                allValues.Add(list.ToArray());
            }

        }
        return allValues;
    }


    public virtual void EvaluateStatsInit()
    {

    }
    public virtual void EvaluateStatsEnd()
    {

    }
    public void StartStats()
    {
        List<UserAnalytics.UnitStat> stats = AnalyticsManager.GetInstance().myAnalytics.stats;
        if (stats == null)
            AnalyticsManager.GetInstance().myAnalytics.stats = new List<UserAnalytics.UnitStat>();
        stats = AnalyticsManager.GetInstance().myAnalytics.stats;

        int unitIndex = -1;
        for (int i = 0; i < stats.Count; i++)
            if (stats[i].unitID == unitID)
            {
                unitIndex = i;
                break;
            }
        if (unitIndex==-1)
        {
            unitIndex = stats.Count;
            stats.Add(new UserAnalytics.UnitStat(unitID));
        }

        int challengeIndex = -1;
        for (int i = 0; i < stats[unitIndex].challengeStat.Count; i++)
            if (stats[unitIndex].challengeStat[i].challengeID == challengeID)
            {
                challengeIndex = i;
                break;
            }
        if (challengeIndex == -1)
        {
            challengeIndex = stats[unitIndex].challengeStat.Count;
            stats[unitIndex].challengeStat.Add(new UserAnalytics.ChallengeStats(challengeID));
        }

        challengeStats = stats[unitIndex].challengeStat[challengeIndex];
        InitStats();
    }

    public void NextFunctionStep()
    {
        functionIndex++;
        Debug.Log("Function Step: " + functionIndex);
        if (functionIndex<stepFunctions.Count)
            stepFunctions[functionIndex].Invoke();
    }

    public static int lastChallengeIndexData = -1;

    protected List<ChallengeValueFloat> GetUserAnalytic(int userID, int unitID, int challengeID, int i)
    {
        var data = AnalyticsManager.GetInstance().GetHistorialChallenges();

        for (int j = 0; j < data[i].UsersData.Count; j++)
        {
            if (data[i].UsersData[j].ID == userID)
            {
                List<ChallengeValueFloat> list = new List<ChallengeValueFloat>();

                foreach (AnalyticsManager.DataStructureToJson.UserDataInfo.Stat stat in data[i].UsersData[j].Data)
                    list.Add(new ChallengeValueFloat(stat.Name, float.Parse(stat.Value)));

                lastChallengeIndexData = i;
                return list;
            }
        }

        Debug.Log("Not found stat");
        return null;
    }
    public int GetUserAnalyticIndex(int unitID, int challengeID)
    {
        var data = AnalyticsManager.GetInstance().GetHistorialChallenges();

        for (int i = data.Count - 1; i >= 0; i--)
        {
            if (data[i].ChallengeID == challengeID && data[i].UnitID == unitID)
            {
                lastChallengeIndexData = i;
                return i;
            }
        }

        return -1;
    }

    internal int GetNextChallengeIndex()
    {
        var data = AnalyticsManager.GetInstance().GetHistorialChallenges();

        for (int i = lastChallengeIndexData+1; i<data.Count; i++)
        {
            if (data[i].ChallengeID == challengeID && data[i].UnitID == unitID)
            {
                return i;
            }
        }

        return -1;
    }

    internal int GetPreviousChallengeIndex()
    {
        var data = AnalyticsManager.GetInstance().GetHistorialChallenges();

        for (int i = lastChallengeIndexData-1; i >= 0; i--)
        {
            if (data[i].ChallengeID == challengeID && data[i].UnitID == unitID)
            {
                return i;
            }
        }

        return -1;
    }


    protected ChallengeStats GetUserAnalyticToSave(UserAnalytics userAnalytics)
    {
        if (userAnalytics.stats == null) return null;

        // Get Unit ID
        int unitIndex = -1;
        for (int j = 0; j < userAnalytics.stats.Count; j++)
            if (userAnalytics.stats[j].unitID == unitID) unitIndex = j;
        if (unitIndex == -1)
            return null;

        if (userAnalytics.stats[unitIndex].challengeStat == null) return null;
        int challengeIndex = -1;
        for (int j = 0; j < userAnalytics.stats[unitIndex].challengeStat.Count; j++)
            if (userAnalytics.stats[unitIndex].challengeStat[j].challengeID == challengeID) challengeIndex = j;
        if (challengeIndex == -1)
            return null;

        return userAnalytics.stats[unitIndex].challengeStat[challengeIndex];
    }

    #region SNAPSHOT to save and load zAnatomy in challenges
    [System.Serializable]
    public class SnapshotZAnatomy{
        [System.Serializable]
        public class MeshInfo{
            public MeshInteractableOnline meshOnline;
            public ZAnatomy.MeshInteractiveController meshInteractive;
            public bool IsInPlace;
            public Vector3 position;
            public Quaternion rotation;
        }

        public List<MeshInfo> MeshList;
        public Vector3 TablePosition;

        public void Save(List<MeshInteractableOnline> onlineMeshesList, Vector3 tablePosition){
            MeshList = new List<MeshInfo>();
            TablePosition = tablePosition;

            foreach(MeshInteractableOnline meshOnline in onlineMeshesList){
                SnapshotZAnatomy.MeshInfo meshInfo = new MeshInfo();
                meshInfo.meshOnline = meshOnline;
                meshInfo.meshInteractive = meshOnline.GetComponent<ZAnatomy.MeshInteractiveController>();
                meshInfo.position = meshOnline.transform.position;
                meshInfo.rotation = meshOnline.transform.rotation;
                meshInfo.IsInPlace = meshInfo.meshInteractive.MorgueIsInPlace;
                MeshList.Add(meshInfo);
            }
        }

        public void RecoverZAnatomy(Vector3 newTablePosition){
            Vector3 offsetTablePosition = TablePosition - newTablePosition;

            foreach(MeshInfo meshInfo in MeshList){
                meshInfo.meshOnline.transform.position = meshInfo.position - offsetTablePosition;
                meshInfo.meshOnline.transform.rotation = meshInfo.rotation;
                meshInfo.meshInteractive.MorgueIsInPlace = meshInfo.IsInPlace;

                FeedBackUtilities.SetMaterialsColor(meshInfo.meshInteractive.gameObject, null, FeedBackUtilities.ActionState.None);
            }

            MeshList = new List<MeshInfo>();
        }
    }


    [SerializeField]
    protected SnapshotZAnatomy Snapshot = new SnapshotZAnatomy();
    #endregion
}
