using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderAnalytic : MonoBehaviour
{
    public enum SliderType
    {
        Red,        Yellow,        Green
    }

    // public Slider redSlider;
    // public Slider yellowSlider;
    // public Slider greenSlider;
    public List<Slider> sliders;
    public List<GameObject> handles;
    public List<TMPro.TextMeshProUGUI> labels;

/*
    public TMPro.TextMeshProUGUI redLabel;
    public TMPro.TextMeshProUGUI yellowLabel;
    public TMPro.TextMeshProUGUI greenLabel;

    public GameObject redHandle;
    public GameObject yellowHandle;
    public GameObject greenHandle;
    */

    public Image backGroundImage;
    public TMPro.TextMeshProUGUI text;

    public void SetSliderValue(float value, float[] maxValues, SliderType[] types)
    {
        if (sliders.Count != types.Length) {
            // Destroy all elementes from list except the first one
            for (int i = sliders.Count-1; i > 0; i--)
            {
                Destroy(sliders[i].gameObject);
                sliders.RemoveAt(i);
                
                Destroy(labels[i].gameObject);
                labels.RemoveAt(i);
                
                Destroy(handles[i].gameObject);
                handles.RemoveAt(i);
            }



            // Create new sliders
            for (int i = 1; i < types.Length; i++)
            {
                // Instantiate as the first child
                Slider slider = Instantiate(sliders[0], sliders[0].transform.parent);
                slider.gameObject.SetActive(true);
                sliders.Add(slider);

                // Set slider as the first child
                slider.transform.SetAsFirstSibling();

                // Instantiate Handle
                GameObject handle = Instantiate(handles[0], handles[0].transform.parent);
                handle.gameObject.SetActive(true);
                handles.Add(handle);
                slider.handleRect = handle.GetComponent<RectTransform>();

                // Instantiate Text
                labels.Add(handle.GetComponentInChildren<TMPro.TextMeshProUGUI>());
            }
            Debug.Log("Create " + sliders.Count);
        }

        bool printHandles = true;
        for (int i=0; i<sliders.Count; i++)
        {
            sliders[i].maxValue = value;

            float sliderValue = (i<sliders.Count-1) ? Mathf.Min(maxValues[i], value) : value;
            sliders[i].value = sliderValue;


            sliders[i].GetComponentInChildren<Image>().color = GetColor(types[i]);

            handles[i].SetActive(printHandles);
            if (printHandles)
            {    
                if (value == sliderValue)
                    printHandles = false;   
                if (sliderValue * 12 < value) handles[i].SetActive(false);

                handles[i].transform.GetChild(0).GetChild(0).GetComponent<Image>().color = Color.Lerp(GetColor(types[i]), Color.white, 0.7f);

                labels[i].text = Math.Round(sliderValue,2) + "";
            }
        }
    }

    public Color GetColor(SliderType type)
    {
        switch (type)
        {
            case SliderType.Red:
                return Color.red;
            case SliderType.Yellow:
                return Color.yellow;
            case SliderType.Green:
                return Color.green;
            default:
                return Color.white;
        }
    }
    
    
    /*
    {
        /*
        greenSlider.maxValue = value;
        yellowSlider.maxValue = value;
        redSlider.maxValue = value;

        redSlider.value = value;
        yellowSlider.value = Mathf.Min(value, maxYellowValue);
        greenSlider.value = Mathf.Min(value, maxGreenValue);
        

        if (value <= maxGreenValue)
        {
            greenHandle.SetActive(true);
            yellowHandle.SetActive(false);
            redHandle.SetActive(false);
        } else if (value <=maxYellowValue)
        {
            greenHandle.SetActive(true);
            yellowHandle.SetActive(true);
            redHandle.SetActive(false);
        } else
        {
            greenHandle.SetActive(true);
            yellowHandle.SetActive(true);
            redHandle.SetActive(true);
        }

        /*
        redLabel.text = Math.Round(redSlider.value,2) + "";
        yellowLabel.text = Math.Round(yellowSlider.value,2) + "";
        greenLabel.text = Math.Round(greenSlider.value,2) + "";

        // If it's too small don't show it
        if (greenSlider.value * 12 < value) greenHandle.SetActive(false);
        if (yellowSlider.value * 12 < value) yellowHandle.SetActive(false);
        
    }*/
}
