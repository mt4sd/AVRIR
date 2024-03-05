#if !UNITY_WEBGL
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRGrabbingOHand : VRGrabbing
{
    public bool grabbing;
    // #if !UNITY_WEBGL
        public OVRHand ovrHand;
    // #endif

    public override bool CheckGrabbing()
    {
        // #if !UNITY_WEBGL
            if (forceGrab) return true;
            return this.grabbing || 
                ovrHand.GetFingerIsPinching(OVRHand.HandFinger.Index) ||
                ovrHand.GetFingerIsPinching(OVRHand.HandFinger.Middle) ||
                ovrHand.GetFingerIsPinching(OVRHand.HandFinger.Pinky) ||
                ovrHand.GetFingerIsPinching(OVRHand.HandFinger.Ring) ;
        // #else
        //     return forceGrab;
        // #endif
    }

    public void SetGrabbingState(bool grabbing)
    {
        this.grabbing = grabbing;
    }
}
#endif
 