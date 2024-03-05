using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NamePanelGroup : MonoBehaviour
{
    public NamePanelScript[] namePanels;

    private List<int> userNaming;

    private bool inChallenge;

    public void Init(bool inChallenge) {
        userNaming = new List<int>();
        foreach(NamePanelScript namePanel in namePanels)
            namePanel.gameObject.SetActive(false);

        List<UserInitializerController.UserData> users = UserInitializerController.Instance.UsersData; 
        int index = 0;

        foreach (var user in users) 
            if (user.IsConnected) {
                namePanels[index].gameObject.SetActive(true);
                namePanels[index].Init(user.Color, user.UserName, user.UserSurname);
                index++;

                userNaming.Add(user.UniqueID);
            }

        this.inChallenge = inChallenge;
    }

    public void AcceptButton() {
        List<UserInitializerController.UserData> users = UserInitializerController.Instance.UsersData; 
        int index = 0;

        foreach (int userID in userNaming) {
                users[userID].UserName = namePanels[index].nameField.text;
                users[userID].UserSurname = namePanels[index].surnameField.text;
                index++;
        }

        this.gameObject.SetActive(false);
        if (inChallenge && NotEmptyName()) {
            GameObject.FindObjectOfType<PanelTabletHostInGame>().SelectTab((int) PanelTabletHostInGame.TabPanel.Challenges);
        }

        OnlineUtilities.GetOnlineUtilities().SendUserNames();
    }

    private bool NotEmptyName()
    {
        foreach (var namePanel in namePanels)
            if (namePanel.gameObject.activeSelf && (namePanel.nameField.text == "" && namePanel.surnameField.text == ""))
                return false;

        return true;
    }

    public void CancelButton() {

        this.gameObject.SetActive(false);
    }
}
