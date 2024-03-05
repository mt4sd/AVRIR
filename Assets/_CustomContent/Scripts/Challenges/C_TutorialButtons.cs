using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using static SliderAnalytic;
using static UserAnalytics;

public class C_TutorialButtons : Challenge
{

    // Current State
    private int currentState = 0;
    private int maxState = 9;
    private int preesPerButton = 3;
    private float challengeTime;

    public bool canCheckNextInput = true;
    private float minButtonActivation = 0.8f;
    private int count = 0;

    private XRInitialization myUser;
    private XRDeviceInput leftHand;
    private XRDeviceInput rightHand;

    public int[] pressButtons;

    float waitValue = .3f;

    public void Start()
    {
        challengeID = 1;
    }

    public override void InitStats()
    {
        // Tres etapas, cuando pulsas con la mano derecha el gatillo, despu�s con la izquierda y despu�s con los dos a la vez
        // Ese se repite para el bot�n del mango y para los dos a la vez (un total de 9)
        challengeStats.stats = new ChallengeValueFloat[]
        {
            new ChallengeValueFloat("CurrentStep",0), // N�mero de tareas realizadas (9 en total)
            new ChallengeValueFloat("Time",0),
            new ChallengeValueFloat("Press button r 1",0), // Boton etapa 1_1
            new ChallengeValueFloat("Press button l 1",0), // Boton etapa 1_2
            new ChallengeValueFloat("Press button b 1",0), // Boton etapa 1_3
            new ChallengeValueFloat("Press button r 2",0), // Boton etapa 2_1
            new ChallengeValueFloat("Press button l 2",0), // Boton etapa 2_2
            new ChallengeValueFloat("Press button b 2",0), // Boton etapa 2_3
            new ChallengeValueFloat("Press button r 12",0), // Boton etapa 3_1
            new ChallengeValueFloat("Press button l 12",0), // Boton etapa 3_2
            new ChallengeValueFloat("Press button b 12",0), // Boton etapa 3_3
            
            // x_1 derecha
            // x_2 izquierda
            // x_3 ambos
        };

        currentState = 0;
        myUser = UserInitializerController.Instance.GetMainUser();
        challengeTime = 0;

        rightHand = myUser.trackedPoseDriverR.GetComponent<XRDeviceInput>();
        leftHand = myUser.trackedPoseDriverL.GetComponent<XRDeviceInput>();
    }

    public override void EvaluateStats()
    {
        if (count >= preesPerButton)
        {
            currentState++;
            count = 0;
        }
        if (currentState >= maxState)
        {
            ChallengeStats.SetValue(challengeStats, "CurrentStep", currentState);
            challengeRunning = false;
            challengeFinish = true;
            return;
        }
        CheckState();
        
        ChallengeStats.SetValue(challengeStats, "CurrentStep", currentState);
        ChallengeStats.SetValue(challengeStats, "CurrentCheckState", count);

        ChallengeStats.SetValue(challengeStats, "Time", challengeTime);

        // for (int i = 0; i<maxState; i++) ChallengeStats.SetValue(challengeStats, "Incorrect button" + (i + 1), statesFails[i]);
    }

