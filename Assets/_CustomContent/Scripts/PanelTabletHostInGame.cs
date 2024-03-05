using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System;

public class PanelTabletHostInGame : MonoBehaviour
{
    public enum TabPanel{
        Settings,
        General,
        Challenges
    }

    public GameObject SettingsPanel;
    public GameObject GeneralPanel;
    public GameObject ChallengesPanel;
    public GameObject InformationPanel;

    public TabPanel Tab;
    public GameObject tabButtonsParent;
    public GameObject informationButton;
    public GameObject informationPrevButton;
    public GameObject informationNextButton;
    // public Transform GridParent;
    [Header("Challenge References")]
    public ScriptableChallengeManagement SelectedDocentUnit;
    // public GameObject SelectedChallengePanel;
    public Transform ChallengesListParent;
    public Transform ChallengeEnabledParent;
    public TMPro.TextMeshProUGUI EnabledChallengeDescription;

    public UITabletChallengeButton ChallengeUIButtonPrefab;
    public TMPro.TextMeshProUGUI DocentUnitDataText;
    public TMPro.TextMeshProUGUI ChallengeDataText;
    public TMPro.TextMeshProUGUI ChallengeTitleText;
    public Image ChallengePrevisualizationImage;
    public Button ButtonStartChallenge;
    public Button ButtonStopChallenge;
    // public Button ButtonViewLastResults;
    public TMPro.TextMeshProUGUI NameChallenge;
    public Image IconChallenge;
    public Image ImageTutorialChallenge;
    private Challenge m_CurrentChallengeObject;

    private void RefreshChallengesTab(){
        DocentUnitDataText.text = SelectedDocentUnit.NameDocentUnit;

        Utils.ClearChilds(ChallengesListParent);

        foreach(ScriptableChallenge challenge in SelectedDocentUnit.ChallengesList){
            UITabletChallengeButton challengeButton = Instantiate(ChallengeUIButtonPrefab, ChallengesListParent);
            challengeButton.InitData(this,challenge);

            if(challenge == Challenge.CurrentChallengeData){
                challengeButton.Select();
            }
        }

        bool isChallengeActived = Challenge.CurrentChallengeData != null && Challenge.CurrentChallengeData.isActived && !Challenge.CurrentChallengeData.IsFinished;
        ButtonStartChallenge.gameObject.SetActive(!isChallengeActived);//interactable = !isChallengeActived;
        ButtonStopChallenge.gameObject.SetActive(isChallengeActived);//.interactable = isChallengeActived;

        // ButtonViewLastResults.interactable = Challenge.CurrentChallengeData != null && Challenge.CurrentChallengeData.IsFinished && !Challenge.CurrentChallengeData.isActived;

        if(isChallengeActived){
            NameChallenge.text = Challenge.CurrentChallengeData.Name;
            IconChallenge.sprite = Challenge.CurrentChallengeData.Icon;
            ImageTutorialChallenge.sprite = Challenge.CurrentChallengeData.VRScreensImage;
        }

        // SelectedChallengePanel.SetActive(Challenge.CurrentChallengeData!=null);
    }

    public void OnClickSelectChallenge(ScriptableChallenge challenge = null){
        bool selectChallenge = challenge != null;
        Challenge.CurrentChallengeData = challenge;

        if(selectChallenge){
            ChallengeDataText.text = /*Challenge.CurrentChallengeData.Name + "\n\n"+*/Challenge.CurrentChallengeData.Description;
            ChallengeTitleText.text = Challenge.CurrentChallengeData.Name;
            ChallengePrevisualizationImage.sprite = Challenge.CurrentChallengeData.VRScreensImage;
        }

        RefreshChallengesTab();
    }

    public void OnClickToggleChallenge(ScriptableChallenge challenge = null){
        if(Challenge.CurrentChallengeData == null){
            ForceStartChallenge(challenge);
        }else{
            bool sameChallenge = challenge == Challenge.CurrentChallengeData;
            OnClickStopSelectedChallenge();

            if(challenge != null && !sameChallenge){
                // LeanTween.value(gameObject, 0, 1, 0.5f).setOnComplete(()=>{
                    Debug.Log("Force Start Challenge");
                    // ForceStartChallenge(challenge);
                // });
            }
        }

        RefreshChallengesTab();
    }

    private void ForceStartChallenge(ScriptableChallenge challenge){
        OnClickSelectChallenge(challenge);
        OnClickStartSelectedChallenge();
    }

