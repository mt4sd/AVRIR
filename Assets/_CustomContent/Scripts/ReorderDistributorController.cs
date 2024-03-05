using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class ReorderDistributorController : MonoBehaviour
{
    [System.Serializable]
    public class PositionBase{
        public GameObject Parent;
        public Transform Marker;
    }
    public Transform WorldUIParent;
    public Transform WorldCenter;

    public List<PositionBase> PositionList;
    
    [Button]
    private void InitPositions(){
        PositionList= new List<PositionBase>();

        // First base
        PositionBase newBase = new PositionBase();
        newBase.Parent = WorldCenter.gameObject;
        newBase.Marker = WorldCenter.GetChild(0);
        PositionList.Add(newBase);

        foreach(Transform child in WorldUIParent){
            newBase = new PositionBase();
            newBase.Parent = child.gameObject;
            newBase.Marker = child.GetChild(0);
            PositionList.Add(newBase);
        }
    }

    [Button]
    public List<Transform> RequestPositions(int positionsCount){
        List<Transform> positionsList = new List<Transform>();

        if(positionsCount> PositionList.Count){
            Debug.LogError("Requesting too many positions to ReorderDistributorController. Max: "+PositionList.Count);
        }

        for (int i = 0; i < PositionList.Count; i++)
        {
            bool enable = i<positionsCount;

            PositionList[i].Parent.SetActive(enable);
            if(enable){
                positionsList.Add(PositionList[i].Marker);
            }
        }

        return positionsList;
    }
}
