using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomPhysicsHand : MonoBehaviour
{
    public enum HandType { LeftHand, RightHand}
    // public MorguePlayerController MorguePlayer;
    public VRGrabbing vrController;
    private bool lastCheckGrabbing = false;

    public Transform Head;
    public Renderer HandRenderer;
    public bool AlwaysShowRealHand = true;
    public Transform FollowTransform;
    public Renderer FollowRenderer;
    public Transform FollowCenterBall;
    public Transform CenterBall;
    public float TimeFactor = 0.1f;
    public float OffsetRaycast = 0.1f;
    public float GrabDistance = 1;
    public float MaxRealHandDistance = 2;
    public AnimationCurve VibrationIntensityCurve;
    public LayerMask mask;
    public OVRInput.Controller Controller;
    private Vector3 m_LastValidPosition;
    private Quaternion m_LastValidRotation;
    private bool m_LastCanFollow;
    private Color m_RendererColorTarget;
    [Header("Impact Vibration")]
    public float ImpactVibrationTime = 0.5f;
    public float MaxImpactDistance = 1;
    private bool m_IsOnImpactVibration;
    private Vector3 m_OnFirstTouchNormal;
    private bool m_HandIsGrabbing;
    private Vector3 m_GrabbingValidPosition;
    public HandType handType;

    // Start is called before the first frame update
    void Start()
    {
        
        if (vrController == null) vrController = this.GetComponent<VRGrabbing>();
        m_RendererColorTarget = FollowRenderer.material.color;
        m_RendererColorTarget.a = 0;

        m_LastValidPosition = FollowTransform.position;
        m_LastValidRotation = FollowTransform.rotation;
    }


    // Update is called once per frame
    void LateUpdate()
    {
        if(!gameObject.activeInHierarchy || !this.enabled){
            return;
        }

        bool isStartGrab;
        bool isGrabbing;
            
        bool grabbing = vrController.CheckGrabbing();
        isStartGrab = vrController.IsAWrongGrab() && (grabbing && !lastCheckGrabbing);

        isGrabbing = vrController.IsAWrongGrab() && grabbing;

        lastCheckGrabbing = grabbing;

        Vector3 validPosition = m_LastValidPosition;
        Quaternion validRotation = m_LastValidRotation;

        Vector3 direction = FollowCenterBall.position-CenterBall.position;
        Vector3 differenceToCenter = CenterBall.position - transform.position;
        Vector3 startRaycastPos = CenterBall.position - direction.normalized /*CenterBall.up.normalized*/ * OffsetRaycast;
        float distance = Vector3.Distance(FollowCenterBall.position, startRaycastPos);
        float distanceFactor = VibrationIntensityCurve.Evaluate(distance / MaxRealHandDistance);

        m_OnFirstTouchNormal = (transform.position - Head.position).normalized;
        bool canFollow = true;
        RaycastHit hit;

        distance += OffsetRaycast;
        // RaycastHit[] hits = Physics.RaycastAll(startRaycastPos, direction, distance, mask);
        if (Physics.Raycast(startRaycastPos, direction, out hit, distance, mask)){
            ZAnatomy.MeshInteractiveController meshInteractive = hit.collider.gameObject.GetComponentInParent<ZAnatomy.MeshInteractiveController>();
            if(meshInteractive == null || (meshInteractive != null && meshInteractive.MorgueIsInPlace) ){
                if(!isGrabbing){

                    canFollow = false;
                    if(m_LastCanFollow && !canFollow){
                        // ImpactVibration(Vector3.Distance(startRaycastPos, hit.point)/MaxImpactDistance);
                        m_OnFirstTouchNormal = hit.normal;
                    }
                        
                    validPosition = hit.point - differenceToCenter + hit.normal * GrabDistance;
                    validRotation = FollowTransform.rotation;

                }

            }
        }

        if(isStartGrab)
        {
            m_GrabbingValidPosition = validPosition;
            m_OnFirstTouchNormal = FollowCenterBall.position - CenterBall.position; 
        }

        if(isGrabbing)
        {
            canFollow = false;

            // this.transform.LookAt
            Vector3 directionToHand = FollowCenterBall.position - CenterBall.position;
            if(Vector3.Dot(m_OnFirstTouchNormal, directionToHand)<0){
                
                // validRotation = Quaternion.LookRotation(-directionToHand.normalized);//Vector3.Cross(FollowTransform.right, m_OnFirstTouchNormal), m_OnFirstTouchNormal);
                validRotation = Quaternion.LookRotation(-directionToHand.normalized) * Quaternion.Euler(rotationToForward);
            }
            validPosition = m_GrabbingValidPosition;
        }


        // Debug.DrawRay(startRaycastPos, transform.up, Color.yellow);
        // Debug.DrawRay(startRaycastPos, m_OnFirstTouchNormal, Color.blue);

        if(canFollow){
            validPosition = Vector3.Lerp(transform.position, FollowTransform.position, TimeFactor*Time.deltaTime);
            validRotation = Quaternion.Lerp(transform.rotation, FollowTransform.rotation, TimeFactor*Time.deltaTime);
            distanceFactor = 0;
        }

        if(!m_IsOnImpactVibration){
            // OVRInput.SetControllerVibration(distanceFactor,distanceFactor,Controller);
        }
        
        
        if(AlwaysShowRealHand){
            distanceFactor = 1;
        }
        

        if (distanceFactor != 0)
        {
            FollowRenderer.enabled = true;

            m_RendererColorTarget = FollowRenderer.material.color;
            m_RendererColorTarget.a = distanceFactor;
            FollowRenderer.material.color = m_RendererColorTarget;
        } else
        {
            FollowRenderer.enabled = false;

            // m_RendererColorTarget = FollowRenderer.material.color;
            // m_RendererColorTarget.a = 1;
            // FollowRenderer.material.color = m_RendererColorTarget;
        }
        // Debug.Log(distanceFactor);

        if (Vector3.Distance(FollowRenderer.transform.position, transform.position) > 0.3f && !isGrabbing)
        {
            transform.position = FollowRenderer.transform.position;
            transform.rotation = FollowRenderer.transform.rotation;
        } else
        {
            transform.position = validPosition;
            transform.rotation = validRotation;
        }

        m_LastValidPosition = transform.position;
        m_LastValidRotation = transform.rotation;

        m_LastCanFollow = canFollow;

        // if (isGrabbing) this.transform.LookAt(FollowCenterBall);
        // if (isGrabbing) this.transform.LookAt(FollowCenterBall.position - CenterBall.position, up);
    }

    public Vector3 rotationToForward;

    // private void ImpactVibration(float force){
    //     if(!gameObject.activeInHierarchy || !this.enabled){
    //         return;
    //     }

    //     m_IsOnImpactVibration = true;

    //     LeanTween.value(gameObject, force, 0, ImpactVibrationTime).setOnUpdate((float val)=>{
    //         OVRInput.SetControllerVibration(val,val,Controller);
    //     }).setOnComplete(()=>{
    //         m_IsOnImpactVibration = false;
    //     });

    // }
}