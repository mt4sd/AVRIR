using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyTransformPosition : MonoBehaviour
{
    public Transform[] copy;
    public Transform[] target;

    // Start is called before the first frame update
    void Start()
    {
        if (copy.Length != target.Length) Destroy(this);
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i<copy.Length; i++)
        {
            target[i].localPosition = copy[i].localPosition;
            target[i].localRotation = copy[i].localRotation;
        }
    }
}
