using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class UIToggleController : UIElementForVR
{
    public Toggle Toggle;
    public TMPro.TMP_Text Label;
    public Image Background;
    private bool m_Enabled;
    public GameObject toggleOn;
    public GameObject toggleOff;
    private Color m_SelectedColor = Color.white;
    void Awake()
    {
        m_Enabled = Toggle.isOn;
        toggleOn.SetActive(m_Enabled);
        toggleOff.SetActive(!m_Enabled);
    }

    public void Init(string nameLabel, Color color, bool enabled){
        m_SelectedColor = color;
        Label.text = nameLabel;
        Background.color = new Color(0.25f, 0.25f, .25f); // color;
        SetLabelColor(enabled);
        // Label.color = color;// Color.white;

        Collider.size = new Vector3(Background.rectTransform.rect.width, Background.rectTransform.rect.height, Collider.size.z);
        RectTransform = Background.rectTransform;
        Toggle.SetIsOnWithoutNotify(enabled);

        toggleOn.SetActive(enabled);
        toggleOff.SetActive(!enabled);
    }

    private void SetLabelColor(bool enabled){
        Label.color = enabled? m_SelectedColor:Color.white;// Color.white;
    }

    public void OnToggleClick(bool enabled){
        m_Enabled = enabled;

        toggleOn.SetActive(m_Enabled);
        toggleOff.SetActive(!m_Enabled);
        SetLabelColor(m_Enabled);
    }
    
    override public void OnTriggerEnter_ActionStart(Collider other){
        base.OnTriggerEnter_ActionStart(other);

        m_Enabled = !m_Enabled;
        // Toggle.isOn = m_Enabled;
        GameObject button = Toggle.gameObject;
        base.OnTriggerEnter_ActionEvents(button);
    }

    override public void OnTriggerExit_ActionStart(Collider other){
        base.OnTriggerExit_ActionStart(other);

        GameObject button = Toggle.gameObject;
        base.OnTriggerExit_ActionEvents(button);
    }
}
