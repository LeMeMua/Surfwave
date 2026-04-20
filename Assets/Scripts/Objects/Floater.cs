
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class Floater : MonoBehaviour
{
    private Rigidbody body;

    List<Vector3> vertices = new List<Vector3>();
    List<Vector3> worldVertices = new List<Vector3>();

    MeshFilter ocean;
    Vector3[] verticesArray;
    int objectVertex;

    public float oceanHeight = 0;

    public Vector3 normal = Vector3.zero;
    public Vector3 tangent = Vector3.zero;
    public Vector3 bitangent = Vector3.zero;




    [SerializeField] WavesParameters waves;


    float displacementAmount = 2f;
    float depthBeforeSubmerged =0.2f;


    void Start()
    {
        body= GetComponent<Rigidbody>();
        waves.waveA.Precompute();
        waves.waveB.Precompute();
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        oceanHeight = 0;
        CalculateOceanHeight(waves.waveA);
        CalculateOceanHeight(waves.waveB);
        if (transform.position.y < oceanHeight)
        {
            normal = Vector3.Cross(tangent,bitangent);
            normal = new Vector3(Mathf.Abs(normal.x), normal.y, Mathf.Abs(normal.z));
            transform.up = normal;
            Floating();
        }
    }

    void Floating()
    {
        float depth = oceanHeight - transform.position.y;  //que tanto se va a hundir
        float displacementMultiplier = Mathf.Clamp01(depth/depthBeforeSubmerged)*displacementAmount;
        body.AddForce(new Vector3(0f, Mathf.Abs(Physics.gravity.y)*displacementMultiplier, 0f), ForceMode.Acceleration);
        //body.AddForce(Vector3.up*displacementMultiplier, ForceMode.Acceleration);
    }

    void CalculateOceanHeight(Wave_Class wave)
    {
        float f = wave.k *(Vector2.Dot(wave.direction, new Vector2(transform.position.x, transform.position.z))- wave.c * Time.time);

        tangent += new Vector3(-wave.directionX * wave.directionX * (wave.steepness * Mathf.Sin(f)),
                    wave.directionX * (wave.steepness * Mathf.Cos(f)),
                    -wave.directionX* wave.directionY * (wave.steepness * Mathf.Sin(f)));

        bitangent += new Vector3(-wave.directionX * wave.directionY * (wave.steepness * Mathf.Sin(f)),
                    wave.directionY * (wave.steepness * Mathf.Cos(f)),
                    -wave.directionX* wave.directionY * (wave.steepness * Mathf.Sin(f)));
        

        oceanHeight+= wave.a * Mathf.Sin(f);

    }
    /* void GenerateVertices()
    {
        body = GetComponent<Rigidbody>();

        verticesArray = ocean.mesh.vertices;
        for (int i = 0; i<verticesArray.Length; i++)
        {
            vertices.Add(verticesArray[i]);
            //print(verticesArray[i]);
        }
        
        Vector3 worldVertex;
        for (int i = 0; i < vertices.Count; i++)
        {
            worldVertex = ocean.transform.TransformPoint(vertices[i]);
            worldVertices.Add(worldVertex);
        }
        
        
        if (ocean == null)
        {
            Debug.LogError("Ocean no esta bien");
        }

        if(worldVertices == null)
        {
            Debug.LogError("worldvertice no esta bien");
        }
    }

    int CalculateClosserVertex()
    {
        int closestVer = 0;
        int actualVer = 0;
        Vector2 vertexPositionCloser = new Vector2(worldVertices[0].x, worldVertices[0].z);
        Vector2 vertexPositionActual;
        float sqrDistanceCloser = (transform.position.x - vertexPositionCloser.x)*(transform.position.x - vertexPositionCloser.x) 
        + (transform.position.z - vertexPositionCloser.y)*(transform.position.z - vertexPositionCloser.y);
        float sqrDistanceActual;


        foreach(Vector3 v in worldVertices)
        {
            vertexPositionActual = new Vector2 (v.x,v.z) ;
            sqrDistanceActual = (transform.position.x - vertexPositionActual.x)*(transform.position.x - vertexPositionActual.x) 
            + (transform.position.z - vertexPositionActual.y)*(transform.position.z - vertexPositionActual.y);
            
            if (sqrDistanceActual < sqrDistanceCloser)
            {
                sqrDistanceCloser = sqrDistanceActual;
                closestVer = actualVer;
            }

            actualVer++;
        }
        return closestVer;
    }

    void SetOceanHeight(Vector3 closerPoint)
    {
        oceanHeight = closerPoint.y;
    } */

    /* void OnEnable()
    {
        Quadgenerator.QuadGenerated += AssingMeshFilter;
    }

    void OnDisable()
    {
        Quadgenerator.QuadGenerated -= AssingMeshFilter;
    }

    void AssingMeshFilter(MeshFilter _ocean)
    {
        ocean = _ocean;
        GenerateVertices();
    } */
}
