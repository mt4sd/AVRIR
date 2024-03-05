using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshInteractableOnline : MonoBehaviour
{
    public int ID { get; private set; }
    public bool isGrabbed = false;
    public bool isGrabbedLocal = false;
    // private NetworkIdentity identity;

    public void Grab()
    {
        isGrabbed = true;
        isGrabbedLocal = true;

        OnlineUtilities.GetOnlineUtilities().SetGrabState(ID, isGrabbed, UserInitializerController.Instance.GetMainUser().PlayerID);
    }
    public void UnGrab()
    {
        isGrabbed = false;
        isGrabbedLocal = false;

        OnlineUtilities.GetOnlineUtilities().SetGrabState(ID, isGrabbed, UserInitializerController.Instance.GetMainUser().PlayerID);
    }

    public void LateUpdate()
    {
        if (isGrabbedLocal)
        {
            OnlineUtilities.GetOnlineUtilities().ShareTransform(ID);
        }
    }
    internal void SetID(int ID)
    {
        this.ID = ID;
    }
}