    public void OnClickStartSelectedChallenge(){
        Debug.Log("OnClick Start SelectedChallenge");
        Challenge.CurrentChallengeData.IsFinished = false;
        Challenge.CurrentChallengeData.isActived = true;

        // Do challenge stuff
        AnalyticsManager.GetInstance().StartChallenge(Challenge.CurrentChallengeData);

        // ShowChallengesList(false);

        RefreshChallengesTab();
    }

    public void OnClickShowLastResults(){
        Debug.Log("Show last results of current challenge");
    }

    public void OnClickStopSelectedChallenge(){
        Debug.Log("OnClick Stop SelectedChallenge");
        
        Challenge.CurrentChallengeData.isActived = false;
        Challenge.CurrentChallengeData.IsFinished = true;
        Challenge.CurrentChallengeData = null;

        // ShowChallengesList(true);
        
        AnalyticsManager.GetInstance().ForceEnd();

        RefreshChallengesTab();
    }

    private void ShowChallengesList(bool show){

        ChallengeEnabledParent.gameObject.SetActive(!show);
        ChallengesListParent.gameObject.SetActive(show);

        if(Challenge.CurrentChallengeData){
            EnabledChallengeDescription.text = Challenge.CurrentChallengeData.Description;
        }
    }

    private int lastUnitID;
    private int lastChallengeID;
    public void OnClickChallengeInformation(int unitID, int ChallengeID)
    {
        InformationPanel.SetActive(true);
        ChallengesPanel.SetActive(false);
        tabButtonsParent.SetActive(false);

        lastUnitID = unitID;
        lastChallengeID = ChallengeID;

        InformationPanel.GetComponentInChildren<AnalyticPanel>().SetCancelState(true);

        Challenge c = AnalyticsManager.GetInstance().GetChallengeToCheck(lastUnitID, lastChallengeID);
        Challenge.lastChallengeIndexData = AnalyticsManager.GetInstance().GetHistorialChallenges().Count-1;

        OnlineUtilities.GetOnlineUtilities().SetInformationInPanelState(true, unitID, ChallengeID,
                                                        c.GetUserAnalyticIndex(unitID, ChallengeID),
                                                        AnalyticsManager.GetInstance().GetDataToServer());

        CheckNextAndPrevButton();
    }

    private void CheckNextAndPrevButton()
    {
        Challenge c = AnalyticsManager.GetInstance().GetChallengeToCheck(lastUnitID, lastChallengeID);

        informationPrevButton.SetActive(c.GetPreviousChallengeIndex() != -1);
        informationNextButton.SetActive(c.GetNextChallengeIndex() != -1);
    }

    public void OnClickChallengeInformationBack()
    {
        InformationPanel.SetActive(false);
        ChallengesPanel.SetActive(true);
        tabButtonsParent.SetActive(true);

        OnlineUtilities.GetOnlineUtilities().SetInformationInPanelState(false, -1, -1, -1, null);
    }

    public void OnClickChallengeInformationPrevious()
    {
        Challenge c = AnalyticsManager.GetInstance().GetChallengeToCheck(lastUnitID, lastChallengeID);
        int index = c.GetPreviousChallengeIndex();
        Challenge.lastChallengeIndexData = index;

        OnlineUtilities.GetOnlineUtilities().SetInformationInPanelState(true, lastUnitID, lastChallengeID,
                                                       index,
                                                       AnalyticsManager.GetInstance().GetDataToServer());
        CheckNextAndPrevButton();
    }

    public void OnClickChallengeInformationNext()
    {
        Challenge c = AnalyticsManager.GetInstance().GetChallengeToCheck(lastUnitID, lastChallengeID);
        int index = c.GetNextChallengeIndex();
        Challenge.lastChallengeIndexData = index;
        
        OnlineUtilities.GetOnlineUtilities().SetInformationInPanelState(true, lastUnitID, lastChallengeID,
                                                        index,
                                                        AnalyticsManager.GetInstance().GetDataToServer());
        CheckNextAndPrevButton();
    }

    public void OnClickResetChallenge(){
        Debug.Log("OnClick Reset Challenge");

        ResetChallenges();

        RefreshChallengesTab();

        // ShowPopup("Save & Reset?", "", "Reset", OnClickResetChallenge_Confirm);
    }

