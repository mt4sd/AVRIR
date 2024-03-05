using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using UnityEngine.UI;

public class ReorderController : StationController
{
    public ZAnatomy.DividedOrganController CurrentDividedOrgan;
    public ReorderDistributorController DistributorController;
    private float maxDistance = 0.035f;

    [Header("Teleports")]
    public TeleportController morgueTeleport;
    public TeleportController stationTeleport;

    [Header("Buttons")]
    public Button startButton;

    public void Update()
    {
        if (CurrentDividedOrgan != null) return;

        if (morgueTeleport.GetCurrentInnerObject() != null)
        {
            ZAnatomy.MeshInteractiveController teleportInnerObject = morgueTeleport.GetCurrentInnerObject().GetComponent<ZAnatomy.MeshInteractiveController>();
            if(teleportInnerObject.DividedPrefab == null){
                string path = teleportInnerObject.DividedPrefabRelativePath+".prefab";
                teleportInnerObject.DividedPrefab = Resources.Load<ZAnatomy.DividedOrganController>(path);
            }
            
            ZAnatomy.DividedOrganController divided = teleportInnerObject.DividedPrefab;
            bool teleportInnerObjectIsGrabbed = teleportInnerObject.GetComponent<DraggingMeshInteractive>() != null;
            //Debug.Log("Inside");
            if (divided != null && CurrentDividedOrgan != divided && !teleportInnerObjectIsGrabbed)//teleportInnerObject.IsGrabbed())
            {
                Debug.Log("Init");
                Transform transform = Instantiate(divided, teleportInnerObject.transform/*stationTeleport.transform*/).transform;
                transform.position = stationTeleport.transform.position;// + Utils.GetDifferenceFromMeshCenter(teleportInnerObject);

                // Reset data as child of origin object to clone position, orientation and scale
                transform.localEulerAngles = Vector3.zero;
                transform.localScale = Vector3.one;
                transform.SetParent(stationTeleport.transform, true);

                CurrentDividedOrgan = transform.GetComponent<ZAnatomy.DividedOrganController>();

                
                startButton.interactable = true;

                foreach (ZAnatomy.MeshInteractiveController interactive in transform.GetComponentsInChildren<ZAnatomy.MeshInteractiveController>())
                {
                    interactive.CanBeGrabbed = true;
                    interactive.MorgueIsInPlace = false;
                    // interactive.transform.position += Utils.GetDifferenceFromMeshCenter(teleportInnerObject);
                }

                morgueTeleport.ResetCurrentInnerObject();
            }
        }
    }

    [Button(size:ButtonSizes.Gigantic)]
    public void OnClickStart(){
        List<Transform> positions = DistributorController.RequestPositions(CurrentDividedOrgan.Pieces.Count);
        
        // CurrentDividedOrgan.SetPositions(positions);
        Utils.DividedOrganSetPositionsByLeanTween(CurrentDividedOrgan, positions);

        Debug.Log("Clicked on START");
        Destroy(CurrentDividedOrgan.GetComponent<ZAnatomy.DividedOrganGroup>());

        foreach (ZAnatomy.DividedOrganPiece p in this.GetComponentsInChildren<ZAnatomy.DividedOrganPiece>())
            p.organGroup = null;

        maxDistance = CurrentDividedOrgan.MaxDistanceConnectedPieces;
    }

    [Button(size:ButtonSizes.Gigantic)]
    public void OnClickReset(){

        Debug.Log("Clicked on RESET");
        // CurrentDividedOrgan.ResetData();
        Utils.DividedOrganResetDataByLeanTween(CurrentDividedOrgan);
        CurrentDividedOrgan.gameObject.AddComponent<ZAnatomy.DividedOrganGroup>();
    }

    [Button(size:ButtonSizes.Gigantic)]
    public void OnClickFinish(){

        Debug.Log("Clicked on FINISH");
        // CurrentDividedOrgan.ResetData();
        Utils.DividedOrganResetDataByLeanTween(CurrentDividedOrgan);
        Destroy(CurrentDividedOrgan.gameObject);
        // CurrentDividedOrgan.ResetData();
    }

    [Button(size: ButtonSizes.Gigantic)]
    public bool CheckPieceState(GameObject piece, bool connect = true)
    {
        ZAnatomy.DividedOrganPiece organPiece = piece.GetComponent<ZAnatomy.DividedOrganPiece>();
        if (organPiece.organGroup != null)
        {
            foreach (ZAnatomy.DividedOrganPiece p in organPiece.organGroup.GetComponentsInChildren<ZAnatomy.DividedOrganPiece>())
                if (CheckPiece(p, connect)) return true;
        }
        else
            if (CheckPiece(organPiece,connect)) return true;

        return false;
    }

