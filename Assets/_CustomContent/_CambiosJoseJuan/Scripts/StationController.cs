using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StationController : MonoBehaviour
{
    public abstract void WrongGrabbing(ZAnatomy.MeshInteractiveController bestCandidate, VRGrabbing hand);

    public abstract void CheckRelease(ZAnatomy.MeshInteractiveController grabbedObject, VRGrabbing hand);

    public abstract bool CanBeGrabbed(ZAnatomy.MeshInteractiveController bestCandidate);

    public abstract bool CheckGrabbedModelDistance(ZAnatomy.MeshInteractiveController interactive, float distanceMultiplayer = 1);

    public abstract bool CheckGrabbedModelInsideHole(ZAnatomy.MeshInteractiveController interactive);

    internal void SelectPointedModel(ZAnatomy.MeshInteractiveController pointedModel, ZAnatomy.MeshInteractiveController lastPointed, VRGrabbing hand)
    {
        if (lastPointed)
            DeselectLastPointed(lastPointed, hand);
        if (pointedModel == null) return;

        // ModelsOnHumanController human = pointedModel.GetComponentInParent<ModelsOnHumanController>();

        if (!pointedModel.MorgueIsInPlace || (pointedModel.MorgueIsInPlace))
        {
            FeedBackUtilities.SetMaterialsColor(pointedModel.gameObject, hand, FeedBackUtilities.ActionState.Selected);
        }
    }
    public void DeselectLastPointed(ZAnatomy.MeshInteractiveController pointedModel, VRGrabbing hand)
    {
        FeedBackUtilities.SetMaterialsColor(pointedModel?.gameObject, hand, FeedBackUtilities.ActionState.None);
    }

}
