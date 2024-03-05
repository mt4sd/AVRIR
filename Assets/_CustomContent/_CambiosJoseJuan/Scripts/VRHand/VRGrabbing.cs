using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class VRGrabbing : MonoBehaviour
{
    public VRFloatingNameModel floatingNameModel;
    public StationController station;
    
    public Transform center; // Center of grabbing
    public bool forceGrab;
    protected bool isGrabbing;
    private bool wasRelease = false;
    public bool wrongAttempt = false;


    [SerializeField]
    private HandController handController;
    private LayerMask layerMask;

    [SerializeField]
    protected GameObject grabbedGroup = null;
    [SerializeField]
    private ZAnatomy.MeshInteractiveController grabbedObject = null;
    [SerializeField]
    protected ZAnatomy.MeshInteractiveController bestCandidate = null;

    protected ZAnatomy.MeshInteractiveController lastWrongCandidate = null;
    private List<Collider> grabbingCandidates;

    public abstract bool CheckGrabbing();

    protected virtual void Start()
    {
        layerMask = 1 << LayerMask.NameToLayer("Mask");
        grabbingCandidates = new List<Collider>();

        if (center == null) center = this.transform;

        XRInitialization xrParent = this.GetComponentInParent<XRInitialization>();
        xrParent.RegisterVRGrabbing(this);
        station = xrParent.currentStation;

    }

    public void OnEnable()
    {
        handController.transform.parent = this.transform;
    }

    private void FixedUpdate()
    {
        if (station == null)
        {
            // Debug.Log("Null station");
            station = this.GetComponentInParent<XRInitialization>().currentStation;
            if (station ==null)
                return;
        }
        CheckColliders();

        isGrabbing = CheckGrabbing();
        ZAnatomy.MeshInteractiveController tryGrab = bestCandidate;

        if (!isGrabbing)
        {
            wasRelease = true;
            if (lastWrongCandidate)
            {
                FeedBackUtilities.SetMaterialsColor(lastWrongCandidate.gameObject, this, FeedBackUtilities.ActionState.None);
                lastWrongCandidate = null;
            }
            if (floatingNameModel) floatingNameModel.UpdateFloatingPanel(this, false);
        } else
        {
            if (floatingNameModel) floatingNameModel.UpdateFloatingPanel(this);
        }

        if (isGrabbing && wasRelease && bestCandidate != null)
        {
            wasRelease = false;
            if (IsPointingGrabbableModel())
            {
                if (grabbedObject == null)
                    Grab(tryGrab);
            }
            else
            {
                lastWrongCandidate = bestCandidate; // When realise the renderer will set in not selected
                station.WrongGrabbing(bestCandidate, this);
            }
        }
        else if (!isGrabbing && grabbedObject != null) Release();
    }


    protected void Grab(ZAnatomy.MeshInteractiveController tryGrab)
    {
        grabbedObject = tryGrab.GetComponent<ZAnatomy.MeshInteractiveController>();
        ZAnatomy.DividedOrganGroup tmpGroup = grabbedObject.GetComponentInParent<ZAnatomy.DividedOrganGroup>();

        if (tmpGroup == null) grabbedGroup = grabbedObject.gameObject;
        else grabbedGroup = tmpGroup.gameObject;

        handController.OnSelectObject(grabbedGroup.gameObject, this.transform);
        foreach (MeshInteractableOnline online in grabbedGroup.GetComponentsInChildren<MeshInteractableOnline>())
        {
            online.Grab();
        }

        
        foreach (ZAnatomy.MeshInteractiveController interactiveController in grabbedGroup.GetComponentsInChildren<ZAnatomy.MeshInteractiveController>())
        {
            interactiveController.SetCollider(true);
            // interactiveController.IsBeingGrabbedByPlayer = true;
            interactiveController.MorgueIsInPlace = false;
            
            if(interactiveController.gameObject.GetComponent<DraggingMeshInteractive>() != null){
                interactiveController.gameObject.AddComponent<DraggingMeshInteractive>();
            }

            station.DeselectLastPointed(interactiveController, this);
        }

    }

    protected void Release()
    {
        station.CheckRelease(grabbedObject, this);

        foreach (ZAnatomy.MeshInteractiveController interactiveController in grabbedGroup.GetComponentsInChildren<ZAnatomy.MeshInteractiveController>())
        {
            interactiveController.SetCollider(true);
            // interactiveController.IsBeingGrabbedByPlayer = false;
            Destroy(interactiveController.gameObject.GetComponent<DraggingMeshInteractive>());
        }
        

        foreach (MeshInteractableOnline online in grabbedGroup.GetComponentsInChildren<MeshInteractableOnline>())
        {
            online.UnGrab();
        }

        handController.OnSelectExit();
        grabbedObject = null;
        grabbedGroup = null;
    }

    protected virtual void CheckColliders()
    {
        float VRDistanceGrabObject = float.MaxValue;
        float m_LastHandMeshDistance = VRDistanceGrabObject;
        ZAnatomy.MeshInteractiveController lastBestCandidate = this.bestCandidate;
        this.bestCandidate = null;

        for (int i = 0; i < grabbingCandidates.Count; i++)
        {
            if (grabbingCandidates[i].enabled && grabbingCandidates[i].gameObject.activeInHierarchy &&
                grabbingCandidates[i].TryGetComponent(out ZAnatomy.MeshInteractiveController item))
            {
                if (!item.CanBeGrabbed) continue;

                Vector3 referencePoint = item.GetMeshKDTree().ClosestPointOnSurface(center.position);

                float distanceHandMesh = Vector3.Distance(referencePoint, center.position);

                if (distanceHandMesh <= VRDistanceGrabObject)
                {
                    if (distanceHandMesh <= m_LastHandMeshDistance)
                    {
                        if (item.TryGetComponent(out MeshInteractableOnline online) && !online.isGrabbed)
                        {
                            this.bestCandidate = item;
                            m_LastHandMeshDistance = distanceHandMesh;
                        }
                    }
                }
            }

        }

        CheckColliderColor(lastBestCandidate);
    }

    protected void CheckColliderColor(ZAnatomy.MeshInteractiveController lastBestCandidate)
    {
        if (!IsGrabbing() && wasRelease)
        {
            if (this.bestCandidate != lastBestCandidate)
            {
                station.SelectPointedModel(this.bestCandidate, lastBestCandidate, this);
            }
            else
            {
                station.SelectPointedModel(this.bestCandidate, lastBestCandidate, this);
            }
        }
        else if (grabbedObject != null)
        {

            ZAnatomy.MeshInteractiveController interactive = grabbedObject.GetComponent<ZAnatomy.MeshInteractiveController>();

            if (station.CheckGrabbedModelDistance(interactive))
            {
                // Correct Position
                FeedBackUtilities.SetMaterialsColor(grabbedObject.gameObject, this, FeedBackUtilities.ActionState.Correct);
            }
            else
            {
                if (station.CheckGrabbedModelInsideHole(interactive))
                    FeedBackUtilities.SetMaterialsColor(grabbedObject.gameObject, this, FeedBackUtilities.ActionState.Near);
                // else FeedBackUtilities.SetMaterialsColor(grabbedObject, FeedBackUtilities.ActionState.Grabbed);
                else station.DeselectLastPointed(grabbedObject, this);
            }
        }
    }

    public bool IsGrabbing()
    {
        return grabbedObject != null;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<ZAnatomy.MeshInteractiveController>() != null)
            grabbingCandidates.Remove(other);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!grabbingCandidates.Contains(other) && other.GetComponent<ZAnatomy.MeshInteractiveController>() != null)
            grabbingCandidates.Add(other);
    }

    public bool IsAWrongGrab()
    {
        return lastWrongCandidate != null; 
    }

    public bool IsPointingGrabbableModel()
    {
        if (bestCandidate == null) return true;

        if (!bestCandidate.MorgueIsInPlace)
        {
            return true;
        }

        return station.CanBeGrabbed(bestCandidate);
    }

    public ZAnatomy.MeshInteractiveController GetBestCandidate()
    {
        return bestCandidate;
    }
    public ZAnatomy.MeshInteractiveController GetGrabbedObject()
    {
        return grabbedObject;
    }
    public GameObject GetGrabbedGroup()
    {
        return grabbedGroup;
    }

    public void ChangeGrabState()
    {
        if (grabbedObject != bestCandidate && grabbedObject != null && bestCandidate != null)
        {
            Release();
            isGrabbing = false;
            forceGrab = false;
        }
        forceGrab = !forceGrab;
    }
}
