using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RoadMeshGeneratorStarter : MonoBehaviour
{
    public SplinePathTemp splinePath;
    public int rungs = 10;
    public float width = 3f;
    public Material material;
    public float textureRepeatLength = 2f;
    public bool rebuildEveryFrame;
    
    //instead of having a bank angle list, it is required to instead specify the bank angle data in each know that is create in unity 
    // and refactor the code here so it can gather that data as get the same behavior we are currently getting (since it works rn)
    public float[] bankAngles;
    
    MeshFilter _meshFilter;
    MeshRenderer _meshRenderer;
    
    void Start()
    {
        _meshFilter = gameObject.AddComponent<MeshFilter>();
        _meshRenderer = gameObject.AddComponent<MeshRenderer>();
        
        BuildRoadMesh();
    }

    void Update()
    {
        if(rebuildEveryFrame)
            BuildRoadMesh();
    }

    void BuildRoadMesh()
    {

        List<Vector3> verts = new();
        List<Vector2> uvs = new();
        Array.Resize(ref bankAngles, splinePath.SegmentCount + 1);

        for (int i = 0; i < rungs; i++)
        {
            //Get the distance to the rung
            float fraction = i / (float)(rungs - 1);
            float distance = splinePath.TotalLength * fraction;
            
            

            //Use distance to look up u
            float u = splinePath.ParameterAtDistance(distance);
           
            
            //use u to ge the center point
            Vector3 centerPoint = splinePath.SamplePoint(u);
            Vector3 tangent = splinePath.SampleTangent(u);
            
            //calculate the "right" direction
            Vector3 right = Vector3.Cross(Vector3.up, tangent).normalized;

            int segment = (int)Mathf.Min(u, splinePath.SegmentCount - 1);
            float t = u - segment;
            float bankAngle = Mathf.SmoothStep(bankAngles[segment], bankAngles[segment + 1], t);
            
            right = Quaternion.AngleAxis(bankAngle, tangent) * right;
            
            float halfWidth = width * 0.5f;
            float lift = Mathf.Sin(bankAngle * Mathf.Deg2Rad) * halfWidth;
            
            centerPoint += Vector3.up * Mathf.Abs(lift);
            
            //create the left and right verts
            Vector3 leftVert = centerPoint - right * halfWidth;
            Vector3 rightVert = centerPoint + right * halfWidth; 
            
            verts.Add(leftVert);
            verts.Add(rightVert);
            
            uvs.Add(new Vector2(0f, distance / textureRepeatLength));
            uvs.Add(new Vector2(1f, distance / textureRepeatLength));
        }

        List<int> triangles = new();
        for (int i = 0; i < rungs - 1 ; i++)
        {
            int bottomLeft = i * 2;
            int bottomRight = bottomLeft + 1;
            int topLeft = (i+1) * 2;
            int topRight = topLeft + 1;
            
            triangles.Add(bottomLeft);
            triangles.Add(topLeft);
            triangles.Add(bottomRight);
            
            
            triangles.Add(topLeft);
            triangles.Add(topRight);
            triangles.Add(bottomRight);
            
        }
        
        Mesh mesh = new();
        mesh.vertices = verts.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        
        _meshFilter.mesh = mesh;
        _meshRenderer.material = material;
        
        
        

    }
}