    private float gripValueRight;
    private float triggerValueRight;
    private float gripValueLeft;
    private float triggerValueLeft;
    private void CheckState(bool checkLater=true)
    {
        challengeTime += Time.deltaTime;

        gripValueRight = rightHand.GetFloatValue(CommonUsages.grip);
        triggerValueRight = rightHand.GetFloatValue(CommonUsages.trigger);

        gripValueLeft = leftHand.GetFloatValue(CommonUsages.grip);
        triggerValueLeft = leftHand.GetFloatValue(CommonUsages.trigger);

        int num_button_press = IsPressedFloat(triggerValueRight) + IsPressedFloat(triggerValueLeft) + IsPressedFloat(gripValueRight) + IsPressedFloat(gripValueLeft);

        if (num_button_press == 0)
        {
            canCheckNextInput = true;
        }
        if (!canCheckNextInput) return;

        if (currentState == 0)
        {
            if (IsPressed(triggerValueRight) && num_button_press == 1)
            {
                canCheckNextInput = false;
                count++;

                CheckButtonToAnalytic();
                return;
            }
        }
        else if (currentState == 1)
        {
            if (IsPressed(triggerValueLeft) && num_button_press == 1)
            {
                canCheckNextInput = false;
                count++;

                CheckButtonToAnalytic();
                return;
            }

        }
        else if (currentState == 2)
        {
            if (IsPressed(triggerValueRight) && IsPressed(triggerValueLeft) && num_button_press == 2)
            {
                canCheckNextInput = false;
                count++;

                CheckButtonToAnalytic();
                return;
            }
            else if (checkLater&& num_button_press > 0)
            {
                StartCoroutine(WaitForCheacking());
                return;
            }

        }
        else if (currentState == 3)
        {
            if (IsPressed(gripValueRight) && num_button_press == 1)
            {
                canCheckNextInput = false;
                count++;

                CheckButtonToAnalytic();
                return;
            } 

        }
        else if (currentState == 4)
        {
            if (IsPressed(gripValueLeft) && num_button_press == 1)
            {
                canCheckNextInput = false;
                count++;

                CheckButtonToAnalytic();
                return;
            }

        }
        else if (currentState == 5)
        {
            if (IsPressed(gripValueRight) && IsPressed(gripValueLeft) && num_button_press == 2)
            {
                canCheckNextInput = false;
                count++;

                CheckButtonToAnalytic();
                return;
            }
            else if (checkLater && num_button_press > 0)
            {
                StartCoroutine(WaitForCheacking());
                return;
            }

        }
        else if (currentState == 6)
        {
            if (IsPressed(triggerValueRight) && IsPressed(gripValueRight) && num_button_press == 2)
            {
                canCheckNextInput = false;
                count++;
                CheckButtonToAnalytic();
                return;
            }
            else if (checkLater && num_button_press > 0)
            {
                StartCoroutine(WaitForCheacking());
                return;
            }

        }
        else if (currentState == 7)
        {
            if (IsPressed(triggerValueLeft) && IsPressed(gripValueLeft) && num_button_press == 2)
            {
                canCheckNextInput = false;
                count++;
                CheckButtonToAnalytic();
                return;
            }
            else if (checkLater && num_button_press > 0)
            {
                StartCoroutine(WaitForCheacking());
                return;
            }

        }
        else if (currentState == 8)
        {
            if (IsPressed(gripValueRight) && IsPressed(gripValueLeft) &&
                    IsPressed(triggerValueRight) && IsPressed(triggerValueLeft) && num_button_press == 4)
            {
                canCheckNextInput = false;
                count++;
                CheckButtonToAnalytic();
                return;
            }
            else if (checkLater && num_button_press > 0)
            {
                StartCoroutine(WaitForCheacking());
                return;
            }
        }

        if (num_button_press > 0)
        {
            if (canCheckNextInput) CheckButtonToAnalytic();
            canCheckNextInput = false;
        }
    }

    public void CheckButtonToAnalytic()
    {
        // 1 trigger, 2 grab
        string buttonString;

        if (IsPressed(gripValueRight) && IsPressed(gripValueLeft) && IsPressed(triggerValueRight) && IsPressed(triggerValueLeft))
            buttonString = "Press button b 12";
        else if (!IsPressed(gripValueRight) && IsPressed(gripValueLeft) && !IsPressed(triggerValueRight) && IsPressed(triggerValueLeft))
            buttonString = "Press button l 12";
        else if (IsPressed(gripValueRight) && !IsPressed(gripValueLeft) && IsPressed(triggerValueRight) && !IsPressed(triggerValueLeft))
            buttonString = "Press button r 12";
        else if (!IsPressed(gripValueRight) && IsPressed(gripValueLeft) && !IsPressed(triggerValueRight) && !IsPressed(triggerValueLeft))
            buttonString = "Press button l 2";
        else if (IsPressed(gripValueRight) && !IsPressed(gripValueLeft) && !IsPressed(triggerValueRight) && !IsPressed(triggerValueLeft))
            buttonString = "Press button r 2";
        else if (IsPressed(gripValueRight) && IsPressed(gripValueLeft) && !IsPressed(triggerValueRight) && !IsPressed(triggerValueLeft))
            buttonString = "Press button b 2";
        else if (!IsPressed(gripValueRight) && !IsPressed(gripValueLeft) && !IsPressed(triggerValueRight) && IsPressed(triggerValueLeft))
            buttonString = "Press button l 1";
        else if (!IsPressed(gripValueRight) && !IsPressed(gripValueLeft) && IsPressed(triggerValueRight) && !IsPressed(triggerValueLeft))
            buttonString = "Press button r 1";
        else if (!IsPressed(gripValueRight) && !IsPressed(gripValueLeft) && IsPressed(triggerValueRight) && IsPressed(triggerValueLeft))
            buttonString = "Press button b 1";
        else
            return;

        ChallengeStats.SetValue(challengeStats, buttonString, ChallengeStats.GetValue(challengeStats, buttonString) + 1);
    }

    public bool IsPressed(float value)
    {
        return value > minButtonActivation;
    }
    public int IsPressedFloat(float value)
    {
        if (value > minButtonActivation) return 1;
        else return 0;
    }

    // Wait for all buttons
    IEnumerator WaitForCheacking()
    {
        canCheckNextInput = false;

        yield return new WaitForSeconds(waitValue);
        canCheckNextInput = true;
        CheckState(false);
    }