    private bool CheckPiece(ZAnatomy.DividedOrganPiece organPiece, bool connect)
    {
        foreach (ZAnatomy.DividedOrganPiece.ConnectionData data in organPiece.ConnectedPieces)
        {
            if (data.IsConnected && data.Piece.isActiveAndEnabled)
            {
                if (data.Piece.transform.parent == organPiece.transform.parent)
                {
                    if ((data.Piece.transform.parent != null &&
                        data.Piece.transform.parent.GetComponent<ZAnatomy.DividedOrganGroup>() != null))
                        continue;
                }
                // Vector3 localDirection = (organPiece.transform.rotation) * (data.Piece.transform.position - organPiece.transform.position);
                // Vector3 localDirection = (organPiece.transform.rotation) * (data.Piece.transform.position - organPiece.transform.position);
                Vector3 localDirection = data.GetLocalDirection(); 
                float distance = Vector3.Distance(localDirection, data.DistanceTo);
                if (distance < maxDistance)
                {
                    if (connect) Connect(organPiece, data.Piece);
                    return true;
                }
            }
        }

        return false;
    }

    private void Connect(ZAnatomy.DividedOrganPiece piece1, ZAnatomy.DividedOrganPiece piece2)
    {
        ZAnatomy.DividedOrganGroup dividedGroupScript;
        if (piece1.organGroup == null && piece2.organGroup == null)
        {
            Debug.Log("Ambos null");
            dividedGroupScript = new GameObject("DividedGroup").AddComponent<ZAnatomy.DividedOrganGroup>();
            dividedGroupScript.gameObject.name = "Divided Group";
            dividedGroupScript.gameObject.transform.parent = piece1.transform.parent;
            dividedGroupScript.AddPieces(new ZAnatomy.DividedOrganPiece[] { piece1, piece2 });
            // dividedGroupScript.ConnectTo(piece1, piece2);
            Utils.DividedOrganGroupConnectToByLeanTween(dividedGroupScript, piece1, piece2);
        }
        else if (piece1.organGroup != null && piece2.organGroup != null)
        {
            if (piece1.organGroup == piece2.organGroup) return;
            Debug.Log("Ambos existe");
            dividedGroupScript = piece1.organGroup;
            ZAnatomy.DividedOrganPiece[] piecesToMove = piece2.organGroup.GetComponentsInChildren<ZAnatomy.DividedOrganPiece>();
            dividedGroupScript.AddPieces(piece2.organGroup);
            foreach(ZAnatomy.DividedOrganPiece piece in piecesToMove)
                // dividedGroupScript.ConnectTo(piece1, piece);
                Utils.DividedOrganGroupConnectToByLeanTween(dividedGroupScript, piece1, piece);
        }
        else if (piece1.organGroup != null)
        {
            Debug.Log("Existe el primero");
            dividedGroupScript = piece1.organGroup;
            dividedGroupScript.AddPieces(new ZAnatomy.DividedOrganPiece[] { piece2 });
            //dividedGroupScript.ConnectTo(piece1, piece2);
            Utils.DividedOrganGroupConnectToByLeanTween(dividedGroupScript, piece1, piece2);
        }
        else
        {
            Debug.Log("Existe el segundo");
            dividedGroupScript = piece2.organGroup;
            dividedGroupScript.AddPieces(new ZAnatomy.DividedOrganPiece[] { piece1 });
            // dividedGroupScript.ConnectTo(piece2, piece1);
            Utils.DividedOrganGroupConnectToByLeanTween(dividedGroupScript, piece2, piece1);
        }
    }

    public override void WrongGrabbing(ZAnatomy.MeshInteractiveController bestCandidate, VRGrabbing hand)
    {
        // Nothing
    }

    public override void CheckRelease(ZAnatomy.MeshInteractiveController grabbedObject, VRGrabbing hand)
    {
        CheckPieceState(grabbedObject.gameObject);
    }

    public override bool CanBeGrabbed(ZAnatomy.MeshInteractiveController bestCandidate)
    {
        return true;
    }

    public override bool CheckGrabbedModelDistance(ZAnatomy.MeshInteractiveController interactive, float distanceMultiplayer = 1)
    {
        bool canConnect = CheckPieceState(interactive.gameObject, false);
        return canConnect;
    }

    public override bool CheckGrabbedModelInsideHole(ZAnatomy.MeshInteractiveController interactive)
    {
        maxDistance = CurrentDividedOrgan.MaxDistanceConnectedPieces*2;

        bool canConnect = CheckPieceState(interactive.gameObject, false);

        maxDistance = CurrentDividedOrgan.MaxDistanceConnectedPieces;
        return canConnect;
    }
}
