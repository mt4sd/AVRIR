using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class PlayerFingerUIInteractor : MonoBehaviour
{
    public bool ReadyToClick=true;
    public InputDeviceCharacteristics Hand;
    private XRDeviceInput deviceInput;
    private Renderer Renderer;
    private bool IsPointingWithIndexFinger;
    private Vector3 CurrentDirection;
    private Vector3 LastPosition;

    void Start()
    {
        Renderer = GetComponent<Renderer>();
        ReadyToClick = true;
        LastPosition = transform.position;
    }

    public void UpdateDeviceInput(XRDeviceInput input){
        deviceInput = input;
    }

    public bool IsActivated(){
        Debug.Log("FingerUI - IsActivated? Ready:"+ReadyToClick+ " && Pointing:"+IsPointingWithIndexFinger);
        return ReadyToClick && IsPointingWithIndexFinger;
    }

    private void FixedUpdate() {
        IsPointingWithIndexFinger = deviceInput != null && !deviceInput.GetBoolValue(CommonUsages.triggerButton);
        Renderer.enabled = IsPointingWithIndexFinger;
        // Renderer.material.color = IsPointingWithIndexFinger?Color.green:Color.red;
        CurrentDirection = transform.position - LastPosition;
        LastPosition = transform.position;

        //Debug.DrawLine(transform.position, transform.position + CurrentDirection, Color.green);
    }

    public Vector3 GetLastDirection(){
        return CurrentDirection;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent( out UIElementForVR ui)
            && Vector3.Angle(GetLastDirection(), other.transform.forward) < 60) // Check the direction
        {
            if (ReadyToClick)
            {
                ui.OnTriggerEnter_ActionStart(other);
                ReadyToClick = false;
            }
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent(out UIElementForVR ui))
        {
            if (!ReadyToClick)
            {
                ui.OnTriggerExit_ActionStart(other);
                ReadyToClick = true;
            }
        }
    }
}
