
using Mono.Cecil.Cil;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class StateFloating : StateBasePlayer
{
    public Rigidbody body;


    public float oceanHeight {get; private set;}

    public float maxOceanHeight{get;private set;}

    public Vector3 normal = Vector3.up;

    Vector3 newNormal = Vector3.up;
    Vector3 tangent = Vector3.zero;
    Vector3 bitangent = Vector3.zero;
    Quaternion rotationRef = Quaternion.identity;
    WavesParameters Groupwaves;

    float displacementAmount = 2f;
    float depthBeforeSubmerged =0.2f;

    void Awake()
    {
        //playerplayerObj = transform.parent.gameplayerplayerObject;
    }

    public override void Begin()
    {
        base.Begin();
        body=playerObj.GetComponent<Rigidbody>();
        Groupwaves=WavesManager.instance.Groupwaves;
        foreach (Wave_Class wave in Groupwaves.waves)
        {
            wave.Precompute();
            maxOceanHeight+=wave.a;
        }
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
        if (playerObj.transform.position.y < oceanHeight)
        {
            newNormal = Vector3.Cross(bitangent.normalized, tangent.normalized);
            normal = newNormal;

            // evitar flips
            /* if (Vector3.Dot(Vector3.up, newNormal) < 0)
            {
                newNormal = -newNormal;
            }
            normal = newNormal;
            normal.y = Mathf.Abs(newNormal.y); */
            //normal = new Vector3(normal.x, Mathf.Abs(normal.y), normal.z);
            //time += Time.fixedDeltaTime;
            AligntoSurface(newNormal);
            
            Floating();

            //print(obj.transform.rotation.z);
        }
        else if (playerObj.transform.position.y > maxOceanHeight + 1)
        {
            stateMachine.ChangeTo("JumpingMovement");
        }
        
    }
    void Floating()
    {
        float depth = oceanHeight - playerObj.transform.position.y;  //que tanto se va a hundir
        float displacementMultiplier = Mathf.Clamp01(depth/depthBeforeSubmerged)*displacementAmount;
        body.AddForce(new Vector3(0f, Mathf.Abs(Physics.gravity.y)*displacementMultiplier, 0f), ForceMode.Force);
        //body.AddForce(Vector3.up*displacementMultiplier, ForceMode.Acceleration);
    }

    void CalculateOceanHeight(Wave_Class wave)
    {
        float f = wave.k *(Vector2.Dot(wave.direction, new Vector2(playerObj.transform.position.x, playerObj.transform.position.z))- wave.c * Time.time);

        tangent += new Vector3(-wave.directionX * wave.directionX * (wave.steepness * Mathf.Sin(f)),
                    wave.directionX * (wave.steepness * Mathf.Cos(f)),
                    -wave.directionX* wave.directionY * (wave.steepness * Mathf.Sin(f)));

        bitangent += new Vector3(-wave.directionX * wave.directionY * (wave.steepness * Mathf.Sin(f)),
                    wave.directionY * (wave.steepness * Mathf.Cos(f)),
                    -wave.directionX * wave.directionY * (wave.steepness * Mathf.Sin(f)));
        

        oceanHeight+= wave.a * Mathf.Sin(f);

    }

    /* void AligntoSurface(Vector3 prueba) 
    { 
        float check =Vector3.Dot(Vector3.up, prueba);
        if(check>0.1)
        {
            rotationRef= Quaternion.Slerp(obj.transform.rotation, Quaternion.FromToRotation(Vector3.up, prueba), animCurve.Evaluate(time % animationtime)); 
            obj.transform.rotation = rotationRef; 
        }
        else
        {
            float maxangle = -90f;
            float myangle = Vector3.Angle(Vector3.up,prueba);
            float excess = myangle - maxangle;
    // lo regresa hacia arriba gradualmente
            if(myangle> maxangle)
            {
            normal = Vector3.Slerp(newNormal, Vector3.up, excess / maxangle);
            obj.transform.up = normal;
            }
        }
        
        //print(obj.transform.rotation); 
    } */
    void AligntoSurface(Vector3 surfaceNormal) 
    { 
        float check = Vector3.Dot(Vector3.up, surfaceNormal);

        Vector3 targetNormal = check > 0.1f 
            ? surfaceNormal 
            : Vector3.Slerp(Vector3.up, surfaceNormal, 0.8f).normalized;

        Vector3 projectedForward = Vector3.ProjectOnPlane(playerObj.transform.forward, targetNormal);

        if (projectedForward.sqrMagnitude < 0.001f)
            projectedForward = Vector3.ProjectOnPlane(playerObj.transform.right, targetNormal);

        projectedForward.Normalize();

        Quaternion targetRotation = Quaternion.LookRotation(projectedForward, targetNormal);

        playerObj.transform.rotation = Quaternion.Slerp(
            playerObj.transform.rotation, 
            targetRotation, 
            Time.fixedDeltaTime * 3f  // ajusta la velocidad aquí
        );
    }

}
