using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using Sirenix.OdinInspector;


namespace ZAnatomy{
    public class DividedOrganController : MonoBehaviour
    {
        public List<DividedOrganPiece> Pieces;
        public float MaxDistanceConnectedPieces = 0.05f;

        // [Button]
        public void GetPiecesFromChildren(){
            Pieces = new List<DividedOrganPiece>(GetComponentsInChildren<DividedOrganPiece>());
        }
        // [Button]
        public void InitAllPieces(){
            foreach(DividedOrganPiece piece in Pieces){
                piece.InitData();
            }
            Debug.Log("Initted all Pieces");
        }

        // [Button]
        public void UpdatePieces(){
            foreach(DividedOrganPiece piece in Pieces){
                piece.UpdateConnections(Pieces);
            }

            Debug.Log("Updated Pieces");
        }

        public void SetPositions(List<Transform> positions){
            for (int i = 0; i < Pieces.Count; i++)
            {
                DividedOrganPiece piece = Pieces[i];

                // LeanTween.move(piece.gameObject, positions[i].position, 1f);
                piece.gameObject.transform.position = positions[i].position;
            }
        }

        public void ResetData(){

            for (int i = 0; i < Pieces.Count; i++)
            {
                DividedOrganPiece piece = Pieces[i];
                piece.ResetData();
            }
        }
    }
}
