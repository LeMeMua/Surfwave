using UnityEngine;

public class WavesManager : MonoBehaviour
{
    public WavesParameters waves;
    Vector4 waveA;
    Vector4 waveB;

    public Material waveMovement;

    void Start()
    {
        SetParameters();
    }

    void SetParameters()
    {
        waveA= new Vector4(waves.waveA.directionX,waves.waveA.directionY, waves.waveA.steepness, waves.waveA.waveLength);
        waveB= new Vector4(waves.waveB.directionX,waves.waveB.directionY, waves.waveB.steepness, waves.waveB.waveLength);
        waveMovement.SetVector("_WaveA", waveA);
        waveMovement.SetVector("_WaveB", waveB);
    }
}
