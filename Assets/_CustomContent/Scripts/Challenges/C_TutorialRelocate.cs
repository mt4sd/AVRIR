using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UserAnalytics;
using static SliderAnalytic;

public class C_TutorialRelocate : Challenge
{
    private float challengeTime;
    private float minDistanteToPos = 0.15f;

    public void Start()
    {
        challengeID = 0;
    }

    public override void InitStats()
    {
        challengeTime = 0;
        challengeStats.stats = new ChallengeValueFloat[]
           {
            new ChallengeValueFloat("Initial Distance",0), // Distancia cuando empieza la tarea
            new ChallengeValueFloat("Final Distance",0), // Distancia cuando acaba la tarea
            new ChallengeValueFloat("Time",0), // Time para superar la tarea (si te mueves de tu posici�n seguir� contando)
           };
    }

    public override void EvaluateStats()
    {
        Vector3 offPos = UserInitializerController.Instance.GetMainUser().ShoesParent.GetChild(0).position;
        MorgueController.ReferencePlayerPosition position = MorgueGrabUtilities.morgue.ReferencePositions[UserInitializerController.Instance.GetMainUser().UserDataID];
        float distance = Vector3.Distance(offPos, position.PositionMarker.transform.position);
        ChallengeStats.SetValue(challengeStats, "Final Distance", distance);

        challengeTime += Time.deltaTime;
        if (distance > minDistanteToPos)
        {
            challengeFinish = false;
            ChallengeStats.SetValue(challengeStats, "Time", challengeTime);
        }
        else
            challengeFinish = true;

    }

    public override void EvaluateStatsInit()
    {
        Vector3 offPos = UserInitializerController.Instance.GetMainUser().ShoesParent.position;
        MorgueController.ReferencePlayerPosition position = MorgueGrabUtilities.morgue.ReferencePositions[UserInitializerController.Instance.GetMainUser().UserDataID];
        ChallengeStats.SetValue(challengeStats, "Initial Distance", Vector3.Distance(offPos, position.PositionMarker.transform.position));
    }

    public override ChallengeValueFloatSlider[] ShowStats(int userID, int unitID, int challengeID, int infoStatIndex)
    {

        List<ChallengeValueFloat> challengeStat = GetUserAnalytic(userID, unitID, challengeID, infoStatIndex);

        if (challengeStat == null)
        {
            return null;
        }

        return new ChallengeValueFloatSlider[]
        {
            //new ChallengeValueString("Name",AnalyticsManager.GetInstance().userAnalytics[i].name),
            new ChallengeValueFloatSlider("Finish", ChallengeStats.GetValue(challengeStat, "Final Distance") < minDistanteToPos ? 1: 0), // 1 finish, 0 not finish
            new ChallengeValueFloatSlider("Distance (m)",ChallengeStats.GetValue(challengeStat, "Final Distance"), 
                new float[] {minDistanteToPos, minDistanteToPos*2f}),
            new ChallengeValueFloatSlider("Time (s)",(float) Math.Round(ChallengeStats.GetValue(challengeStat, "Time")),
                new float[]{20.0f, 40.0f}) // Time para superar la tarea (si te mueves de tu posici�n seguir� contando)
        };
    }
}
