using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIElementForVR : MonoBehaviour
{
    public BoxCollider Collider;
    protected RectTransform RectTransform;

    void FixedUpdate()
    {
        if(RectTransform != null){
            if(Collider.size.x != RectTransform.rect.width){
                UpdateColliderSize();
            }
        }
    }

    protected void UpdateColliderSize(){
        if(RectTransform == null){
            RectTransform = GetComponent<RectTransform>();
        }
        Collider.size = new Vector3(RectTransform.rect.width, RectTransform.rect.height, Collider.size.z);
    }

    virtual public void OnTriggerEnter_ActionStart(Collider other){
    }

    virtual public void OnTriggerEnter_ActionEvents(GameObject button){
        var go = button.gameObject;
        var ped = new PointerEventData(EventSystem.current);
        ExecuteEvents.Execute(go, ped, ExecuteEvents.pointerEnterHandler);
        ExecuteEvents.Execute(go, ped, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(go, ped, ExecuteEvents.submitHandler);
    }

    virtual public void OnTriggerExit_ActionStart(Collider other){

    }

    virtual public void OnTriggerExit_ActionEvents(GameObject button){
        var go = button.gameObject;
        var ped = new PointerEventData(EventSystem.current);
        ExecuteEvents.Execute(go, ped, ExecuteEvents.pointerUpHandler);
        ExecuteEvents.Execute(go, ped, ExecuteEvents.pointerExitHandler);
    }

}
