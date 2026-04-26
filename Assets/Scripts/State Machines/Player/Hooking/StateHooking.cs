using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;
using UnityEditor.Callbacks;
using Unity.Mathematics;
using Mono.Cecil.Cil;

public class StateHooking : StateBasePlayer
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    float verticalInput;
    float horizontalInput;

    float isThrowing;

    bool isCanceled;
    Rigidbody body;

    float maxRadio=20f;
    bool ishitting;
    float angularSpeed = 360;

    float ropeLength;
    RaycastHit hit;

    Vector3 r;
    Vector3 axis;
    Vector3 tangent;
    Vector3 hookPoint = Vector3.zero;

    Quaternion reference;

    float time;

    float durationanimation = 0.5f;

    public override void Begin()
    {
        base.Begin();
        body = playerObj.GetComponent<Rigidbody>();
        stateMachine.cam.GetComponent<CinemachineCamera>().Priority = 10;
        stateMachine.aimCam.GetComponent<CinemachineCamera>().Priority = 0;
        isCanceled=false;
        

        // Inicializa aquí si aún no existe
        if (_inputActions == null)
        {
            _inputActions = new Player_Actions();
            _inputActions.Hooking.Enable();
            _inputActions.Hooking.Forward.performed += MovementF;
            _inputActions.Hooking.Forward.canceled += MovementF;
            _inputActions.Hooking.Right.performed += MovementR;
            _inputActions.Hooking.Right.canceled += MovementR;
            //_inputActions.Hooking.Throw.started += Throw;
            //_inputActions.Hooking.Throw.canceled += Throw;
        }
    }

    public override void OnOnEnable()
    {
        base.OnOnEnable();
        Debug.Log("Hooking ENABLED");
    }

    public override void OnOnDisable()
    {
        base.OnOnDisable();
        _inputActions.Hooking.Disable();
        _inputActions.Hooking.Forward.performed -= MovementF;
        _inputActions.Hooking.Forward.canceled -= MovementF;

        _inputActions.Hooking.Right.performed -= MovementR;
        _inputActions.Hooking.Right.canceled -= MovementR;
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
        if (_inputActions.Hooking.Throw.IsPressed())
        {
            startSwing3();
        }

        else
        {
            ishitting=false;
            body.useGravity = true;
            time=0;
            stateMachine.ChangeTo("JumpingMovement");
        }
    }

    public override void OnAwake()
    {
        base.OnAwake();
        _inputActions.Hooking.Forward.performed += MovementF;
        _inputActions.Hooking.Forward.canceled += MovementF;

        _inputActions.Hooking.Right.performed += MovementR;
        _inputActions.Hooking.Right.canceled += MovementR;
    }


    private void MovementF(InputAction.CallbackContext context)
    {
        verticalInput = context.ReadValue<float>();
    }

    private void MovementR(InputAction.CallbackContext context)
    {
        horizontalInput = context.ReadValue<float>();
    }

    void startSwing()
    {
        
        if(!ishitting)
        {
            ishitting=true;
            if(Physics.Raycast(stateMachine.aimCam.transform.position, stateMachine.aimCam.transform.forward, out hit, maxRadio, WavesManager.instance.mask))
            {
                Debug.DrawLine(playerObj.transform.position, hit.point, Color.red, 60f);
                hookPoint=hit.point;
                body.useGravity = false;
            }
        }

        else if (ishitting)
        {
            Vector3 r = playerObj.transform.position - hookPoint;
            axis = Vector3.Cross(r, body.linearVelocity).normalized;
            tangent = Vector3.Cross(axis, r).normalized;
            body.angularVelocity = tangent*Vector3.Angle(axis,tangent)*8;
        }
        
    }

    void startSwing2()
    {
        if(!ishitting)
        {
            if(Physics.Raycast(stateMachine.aimCam.transform.position, stateMachine.aimCam.transform.forward, out hit, maxRadio, WavesManager.instance.mask))
            {
                ishitting=true;
                Debug.DrawLine(playerObj.transform.position, hit.point, Color.red, 60f);
                hookPoint=hit.point;
                body.useGravity = false;
                r = playerObj.transform.position - hookPoint;
                if (body.linearVelocity.sqrMagnitude < 0.01f)
                {
                    axis = Vector3.Cross(r, stateMachine.aimCam.transform.right).normalized;
                    tangent = Vector3.Cross(axis, r).normalized;
                    reference = Quaternion.LookRotation(tangent, Vector3.up);

                }
                else
                {
                    axis= Vector3.Cross(r,body.linearVelocity).normalized;
                    tangent = Vector3.Cross(axis, r).normalized;
                    reference = Quaternion.LookRotation(tangent, Vector3.up);
                }
            }
        }

        if (ishitting)
        {
            time+=Time.fixedDeltaTime;
            var t = Mathf.Clamp01(time/durationanimation);
            r= Quaternion.AngleAxis(angularSpeed * Time.deltaTime, axis) * r;
            playerObj.transform.position = Vector3.Slerp(playerObj.transform.position, hookPoint + r, Time.deltaTime* 3f);
            //playerObj.transform.position = hookPoint + r;
            playerObj.transform.rotation = Quaternion.Slerp(playerObj.transform.rotation, reference, t);
            
        }
    }

    void startSwing3()
    {
        if (!isCanceled)
        {
            if(!ishitting)
            {
                if(Physics.Raycast(stateMachine.aimCam.transform.position, stateMachine.aimCam.transform.forward, out hit, maxRadio, WavesManager.instance.mask))
                {
                    ishitting=true;
                    Debug.DrawLine(playerObj.transform.position, hit.point, Color.red, 60f);
                    hookPoint=hit.point;
                    body.useGravity = false;
                    r = playerObj.transform.position - hookPoint;
                    if (body.linearVelocity.sqrMagnitude < 0.01f)
                    {
                        axis = Vector3.Cross(r, stateMachine.aimCam.transform.right).normalized;
                        tangent = Vector3.Cross(axis, r).normalized;
                        reference = Quaternion.LookRotation(tangent, Vector3.up);

                    }
                    else
                    {
                        axis= Vector3.Cross(r,body.linearVelocity).normalized;
                        tangent = Vector3.Cross(axis, r).normalized;
                        reference = Quaternion.LookRotation(tangent, Vector3.up);
                    }
                }
            }

            if (ishitting)
            {
                time += Time.fixedDeltaTime;
                var t = Mathf.Clamp01(time / durationanimation);

                Vector3 nextR = Quaternion.AngleAxis(angularSpeed * Time.deltaTime, axis) * r;
                Vector3 targetPos = hookPoint + nextR;

                Vector3 currentPos = playerObj.transform.position;
                Vector3 dir = targetPos - currentPos;
                float dist = dir.magnitude;

                float radiusCast = 0.5f;

                if (Physics.SphereCast(currentPos, radiusCast, dir.normalized, out RaycastHit hitCol, dist))
                {
                    isCanceled=true;
                    return;
                }

                r = nextR;
                playerObj.transform.position = targetPos;

                // rotación
                playerObj.transform.rotation = Quaternion.Slerp(playerObj.transform.rotation, reference, t);
            }
        }       
    }
}

