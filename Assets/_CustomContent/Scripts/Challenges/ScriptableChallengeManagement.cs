using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DissectionProject/ScriptableChallengeManagement")]
public class ScriptableChallengeManagement : ScriptableObject
{
    public string NameDocentUnit;
    public List<ScriptableChallenge> ChallengesList;

    public ScriptableChallenge EnabledChallenge;

    public void SelectChallenge(int idList){
        if(idList < 0){
            EnabledChallenge = null;
        }else{
            EnabledChallenge = ChallengesList[idList];
        }
    }
}
