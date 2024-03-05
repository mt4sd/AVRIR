using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using ZAnatomy;

public class MorgueController : MonoBehaviour
{

    [System.Serializable]
    public class OrganTypeInfo{
        public ZAnatomy.ZAnatomyController.OrganType Type;
        public string Name;
        public Color Color;
        public bool Enabled;
    }

    [System.Serializable]
    public class ReferencePlayerPosition{
        public int ID;
        public GameObject PositionMarker;
        public Transform TabletMarker;
        public StationController Station;
        public int OcuppedByPlayerID = -1;

        public bool IsOcuppedByPlayer(){
            return OcuppedByPlayerID != -1;
        }
        public Vector3 GetPosition(){
            return PositionMarker.transform.position;
        }

        internal Quaternion GetRotation()
        {
            return PositionMarker.transform.rotation;
        }
    }

    public bool InstantiateZAnatomyOnStart;
    public TMPro.TextMeshProUGUI ModelNameText;
    public float DistanceToInsertPiece = 0.05f;
    public Transform ZAnatomyParentModel;
    public List<ReferencePlayerPosition> ReferencePositions;
    public Transform DesktopDummyTransformParent;
    public List<string> ZAnatomyList; 
    public GameObject TabletPrefab;

    public List<OrganTypeInfo> OrgansTypeInfo;
    private ZAnatomy.ZAnatomyController m_CurrentZAnatomyController;

    [Header("Station")]
    public StationController reorder;
    public StationController morgue;
    public static Action OnZAnatomyLoadStarted;
    public static Action OnZAnatomyLoadedFinished;
    private bool m_IsLoadingZAnatomy;
    public static Action OnChangedTabletData;
    private List<OrganTypeInfo> m_LastSelectedOrganTypeList;

    public void OnToggleOrganType(ZAnatomy.ZAnatomyController.OrganType organType ,bool enable){
        OrganTypeInfo info = OrgansTypeInfo.Find((x)=> x.Type == organType);
        info.Enabled = enable;
        if(m_CurrentZAnatomyController != null){
            m_CurrentZAnatomyController.EnableOrganType(organType, enable);
        }

        
        if(OnChangedTabletData!=null){
            OnChangedTabletData();
        }
    }
    

    internal void OnSetOrganType(ZAnatomyController.OrganType organType)
    {
        foreach (OrganTypeInfo info in OrgansTypeInfo)
            info.Enabled = info.Type == organType;

        if (m_CurrentZAnatomyController != null)
            m_CurrentZAnatomyController.SetOneOrganType(organType);
        

        if (OnChangedTabletData != null)
        {
            OnChangedTabletData();
        }
    }

    internal void SaveLastSelectedOrganTypes(){
        m_LastSelectedOrganTypeList = new List<OrganTypeInfo>();
        foreach (OrganTypeInfo info in OrgansTypeInfo){
            if(info.Enabled){
                m_LastSelectedOrganTypeList.Add(info);
            }
        }
    }

    internal void ReloadLastSelectedOrganTypes(){
        if(m_LastSelectedOrganTypeList != null){
            foreach(OrganTypeInfo organ in m_LastSelectedOrganTypeList){
                if(!organ.Enabled){
                    OnToggleOrganType(organ.Type, true);
                }
            }
        }
    }

    public ZAnatomy.ZAnatomyController GetZAnatomyController(){
        return m_CurrentZAnatomyController;
    }

    public OrganTypeInfo GetOrganType(ZAnatomy.ZAnatomyController.OrganType organType){
        return OrgansTypeInfo.Find((x)=> x.Type == organType);
    }


    public ReferencePlayerPosition RequestStartPosition(int id, int playerID){
        Debug.Log("Requesting start position: "+id);
        foreach (ReferencePlayerPosition item in ReferencePositions)
        {
            if(item.ID==id && !item.IsOcuppedByPlayer()){
                MorgueController.ReferencePlayerPosition position = item;//MorgueGrabUtilities.morgue.ReferencePositions[id];
                position.OcuppedByPlayerID = playerID;
                position.Station = id == 5 ? MorgueGrabUtilities.morgue.reorder : MorgueGrabUtilities.morgue.morgue;
                // OnlineUtilities.GetOnlineUtilities().SetPlayerStartPosition(playerID, id);
                return item;
            }
        }

        Debug.LogError("There is not enough free start positions");
        return null;
    }

    void Awake()
    {
        MorgueGrabUtilities.morgue = this;        
        LeanTween.reset();
    }

    void Start()
    {
        // Clean old possible models
        foreach(Transform child in ZAnatomyParentModel){
            Destroy(child.gameObject);
        }

        if(InstantiateZAnatomyOnStart){
            Init();// m_CurrentZAnatomyController = Instantiate(Resources.Load<ZAnatomyController>(ZAnatomyList[0]), ZAnatomyParentModel);// null;// Instantiate(ZAnatomyList[0], ZAnatomyParentModel); // Forced to choose first in list
        }
    }

    public void Init(){
        StartCoroutine(LoadSection());
    }

