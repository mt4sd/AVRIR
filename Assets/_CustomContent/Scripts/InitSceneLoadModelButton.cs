using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class InitSceneLoadModelButton : MonoBehaviour
{
    public TMPro.TextMeshProUGUI Label;
    public Image IconImage;
    public Button Button;
    private InitSceneCanvasController m_CanvasController;
    // private string Path;
    private InitSceneCanvasController.ResourcesModelData m_Data;
    private int m_DataID;

    public void Init(InitSceneCanvasController canvasController, int dataID, InitSceneCanvasController.ResourcesModelData data){
        m_CanvasController = canvasController;
        Label.text = data.Name;
        IconImage.sprite = data.Icon;
        // Path = data.Path;
        m_Data = data;
        m_DataID = dataID;
    }

    [Button]
    public void OnClickButton(){
        m_CanvasController.OnSelectModelButton(this);
    }

    public void SetInteractable(bool interactable){
        Button.interactable = interactable;
    }

    public InitSceneCanvasController.ResourcesModelData GetData(){
        return m_Data;
    }
    public int GetDataID(){
        return m_DataID;
    }
}