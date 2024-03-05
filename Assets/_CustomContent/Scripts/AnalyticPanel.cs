using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UserAnalytics;
using UnityEngine.UI;

public class AnalyticPanel : MonoBehaviour
{
    public SliderAnalytic[] analytics;

    public GameObject ChallengePanel;
    public GameObject StatPanel;

    public TMPro.TextMeshProUGUI ChallengeText;
    public TMPro.TextMeshProUGUI ChallengeSubText;
    public TMPro.TextMeshProUGUI ChallengeAnalyticText;

    public Image ChallengeImage;

    public GameObject[] titlesLabel;
    public UserIcon[] userIcons;
    public GameObject cancelIcon;

    private AnalyticsManager analyticManager;
    public bool inChallenge = false;
    public bool showChallenge = false;

    private void Awake()
    {
        StatPanel.SetActive(false);
        ChallengePanel.SetActive(false);
        SetCancelState(false);
    }

    public void Update()
    {
        bool activateChallengePanel = inChallenge && !cancelIcon.activeSelf;
        ChallengePanel.SetActive(activateChallengePanel);
        StatPanel.SetActive(!activateChallengePanel && showChallenge);

        if (analyticManager.lastChallenge == null)
            StatPanel.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        // UpdateInformation();

        ChallengeAnalyticText.text = "";

        for (int i = 0; i < analytics.Length; i++)
            analytics[i].backGroundImage.enabled = (i % 2 == 0);

        analyticManager = AnalyticsManager.GetInstance();
    }
    public void SetCancelState(bool state)
    {
        cancelIcon.SetActive(state);
    }
    
    internal void UpdateInformation(int currentShownUser, int unitID, int challengeID, int infoStatIndex)
    {
        Challenge challenge = analyticManager.GetChallengeToCheck(unitID, challengeID);

        if (challenge != null)
        {
            for (int currentID = 0; currentID < userIcons.Length; currentID++)
            {
                userIcons[currentID].SetSelected(currentID == currentShownUser);
                userIcons[currentID].SetGoodBad(false, false);
                userIcons[currentID].userID = currentID;
            }

            ChallengeValueFloatSlider[] values = challenge.ShowStats(currentShownUser, unitID, challengeID, infoStatIndex);

            if (values != null)
            {
                string userText = "User: " + AnalyticsManager.GetInstance().userAnalytics[currentShownUser].colorName;
                userIcons[currentShownUser].SetGoodBad(values[0].statValue == 1, values[0].statValue != 1);

                ChallengeSubText.text = userText;

                int index;
                for (index = 1; index < values.Length && index - 1 < analytics.Length; index++)
                {
                    analytics[index - 1].gameObject.SetActive(true);
                    analytics[index - 1].text.text = values[index].statName;
                    analytics[index - 1].SetSliderValue(values[index].statValue, values[index].maxStatesValue, values[index].maxTypes);
                    //, values[index].maxGreenValue, values[index].maxYellowValue);
                }

                for (; index <= analytics.Length; index++)
                {
                    analytics[index - 1].gameObject.SetActive(false);
                }
            } else {
                for (int index = 1; index <= analytics.Length; index++)
                {
                    analytics[index - 1].gameObject.SetActive(false);
                }
            }
        }
    }

    public void ShowChallenge(bool enable, ScriptableChallenge challenge){

        inChallenge = enable;

        if (challenge!=null)
        {
            ChallengeAnalyticText.text = challenge.Name;

            InitUserIcons();
        }
         
        // ChallengeText.text = currentChallengeText;
        if (enable && challenge != null){
            ChallengeImage.sprite = challenge.VRScreensImage;
        }
    }

    public void InitUserIcons() {
        List<UserInitializerController.UserData> list = UserInitializerController.Instance.UsersData;
        for (int currentID = 0; currentID < list.Count && currentID < userIcons.Length; currentID++)
        {
            userIcons[currentID].SetIconColor(list[currentID].Color);
            userIcons[currentID].gameObject.SetActive(list[currentID].IsConnected);
        }
    }

    public void SetTitles(ChallengeValue[] values)
    {
        for (int i = 0; i < titlesLabel.Length; i++)
        {
            titlesLabel[i].SetActive(true);
            if (i < values.Length)
            {
                titlesLabel[i].GetComponentInChildren<TMPro.TextMeshProUGUI>().text = values[i].statName;
            }
            else
                titlesLabel[i].SetActive(false);
        }
    }

    public void SetDefaultAnalyticTitle()
    {
        titlesLabel[0].GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "Name";
        titlesLabel[1].GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "Position";
        titlesLabel[2].GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "Time";
        titlesLabel[3].GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "Head";
        titlesLabel[4].GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "Left Hand";
        titlesLabel[5].GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "Right Hand";
        titlesLabel[6].SetActive(false);
        titlesLabel[7].SetActive(false);
    }

    internal void SetInfoShow(bool show)
    {
        showChallenge = show;
    }
}
