using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

public class StateGroundingMovement : StateGrounding
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
    Quaternion lastAimRotation;
    #endregion
    
    #region velocityandjump
    float rotationSpeed = 7;
    float velocity = 12;

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
        jumpForce = Mathf.Abs(5* Physics.gravity.y);
        body.linearDamping = 3f;
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();

        if (!isGrounded)
        {
            stateMachine.ChangeTo("JumpingMovement");
            return;
        }

        // Si NO estás apuntando y NO acabas de soltar el apuntado,
        // la orientación se calcula normal usando la cámara principal.
        if (isAiming == 0f && !wasAiming)
        {
            viewDir = playerObj.transform.position - new Vector3(
                stateMachine.cam.transform.position.x,
                playerObj.transform.position.y,
                stateMachine.cam.transform.position.z
            );

            orientation.transform.forward = viewDir.normalized;
        }

        // Movimiento basado en la orientación actual
        inputDir = orientation.transform.forward * forwardInput + orientation.transform.right * horizontalInput;

        // Aiming
        if (isAiming == 1f)
        {
            wasAiming = true;
            Aiming();
            mira.SetActive(true);
        }
        else
        {
            if (wasAiming)
            {
                ReturnFromAiming();
                wasAiming = false;
                mira.SetActive(false);
            }

            stateMachine.cam.GetComponent<CinemachineCamera>().Priority = 10;
            stateMachine.aimCam.GetComponent<CinemachineCamera>().Priority = 0;
        }

        // Movimiento normal
        if (inputDir != Vector3.zero && isAiming == 0f)
        {
            RotatePlayertoInput();
            ApplyMovement();
            SetMaxVelocity();
        }

        // Salto
        if (readyJump && isJumping == 1f && isGrounded)
        {
            SetJumpForce();
        }
    }

    public override void OnOnEnable()
    {
        base.OnOnEnable();
        _inputActions.Grounding.Enable();
        Debug.Log("Grounding ENABLED");
    }

    public override void OnOnDisable()
    {
        base.OnOnDisable();
        _inputActions.Grounding.Disable();
    }

    public override void OnAwake()
    {
        base.OnAwake();
        _inputActions = new Player_Actions();
        _inputActions.Grounding.Forward.performed += MovementF;
        _inputActions.Grounding.Forward.canceled += MovementF;

        _inputActions.Grounding.Right.performed += MovementR;
        _inputActions.Grounding.Right.canceled += MovementR;

        _inputActions.Grounding.Aim.performed+=Aim;
        _inputActions.Grounding.Aim.canceled+=Aim;

        _inputActions.Grounding.MoveCam.performed+=GetCameraAim;
        _inputActions.Grounding.MoveCam.canceled+=GetCameraAim;

        _inputActions.Grounding.Jump.performed+=Jump;
        _inputActions.Grounding.Jump.canceled+=Jump;

        _inputActions.Grounding.Throw.performed+=Throw;
        _inputActions.Grounding.Throw.canceled+=Throw;
    }

    public override void OnOnDestroy()
    {
        base.OnOnDestroy();
        _inputActions.Grounding.Forward.performed -= MovementF;
        _inputActions.Grounding.Forward.canceled -= MovementF;

        _inputActions.Grounding.Right.performed -= MovementR;
        _inputActions.Grounding.Right.canceled -= MovementR;

        _inputActions.Grounding.Aim.performed-=Aim;
        _inputActions.Grounding.Aim.canceled-=Aim;

        _inputActions.Grounding.MoveCam.performed-=GetCameraAim;
        _inputActions.Grounding.MoveCam.canceled-=GetCameraAim;

        _inputActions.Grounding.Jump.performed-= Jump;
        _inputActions.Grounding.Jump.canceled-= Jump;

        _inputActions.Grounding.Throw.performed-=Throw;
        _inputActions.Grounding.Throw.canceled-=Throw;
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

            // Esto evita que al empezar a apuntar se voltee raro
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

        if (isThrowing == 1)
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
        body.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

}
