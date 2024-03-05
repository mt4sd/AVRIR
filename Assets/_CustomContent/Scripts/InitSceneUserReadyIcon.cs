using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class InitSceneUserReadyIcon : MonoBehaviour
{
    public Image BaseIcon;
    public Sprite HostIconSprite;
    public Sprite UserIconSprite;
    public GameObject ReadyIcon;
    public GameObject LocalPlayerIcon;
    public Sprite ToggleOff;
    public Sprite ToggleOn;
    private Color m_PlayerColor;

    public NetworkConnectionToClient PlayerConnection;
    private int NetID;
    private bool IsUser;
    private int AvatarID = 1;
    public List<Sprite> UserIconSpriteList;

    public Button AdminButton;
    public Image AdminToggleIcon;
    public Button ChangeAvatarButton;
    public static System.Action<int, bool> OnChangeAdminTablet;
    public static System.Action<int, int> OnChangeUserAvatar;

    public int GetNetId(){
        return (int)PlayerConnection.identity.netId;
    }

    public void Init(int netId, int avatarId, NetworkConnectionToClient conn, Color playerColor, bool isLocalPlayer, bool isInvokedByHost, bool enabledTablet){
        NetID = netId;
        PlayerConnection = conn;
        m_PlayerColor = playerColor;

        bool isHost = NetID == 0;
        AvatarID = avatarId;
        // Debug.Log("New user icon: AvatarID: "+avatarId);
        BaseIcon.sprite = isHost?HostIconSprite:UserIconSpriteList[AvatarID];
        ChangeAvatarButton.gameObject.SetActive(!isHost && isInvokedByHost);
        ChangeAvatarButton.image.color = playerColor;

        if(isInvokedByHost && !isLocalPlayer){
            AdminButton.gameObject.SetActive(!isHost);
            // ForceAsUser(false);

            if(enabledTablet){
                ForceAsAdmin();
            }else{
                ForceAsUser(false);
            }
        }else{
            AdminButton.gameObject.SetActive(false);
        }

        SetReady(false);
        LocalPlayerIcon.SetActive(isLocalPlayer);
    }

    public void SetReady(bool ready){
        ReadyIcon.SetActive(ready);

        Color finalBaseColor = m_PlayerColor;
        finalBaseColor.a = ready?1:0.5f;

        BaseIcon.color = finalBaseColor;
    }

    private void ForceAsUser(bool notify=true){
        IsUser = true;
        // AdminButton.image.color = Color.red;
        AdminToggleIcon.sprite = ToggleOff;

        if(OnChangeAdminTablet!=null && notify){
            OnChangeAdminTablet(NetID,!IsUser);
        }
    }

    private void ForceAsAdmin(bool notify=true){
        IsUser = false;
        // AdminButton.image.color = Color.green;
        AdminToggleIcon.sprite = ToggleOn;

        if(OnChangeAdminTablet!=null && notify){
            OnChangeAdminTablet(NetID,!IsUser);
        }
    }

    public bool IsTabletEnabled(){
        return !IsUser;
    }

    public void OnClickToggleAdmin(){
        if(IsUser){
            ForceAsAdmin();
        }else{
            ForceAsUser();
        }
    }

    public void OnClickToggleAvatar(){
        AvatarID++;
        AvatarID = AvatarID % (UserIconSpriteList.Count);

        bool isHost = NetID == 0;
        BaseIcon.sprite = isHost?HostIconSprite:UserIconSpriteList[AvatarID];
        
        if(OnChangeUserAvatar!=null){
            OnChangeUserAvatar(NetID,AvatarID);
        }
    }
}
