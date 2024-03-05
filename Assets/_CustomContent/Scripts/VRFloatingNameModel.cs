using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class VRFloatingNameModel : MonoBehaviour
{
    public static bool FloatingInfo = false;
    public GameObject ParentContent;
    public float AnimationTime = 0.5f;
    public TMPro.TextMeshPro Text;
    private bool wasOpened;
    public Vector3 Final_Scale;
    public Transform FollowHandTransform;

    void Start()
    {
        Final_Scale = ParentContent.transform.localScale;
        gameObject.SetActive(false);
    }

    public void SetName(string name, Color color){
        Text.text = name;
        Text.color = color;
    }
    
    public void Show(bool show){
        // gameObject.SetActive(show);
        if(show){
            AnimatedShow();
        }

        if(!show){
            AnimatedHide();
        }

        wasOpened = show;
    }

    public void SetPosition(Vector3 position){
        transform.position = position;
    }

    private void AnimatedShow(){
        LeanTween.cancel(ParentContent);
        gameObject.SetActive(true);
        ParentContent.transform.localScale = Vector3.zero;
        LeanTween.scale(ParentContent, Final_Scale, AnimationTime).setEaseOutBack();
    }

    private void AnimatedHide(){
        LeanTween.cancel(ParentContent);
        LeanTween.scale(ParentContent, Vector3.zero, AnimationTime).setEaseOutBack().setOnComplete(()=>{
            gameObject.SetActive(false);
        });
    }

    public void UpdateFloatingPanel(VRGrabbing grabbing, bool active = true)
    {
        if (!active)
        {
            this.gameObject.SetActive(false);
            return;
        }
        if (grabbing.GetGrabbedObject() != null || grabbing.GetBestCandidate() != null)
        {
            ZAnatomy.MeshInteractiveController meshInteractive = grabbing.GetGrabbedObject() != null ? grabbing.GetGrabbedObject() : grabbing.GetBestCandidate();
            int id = meshInteractive.GetID();

            if (FloatingInfo) this.gameObject.SetActive(true);
            this.SetPosition(FollowHandTransform!=null?FollowHandTransform.position:meshInteractive.GetCenterPosition());

            SetNameText(this, id, meshInteractive.GetPublicName());
            return;

        }

        this.gameObject.SetActive(false);
    }
    public static void SetNameText(VRFloatingNameModel floatingNameModel, int id, string publicName)
    {
        //Debug.Log("SetNameText ----- "+id);
        ZAnatomy.DataList.DataModel grabbedData = MorgueGrabUtilities.morgue.GetZAnatomyController().Data.DataModelsList.Find((x) => x.ID == id);// GetActiveModels().Find((x)=>x.ID==id);
        string region = "REGION";
        string name = "Name";
        if (grabbedData != null)
        {
            MorgueController.OrganTypeInfo organTypeInfo = MorgueGrabUtilities.morgue.GetOrganType(grabbedData.BodyPart);
            Color color = organTypeInfo.Color;//Color.white;//Utils.ColorFromHex(grabbedData.Color);

            region = organTypeInfo.Name;
            name = Utils.UpperFirstChar(publicName);
            string text = "<size=50%>" + region + "</size>\n" + name;

            if (string.IsNullOrEmpty(region))
            {
                text = name;
            }

            floatingNameModel.SetName(text, color);
        }
        else
        {
            Debug.LogWarning("SetNameText: Cannot find DataModel for id: " + id + " - Using public Name:" + publicName);
            floatingNameModel.SetName(publicName, Color.white);
        }
    }
}
