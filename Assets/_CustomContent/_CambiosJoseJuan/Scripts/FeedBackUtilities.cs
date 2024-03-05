using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeedBackUtilities
{
    public enum ActionState { Correct, Wrong, Near, Selected, None }
    public static bool ChangeObjectsColor = true;
    public static bool ShowColorFeedback = true;

    public static Color colorCorrect = new Color(0, 0.6196079f, 0.4509804f); // Verde
    public static Color colorWrong = Color.red; // Color.black; // Negro;
    public static Color colorSelected = new Color(0.9528302f, 0.9095425f, 0.8126023f); // Blanco
    public static Color colorNear = new Color(0.73f, 0.6669999f,0); //amarillo new Color(0.8f, 0.4745098f, 0.654902f); // Rosa

    public static ActionState lastActionState = FeedBackUtilities.ActionState.None;

    public static void SetMaterialsColor(GameObject interactive, VRGrabbing hand, ActionState colorState)
    {
        lastActionState = colorState;
        if (!ShowColorFeedback) return;

        ZAnatomy.DividedOrganGroup group = interactive.GetComponentInParent<ZAnatomy.DividedOrganGroup>();

        if (group != null)

        {
            foreach (ZAnatomy.MeshInteractiveController i in group.GetComponentsInChildren<ZAnatomy.MeshInteractiveController>())
                SetMaterialsColor(i.GetRenderer(), hand, colorState);
        }
        else
        {
            SetMaterialsColor(interactive.GetComponent<ZAnatomy.MeshInteractiveController>().GetRenderer(), hand, colorState);
        }
    }

    private static void SetMaterialsColor(Renderer renderer, VRGrabbing hand, ActionState colorState)
    {
        if (!ShowColorFeedback) return;
        if (hand != null && hand.wrongAttempt && (colorState == ActionState.None || colorState == ActionState.Selected)) return;

        Color selectedColor = GetColor(colorState);
        if (colorState == ActionState.Wrong)
            hand.wrongAttempt = true;

        if (ChangeObjectsColor)
        {
            if (colorState == ActionState.None)
                renderer.GetComponent<ZAnatomy.MeshInteractiveController>().SetNotSelected(true);
            else if (colorState == ActionState.Selected)
            {
                MeshRenderer meshRenderer = (MeshRenderer)renderer;
                for (int i = 0; i < meshRenderer.materials.Length; i++)
                    meshRenderer.materials[i].color = selectedColor;
            }
        }

        if (hand!= null && (colorState != ActionState.Selected))
        {
            CustomPhysicsHand physicsHand = hand.GetComponent<CustomPhysicsHand>();

            if (physicsHand != null)
            {
                if (colorState == ActionState.None)
                    selectedColor = UserInitializerController.Instance.GetMainUser().PlayerColor;

                OnlineUtilities.GetOnlineUtilities().SendHandFeedBackState(physicsHand, colorState);
                if(physicsHand.FollowRenderer!=null){
                    physicsHand.FollowRenderer.material.color = selectedColor;
                }

                if(physicsHand.HandRenderer!=null){
                    physicsHand.HandRenderer.material.color = selectedColor;
                }
            }
        }
    }

    public static void SetHandColor(CustomPhysicsHand hand, ActionState colorState, int iD)
    {
        Color selectedColor = GetColor(colorState);
        
        if (colorState == ActionState.None)
            selectedColor = UserInitializerController.Instance.GetPlayerData(iD).Color;

        if(hand.FollowRenderer!=null){
            hand.FollowRenderer.material.color = selectedColor;
        }

        if(hand.HandRenderer!=null){
            hand.HandRenderer.material.color = selectedColor;
        }
    }

    private static Color GetColor(ActionState colorState)
    {
        if (colorState == ActionState.Correct)
            return colorCorrect;
        else if (colorState == ActionState.Wrong)
            return colorWrong;
        else if (colorState == ActionState.Near)
            return colorNear;
        return colorSelected;// Color.yellow; // Blanco
    }
}
