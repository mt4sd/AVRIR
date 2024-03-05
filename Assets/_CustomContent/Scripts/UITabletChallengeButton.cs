using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITabletChallengeButton : MonoBehaviour
{
    public PanelTabletHostInGame Tablet;

    public Image BGProgress;
    // public GameObject BGSelection;
    public GameObject IconDone;
    public TMPro.TextMeshProUGUI NameText;
    public Image Icon;

    public Image ToggleIcon;

    public Color ColorFinished;
    public Color ColorActived;
    public Color ColorNotFinished;

    public Sprite ToggleSpriteOn;
    public Sprite ToggleSpriteOff;

    private ScriptableChallenge m_Challenge;

    // Data
    public GameObject statIcon;

    public void InitData(PanelTabletHostInGame tablet, ScriptableChallenge challenge){
        m_Challenge = challenge;
        Tablet = tablet;
        NameText.text = challenge.Name;
        Icon.sprite = challenge.Icon;

        SetButtonState();
    }

    public void Select(){
        // BGSelection.SetActive(true);
    }

    public void OnClickButton(){
        Tablet.OnClickSelectChallenge(m_Challenge);
    }

    public void OnClickToggleButton(){
        Tablet.OnClickToggleChallenge(m_Challenge);
    }

    public void Update()
    {
        SetButtonState();
    }
    public void SetButtonState()
    {
        Challenge c = AnalyticsManager.GetInstance().GetCurrentChallenge();
        bool IsFinished;
        bool isActived;

        if (c != null && c.ChallengeData == m_Challenge) {
            isActived = true;
            IsFinished = false;
        } else 
        {
            isActived = false;
            IsFinished = false;


            // Por ahora no aparece ningún reto como acabado
            // Challenge c2 = AnalyticsManager.GetInstance().lastChallenge;
            // if (c2 != null && c2.ChallengeData == m_Challenge) IsFinished = true;
        }

        BGProgress.color = IsFinished ? ColorFinished : (isActived ? ColorActived : ColorNotFinished);
        IconDone.SetActive(IsFinished);

        ToggleIcon.sprite = isActived ? ToggleSpriteOn : ToggleSpriteOff;

        statIcon.SetActive(InfoOfThisChallenge());
    }

    private bool InfoOfThisChallenge()
    {
        var list = AnalyticsManager.GetInstance().GetHistorialChallenges();

        if (list == null) return false;

        for (int i = list.Count - 1; i >= 0; i--)
        {
            if (AnalyticsManager.unitID == list[i].UnitID && list[i].ChallengeID == m_Challenge.challengeID)
                return true;
        }
        return false;
    }

    public void ShowChallengeInfo()
    {
        this.GetComponentInParent<PanelTabletHostInGame>().OnClickChallengeInformation(AnalyticsManager.unitID, m_Challenge.challengeID);
    }
}
