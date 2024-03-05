using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using Sirenix.OdinInspector;
using System;


namespace ZAnatomy{
    public class DividedOrganPiece : MonoBehaviour
    {
        [System.Serializable]
        public class ConnectionData{
            public Transform originalPiece;

            public DividedOrganPiece Piece;
            public Vector3 LocalPosition;
            public Vector3 DistanceTo;
            public Quaternion rotation;
            public bool IsConnected;

            public ConnectionData(Transform piece)
            {
                originalPiece = piece;
            }
            internal Vector3 GetLocalDirection()
            {
                Vector3 tmp = Quaternion.Inverse(originalPiece.transform.rotation) *(originalPiece.transform.position - Piece.transform.position);
                // return (Piece.transform.rotation) * (originalPiece.transform.position - Piece.transform.position);
                return tmp;
                // return (Piece.transform.position - originalPiece.transform.position);
            }
        }

        public DividedOrganController OrganController;
        public List<ConnectionData> ConnectedPieces;
        public Vector3 LocalPositionOnParent;
        public Vector3 LocalRotationOnParent;
        public MeshKDTree KDTree;
        public MeshCollider Collider;

        public DividedOrganGroup organGroup = null;

        public void Start()
        {
            organGroup = this.GetComponentInParent<DividedOrganGroup>();
        }

        public void UpdateConnections(List<DividedOrganPiece> pieces){
            ConnectedPieces = new List<ConnectionData>();

            Debug.Log("UPDT "+gameObject.name);
            Collider.convex = true;
            
            foreach(DividedOrganPiece piece in pieces){
                if(piece != this){
                    ConnectionData newConnectionData = new ConnectionData(this.transform);
                    newConnectionData.Piece = piece;
                    newConnectionData.LocalPosition = transform.InverseTransformPoint(piece.transform.position);
                    // newConnectionData.DistanceTo = (this.transform.rotation)*(piece.transform.position - this.transform.position);
                    newConnectionData.DistanceTo = newConnectionData.GetLocalDirection(); // (piece.transform.rotation)*(this.transform.position - piece.transform.position);
                    newConnectionData.rotation = this.transform.rotation * Quaternion.Inverse(piece.transform.rotation);
                    Debug.Log("-- Update connection: "+piece.gameObject.name);
                    // Vector3 closestPositionA = piece.KDTree.ClosestPointOnSurface(Collider.bounds.center);
                    // Vector3 closestPositionB = KDTree.ClosestPointOnSurface(piece.Collider.bounds.center);

                    piece.Collider.convex = true;
                    Vector3 closestPositionA = piece.Collider.ClosestPoint(Collider.bounds.center);
                    Vector3 closestPositionB = Collider.ClosestPoint(piece.Collider.bounds.center);
                    piece.Collider.convex = false;

                    newConnectionData.IsConnected = Vector3.Distance(closestPositionA, closestPositionB)<OrganController.MaxDistanceConnectedPieces;
                    ConnectedPieces.Add(newConnectionData);
                }
            }
            
            Collider.convex = false;
        }

        public Vector3 GetConnectedPiecePosition(DividedOrganPiece piece){
            ConnectionData data = ConnectedPieces.Find((x)=>x.Piece.gameObject == piece.gameObject);
            Vector3 globalPosition = transform.TransformPoint(data.LocalPosition);
            return globalPosition;
        }

        // [Button]
        public void InitData(){
            OrganController = GetComponentInParent<DividedOrganController>();

            KDTree = GetComponent<MeshKDTree>();
            if(KDTree == null)
                KDTree = gameObject.AddComponent<MeshKDTree>();

            Collider = GetComponent<MeshCollider>();
            if(Collider == null){
                Collider = gameObject.AddComponent<MeshCollider>();
            }

            LocalPositionOnParent = transform.localPosition;
            LocalRotationOnParent = transform.localEulerAngles;
        }

        // [Button]
        void ViewConnections()
        {
            foreach(ConnectionData piece in ConnectedPieces){
                Debug.Log("connection to: "+ piece.Piece.name + " - "+ GetConnectedPiecePosition(piece.Piece));
                Debug.DrawLine(transform.position, GetConnectedPiecePosition(piece.Piece), piece.IsConnected?Color.green:Color.red, 3);
            }
        }

        public void ResetData(){
            transform.SetParent(OrganController.transform,true);

    #if UNITY_EDITOR
            this.transform.localPosition = LocalPositionOnParent;
            this.transform.localEulerAngles = LocalRotationOnParent;//this.transform.eulerAngles = LocalRotationOnParent;
    #endif

            // LeanTween.moveLocal(this.gameObject, LocalPositionOnParent, 1f).setOnComplete(() =>
            // {
            //     this.transform.localPosition = LocalPositionOnParent;
            // });
            // LeanTween.rotateLocal(this.gameObject, LocalRotationOnParent, 1f).setOnComplete(() => { 
            //     this.GetComponent<MeshInteractiveController>().CanBeGrabbed = true;
            //     this.transform.localEulerAngles = LocalRotationOnParent; //this.transform.eulerAngles = LocalRotationOnParent;
            // });

            this.GetComponent<MeshInteractiveController>().CanBeGrabbed = true;
            transform.localPosition = LocalPositionOnParent;
            transform.localEulerAngles = LocalRotationOnParent; //this.transform.eulerAngles = LocalRotationOnParent;

            organGroup = this.GetComponentInParent<DividedOrganGroup>();
        }
    }

}