using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class EditorToolsHelper : MonoBehaviour
{
    [Button]
    private void RecalculateNormals(){
        Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
    }
}
