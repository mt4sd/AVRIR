using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandController : MonoBehaviour
{
    private GameObject grabbedObject = null;

    private Transform handCenter;
    private Vector3 directionToHandCenter;

    // Control movement and rotation
    private Quaternion lastGrabRot;
    private Vector3 lastControllerUp;
    private Vector3 init_direction_to_look;
    private Vector3 init_position;

    public void LateUpdate()
    {
        // Move and rotate an object according to the hand movement
        if (this.grabbedObject)
        {
            this.grabbedObject.transform.position -= (init_position - this.transform.position);

            init_position = this.transform.position;


            Quaternion angle = Quaternion.LookRotation(-this.transform.forward, this.transform.up) * Quaternion.Inverse(Quaternion.LookRotation(init_direction_to_look, lastControllerUp)) * lastGrabRot;

            this.grabbedObject.transform.rotation = angle;
        }
    }

    public void OnSelectObject(GameObject grabbedObject, Transform handCenter)
    {
        this.grabbedObject = grabbedObject;
        this.handCenter = handCenter;
        this.transform.position = grabbedObject.transform.position;
        SetInitialValues();

        directionToHandCenter = handCenter.transform.position - this.transform.position;
    }



    public void OnSelectExit()
    {
        if (this.grabbedObject)
        {
            this.grabbedObject = null;
        }
    }

    private void SetInitialValues()
    {
        if (!this.grabbedObject) return;

        lastGrabRot = Quaternion.Euler(new Vector3(this.grabbedObject.transform.rotation.eulerAngles.x, this.grabbedObject.transform.rotation.eulerAngles.y, this.grabbedObject.transform.rotation.eulerAngles.z));
        init_position = this.transform.position;

        lastControllerUp = this.transform.up;

        init_direction_to_look = -this.transform.forward;
    }
}
