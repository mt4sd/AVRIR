using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserHead : MonoBehaviour
{
    public MeshRenderer[] meshesToRecolor;

    public void RecolorMeshes(Color PlayerColor)
    {
        foreach (MeshRenderer mesh in meshesToRecolor)
        {
            mesh.material.color = PlayerColor;
        }
    }
}
