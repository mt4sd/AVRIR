using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SpatialTracking;
using Sirenix.OdinInspector;

public class TrackedPoseDriverDissection : TrackedPoseDriver
{
    public bool dataAvailable;
    public GameObject[] handList;

    // Update is called once per frame
    void Update()
    {
        Pose currentPose = new Pose();
        currentPose = Pose.identity;
        dataAvailable = (GetPoseData(poseSource, out currentPose) != PoseDataFlags.NoData);
    }

    PoseDataFlags GetPoseData(TrackedPose poseSource, out Pose resultPose)
    {
        return PoseDataSource.GetDataFromSource(poseSource, out resultPose);
    }

    [Button (ButtonSizes.Gigantic)]
    private void ToggleForceGrabOfChildVRGrabbing(){
        foreach (GameObject hand in handList)
        {
            if (hand.activeInHierarchy)
            {
                VRGrabbing grabber = hand.GetComponent<VRGrabbing>();
                grabber.forceGrab = !grabber.forceGrab;
            }
        }
    }
}
