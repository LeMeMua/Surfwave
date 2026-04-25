
using NUnit.Framework;
using Unity.Cinemachine;
using UnityEditor.Callbacks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;

public class StateFloatingMovement : StateFloating
{
    #region inputdirection
    Vector3 viewDir;
    float forwardInput;
    float horizontalInput;

    Vector3 inputDir;
    #endregion
    
    #region aiming
    float sensX=200;

    float sensY=200;

    float yRotation;

    float xRotation;
    float isAiming;
    Vector2 aimDir;
    #endregion
    
    #region velocityandjump
    float rotationSpeed = 7;
    float velocity = 7;

    bool isGrounded;

    float isJumping;

    float jumpForce;

    #endregion
    public override void Begin()
    {
        base.Begin();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        jumpForce = Mathf.Abs(100* Physics.gravity.y);
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
        viewDir = playerObj.transform.position - new Vector3(stateMachine.cam.transform.position.x, playerObj.transform.position.y, 
        stateMachine.cam.transform.position.z);
        orientation.transform.forward = viewDir.normalized;
        inputDir = orientation.transform.forward * forwardInput + orientation.transform.right * horizontalInput;

        if (inputDir!= Vector3.zero)
        {
           RotatePlayertoInput();
           ApplyMovement();
           ApplyDrag();
           SetMaxVelocity();
        }
        else if (isAiming==1f)
        {
            Aiming();
        }

        if (isJumping == 1f)
        {
            SetJumpForce();
        }

        else
        {
            yRotation=0;
            xRotation=0;
            stateMachine.cam.GetComponent<CinemachineCamera>().Priority = 10;
            stateMachine.aimCam.GetComponent<CinemachineCamera>().Priority = 0;
        }

        //print(inputDir);
    }

    public override void OnOnEnable()
    {
        base.OnOnEnable();
        _inputActions.Floating.Enable();
        Debug.Log("Floating ENABLED");
    }

    public override void OnOnDisable()
    {
        base.OnOnDisable();
        _inputActions.Floating.Disable();
    }

    public override void OnAwake()
    {
        base.OnAwake();
        _inputActions = new Player_Actions();
        _inputActions.Floating.Forward.performed += MovementF;
        _inputActions.Floating.Forward.canceled += MovementF;

        _inputActions.Floating.Right.performed += MovementR;
        _inputActions.Floating.Right.canceled += MovementR;

        _inputActions.Floating.Aim.performed+=Aim;
        _inputActions.Floating.Aim.canceled+=Aim;

        _inputActions.Floating.MoveCam.performed+=GetCameraAim;
        _inputActions.Floating.MoveCam.canceled+=GetCameraAim;

        _inputActions.Floating.Jump.performed+=Jump;
        _inputActions.Floating.Jump.canceled+=Jump;
    }

    public override void OnOnDestroy()
    {
        base.OnOnDestroy();
        _inputActions.Floating.Forward.performed -= MovementF;
        _inputActions.Floating.Forward.canceled -= MovementF;

        _inputActions.Floating.Right.performed -= MovementR;
        _inputActions.Floating.Right.canceled -= MovementR;

        _inputActions.Floating.Aim.performed-=Aim;
        _inputActions.Floating.Aim.canceled-=Aim;

        _inputActions.Floating.MoveCam.performed-=GetCameraAim;
        _inputActions.Floating.MoveCam.canceled-=GetCameraAim;

        _inputActions.Floating.Jump.performed-= Jump;
        _inputActions.Floating.Jump.canceled-= Jump;

    }

    private void MovementF(InputAction.CallbackContext context)
    {
        forwardInput = context.ReadValue<float>();
    }

    private void MovementR(InputAction.CallbackContext context)
    {
        horizontalInput = context.ReadValue<float>();
    }

    private void Aim(InputAction.CallbackContext context)
    {
        isAiming = context.ReadValue<float>();
    }

    private void GetCameraAim(InputAction.CallbackContext context)
    {
        aimDir = context.ReadValue<Vector2>();
    }

    private void Jump(InputAction.CallbackContext context)
    {
        isJumping = context.ReadValue<float>();
    }

    private void Aiming()
    {
        if (stateMachine.aimCam.GetComponent<CinemachineCamera>().Priority != 10)
            {
                stateMachine.cam.GetComponent<CinemachineCamera>().Priority = 0;
                stateMachine.aimCam.GetComponent<CinemachineCamera>().Priority = 10;
            }
            orientation.transform.position=playerObj.transform.position;
            yRotation-=aimDir.y*Time.fixedDeltaTime*sensY;
            xRotation+=aimDir.x*Time.fixedDeltaTime*sensX;
            orientation.transform.rotation= Quaternion.Euler(yRotation,xRotation,0);
            playerObj.transform.rotation = Quaternion.Euler(0,yRotation,0);
    }

    /* private void RotatePlayertoInput()
    {
        Vector3 projectedInput = Vector3.ProjectOnPlane(inputDir, playerObj.transform.up);
        
        if (projectedInput.sqrMagnitude < 0.001f) return;
        
        projectedInput.Normalize();
        playerObj.transform.forward = Vector3.Slerp(playerObj.transform.forward, projectedInput, Time.fixedDeltaTime * rotationSpeed);
    } */

    private void RotatePlayertoInput()
    {

        // Solo rotar en el eje Y, ignorar inclinación
        Quaternion targetYaw = Quaternion.LookRotation(
            new Vector3(inputDir.x, 0f, inputDir.z), 
            Vector3.up
        );

        // Extraer solo el yaw actual del personaje
        Quaternion currentYaw = Quaternion.Euler(0f, playerObj.transform.eulerAngles.y, 0f);

        // Interpolar solo el yaw
        Quaternion newYaw = Quaternion.Slerp(currentYaw, targetYaw, Time.fixedDeltaTime * rotationSpeed);

        // Aplicar el nuevo yaw manteniendo el pitch y roll que tiene el personaje
        Vector3 currentEuler = playerObj.transform.eulerAngles;
        playerObj.transform.eulerAngles = new Vector3(currentEuler.x, newYaw.eulerAngles.y, currentEuler.z);
    }

    private void ApplyMovement()
    {
        body.AddForce(inputDir.normalized * velocity * 10, ForceMode.Acceleration);
    }

    private void ApplyDrag()
    {
        if (isGrounded)
        {
            body.linearDamping=3;
        }

        else
        {
            body.linearDamping=1;
        }
    }

    private void SetMaxVelocity()
    {
        if(body.linearVelocity.magnitude > velocity)
        {
            Vector3 max_vel=body.linearVelocity.normalized * velocity;
            body.linearVelocity = max_vel;
        }
    }

    private void SetJumpForce()
    {
        float t_jump= Mathf.Abs(Vector3.Dot(playerObj.transform.forward.normalized, Vector3.up));
        t_jump=1-t_jump;
        float t_height = oceanHeight/maxOceanHeight*maxOceanHeight;
        body.AddForce(Vector3.up * t_jump * t_height * jumpForce, ForceMode.Acceleration);
    }
}
