using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportController : MonoBehaviour
{
    [SerializeField]
    private GameObject CurrentInnerObject;

    public void ResetCurrentInnerObject()
    {
        CurrentInnerObject = null;
    }
    public GameObject GetCurrentInnerObject()
    {
        return CurrentInnerObject;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Mask")){
            //Debug.Log(other.gameObject.name);
            // ZAnatomy.MeshInteractiveController parentObject = other.gameObject.GetComponentInParent<ZAnatomy.MeshInteractiveController>();
            // if(parentObject != null){
            //     CurrentInnerObject = parentObject.gameObject;
            // }else{
                CurrentInnerObject = other.gameObject;
            // }
        }
    }

    // void OnTriggerStay(Collider other)
    // {
    //     if (other.gameObject.layer == LayerMask.NameToLayer("Mask")){
    //         if()
    //         CurrentInnerObject = other.gameObject;
    //     }
    // }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Mask"))
            CurrentInnerObject = null;
    }
}
