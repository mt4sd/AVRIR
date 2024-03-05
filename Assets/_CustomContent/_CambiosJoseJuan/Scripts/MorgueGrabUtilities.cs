using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MorgueGrabUtilities : StationController
{
    public static MorgueController morgue;

    // Check Final Morgue Position
    public override void CheckRelease(ZAnatomy.MeshInteractiveController grabbedObject, VRGrabbing hand)
    {
        bool isInsideHole = CheckGrabbedModelInsideHole(grabbedObject);
        bool stayOnOriginalPos = grabbedObject.MorgueIsInPlace && isInsideHole;

        if (stayOnOriginalPos || CheckGrabbedModelDistance(grabbedObject)){
            // grabbedObject.MorgueSetOnPosition(true);
            OnlineUtilities.GetOnlineUtilities().MeshInteractiveSetPosition(grabbedObject.GetComponent<MeshInteractableOnline>().ID, UserInitializerController.Instance.GetMainUser().PlayerID, true);
            Utils.MeshInteractiveMorgueSetOnPositionByLeanTween(grabbedObject, true);

            FeedBackUtilities.SetMaterialsColor(grabbedObject.gameObject, hand, FeedBackUtilities.ActionState.Selected);
        }
        else{
            if (CheckGrabbedModelDistance(grabbedObject,2))
            {
                FeedBackUtilities.SetMaterialsColor(grabbedObject.gameObject, hand, FeedBackUtilities.ActionState.Wrong);

                LeanTween.value(grabbedObject.gameObject, 0, 1, 0.5f).setOnComplete(() => {
                    if (hand.wrongAttempt && hand.GetBestCandidate() == null)
                    {
                        hand.wrongAttempt = false;
                        FeedBackUtilities.SetMaterialsColor(grabbedObject.gameObject, hand, FeedBackUtilities.ActionState.None);
                    }
                    else if (hand.wrongAttempt && hand.GetBestCandidate() != null)
                    {
                        hand.wrongAttempt = false;
                        FeedBackUtilities.SetMaterialsColor(hand.GetBestCandidate().gameObject, hand, FeedBackUtilities.ActionState.Selected);
                    }
                });
            }
            // grabbedObject.MorgueSetOnPosition(false, !grabbedObject.MorgueIsInPlace && isInsideHole);
            // OnlineUtilities.GetOnlineUtilities().MeshInteractiveSetPosition(grabbedObject.GetComponent<MeshInteractableOnline>().ID, UserInitializerController.Instance.GetMainUser().PlayerID, false, !grabbedObject.MorgueIsInPlace && isInsideHole);
            // Utils.MeshInteractiveMorgueSetOnPositionByLeanTween(grabbedObject, false, !grabbedObject.MorgueIsInPlace && isInsideHole);
        }
    }

    

    public override void WrongGrabbing(ZAnatomy.MeshInteractiveController bestCandidate, VRGrabbing hand)
    {
        FeedBackUtilities.SetMaterialsColor(bestCandidate.gameObject, hand, FeedBackUtilities.ActionState.Wrong);

        bestCandidate.ZAnatomyController = morgue.GetZAnatomyController();
        foreach (ZAnatomy.MeshInteractiveController model in bestCandidate.GetModelsOver())
        {
            if (model.MorgueIsInPlace && model.CanBeGrabbed)
            {
                //model.GetRenderer().material.color = selectedColor;
                FeedBackUtilities.SetMaterialsColor(model.gameObject, hand, FeedBackUtilities.ActionState.Wrong);
                // LeanTween.cancel(model.gameObject);
                LeanTween.value(model.gameObject, 0, 1, 1f).setOnComplete(() => {
                    FeedBackUtilities.SetMaterialsColor(model.gameObject, hand, FeedBackUtilities.ActionState.Selected);
                });
            }
        }
    }

    public override bool CheckGrabbedModelDistance(ZAnatomy.MeshInteractiveController grabbedModel, float distanceMultiplayer=1)
    {
        return Vector3.Distance(grabbedModel.GetMorgueOriginalPosition(), grabbedModel.transform.position) < morgue.DistanceToInsertPiece * distanceMultiplayer;
    }
    public override bool CheckGrabbedModelInsideHole(ZAnatomy.MeshInteractiveController grabbedModel)
    {
        return CheckGrabbedModelDistance(grabbedModel, 1.5f);
        // return morgue.GetZAnatomyController().BoxRegionDropObjects.bounds.Contains(grabbedModel.GetCenterPosition());
        // ModelsOnHumanController.ModelZone hole = morgue.GetCurrentHuman().GetModelZone();
        // if (hole == null)
        // {
        //     return morgue.GetCurrentHuman().IsModelInsideActiveCollider(grabbedModel.GetCenterPosition());
        // }
        // return Vector3.Distance(hole.HoleInBody.transform.position, grabbedModel.transform.position) < hole.GetHoleRadius();
    }

    public override bool CanBeGrabbed(ZAnatomy.MeshInteractiveController bestCandidate)
    {
        bool canBeGrabbed = true;
        bestCandidate.ZAnatomyController = morgue.GetZAnatomyController();
        foreach (ZAnatomy.MeshInteractiveController model in bestCandidate.GetModelsOver())
        {
            if (model != bestCandidate && model.gameObject.activeInHierarchy && model.MorgueIsInPlace && model.CanBeGrabbed)
            {
                canBeGrabbed = false;
            }
        }

        return canBeGrabbed;
    }
}
