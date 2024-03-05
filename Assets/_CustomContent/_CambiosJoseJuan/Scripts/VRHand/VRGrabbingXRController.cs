using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class VRGrabbingXRController : VRGrabbing
{
    [SerializeField]
    private XRDeviceInput deviceInput;

    protected override void Start()
    {
        base.Start();
    }

    public override bool CheckGrabbing()
    {
        return PressGrabButton() || forceGrab;
    }

    private bool PressGrabButton()
    {
        return  deviceInput.GetFloatValue(CommonUsages.grip) > 0.3f || deviceInput.GetFloatValue(CommonUsages.trigger) > 0.3f;
    }
}
