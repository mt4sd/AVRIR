using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRGrabbingDesktop : VRGrabbing
{
    public DesktopPlayerController desktopPlayerController;

    public override bool CheckGrabbing()
    {
        bool isGrab = forceGrab;
        if (!IsGrabbing())
        {
            isGrab &= desktopPlayerController.IsObjectSelected();
        }
        return isGrab;
    }

    protected override void CheckColliders()
    {
        ZAnatomy.MeshInteractiveController lastBestCandidate = bestCandidate;
        if (!IsGrabbing())
        {
            bestCandidate = desktopPlayerController.GetPointed();
        }
        CheckColliderColor(lastBestCandidate);
    }

    // public void RotateObject(bool start){
    //     if(start){
    //         Grab(bestCandidate);
    //     }else{
    //         if(GetGrabbedObject() != null)
    //             Release();
    //     }
    // }

    public void MoveObject(Vector3 directionForward)
    {
        // if (!forceGrab) return;
            
        Grab(bestCandidate);

        if(desktopPlayerController == null){
            desktopPlayerController = this.GetComponentInParent<DesktopPlayerController>();
        }

        // Vector3 directionForward = grabbedGroup.transform.position-desktopPlayerController.PlayerCamera.transform.position;//desktopPlayerController.PlayerCamera.transform.forward;

        grabbedGroup.transform.position = grabbedGroup.transform.position + /*directionValue **/ directionForward;
        // LeanTween.cancel(grabbedGroup);
        // LeanTween.move(grabbedGroup, grabbedGroup.transform.position + directionValue * this.GetComponentInParent<DesktopPlayerController>().PlayerCamera.transform.forward /** 0.1f*/, 0.2f).setOnComplete(() =>
        // {
        // });
    }
}
