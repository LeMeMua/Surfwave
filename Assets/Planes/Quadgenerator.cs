using System.Collections.Generic;
using System;
using UnityEngine;

public class Quadgenerator : MonoBehaviour
{
    
    Mesh DynamicQuad;
    List<Vector3> Vertices = new List<Vector3>() ;

    List<int> TrianglesIndices = new List<int>() ;

    public int DimensionQuad = 100;

    //public static Action<MeshFilter> QuadGenerated;
    
    void Start()
    {
        DynamicQuad = new Mesh();
        DynamicQuad.name= "DynamicQuad";
        Create_vertices();
        Create_triangles();
        Generate_mesh();

        GetComponent<MeshFilter>().mesh = DynamicQuad;
        //QuadGenerated?.Invoke(GetComponent<MeshFilter>());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Create_vertices()
    {
        for(int i= 0; i<DimensionQuad; i++)
        {
            for(int j=0; j < DimensionQuad; j++)
            {
                Vertices.Add(new Vector3(j,0,i));
            }
        }
    }
    void Create_triangles()
    {
        for (int i= 0; i<DimensionQuad-1; i++)
        {
            for (int j= 0; j<DimensionQuad-1; j++)
            {
            int actualvertex = j + (DimensionQuad*i);
            int rightvertex = actualvertex + 1;
            int bottomLvertex = actualvertex+DimensionQuad;
            int bottomRvertex = bottomLvertex + 1 ;
            
            TrianglesIndices.Add(actualvertex);
            TrianglesIndices.Add(bottomLvertex);
            TrianglesIndices.Add(rightvertex);

            TrianglesIndices.Add(rightvertex);
            TrianglesIndices.Add(bottomLvertex);
            TrianglesIndices.Add(bottomRvertex);
            }
            
        }
    }

    void Generate_mesh()
    {
        DynamicQuad.SetVertices(Vertices);
        DynamicQuad.SetTriangles(TrianglesIndices, 0);
        DynamicQuad.RecalculateNormals();
        DynamicQuad.RecalculateBounds();

    }
}
