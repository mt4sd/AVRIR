using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Check when an object (with tag "Mask") is touching this object
public class HandAnimationGrabController : MonoBehaviour
{
    [SerializeField]
    private float radius = 0.01f;
    private LayerMask layerMask;

    public bool inside;

    private void Start()
    {
        layerMask = 1 << LayerMask.NameToLayer("Mask");
    }

    public bool getInside()
    {
        inside = 0 < Physics.OverlapSphereNonAlloc(this.transform.position, radius * 2, new Collider[10], layerMask.value);
        return inside;
    }
}
