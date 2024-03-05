using UnityEngine;
using System.Collections.Generic;
 
[RequireComponent(typeof(MeshCollider))]
public class MeshKDTree : MonoBehaviour
{
    int[] tris;
    Vector3[] verts;
    KDTree kd;
    VertTriList vt;

    void Awake()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        Mesh mesh = mf.mesh;
        vt = new VertTriList(mesh);
        verts = mesh.vertices;
        tris = mesh.triangles;
        kd = KDTree.MakeFromPoints(verts);
    }
 
    public Vector3 ClosestPointOnSurface(Vector3 position)
    {
        position = transform.InverseTransformPoint(position);
        return transform.TransformPoint(NearestPointOnMesh(position, verts, kd, tris, vt));
    }
 
    List<int> nearests = new List<int>();
    Vector3 NearestPointOnMesh(Vector3 pt, Vector3[] verts, KDTree vertProx, int[] tri, VertTriList vt)
    {
        //    First, find the nearest vertex (the nearest point must be on one of the triangles
        //    that uses this vertex if the mesh is convex).
        //  Since there can be multiple vertices on a single spot, we need to find the correct vert and triangle.
        vertProx.FindNearestEpsilon(pt, nearests);
 
        Vector3 nearestPt = Vector3.zero;
        float nearestSqDist = 100000000f;
        Vector3 possNearestPt;
 
        for(int i = 0; i < nearests.Count; i++)
        {
            //    Get the list of triangles in which the nearest vert "participates".
            int[] nearTris = vt[nearests[i]];
 
            for(int j = 0; j < nearTris.Length; j++)
            {
                int triOff = nearTris[j] * 3;
                Vector3 a = verts[tri[triOff]];
                Vector3 b = verts[tri[triOff + 1]];
                Vector3 c = verts[tri[triOff + 2]];
 
                ClosestPointOnTriangleToPoint(ref pt, ref a, ref b, ref c, out possNearestPt);
                float possNearestSqDist = (pt - possNearestPt).sqrMagnitude;
 
                if(possNearestSqDist < nearestSqDist)
                {
                    nearestPt = possNearestPt;
                    nearestSqDist = possNearestSqDist;
                }
            }
        }
 
        return nearestPt;
    }
 
    public static void ClosestPointOnTriangleToPoint(ref Vector3 point, ref Vector3 vertex1, ref Vector3 vertex2, ref Vector3 vertex3, out Vector3 result)
    {
        //Source: Real-Time Collision Detection by Christer Ericson
        //Reference: Page 136
 
        //Check if P in vertex region outside A
        Vector3 ab = vertex2 - vertex1;
        Vector3 ac = vertex3 - vertex1;
        Vector3 ap = point - vertex1;
 
        float d1 = Vector3.Dot(ab, ap);
        float d2 = Vector3.Dot(ac, ap);
        if (d1 <= 0.0f && d2 <= 0.0f)
        {
            result = vertex1; //Barycentric coordinates (1,0,0)
            return;
        }
 
        //Check if P in vertex region outside B
        Vector3 bp = point - vertex2;
        float d3 = Vector3.Dot(ab, bp);
        float d4 = Vector3.Dot(ac, bp);
        if (d3 >= 0.0f && d4 <= d3)
        {
            result = vertex2; // barycentric coordinates (0,1,0)
            return;
        }
 
        //Check if P in edge region of AB, if so return projection of P onto AB
        float vc = d1 * d4 - d3 * d2;
        if (vc <= 0.0f && d1 >= 0.0f && d3 <= 0.0f)
        {
            float v = d1 / (d1 - d3);
            result = vertex1 + v * ab; //Barycentric coordinates (1-v,v,0)
            return;
        }
 
        //Check if P in vertex region outside C
        Vector3 cp = point - vertex3;
        float d5 = Vector3.Dot(ab, cp);
        float d6 = Vector3.Dot(ac, cp);
        if (d6 >= 0.0f && d5 <= d6)
        {
            result = vertex3; //Barycentric coordinates (0,0,1)
            return;
        }
 
        //Check if P in edge region of AC, if so return projection of P onto AC
        float vb = d5 * d2 - d1 * d6;
        if (vb <= 0.0f && d2 >= 0.0f && d6 <= 0.0f)
        {
            float w = d2 / (d2 - d6);
            result = vertex1 + w * ac; //Barycentric coordinates (1-w,0,w)
            return;
        }
 
        //Check if P in edge region of BC, if so return projection of P onto BC
        float va = d3 * d6 - d5 * d4;
        if (va <= 0.0f && (d4 - d3) >= 0.0f && (d5 - d6) >= 0.0f)
        {
            float w = (d4 - d3) / ((d4 - d3) + (d5 - d6));
            result = vertex2 + w * (vertex3 - vertex2); //Barycentric coordinates (0,1-w,w)
            return;
        }
 
        //P inside face region. Compute Q through its barycentric coordinates (u,v,w)
        float denom = 1.0f / (va + vb + vc);
        float v2 = vb * denom;
        float w2 = vc * denom;
        result = vertex1 + ab * v2 + ac * w2; //= u*vertex1 + v*vertex2 + w*vertex3, u = va * denom = 1.0f - v - w
    }
}