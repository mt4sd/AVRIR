using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButtonController : UIElementForVR
{
    public Button Button;
    public TMPro.TextMeshProUGUI Label;
    public PanelInArmController.ButtonAction IDAction;
    private PanelInArmController PanelInArm;

    void Start()
    {
        UpdateColliderSize();
    }

    public void Init(PanelInArmController panel, int idAction, string nameLabel, Color color){
        IDAction = (PanelInArmController.ButtonAction)idAction;
        PanelInArm = panel;
        
        if (Label!=null)
            Label.text = nameLabel;
        Button.image.color = color;
        RectTransform = Button.image.rectTransform;
        
        UpdateColliderSize();
    }

    [Button(ButtonSizes.Gigantic)]
    public void ForceClickButton(){
        Button.onClick.Invoke();
    }

    public void OnClickButton(){
        PanelInArm.OnClickButton(IDAction);
    }

    override public void OnTriggerEnter_ActionStart(Collider other){
        base.OnTriggerEnter_ActionStart(other);

        GameObject button = Button.gameObject;
        base.OnTriggerEnter_ActionEvents(button);
    }

    override public void OnTriggerExit_ActionStart(Collider other){
        base.OnTriggerExit_ActionStart(other);

        GameObject button = Button.gameObject;
        base.OnTriggerExit_ActionEvents(button);
    }
}
