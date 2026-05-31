
using NUnit.Framework;
using Unity.Cinemachine;
using Unity.VisualScripting;
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

    bool wasAiming;
    float lastAimYaw;
    #endregion
    
    #region velocityandjump
    float rotationSpeed = 7;
    float velocity = 15;

    bool isGrounded;

    float isJumping;
    bool readyJump;
    float jumpForce;

    #endregion
    float isThrowing;
    public override void Begin()
    {
        base.Begin();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        jumpForce = Mathf.Abs(100* Physics.gravity.y);
        body.linearDamping = 1f;
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();

        CheckGround();
        if (isGrounded)
        {
            stateMachine.ChangeTo("GroundingMovement");
            return;
        }

        if (isAiming == 0f && !wasAiming)
        {
            viewDir = playerObj.transform.position - new Vector3(
                stateMachine.cam.transform.position.x,
                playerObj.transform.position.y,
                stateMachine.cam.transform.position.z
            );

            orientation.transform.forward = viewDir.normalized;
        }

        inputDir = orientation.transform.forward * forwardInput + orientation.transform.right * horizontalInput;

        if (isAiming == 1f)
        {
            wasAiming = true;
            Aiming();
        }
        else
        {
            if (wasAiming)
            {
                ReturnFromAiming();
                wasAiming = false;
            }

            stateMachine.cam.GetComponent<CinemachineCamera>().Priority = 10;
            stateMachine.aimCam.GetComponent<CinemachineCamera>().Priority = 0;
        }

        if (inputDir != Vector3.zero && isAiming == 0f)
        {
            RotatePlayertoInput();
            ApplyMovement();
            SetMaxVelocity();
        }

        if (readyJump && isJumping == 1f)
        {
            SetJumpForce();
        }
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

        _inputActions.Floating.Throw.performed+=Throw;
        _inputActions.Floating.Throw.canceled+=Throw;
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

        _inputActions.Floating.Throw.performed-=Throw;
        _inputActions.Floating.Throw.canceled-=Throw;

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

        if(isJumping==1 && !readyJump)
        {
            readyJump = true;
        }

        if (isJumping == 0)
        {
            readyJump =false;
        }
    }

    private void Throw(InputAction.CallbackContext context)
    {
        isThrowing = context.ReadValue<float>();
    }

        private void Aiming()
    {
        if (stateMachine.aimCam.GetComponent<CinemachineCamera>().Priority != 10)
        {
            stateMachine.cam.GetComponent<CinemachineCamera>().Priority = 0;
            stateMachine.aimCam.GetComponent<CinemachineCamera>().Priority = 10;

            xRotation = playerObj.transform.eulerAngles.y;
            yRotation = 0f;
        }

        orientation.transform.position = Vector3.Lerp(
            orientation.transform.position,
            playerObj.transform.position,
            0.5f
        );

        xRotation += aimDir.x * Time.fixedDeltaTime * sensX;

        yRotation -= aimDir.y * Time.fixedDeltaTime * sensY;
        yRotation = Mathf.Clamp(yRotation, -40f, 60f);

        orientation.transform.rotation = Quaternion.Euler(yRotation, xRotation, 0f);

        playerObj.transform.rotation = Quaternion.Euler(0f, xRotation, 0f);

        lastAimYaw = xRotation;

        if (isThrowing == 1f)
        {
            stateMachine.ChangeTo("Hooking");
        }
    }

    private void ReturnFromAiming()
    {
        Quaternion yawRotation = Quaternion.Euler(0f, lastAimYaw, 0f);

        orientation.transform.rotation = yawRotation;
        playerObj.transform.rotation = yawRotation;

        stateMachine.cam.transform.rotation = yawRotation;
    }
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
        Vector3 newInputDir = new Vector3(inputDir.x, 0f, inputDir.z);
        body.AddForce(newInputDir.normalized * velocity * 10, ForceMode.Force);
    }

    private void SetMaxVelocity()
    {
        Vector3 horizontalVelocity = new Vector3(body.linearVelocity.x, 0f, body.linearVelocity.z);
        if(horizontalVelocity.sqrMagnitude > velocity*velocity)
        {
            Vector3 max_vel=horizontalVelocity.normalized * velocity;
            body.linearVelocity = new Vector3(max_vel.x, body.linearVelocity.y, max_vel.z);
        }
    }

    private void SetJumpForce()
    {
        float t_jump= Mathf.Abs(Vector3.Dot(playerObj.transform.forward.normalized, Vector3.up));
        t_jump=1-t_jump;
        float t_height = Mathf.Clamp((oceanHeight/maxOceanHeight*maxOceanHeight/100), 0, 100);
        body.AddForce(Vector3.up * t_jump * t_height * jumpForce, ForceMode.Impulse);
    }

    private void CheckGround()
    {
        isGrounded = Physics.Raycast(playerObj.transform.position, Vector3.down, 1.5f, WavesManager.instance.mask);
        Debug.DrawRay(playerObj.transform.position, Vector3.down * 1.5f, isGrounded ? Color.green : Color.red);
    }
}
