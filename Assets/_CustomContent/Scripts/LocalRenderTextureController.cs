using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalRenderTextureController : MonoBehaviour
{
    public Camera RenderTextureCamera;
    public TMPro.TextMeshPro NamePlayerText;
    public MeshRenderer BGColor;
    public Transform QuadTexture;
    public float VerticalDistanceFromHead = 1;
    public GameObject FakeLocalHeadParent;
    public List<UserHead> HeadsList;
    private UserHead FakeHead;

    private UserInitializerController.UserData m_SelectedPlayer;

    public void InitData(UserInitializerController.UserData player){
        if(player == null){
            Debug.LogError("LocalRenderTextureController - Cannot find player user data");
            return;
        }

        m_SelectedPlayer = player;
        BGColor.material.color = player.Color;

        NamePlayerText.text = Utils.GetNameFromAnalytics(m_SelectedPlayer);
        NamePlayerText.color = m_SelectedPlayer.Color;
        gameObject.SetActive(true);
        
        foreach(UserHead head in HeadsList){
            head.gameObject.SetActive(false); // Hide all others
            head.transform.localPosition = Vector3.zero;
            head.transform.localEulerAngles = Vector3.zero;
        }

        
        FakeHead = HeadsList[UserInitializerController.Instance.GetMainUser().UserDataSynchronized.AvatarId];
        FakeHead.RecolorMeshes(UserInitializerController.Instance.GetMainUser().PlayerColor);

        FakeHead.gameObject.SetActive(true);
    }

    public void Hide(){
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(gameObject.activeInHierarchy){
            if(m_SelectedPlayer != null && m_SelectedPlayer.XRObject.head != null){
                RenderTextureCamera.transform.position = m_SelectedPlayer.XRObject.XRCamera.transform.position;
                RenderTextureCamera.transform.rotation = m_SelectedPlayer.XRObject.XRCamera.transform.rotation;

                FakeLocalHeadParent.transform.position = UserInitializerController.Instance.GetMainUser().XRCamera.transform.position;
                FakeLocalHeadParent.transform.rotation = UserInitializerController.Instance.GetMainUser().XRCamera.transform.rotation;
            }

            QuadTexture.LookAt(UserInitializerController.Instance.GetMainUser().XRCamera.transform, Vector3.up);
            QuadTexture.position = m_SelectedPlayer.XRObject.XRCamera.transform.position + Vector3.up * VerticalDistanceFromHead;
        }
    }
}