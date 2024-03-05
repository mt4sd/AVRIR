using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    public Color GreenColor;
    public Color YellowColor;
    public Color RedColor;
    private TMPro.TextMeshPro TextLabel;
    // for fps calculation.
    private int frameCount;
    private float elapsedTime;
    private double frameRate;
     /// <summary>
    /// Initialization
    /// </summary>
    private void Awake()
    {
        TextLabel = GetComponent<TMPro.TextMeshPro>();
        //DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Monitor changes in resolution and calcurate FPS
    /// </summary>
    private void Update()
    {
        // FPS calculation
        frameCount++;
        elapsedTime += Time.deltaTime;
        if (elapsedTime > 0.5f)
        {
            frameRate = System.Math.Round(frameCount / elapsedTime, 1, System.MidpointRounding.AwayFromZero);
            frameCount = 0;
            elapsedTime = 0;

            Color finalColor = GreenColor;

            if(frameRate < 15)
                finalColor = RedColor;
            else{
                if(frameRate < 30){
                    finalColor = YellowColor;
                }
            }

            TextLabel.color = finalColor;
            TextLabel.text = "<size=50%>FPS\n<size=100%>"+frameRate.ToString("N0");
        }
    }
}
