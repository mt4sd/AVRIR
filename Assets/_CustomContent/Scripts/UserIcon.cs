using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserIcon : MonoBehaviour
{
    public GameObject goodIcon;
    public GameObject badIcon;
    public GameObject backgroundImage;
    public Image userImage;

    public int userID;

    public void SetIconColor(Color color)
    {
        userImage.color = color;
    }
    public void SetSelected(bool isSelected)
    {
        backgroundImage.SetActive(isSelected);
    }
    public void SetGoodBad(bool isGood, bool isBad)
    {
        goodIcon.SetActive(isGood);
        badIcon.SetActive(isBad);
    }

    public void SetUserID()
    {
        OnlineUtilities.GetOnlineUtilities().SetCurrentShownUser(userID);
    }
}
