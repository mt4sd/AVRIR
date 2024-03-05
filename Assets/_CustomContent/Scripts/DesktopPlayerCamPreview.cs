using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DesktopPlayerCamPreview : MonoBehaviour
{
    public RawImage CameraImage;
    public Image BGColor;
    public TMPro.TextMeshProUGUI TextName;
    private DesktopPlayerController m_PlayerController;
    private UserInitializerController.UserData m_Data;

    public void Init(string namePlayer, Color color, RenderTexture texture, UserInitializerController.UserData data, DesktopPlayerController playerController=null){
        m_PlayerController = playerController;
        CameraImage.texture = texture;
        TextName.text = namePlayer;
        TextName.color = color;
        
        BGColor.color = color;
        m_Data = data;
    }

    public UserInitializerController.UserData GetUserData(){
        return m_Data;
    }

    public void OnClickCamPreview(){
        if(m_PlayerController != null){
            m_PlayerController.OnClickMaximizeUser(this);
        }
    }
}