    public override ChallengeValueFloatSlider[] ShowStats(int userID, int unitID, int challengeID, int infoStatIndex)
    {
        List<ChallengeValueFloat> challengeStat = GetUserAnalytic(userID, unitID, challengeID, infoStatIndex);

        if (challengeStat == null)
        {
            return null;
        }

        int statCurrentState = (int) ChallengeStats.GetValue(challengeStat, "CurrentStep");

        return new ChallengeValueFloatSlider[]
        {
            //new ChallengeValueString("Name",AnalyticsManager.GetInstance().userAnalytics[i].name),
            //new ChallengeValueString("State", statCurrentState >8 ? "Finished": "Not finished"),
            //new ChallengeValueString("Current State", GetCurrentState(statCurrentState)),
            new ChallengeValueFloatSlider("Finish", statCurrentState >8 ? 1: 0), // 1 finish, 0 not finish
            new ChallengeValueFloatSlider("Challenge Time (s)", (float) Math.Round(ChallengeStats.GetValue(challengeStat, "Time")), 
                new float[]{30.0f, 40.0f}), // Time para superar la tarea
            new ChallengeValueFloatSlider("Right: Button 1", GetButtonMarkOverOne(challengeStat, "Press button r 1", 0),
                new float[] {0, 2, 3, 6},
                new SliderType[] {SliderType.Red, SliderType.Yellow, SliderType.Green, SliderType.Yellow,  SliderType.Red}),
            new ChallengeValueFloatSlider("Right: Button 2", GetButtonMarkOverOne(challengeStat, "Press button r 2", 3),
                new float[] {0, 2, 3, 6},
                new SliderType[] {SliderType.Red, SliderType.Yellow, SliderType.Green, SliderType.Yellow,  SliderType.Red}),
            new ChallengeValueFloatSlider("Right: Button 1 & 2", GetButtonMarkOverOne(challengeStat, "Press button r 12", 6),
                new float[] {0, 2, 3, 6},
                new SliderType[] {SliderType.Red, SliderType.Yellow, SliderType.Green, SliderType.Yellow,  SliderType.Red}),
            new ChallengeValueFloatSlider("Left:   Button 1", GetButtonMarkOverOne(challengeStat, "Press button l 1", 1),
                new float[] {0, 2, 3, 6},
                new SliderType[] {SliderType.Red, SliderType.Yellow, SliderType.Green, SliderType.Yellow,  SliderType.Red}),
            new ChallengeValueFloatSlider("Left:   Button 2", GetButtonMarkOverOne(challengeStat, "Press button l 2",4),
                new float[] {0, 2, 3, 6},
                new SliderType[] {SliderType.Red, SliderType.Yellow, SliderType.Green, SliderType.Yellow,  SliderType.Red}),
            new ChallengeValueFloatSlider("Left:   Button 1 & 2", GetButtonMarkOverOne(challengeStat, "Press button l 12",7),
                new float[] {0, 2, 3, 6},
                new SliderType[] {SliderType.Red, SliderType.Yellow, SliderType.Green, SliderType.Yellow,  SliderType.Red}),
            new ChallengeValueFloatSlider("Both:  Button 1", GetButtonMarkOverOne(challengeStat, "Press button b 1",2),
                new float[] {0, 2, 3, 6},
                new SliderType[] {SliderType.Red, SliderType.Yellow, SliderType.Green, SliderType.Yellow,  SliderType.Red}),
            new ChallengeValueFloatSlider("Both:  Button 2", GetButtonMarkOverOne(challengeStat, "Press button b 2",5),
                new float[] {0, 2, 3, 6},
                new SliderType[] {SliderType.Red, SliderType.Yellow, SliderType.Green, SliderType.Yellow,  SliderType.Red}),
            new ChallengeValueFloatSlider("Both:  Button 1 & 2", GetButtonMarkOverOne(challengeStat, "Press button b 12",8),
                new float[] {0, 2, 3, 6},
                new SliderType[] {SliderType.Red, SliderType.Yellow, SliderType.Green, SliderType.Yellow,  SliderType.Red}),
        };
    }

    public float GetButtonMarkOverOne(List<ChallengeValueFloat> challengeStat, string buttonName, int currentIndex)
    {
        float value = ChallengeStats.GetValue(challengeStat, buttonName); 

        return value;
        /*
        int statCurrentState = (int)ChallengeStats.GetValue(challengeStat, "CurrentStep");
        if (statCurrentState < currentIndex) return 1;
        
        float maxPressToCount = 10;

        if (value == preesPerButton) return 0.15f;
        else if (value == 0) return 1;
        else if (value < preesPerButton)
            return 0.2f + ((0.6f - 0.2f) * ((preesPerButton - value)) / preesPerButton);
        else
            return 0.6f + (1f - 0.6f) * ((maxPressToCount - (maxPressToCount - value))- preesPerButton) / maxPressToCount;*/
    }
}
