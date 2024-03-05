using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DissectionProject/ScriptableChallenge")]
public class ScriptableChallenge : ScriptableObject
{
    public string Name;
    public Sprite Icon;
    public Sprite VRScreensImage;
    [Multiline]
    public string Description;
    public bool isActived;
    public bool IsFinished;
    public int challengeID;
    

    public List<int> PlayersList;
    // [Header("Analytics")]
    // public bool 
}
