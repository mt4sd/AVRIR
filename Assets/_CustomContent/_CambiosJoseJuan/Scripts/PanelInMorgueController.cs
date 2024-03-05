using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelInMorgueController : MonoBehaviour
{
    private MorgueController Morgue;
    // [SerializeField]
    // private UserInitializerController userInitializerController;
    private float HeightChangePerClick = 0.05f;
    private float MoveTableChangePerClick = 0.05f;

    [Header("Toggles")]
    public UIToggleController UIToggleTypePrefab;
    public Transform UIToggleParent;

    private static PanelInMorgueController _instance;

    public static PanelInMorgueController Instance { 
        get { 
            if(_instance==null) {

                _instance = GameObject.FindObjectOfType<PanelInMorgueController>(true); 
            }

            return _instance; 
        } 
    }

    public static Action OnChangedTabletData;


    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            // if(!_instance.gameObject.activeInHierarchy){
            //     Debug.Log("Registering Instance of PanelInMorgueController in gameobjecT: "+gameObject.name);
            //     _instance = this;
            // }else{
                this.gameObject.SetActive(false);
            // }
            // Destroy(this.gameObject);
        }
        else
        {
            if(gameObject.activeInHierarchy){
                Debug.Log("Registering Instance of PanelInMorgueController in gameobjecT: "+gameObject.name);
                _instance = this;
            }
        }
    }

    public void ForceAsGlobalInstance(){
        _instance = this;
        // Debug.Log("Forcing PanelInMorgueGlobalInstance from "+_instance.gameObject.name + " to "+ this.gameObject.name);

    }

    // Start is called before the first frame update
    void Start()
    {
        foreach (Transform child in UIToggleParent)
        {
            Destroy(child.gameObject);
        }
        GameObject morgueGameObject = GameObject.Find("_MorgueController");

        if(morgueGameObject == null)
            return;

        Morgue = morgueGameObject.GetComponent<MorgueController>();
        foreach (MorgueController.OrganTypeInfo info in Morgue.OrgansTypeInfo)
        {
            UIToggleController newToggle = Instantiate(UIToggleTypePrefab, UIToggleParent);
            newToggle.Init(info.Name, info.Color, info.Enabled);

            // Event
            newToggle.GetComponentInChildren<Toggle>().onValueChanged.AddListener((value) => {
                OnlineUtilities.GetOnlineUtilities().EnableOrganType_Event((int)info.Type, value);
                //Morgue.OnToggleOrganType(info.Type, value);
            });   
        }
    }

    private MorgueController GetMorgue(){
        if(Morgue == null){
            GameObject morgueGameObject = GameObject.Find("_MorgueController");
            Morgue = morgueGameObject.GetComponent<MorgueController>();
        }

        return Morgue;
    }


    public void OnToggleNameRequest(bool enabled)
    {
        OnlineUtilities.GetOnlineUtilities().MorgueInformation(enabled);
    }
    internal void InformationMorgue(bool enabled)
    {
        if (UserInitializerController.Instance == null)
        {
            return;
        }

        VRFloatingNameModel.FloatingInfo = enabled;
        foreach (VRFloatingNameModel panel in UserInitializerController.Instance.GetMainUser().GetComponentsInChildren<VRFloatingNameModel>())
        {
            // panel.Show(enabled);
            panel.gameObject.SetActive(enabled);
        }

        if(OnChangedTabletData!=null){
            OnChangedTabletData();
        }
    }

    public void OnToggleColorRequest(bool enabled)
    {

        // Debug.Log("Color Request");
        OnlineUtilities.GetOnlineUtilities().MorgueColor(enabled);
    }

    public void ColorMorgue(bool enabled)
    {
        // Debug.Log("Color");
        // Debug.Log("Color  " + UserInitializerController.Instance.GetMainUser().PlayerID);
        FeedBackUtilities.ShowColorFeedback = enabled;
        if (!enabled)
            foreach (VRGrabbing grabbing in UserInitializerController.Instance.GetMainUser().GetComponentsInChildren<VRGrabbing>())
            {
                if (grabbing.GetGrabbedObject() != null)
                    UserInitializerController.Instance.GetMainUser().currentStation.DeselectLastPointed(grabbing.GetGrabbedObject(), null);
            }

        if(OnChangedTabletData!=null){
            OnChangedTabletData();
        }
    }

    public void OnClickUpdateHeight(int direction){
        // UserInitializerController.Instance.UpdateHeight(direction*HeightChangePerClick);
        UserInitializerController.Instance.UpdateTablePosition(-Vector2.up*direction*MoveTableChangePerClick);
    }

    public void OnClickMoveTable(int direction){
        UserInitializerController.Instance.UpdateTablePosition(Vector2.right*direction*MoveTableChangePerClick);
    }

    public void OnClickResetDissectionRequest()
    {
        OnlineUtilities.GetOnlineUtilities().ResetMorgue();
    }
    public void ResetDissection()
    {
        GetMorgue().OnClickResetMorgue();
    }

    public void EnableOrganType(int IdOrganType, bool enable){
        GetMorgue().OnToggleOrganType((ZAnatomy.ZAnatomyController.OrganType)IdOrganType,enable);
    }
}
