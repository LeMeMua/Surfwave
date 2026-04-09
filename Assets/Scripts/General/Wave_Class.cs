using System;
using UnityEngine;


[CreateAssetMenu(fileName = "WavesClass", menuName = "Scriptable Objects/WavesClass")]
public class Wave_Class : ScriptableObject
{
    
    [Range(-1,1)] public float directionX;
    [Range(-1,1)] public float directionY;

    [Range(0,1)] public float steepness;

    public float waveLength;

    [HideInInspector] public Vector2 direction;
    [HideInInspector] public float k;
    [HideInInspector] public float c;
    [HideInInspector] public float a;

    public void Precompute()
    {
        direction = new Vector2(directionX, directionY).normalized;
        k = 2 * Mathf.PI / waveLength;
        c = Mathf.Sqrt(9.8f/k);
        a = steepness /k;
    }
}
