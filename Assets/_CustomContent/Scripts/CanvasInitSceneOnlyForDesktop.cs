using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class CanvasInitSceneOnlyForDesktop : MonoBehaviour
{
    public IpadFollowUserScript IpadFollowUser;
    public TMPro.TextMeshProUGUI CurrentModeText;
    public CustomNetworkRoomManager RoomManager;
    public Button SetDesktopButton;
    public Button SetVRButton;
    public GameObject PopupCloseApp;
    private XRInitialization CurrentPlayer;
    private string CurrentSelectedType="";

    // Start is called before the first frame update
    void Start()
    {   
        StartCoroutine(DelayedStart());
        Application.runInBackground = true;
    }

    // To wait for user to be instantiated before start
    private IEnumerator DelayedStart(){
        yield return new WaitUntil(()=> GetCurrentPlayer() != null );
        OnClickCloseApp_Cancel();

        CurrentSelectedType = Application.platform == RuntimePlatform.Android || OVRInput.IsControllerConnected(OVRInput.Controller.Hands)?"VR":"Desktop";// "Desktop";//"VR";
        UpdateModeText();
        
        if(Application.platform != RuntimePlatform.WindowsPlayer && Application.platform != RuntimePlatform.WindowsEditor){
            Destroy(gameObject);
        }
        
        if(Application.platform == RuntimePlatform.WebGLPlayer){
            OnClickButtonSetDesktop();
        }else{
            if(CurrentSelectedType.Equals("Desktop")){
                OnClickButtonSetDesktop();
            }else{
                OnClickButtonSetVR();
            }
        }
    }

    private void UpdateModeText(){
        CurrentModeText.text = "Current mode:\n<b>"+CurrentSelectedType;
        RoomManager.LocalSelectedXRType = CurrentSelectedType.Equals("VR")?XRInitialization.XRType.Controller:XRInitialization.XRType.Desktop;
    }

    private XRInitialization GetCurrentPlayer(){
        if(CurrentPlayer == null){
            CurrentPlayer = GameObject.FindObjectOfType<XRInitialization>();
        }

        return CurrentPlayer;
    }

    public void OnClickButtonSetDesktop(){
        SetDesktopButton.interactable = false;
        SetVRButton.interactable = true;
        CurrentSelectedType="Desktop";
        XRInitialization currentPlayer = GetCurrentPlayer();

        currentPlayer.ForceDesktop();
        UpdateModeText();

        InitSceneCanvasController initSceneCanvas = GameObject.FindObjectOfType<InitSceneCanvasController>();
        if(initSceneCanvas!=null){
            // initSceneCanvas.ButtonStartClient.interactable = false;
            initSceneCanvas.ButtonStartClient.gameObject.SetActive(false);
        }

        IpadFollowUser.head = currentPlayer.DesktopPlayer.PlayerCamera.transform;
        IpadFollowUser.SpeedMove = 10;
    }

    public void OnClickButtonSetVR(){
        SetDesktopButton.interactable = true;
        SetVRButton.interactable = false;
        CurrentSelectedType="VR";
        IpadFollowUser.SpeedMove = 1;
        UpdateModeText();

        InitSceneCanvasController initSceneCanvas = GameObject.FindObjectOfType<InitSceneCanvasController>();
        if(initSceneCanvas!=null){
            // initSceneCanvas.ButtonStartClient.interactable = true;
            initSceneCanvas.ButtonStartClient.gameObject.SetActive(true);
        }

        XRInitialization player = GetCurrentPlayer();
        if(player){
            player.ForceVR();
            IpadFollowUser.head = player.XRCamera.transform;
        }
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape)){
            OnClickCloseApp();
        }
    }

    public void OnClickMaximizeApp(){
        Screen.fullScreen = !Screen.fullScreen;
    }

    public void OnClickCloseApp(){
        PopupCloseApp.SetActive(true);
    }

    public void OnClickCloseApp_Cancel(){
        PopupCloseApp.SetActive(false);
    }

    public void OnClickCloseApp_Confirm(){
        Utils.CloseApp();
    }

    [Button]
    public void OnClickToggleModePC(){
        if(RoomManager.isNetworkActive){
            return;
        }

        if(CurrentSelectedType.Equals("Desktop")){
            // if(OVRInput.IsControllerConnected(OVRInput.Controller.Hands)){
                OnClickButtonSetVR();
            // }else{
            //     Debug.Log("Not headset connected for VR. Setting Desktop Mode.");
            //     OnClickButtonSetDesktop();
            // }
        }else{
            OnClickButtonSetDesktop();
        }
    }
}