    IEnumerator LoadSection()
    {
        m_IsLoadingZAnatomy= true;
        // yield return new WaitForSeconds(0.5f);
        if(OnZAnatomyLoadStarted!=null){
            OnZAnatomyLoadStarted();
        }

        string zAnatomySelectedPath = "";
        string parentName = "";
        GameObject mirrorRoomManager = GameObject.Find("_CustomMirrorRoomManager");
        if(mirrorRoomManager != null){
            zAnatomySelectedPath = mirrorRoomManager.GetComponent<CustomNetworkRoomManager>().GetSelectedModelPath();
            parentName = zAnatomySelectedPath.Split("/")[0]+"/";
        }

        if(string.IsNullOrEmpty(zAnatomySelectedPath)){
            zAnatomySelectedPath = ZAnatomyList[0];
        }
        
        Debug.Log("Loading Section: "+zAnatomySelectedPath);
        ResourceRequest resourceRequest = Resources.LoadAsync<ZAnatomy.ZAnatomyController>(zAnatomySelectedPath);
        yield return resourceRequest;
        m_CurrentZAnatomyController = Instantiate(resourceRequest.asset as ZAnatomy.ZAnatomyController, ZAnatomyParentModel);

        foreach(ZAnatomy.ZAnatomyController.OrganPrefab organPrefab in m_CurrentZAnatomyController.OrganPrefabsList){
            organPrefab.UnifiedPrefabRelativePath = parentName + organPrefab.UnifiedPrefabRelativePath;
        }
        // m_CurrentZAnatomyController = Instantiate(Resources.Load<ZAnatomy.ZAnatomyController>(ZAnatomyList[0]), ZAnatomyParentModel);// null;// Instantiate(ZAnatomyList[0], ZAnatomyParentModel); // Forced to choose first in list
        
        m_CurrentZAnatomyController.Init(ZAnatomyLoadFinished);
    }

    public bool IsLoadingZAnatomy(){
        return m_IsLoadingZAnatomy;
    }

    private void ZAnatomyLoadFinished(){
        Debug.Log("ZAnatomy - All prefabs loaded");

        m_IsLoadingZAnatomy= false;
        if(OnZAnatomyLoadedFinished!=null){
            OnZAnatomyLoadedFinished();
        }
    }

    // public void Init(bool isVR/*, int playerId,*/, GameObject referenceModel, string nameModel=""){
    //     bool isFirstTime = !string.IsNullOrEmpty(nameModel);

	// 	Cursor.lockState = CursorLockMode.Locked;
	// 	Cursor.visible = false;

    //     gameObject.SetActive(true);

    //     if(ModelNameText != null){
    //         ModelNameText.text = nameModel;
    //     }

    //     ZAnatomyParentModel.gameObject.SetActive(true);
    //     referenceModel.transform.SetParent(ZAnatomyParentModel);
    //     referenceModel.transform.localPosition = Vector3.zero;
    //     referenceModel.transform.localEulerAngles = Vector3.zero;
    //     referenceModel.transform.localScale = Vector3.one;
    // }

    public void OnClickResetMorgue(){
        Debug.Log("Reset Morgue Client");
        if(m_CurrentZAnatomyController != null){
            foreach(ZAnatomy.MeshInteractiveController model in m_CurrentZAnatomyController.Data.GetInteractiveMeshList()){
                if(!model.MorgueIsInPlace){
                    // model.MorgueSetOnPosition(true);
                    Utils.MeshInteractiveMorgueSetOnPositionByLeanTween(model, true);
                }
            }
        }
    }

    public ReferencePlayerPosition RequestFirstEmptyPlayerPos(int idPlayer){
        ReferencePlayerPosition currentPosition = ReferencePositions.Find((x)=> x.OcuppedByPlayerID == idPlayer);
        if(currentPosition != null){
            currentPosition.OcuppedByPlayerID = -1;
        }

        ReferencePlayerPosition pos = ReferencePositions.Find((x)=>!x.IsOcuppedByPlayer());
        int emptyPosIndex = ReferencePositions.IndexOf(pos);
        return RequestStartPosition(emptyPosIndex+1, idPlayer);
    }

    [Button]
    public ReferencePlayerPosition RequestPlayerNewPos(int idPlayer, PanelInArmController.ButtonAction action){
        int positionId = -1;

        ReferencePlayerPosition currentPosition = ReferencePositions.Find((x)=> x.OcuppedByPlayerID == idPlayer);
        if(currentPosition != null){
            currentPosition.OcuppedByPlayerID = -1;
        }

        ReferencePlayerPosition nextPosition = null;// = ReferencePositions.Find((x)=> x.ID == positionId);
        switch(action){
            case PanelInArmController.ButtonAction.GoToP1:
                positionId = 1;
                nextPosition = RequestStartPosition(positionId,idPlayer);
                break;
            case PanelInArmController.ButtonAction.GoToP2:
                positionId = 2;
                nextPosition = RequestStartPosition(positionId,idPlayer);
                break;
            case PanelInArmController.ButtonAction.GoToP3:
                positionId = 3;
                nextPosition = RequestStartPosition(positionId,idPlayer);
                break;
            case PanelInArmController.ButtonAction.GoToP4:
                positionId = 4;
                nextPosition = RequestStartPosition(positionId,idPlayer);
                break;
            case PanelInArmController.ButtonAction.GoToReorder:
                positionId = 5;
                nextPosition = RequestStartPosition(positionId,idPlayer);
                break;
        }

        // nextPosition.Station = positionId == -1 ? reorder : morgue;

        return nextPosition;
    }

    public GameObject InstantiateLocalTablet(Transform parentTablet){
        Utils.ClearChilds(parentTablet);
        return Instantiate(TabletPrefab, parentTablet);
    }

}
