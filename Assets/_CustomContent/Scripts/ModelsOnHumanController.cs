using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class ModelsOnHumanController : MonoBehaviour
{
    [System.Serializable]
    public class ZAnatomyDisectionZone{
        public string ID;
        public bool IsFaceDownModel;
        public BoxCollider Collider; // To enable only objects inside and to be the HoleRegion
    }
    public bool IsZAnatomy;
    [ShowIf("IsZAnatomy")]
    public ZAnatomy.ZAnatomyController ZAnatomyBody;
    [ShowIf("IsZAnatomy")]
    public List<ZAnatomyDisectionZone> ZAnatomyZones;
    [ShowIf("IsZAnatomy")]
    public float DistanceCheckModelInsideCollider = 0.25f;
    private ZAnatomyDisectionZone ZAnatomyCurrentActiveZone;

    [System.Serializable]
    public class ModelZone{
        public string ID;
        public GameObject Parent;
        public Transform ModelParent;
        public GameObject BodyOpenButton;
        public GameObject HoleInBody;
        // Runtime Filled
        public Vector3 OpenedScale;
        private SphereCollider holeSphere;

        public float GetHoleRadius(){
            if(holeSphere == null)
                holeSphere = HoleInBody.GetComponent<SphereCollider>();
            
            return holeSphere.radius*Mathf.Max(holeSphere.transform.lossyScale.x,holeSphere.transform.lossyScale.y,holeSphere.transform.lossyScale.z);
        } 
    }

    
    [HideIf("IsZAnatomy")]
    public List<ModelZone> ModelsList;
    [HideIf("IsZAnatomy")]
    private ModelZone CurrentModelZone;
    [HideIf("IsZAnatomy")]
    public float TimeOpenBody = 2;
    [HideIf("IsZAnatomy")]
    public GameObject DisabledObjects;
    //private ModelZone m_OpenedModelZone;
    private GameObject CurrentModel;
    private bool m_IsBodyOpened;
    private GameObject m_LastOpenedHole;
    // private Vector3 m_ZAnatomyBodyLocalEulerAngles;

    public ModelZone GetModelZone(){
        return CurrentModelZone;
    }

    private void Awake() {
        if(!IsZAnatomy){
            foreach (ModelZone zone in ModelsList)
            {
                if(zone.HoleInBody != null)
                    zone.OpenedScale = zone.HoleInBody.transform.localScale;

                Vector3 localScale = zone.ModelParent.localScale;
                localScale.x = -1*localScale.x;
                zone.ModelParent.localScale = localScale;
            }
        }
    }

    public void InitRealModel(string ID, GameObject model = null, bool isDissection=true){
        if(!IsZAnatomy){
            ModelZone zone = ModelsList.Find((x)=>x.ID.Equals(ID));
            if(zone != null){
                zone.Parent.SetActive(true);
                CurrentModelZone = zone;
                CurrentModel = model;//Instantiate(model, zone.ModelParent);
                CurrentModel.SetActive(true);
                CurrentModel.transform.SetParent(zone.ModelParent, true);
                //m_OpenedModelZone = zone;
                zone.HoleInBody.transform.localScale = Vector3.zero;
            }
            DisabledObjects.SetActive(false);
            m_IsBodyOpened = false;
        }else{
            foreach (ZAnatomyDisectionZone zoneDisable in ZAnatomyZones)
            {
                zoneDisable.Collider.enabled = false;
            }
            
            ZAnatomyDisectionZone zone = ZAnatomyZones.Find((x)=>x.ID.Equals(ID));
            ZAnatomyCurrentActiveZone = zone;
            // if(ZAnatomyCurrentActiveZone == null){
                ZAnatomyBody.EnableAllMeshColliders();
                ZAnatomyBody.EnableAllMeshGameObjects(true);
            // }else{
            //     // ZAnatomyBody.transform.parent.up = -ZAnatomyCurrentActiveZone.Collider.transform.forward;
            //     // Vector3 newAngles = Vector3.zero;
            //     // newAngles.z = ZAnatomyCurrentActiveZone.IsFaceDownModel?180:0;

            //     // ZAnatomyBody.transform.localEulerAngles = GetLocalEulerAngles();//newAngles;
            //     UpdateBodyOrientation();
                
            //     ZAnatomyBody.EnableMeshCollidersInsideBox(ZAnatomyCurrentActiveZone.Collider, isDissection);
            // }
        }
    }

    public void UpdateBodyOrientation(){
        ZAnatomyBody.transform.localEulerAngles = GetLocalEulerAngles();//newAngles;
    }
    private Vector3 GetLocalEulerAngles(){
        Vector3 newAngles = Vector3.zero;
        newAngles.z = ZAnatomyCurrentActiveZone.IsFaceDownModel?180:0;
        return newAngles;
    }

    public void DisableModel(){
        DisabledObjects.SetActive(true);
    }

    public void ReinitModel(){
        if(!IsZAnatomy){
            CurrentModel.transform.localPosition = Vector3.zero;
            CurrentModel.transform.localEulerAngles = Vector3.zero;
            CurrentModel.transform.localScale = Vector3.one;
            CurrentModel.layer = LayerMask.NameToLayer("Mask");
            //int count = 0;
            foreach (Transform item in CurrentModel.transform)
            {

                item.gameObject.layer = LayerMask.NameToLayer("Mask");
                item.transform.localEulerAngles = Vector3.zero;
                item.transform.localScale = Vector3.one;
                //Debug.Log(item.gameObject.name);
                /*MeshInteractiveController mesh = item.GetComponent<MeshInteractiveController>();
                if(mesh != null){
                    mesh.Init(count, Color.yellow);
                    mesh.UpdateBehaviour(behaviour);
                    count++;
                }*/
            }
            DisabledObjects.SetActive(false);
        }else{
            //ZAnatomyBody.ResetInteractiveMeshes();
        }
    }

    public void ToggleBodyHole(GameObject bodyButton, bool forceReset = false){
        if(!IsZAnatomy){
            m_IsBodyOpened = !m_IsBodyOpened;

            ModelZone selectedZone = ModelsList.Find((x)=>x.BodyOpenButton == bodyButton);
            
            if(selectedZone != null){
                LeanTween.cancel(selectedZone.HoleInBody);
                if(!forceReset){
                    LeanTween.scale(selectedZone.HoleInBody, !m_IsBodyOpened?Vector3.zero:selectedZone.OpenedScale, TimeOpenBody);

                    m_LastOpenedHole = selectedZone.HoleInBody;
                }else{
                    m_IsBodyOpened = false;
                    selectedZone.HoleInBody.transform.localScale = Vector3.zero;
                }
            }

            if(!m_IsBodyOpened){
                m_LastOpenedHole = null;
            }
        }
    }

    public bool IsModelInsideActiveCollider(Vector3 position){
        //return Vector3.Distance(ZAnatomyCurrentActiveZone.Collider.transform.position, position) < DistanceCheckModelInsideCollider;
        return ZAnatomyCurrentActiveZone.Collider.bounds.Contains(position);
    }

    public bool IsBodyOpened(){
        if(IsZAnatomy){
            return true;
        }

        return m_IsBodyOpened && m_LastOpenedHole != null && !LeanTween.isTweening(m_LastOpenedHole);
    }

    public void DisableIfActive(){
        if(!IsZAnatomy){
            foreach (ModelZone human in ModelsList)
            {
                if(human.ModelParent.childCount > 0){
                    //Destroy(human.ModelParent.GetChild(0).gameObject);
                }
                human.Parent.SetActive(false);
            }
        }
    }
}
