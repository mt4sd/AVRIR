using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UserAnalytics
{
    // Generales
    public string colorName;
    public string userName;
    public string userSurname;
    
    public int appID;

    public float headMovement;
    public float rightHandMovement;
    public float leftHandMovement;

    public float timeInApplication;

    public List<UnitStat> stats;

    [System.Serializable]
    public class UnitStat
    {
        public int unitID = -1;
        public List<ChallengeStats> challengeStat = new List<ChallengeStats>();

        public UnitStat()
        {

        }
        public UnitStat(int unitID)
        {
            this.unitID = unitID;
        }
    }

    [System.Serializable]
    public class ChallengeStats
    {
        public int challengeID = -1;
        public ChallengeValueFloat[] stats = new ChallengeValueFloat[0];
        public ChallengeStats(int challengeID)
        {
            this.challengeID = challengeID;
        }
        public ChallengeStats()
        {

        }

        public static void SetValue(ChallengeStats statsArray, string name, float value)
        {
            for (int i = 0; i < statsArray.stats.Length; i++)
                if (statsArray.stats[i].statName.Equals(name))
                {
                    statsArray.stats[i].statValue = value;
                    return;
                }
        }
        public static float GetValue(ChallengeStats statsArray, string name)
        {
            for (int i = 0; i < statsArray.stats.Length; i++)
                if (statsArray.stats[i].statName.Equals(name))
                {
                    return statsArray.stats[i].statValue;
                }
            return 0;
        }

        public static float GetValue(List<ChallengeValueFloat> stats, string name)
        {
            for (int i = 0; i < stats.Count; i++)
                if (stats[i].statName.Equals(name))
                {
                    return stats[i].statValue;
                }
            return 0;
        }
    }

    [System.Serializable]
    public class ChallengeValueFloat : ChallengeValue
    {
        public float statValue = 0;

        public ChallengeValueFloat()
        {

        }
        public ChallengeValueFloat(string statName, float statValue)
        {
            this.statName = statName;
            this.statValue = statValue;
        }
        public override string ToString()
        {
            return statValue + "";
        }
    }
    public class ChallengeValueFloatSlider : ChallengeValueFloat
    {
        public float[] maxStatesValue = new float[]{};
        public SliderAnalytic.SliderType[] maxTypes = new SliderAnalytic.SliderType[] { };

        public ChallengeValueFloatSlider()
        {

        }
        
        public ChallengeValueFloatSlider(string statName, float statValue, float[] maxStatesValue, SliderAnalytic.SliderType[] maxTypes)
        {
            this.statName = statName;
            this.statValue = statValue;
            this.maxStatesValue = maxStatesValue;
            this.maxTypes = maxTypes;
        }
        public ChallengeValueFloatSlider(string statName, float statValue, float[] maxStatesValue)
        {
            this.statName = statName;
            this.statValue = statValue;
            this.maxStatesValue = maxStatesValue;

            this.maxTypes = new SliderAnalytic.SliderType[] { SliderAnalytic.SliderType.Green, SliderAnalytic.SliderType.Yellow, SliderAnalytic.SliderType.Red };
        }
        public ChallengeValueFloatSlider(string statName, float statValue)
        {
            this.statName = statName;
            this.statValue = statValue;
            this.maxStatesValue = new float[] {};

            this.maxTypes = new SliderAnalytic.SliderType[] { SliderAnalytic.SliderType.Green, SliderAnalytic.SliderType.Yellow, SliderAnalytic.SliderType.Red };
        }

        public override string ToString()
        {
            return statValue + "";
        }
    }

    [System.Serializable]
    public class ChallengeValueString : ChallengeValue
    {
        public string statString = "";

        public ChallengeValueString()
        {

        }
        public ChallengeValueString(string statName, string statString)
        {
            this.statName = statName;
            this.statString = statString;
        }
        public override string ToString()
        {
            return statString;
        }
    }

    [System.Serializable]
    public class ChallengeValue
    {
        public string statName = "";

    }
}