    private void ResetChallenges(){
        DesktopPlayerController d = this.GetComponentInParent<DesktopPlayerController>();
        if (d!=null) {
            List<UserInitializerController.UserData> users = UserInitializerController.Instance.UsersData; 
            foreach (var user in users) {
                    user.UserName = "";
                    user.UserSurname = "";
            }
            OnlineUtilities.GetOnlineUtilities().SendUserNames();

            SelectTab((int) PanelTabletHostInGame.TabPanel.General);
            SelectTab((int) PanelTabletHostInGame.TabPanel.Challenges);
        }

        Debug.Log("Reset challenges & Analytics");
        
        foreach(ScriptableChallenge challenge in SelectedDocentUnit.ChallengesList){
            challenge.IsFinished = false;
            challenge.isActived = false;
        }

        AnalyticsManager.GetInstance().ForceEnd();

        // ShowChallengesList(true);
        
        Challenge.CurrentChallengeData = null;
    }

    private void OnClickResetChallenge_Confirm(){
        // Debug.Log("Save data from Analytics on file json - Pending");
        // SaveSessionFile();
        
        ResetChallenges();

        RefreshChallengesTab();

        OnClickCancelPopup();
    }


    [Header("Toggles")]
    public UIToggleController UIToggleTypePrefab;
    public Transform UIToggleParent;
    public UIToggleController NamesToggle;
    public UIToggleController ColorsToggle;

    private UserInitializerController m_UserInitializer;
    private PanelInMorgueController m_PanelInMorgueController;

    private UserInitializerController UserInitializer(){
        if(m_UserInitializer == null){
            m_UserInitializer = GameObject.FindObjectOfType<UserInitializerController>();
        }

        return m_UserInitializer;
    }
    private PanelInMorgueController GetPanelInMorgueController(){
        if(m_PanelInMorgueController == null){
            m_PanelInMorgueController = GameObject.FindObjectOfType<PanelInMorgueController>();
        }

        return m_PanelInMorgueController;
    }

    void Start()
    {
        // ResetChallenges();
        // foreach (Transform child in UIToggleParent)
        // {
        //     Destroy(child.gameObject);
        // }
        // GameObject morgueGameObject = GameObject.Find("_MorgueController");

        // if(morgueGameObject == null)
        //     return;

        RefreshUI();
        // MorgueController Morgue = morgueGameObject.GetComponent<MorgueController>();
        // for (int i = 0; i < Morgue.OrgansTypeInfo.Count; i++)
        // {
        //     MorgueController.OrganTypeInfo info = Morgue.OrgansTypeInfo[i];
        //     UIToggleController newToggle = UIToggleParent.GetChild(i).GetComponent<UIToggleController>();// Instantiate(UIToggleTypePrefab, UIToggleParent);
        //     newToggle.Init(info.Name, info.Color, info.Enabled);

        //     // Event
        //     newToggle.GetComponentInChildren<Toggle>().onValueChanged.AddListener((value) => {
        //         OnlineUtilities.GetOnlineUtilities().EnableOrganType_Event((int)info.Type, value);
        //         //Morgue.OnToggleOrganType(info.Type, value);
        //     });   
        // }

        // OnClickOpenTab_TableMovement();
        OnClickCancelPopup();

        SelectTab((int)TabPanel.General);
        // OnClickOpenTab_TableMovement();

        PanelInMorgueController.OnChangedTabletData += RefreshUI;
        MorgueController.OnChangedTabletData += RefreshUI;

        AnalyticsManager analytic = AnalyticsManager.GetInstance();
        if (analytic != null) {
            AnalyticPanel p = InformationPanel.GetComponentInChildren<AnalyticPanel>();
            analytic.infoPanels.Add(p);
            p.InitUserIcons();

        }
    }

    void OnDestroy()
    {
        PanelInMorgueController.OnChangedTabletData -= RefreshUI;
        MorgueController.OnChangedTabletData -= RefreshUI;
    }

    [Button]
    public void SelectTab(int tabID){
        Tab = (TabPanel)tabID;

        if (Tab == TabPanel.Challenges && CheckUserNamesEmpty()) {
            DesktopPlayerController d = this.GetComponentInParent<DesktopPlayerController>();
            if (d!=null) {
                d.AskUserName(true);
                return;
            }
        }
        SettingsPanel.SetActive(Tab == TabPanel.Settings);
        ChallengesPanel.SetActive(Tab == TabPanel.Challenges);
        GeneralPanel.SetActive(Tab == TabPanel.General);

        if(Tab == TabPanel.Challenges){
            RefreshChallengesTab();
        }

    }
    private bool CheckUserNamesEmpty() {
        
        List<UserInitializerController.UserData> users = UserInitializerController.Instance.UsersData; 

        foreach (var user in users) {
            if (user.IsConnected) {
                if (user.UserName == "" && user.UserName == "") {
                    return true;
                }
            }
        }
        return false;
    }

