
using UnityEngine;

public class WavesManager : MonoBehaviour
{
    public static WavesManager instance;
    public WavesParameters Groupwaves;
    Vector4 wave;

    public Material waveMovement;

    void Awake()
    {
        if (!instance)
        {
            instance=this;
        }
    }

    void OnDestroy()
    {
        if (instance)
        {
            instance=null;
        }
    }
    void Start()
    {
        SetVectors();
    }

    void SetVectors()
    {
        /* waveA= new Vector4(waves.waveA.directionX,waves.waveA.directionY, waves.waveA.steepness, waves.waveA.waveLength);
        waveB= new Vector4(waves.waveB.directionX,waves.waveB.directionY, waves.waveB.steepness, waves.waveB.waveLength);
        waveMovement.SetVector("_WaveA", waveA);
        waveMovement.SetVector("_WaveB", waveB); */
        for (int i = 0; i<Groupwaves.waves.Length; i++)
        {
            wave = new Vector4(Groupwaves.waves[i].directionX,Groupwaves.waves[i].directionY, Groupwaves.waves[i].steepness, Groupwaves.waves[i].waveLength);
            ChangeVector(wave, i);
        }
    }

    void ChangeVector(Vector4 wavechanged, int num)
    {
            waveMovement.SetVector($"_Wave{num}",wavechanged);
    }
}
