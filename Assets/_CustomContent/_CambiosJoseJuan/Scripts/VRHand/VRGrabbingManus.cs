#if MANUS_ENABLED
using Manus.Hand;
using Manus.Hand.Gesture;
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRGrabbingManus : VRGrabbing
{
    #if MANUS_ENABLED
    [SerializeField]
    private Hand hand;
    [SerializeField]
    private GestureBase grabGesture;
    #endif

    public override bool CheckGrabbing()
    {
        #if MANUS_ENABLED
        return grabGesture.Evaluate(hand) || forceGrab;
        #endif

        return forceGrab;
    }
}