    private void RefreshUI(){
        
        // GameObject morgueGameObject = GameObject.Find("_MorgueController");
        MorgueController Morgue = MorgueGrabUtilities.morgue;//.GetComponent<MorgueController>();

        if(Morgue == null){
            return;
        }

        for (int i = 0; i < Morgue.OrgansTypeInfo.Count; i++)
        {
            MorgueController.OrganTypeInfo info = Morgue.OrgansTypeInfo[i];
            UIToggleController newToggle = UIToggleParent.GetChild(i).GetComponent<UIToggleController>();// Instantiate(UIToggleTypePrefab, UIToggleParent);
            newToggle.Init(info.Name, info.Color, info.Enabled);

            // Event
            newToggle.GetComponentInChildren<Toggle>().onValueChanged.AddListener((value) => {
                OnlineUtilities.GetOnlineUtilities().EnableOrganType_Event((int)info.Type, value);
                //Morgue.OnToggleOrganType(info.Type, value);
            });   
        }

        NamesToggle.Init("Labels",NamesToggle.Label.color,VRFloatingNameModel.FloatingInfo);
        ColorsToggle.Init("Display colors",ColorsToggle.Label.color,FeedBackUtilities.ShowColorFeedback);
    }

    public void OnClickForceCloseApp(){
        // OnClickOpenTab_Settings();
        SelectTab(1);
        OnClickCloseApp();
    }

    // [Button]
    // public void OnClickOpenTab_Settings(){
    //     SettingsPanel.SetActive(true);
    //     GeneralPanel.SetActive(false);
    // }

    // [Button]
    // public void OnClickOpenTab_TableMovement(){
    //     SettingsPanel.SetActive(false);
    //     GeneralPanel.SetActive(true);
    // }

    public void OnClickToggleColor(bool newVal){
        GetPanelInMorgueController().OnToggleColorRequest(newVal);
    }

    public void OnClickToggleNames(bool newVal){
        GetPanelInMorgueController().OnToggleNameRequest(newVal);
    }

    public void OnClickUpdateHeight(float delta){
        // UserInitializer().UpdateHeight(delta);
        UserInitializer().UpdateTablePosition(-Vector2.up*delta);
    }

    public void OnClickUpdateTablePosition(float delta){
        UserInitializer().UpdateTablePosition(Vector2.right*delta);
    }

    [Button]
    public void OnClickResetDissection(){
        ShowPopup("Reset", "", "Reset", OnClickResetDissection_Confirm);
    }

    [Button]
    public void OnClickCloseApp(){
        ShowPopup("Exit", "", "Exit", OnClickCloseApp_Confirm);
    }

    #region InnerPopup
    public GameObject PopupParent;
    public GameObject PanelsParent;
    public TMPro.TextMeshProUGUI PopupTitle;
    public TMPro.TextMeshProUGUI PopupDescription;
    public TMPro.TextMeshProUGUI PopupConfirmLabel;
    public TMPro.TextMeshProUGUI PopupCancelLabel;
    private System.Action m_PopupCallbackConfirm;
    public void ShowPopup(string title, string description, string confirmText, System.Action callbackConfirm, string cancelText = "", System.Action callbackCancel=null){
        PopupTitle.text = title;
        PopupDescription.text = description;
        PopupConfirmLabel.text = confirmText;
        PopupCancelLabel.text = string.IsNullOrEmpty(cancelText)?"Cancel":cancelText;

        m_PopupCallbackConfirm = callbackConfirm;
        PopupParent.SetActive(true);
        PanelsParent.SetActive(false);
    }

    public void OnClickPopup_Confirm(){
        if(m_PopupCallbackConfirm!=null){
            m_PopupCallbackConfirm();
        }
    }

    [Button]
    public void OnClickCancelPopup(){
        PanelsParent.SetActive(true);
        PopupParent.SetActive(false);
    }
    
    public void OnClickResetDissection_Confirm(){
        GetPanelInMorgueController().OnClickResetDissectionRequest();
        OnClickCancelPopup();
    }

    public void OnClickCloseApp_Confirm(){
        // Close App
        Utils.CloseApp();
    }
    #endregion
}
