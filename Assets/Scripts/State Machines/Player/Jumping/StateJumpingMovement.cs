using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

public class StateJumpingMovement : StateJumping
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
    float velocity = 7;
    float isJumping;

    float jumpForce;

    #endregion

    float isThrowing;
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
    }

    public override void OnOnEnable()
    {
        base.OnOnEnable();
        _inputActions.Jumping.Enable();
        Debug.Log("Jumping ENABLED");
    }

    public override void OnOnDisable()
    {
        base.OnOnDisable();
        _inputActions.Jumping.Disable();
    }

    public override void OnAwake()
    {
        base.OnAwake();
        _inputActions = new Player_Actions();
        _inputActions.Jumping.Forward.performed += MovementF;
        _inputActions.Jumping.Forward.canceled += MovementF;

        _inputActions.Jumping.Right.performed += MovementR;
        _inputActions.Jumping.Right.canceled += MovementR;

        _inputActions.Jumping.Aim.performed+=Aim;
        _inputActions.Jumping.Aim.canceled+=Aim;

        _inputActions.Jumping.MoveCam.performed+=GetCameraAim;
        _inputActions.Jumping.MoveCam.canceled+=GetCameraAim;

        _inputActions.Jumping.Throw.performed+=Throw;
        _inputActions.Jumping.Throw.canceled+=Throw;
    }

    public override void OnOnDestroy()
    {
        base.OnOnDestroy();
        _inputActions.Jumping.Forward.performed -= MovementF;
        _inputActions.Jumping.Forward.canceled -= MovementF;

        _inputActions.Jumping.Right.performed -= MovementR;
        _inputActions.Jumping.Right.canceled -= MovementR;

        _inputActions.Jumping.Aim.performed-=Aim;
        _inputActions.Jumping.Aim.canceled-=Aim;

        _inputActions.Jumping.MoveCam.performed-=GetCameraAim;
        _inputActions.Jumping.MoveCam.canceled-=GetCameraAim;

        _inputActions.Jumping.Throw.performed-=Throw;
        _inputActions.Jumping.Throw.canceled-=Throw;

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
        //print(newInputDir.normalized);
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

}
