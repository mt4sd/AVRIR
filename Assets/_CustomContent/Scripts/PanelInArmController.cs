using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;

public class PanelInArmController : MonoBehaviour
{
    public enum ButtonAction{
        GoToP1,
        GoToP2,
        GoToP3,
        GoToP4,
        GoToReorder
    }

    private XRInitialization XRPlayer;
    private MorgueController Morgue;
    public ParentConstraint Constraint;
    public List<Canvas> WorldCanvasList;
    
    [Header("Toggles")]
    public UIToggleController UIToggleTypePrefab;
    public Transform UIToggleParent;

    [Header("Buttons")]
    public UIButtonController UIMoveButtonPrefab;
    public Transform UIMoveButtonsParent;

    // public UIButtonController UIButtonReset;
    
    public void Init(XRInitialization player,Camera xrCamera, Transform handTransform)
    {
        XRPlayer = player;

        for (int i = 0; i < WorldCanvasList.Count; i++)
        {
            Canvas item = WorldCanvasList[i];
            item.worldCamera = xrCamera;
        }

        if(Constraint.sourceCount == 0){
            AddNewSource(handTransform);
        }else{
            if(handTransform != Constraint.GetSource(0).sourceTransform){
                Constraint.RemoveSource(0);

                AddNewSource(handTransform);
            }
        }

        Constraint.constraintActive = true;

        // TOGGLES
        foreach(Transform child in UIToggleParent){
            Destroy(child.gameObject);
        }
        /*

        foreach (MorgueController.OrganTypeInfo info in Morgue.OrgansTypeInfo){
            UIToggleController newToggle = Instantiate(UIToggleTypePrefab, UIToggleParent);
            newToggle.Init(info.Name, info.Color, info.Enabled);
            // Event
            newToggle.GetComponentInChildren<Toggle>().onValueChanged.AddListener((value) => {
                Morgue.OnToggleOrganType(info.Type, value);
            });
        }
        */
        // BUTTONS
        foreach (Transform child in UIMoveButtonsParent){
            Destroy(child.gameObject);
        }
        string[] actionButtons = System.Enum.GetNames (typeof(ButtonAction));
        for(int i = 0; i < actionButtons.Length; i++){
            UIButtonController newButton = Instantiate(UIMoveButtonPrefab, UIMoveButtonsParent);
            newButton.Init(this, i, actionButtons[i], Color.white);
        }
    }

    private void AddNewSource(Transform handTransform){
        ConstraintSource handSource = new ConstraintSource();
        handSource.weight = 1;
        handSource.sourceTransform = handTransform;
        Constraint.AddSource(handSource);
    }

    public void OnClickButton(PanelInArmController.ButtonAction action){
        XRPlayer.OnMove(action);
    }

}
