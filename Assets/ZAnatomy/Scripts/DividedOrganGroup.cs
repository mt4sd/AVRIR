using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ZAnatomy{
    public class DividedOrganGroup : MonoBehaviour
    {
        private void AddPieceInit(DividedOrganPiece piece)
        {
            piece.organGroup = this;
            piece.transform.parent = this.transform;
        }

        internal void AddPieces(DividedOrganGroup group)
        {
            foreach (DividedOrganPiece piece in group.GetComponentsInChildren<DividedOrganPiece>())
            {
                AddPieceInit(piece);
            }

            Destroy(group.gameObject);
        }

        internal void AddPieces(DividedOrganPiece[] dividedOrganPieces)
        {
            foreach(DividedOrganPiece piece in dividedOrganPieces)
            {
                AddPieceInit(piece);

            }
        }

        public void ConnectTo(DividedOrganPiece targetPiece, DividedOrganPiece newPiece)
        {
            DividedOrganPiece.ConnectionData data = targetPiece.ConnectedPieces.Find((x) => newPiece == x.Piece);

            if (data == null) return;

            Vector3 localDirection = (targetPiece.transform.rotation) * (data.DistanceTo);

            // newPiece.transform.position = targetPiece.transform.position - localDirection;
            // newPiece.transform.rotation = data.rotation * targetPiece.transform.rotation;

            Vector3 finalPos = FinalPosition(targetPiece, data);
            Quaternion finalRot = FinalRotation(targetPiece, data);

            // LeanTween.move(newPiece.gameObject, finalPos, 1).setEaseOutCirc().setOnComplete(() => {
            //     newPiece.transform.position = finalPos;
            // });

            // LeanTween.rotate(newPiece.gameObject, finalRot.eulerAngles, 1).setEaseOutCirc().setOnComplete(() => {
            //     newPiece.transform.rotation = finalRot;
            // });

            newPiece.gameObject.transform.position = finalPos;
            newPiece.gameObject.transform.rotation = finalRot;
        }

        public Vector3 FinalPosition(DividedOrganPiece targetPiece, DividedOrganPiece.ConnectionData data)
        {
            Vector3 localDirection = (targetPiece.transform.rotation) * (data.DistanceTo);
            return targetPiece.transform.position - localDirection;
        }
        public Quaternion FinalRotation(DividedOrganPiece targetPiece, DividedOrganPiece.ConnectionData data)
        {
            return data.rotation * targetPiece.transform.rotation;
        }
    }
}