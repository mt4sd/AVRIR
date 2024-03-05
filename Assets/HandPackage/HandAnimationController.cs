using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class HandAnimationController : MonoBehaviour
{
    // Animation of the hand
    [SerializeField]
    private Animator[] handAnimators;

    private float gripValue = 0.01f;
    private float triggerValue = 0.01f;
    private float lastGripValue = 0.01f;
    private float lastTriggerValue = 0.01f;

    [SerializeField]
    private XRDeviceInput deviceInput;

    public HandAnimationGrabController[] handAnimationGrabController_finger1;
    public HandAnimationGrabController[] handAnimationGrabController_finger2;
    public HandAnimationGrabController[] handAnimationGrabController_finger3;
    public HandAnimationGrabController[] handAnimationGrabController_finger4;
    public HandAnimationGrabController[] handAnimationGrabController_finger5;


    private bool[,] lastIterationMove;
    private float[,] lastStopValue;

    public VRGrabbing handController;


    private void Start()
    {
        lastIterationMove = new bool[5, 3];
        lastStopValue = new float[5, 3];
    }

    void Update()
    {
        ReadInput();
        UpdateHand();
    }

    // Update the hand shape with the grip value
    private void UpdateHand()
    {
        // if (handController.IsGrabbing()) return;

        float velocity = 3.5f;
        // Soft update of lastGripValue
        if (gripValue > 0.1)
            lastGripValue = Mathf.Min(lastGripValue + velocity * Time.deltaTime, 1);
        else
            lastGripValue = Mathf.Max(lastGripValue - velocity * Time.deltaTime, 0);
        lastGripValue = Mathf.Min(lastGripValue, gripValue);

        if (triggerValue > 0.1)
            lastTriggerValue = Mathf.Min(lastTriggerValue + velocity * Time.deltaTime, 1);
        else
            lastTriggerValue = Mathf.Max(lastTriggerValue - velocity * Time.deltaTime, 0);
        lastTriggerValue = Mathf.Min(lastTriggerValue, triggerValue);

        // Update each finger
        for (int i = handAnimationGrabController_finger1.GetLength(0) - 1; i >= 0; i--)
            UpdateFinger(handAnimationGrabController_finger1[i], "Finger" + (1) + "." + (i + 1), lastTriggerValue, 0, i);

        for (int i = handAnimationGrabController_finger2.GetLength(0) - 1; i >= 0; i--)
            UpdateFinger(handAnimationGrabController_finger2[i], "Finger" + (2) + "." + (i + 1), lastGripValue, 1, i);

        for (int i = handAnimationGrabController_finger3.GetLength(0) - 1; i >= 0; i--)
            UpdateFinger(handAnimationGrabController_finger3[i], "Finger" + (3) + "." + (i + 1), lastGripValue, 2, i);

        for (int i = handAnimationGrabController_finger4.GetLength(0) - 1; i >= 0; i--)
            UpdateFinger(handAnimationGrabController_finger4[i], "Finger" + (4) + "." + (i + 1), lastGripValue, 3, i);

        for (int i = handAnimationGrabController_finger5.GetLength(0) - 1; i >= 0; i--)
            UpdateFinger(handAnimationGrabController_finger5[i], "Finger" + (5) + "." + (i + 1), lastTriggerValue, 4, i);
    }

    // Update each finger
    private void UpdateFinger(HandAnimationGrabController controller , string name, float value, int index, int part)
    {
        // If the grip value is near to zero, we can calculate the finger position again
        if (value < 0.1f)
            lastIterationMove[index, part] = false;

        // If the finger is touching something we don't calculate
        if (lastIterationMove[index, part]) return;

        bool inside = controller.getInside();

        foreach (Animator handAnimator in handAnimators)
            if (handAnimator.isActiveAndEnabled) handAnimator.SetFloat(name, value);
        lastStopValue[index, part] = value;

        if (!inside) return;

        // If the user is grabbing, this finger is not calculated again until the object is realeased
        if (handController.IsGrabbing())
            for (int i = 0; i<=part; i++)
                lastIterationMove[index, i] = true;
    }

    // Update grip value
    public void ReadInput()
    {
        gripValue = deviceInput.GetFloatValue(CommonUsages.grip);
        triggerValue = deviceInput.GetFloatValue(CommonUsages.trigger);
    }
}
