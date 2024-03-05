using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NamePanelScript : MonoBehaviour
{
    public TMPro.TMP_InputField nameField;
    public TMPro.TMP_InputField surnameField;
    public Image colorImage;

    public void Init(Color color, string name, string surname) {
        colorImage.color = color;
        nameField.text = name;
        surnameField.text = surname;
    }
}
