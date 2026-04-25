using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class StateJumping : StateBasePlayer
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float oceanHeight = 0;
    Rigidbody body;
    public Vector3 normal = Vector3.zero;
    Vector3 tangent = Vector3.zero;
    Vector3 bitangent = Vector3.zero;

    WavesParameters Groupwaves;

    float animationtime = 2f;
    float time = 0f;

    Quaternion startRotation = Quaternion.identity;

    public override void Begin()
    {
        base.Begin();
        body=playerObj.GetComponent<Rigidbody>();
        Groupwaves=WavesManager.instance.Groupwaves;
        startRotation= playerObj.transform.rotation;
    }
    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
        oceanHeight = 0;
        tangent = Vector3.zero;
        bitangent = Vector3.zero;
        foreach (Wave_Class wave in Groupwaves.waves)
        {
            CalculateOceanHeight(wave);
        }
        FixNormalRotation();
        if(playerObj.transform.position.y < oceanHeight)
        {
            stateMachine.ChangeTo("FloatingMovement");
        }
    }
    void CalculateOceanHeight(Wave_Class wave)
    {
        float f = wave.k *(Vector2.Dot(wave.direction, new Vector2(playerObj.transform.position.x, playerObj.transform.position.z))- wave.c * Time.time);

        tangent += new Vector3(-wave.directionX * wave.directionX * (wave.steepness * Mathf.Sin(f)),
                    wave.directionX * (wave.steepness * Mathf.Cos(f)),
                    -wave.directionX* wave.directionY * (wave.steepness * Mathf.Sin(f)));

        bitangent += new Vector3(-wave.directionX * wave.directionY * (wave.steepness * Mathf.Sin(f)),
                    wave.directionY * (wave.steepness * Mathf.Cos(f)),
                    -wave.directionX* wave.directionY * (wave.steepness * Mathf.Sin(f)));
        

        oceanHeight+= wave.a * Mathf.Sin(f);
    }

    void FixNormalRotation()
    {
        if (Vector3.Dot(playerObj.transform.up, Vector3.up) >= 1.0)
        {
            time = 0;
            startRotation = playerObj.transform.rotation;
            return;
        }
        if (time == 0) startRotation = playerObj.transform.rotation;

        time += Time.fixedDeltaTime;
        float t = Mathf.Clamp01(time/animationtime);
        //print(t);
        Vector3 forwardProjected = Vector3.ProjectOnPlane(playerObj.transform.forward, Vector3.up);
        if (forwardProjected.sqrMagnitude < 0.001f)
        {
            forwardProjected = Vector3.ProjectOnPlane(playerObj.transform.right, Vector3.up);
        }
        Quaternion targetrotation = Quaternion.LookRotation(forwardProjected, Vector3.up);
        
        playerObj.transform.rotation = Quaternion.Slerp(startRotation, targetrotation, t);
    }
}
