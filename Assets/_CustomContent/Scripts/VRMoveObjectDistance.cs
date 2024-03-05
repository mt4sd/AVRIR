using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRMoveObjectDistance : MonoBehaviour
{
    public MenuController Menu;
    private bool IsDragging;
    private float dragDistance;
    public Transform PlayerHeadTransform;
    private float m_DistanceWithHead;
    private bool m_FollowHead;

    void Start()
    {
        m_FollowHead= PlayerHeadTransform !=null;
        if(m_FollowHead){
            m_DistanceWithHead = Vector3.Distance(transform.position, PlayerHeadTransform.position);
        }
    }
    // Update is called once per frame
    void LateUpdate()
    {
        if(m_FollowHead){
            Vector3 nextPos = PlayerHeadTransform.position + PlayerHeadTransform.forward.normalized*m_DistanceWithHead;
            transform.position = Vector3.Lerp(transform.position, nextPos, Time.deltaTime);
        }else{
            if(transform.childCount>0){
                // if(OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch)){
                //     if(Menu.SelectionBar.GazePointer.IsOverUI() && Menu.SelectionBar.GazePointer.GetPointedGameObject().transform.IsChildOf(transform)){
                //         StartDrag();
                //     }
                // }

                // if(OVRInput.GetUp(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch)){
                //     if(Menu.SelectionBar.GazePointer.IsOverUI() && Menu.SelectionBar.GazePointer.GetPointedGameObject().transform.IsChildOf(transform)){
                //         EndDrag();
                //     }
                // }
            }

            if(IsDragging){
                // transform.position = Menu.VRRightHandMarker.position + Menu.VRPointerRight.GetRay().direction.normalized*dragDistance;
            }
        }
    }

    private void StartDrag(){
        IsDragging = true;
        // dragDistance = Vector3.Distance(Menu.VRRightHandMarker.position, transform.position);
    }

    private void EndDrag(){
        IsDragging = false;
    }

    private void OnDisable() {
        EndDrag();
    }
}
