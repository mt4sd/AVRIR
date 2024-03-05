using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UserAnalytics;

public class PanelUserInformation : MonoBehaviour
{
    public int userID;
    public GameObject challengePanel;
    public GameObject disconnectedPanel;

    public TMPro.TMP_Text[] valueLabel;

    // Start is called before the first frame update
    void Start()
    {
        Color color = UserInitializerController.Instance.UsersData[userID].Color;
        color.a = 0.73f;
        this.GetComponent<Image>().color = color;
    }

    public void SetDisconnected()
    {
        disconnectedPanel.SetActive(true);
        challengePanel.SetActive(false);
    }
    public void SetUserInfo(UserAnalytics analytic)
    {
        challengePanel.SetActive(true);
        disconnectedPanel.SetActive(false);

        if (!UserInitializerController.Instance.UsersData[userID].IsConnected)
        {
            SetDisconnected();
            return;
        }
        if (AnalyticsManager.GetInstance().lastChallenge == null)
            SetNormalValues(analytic);
        else SetChallengeValues();

    }

    private void SetChallengeValues()
    {
        List<ChallengeValue[]> stats = new List<ChallengeValue[]>(); // AnalyticsManager.GetInstance().lastChallenge.ShowStats();

        Debug.LogError("Not completed");

        if (stats == null) return;
        ChallengeValue[] values = stats[userID];
        if (values == null) return;

        this.GetComponentInParent<AnalyticPanel>().SetTitles(values);

        for (int i = 0; i < valueLabel.Length; i++)
        {
            if (i < values.Length)
            {
                valueLabel[i].GetComponentInChildren<TMPro.TextMeshProUGUI>().text = values[i].ToString();
            }
            else
                valueLabel[i].GetComponentInChildren<TMPro.TextMeshProUGUI>().text = " ";
        }
        
    }

    private void SetNormalValues(UserAnalytics analytic)
    {
        this.GetComponentInParent<AnalyticPanel>().SetDefaultAnalyticTitle();

        valueLabel[0].text = analytic.colorName;
        valueLabel[1].text = analytic.appID.ToString();

        float timeN = Mathf.Round(analytic.timeInApplication);
        valueLabel[2].text = Utils.GetTimeString(timeN); // Mathf.Floor(timeN / 60).ToString() + ":" + (timeN % 60).ToString();
        valueLabel[3].text = Math.Round(analytic.headMovement, 2).ToString();
        valueLabel[4].text = Math.Round(analytic.leftHandMovement, 2).ToString();
        valueLabel[5].text = Math.Round(analytic.rightHandMovement, 2).ToString();

        valueLabel[6].text = " ";
        valueLabel[7].text = " ";
    }

}
