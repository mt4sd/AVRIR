using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class XRDeviceInput : MonoBehaviour
{
    private InputDevice targetDevice;
    public InputDeviceCharacteristics controllerCharacteristics;

    private XRInitialization init;
    public Transform oculusHandCenter;

    private void Start()
    {
        StartCoroutine(FindDevice());
        init = this.gameObject.GetComponentInParent<XRInitialization>();
    }

    public void FixedUpdate()
    {
        if (init.xrType == XRInitialization.XRType.OculusHand)
        {
            this.transform.position = oculusHandCenter.position;
            this.transform.rotation = oculusHandCenter.rotation;
        }
    }

    IEnumerator FindDevice()
    {

        yield return null;
        List<InputDevice> devices = new List<InputDevice>();

        InputDevices.GetDevicesWithCharacteristics(controllerCharacteristics, devices);

        if (devices.Count > 0)
        {
            targetDevice = devices[0];
        }
        else
            StartCoroutine("FindDevice");
    }

    public float gripValue;
    public float TrigerValue;

    public float GetFloatValue(InputFeatureUsage<float> commonUsages)
    {
        
        if(CommonUsages.grip == commonUsages && gripValue > 0)
            return gripValue;
        if (CommonUsages.trigger == commonUsages && TrigerValue > 0)
            return TrigerValue;

        if (targetDevice != null && targetDevice.TryGetFeatureValue(commonUsages, out float value))
            return value;
        else
            return 0;
    }

    public bool GetBoolValue(InputFeatureUsage<bool> commonUsages)
    {
        if (targetDevice != null && targetDevice.TryGetFeatureValue(commonUsages, out bool value))
            return value;
        else
            return false;
    }
}
