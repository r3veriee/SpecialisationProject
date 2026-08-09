using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class MovementTech : MonoBehaviour
{
    [Header("Core Movement Stats")]
    public float walkSpeed = 16f;
    public float acceleration = 12f;
    public float groundFriction = 25f;

    [Header("Anti-Slip & Air Control")]
    public float airAcceleration = 18f;
    public float airDrag = 1.5f;
    public float turnMultiplier = 2.5f;

    [Header("Tech Stats")]
    public float dashForce = 45f;
    public float jumpForce = 20f;
    public float hyperMultiplier = 3.5f;
    public float hyperVerticalMultiplier = 1.2f;
    public float superMultiplier = 1.75f;
    public float superVerticalMultiplier = 1.35f;
    public float techFrictionMultiplier = 0.15f;
    public float dashDuration = 0.25f;
    public float dashCooldown = .7f;
    public float wavedashAngleThreshold = -0.1f;

    [Header("Slide")]
    public float slideBoost = 10f;
    public float slideFriction = 1f;
    public float normalHeight = 2f;
    public float slideHeight = 1f;
    public CapsuleCollider playerCollider;

    [Header("Input & Camera")]
    public Transform playerCamera;
    public float mouseSensitivity = 0.1f;
    public float jumpBufferTime = 0.2f;
    public float groundCoyoteTime = 0.3f;
    public Transform groundCheckPoint;
    public float groundCheckRadius = 0.3f;
    public LayerMask groundLayer;

    [Header("Camera Tilt & Slide Effects")]
    public float wallRunTilt = 15f;
    public float tiltSpeed = 6f;
    public float standingCamHeight = 0.8f;
    public float slidingCamHeight = 0.4f;
    public float crouchTransitionSpeed = 10f;
    private float currentTilt = 0f;

    [Header("Wallrun")]
    public LayerMask wallLayer;
    public float wallCheckDistance = 0.6f;
    public float minWallRunSpeed = 4f;
    public float wallRunGravity = 0.5f;
    public float wallJumpForce = 25f;
    public float wallJumpForwardBoost = 12f;
    public float wallRunAcceleration = 4f;
    public float maxWallRunSpeed = 24f;
    private bool isWallrunning;
    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;
    private bool wallLeft;
    private bool wallRight;

    public float upwardDashMultiplier = 0.4f;

    private PlayerControls controls;
    private Rigidbody rb;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private float xRotation = 0f;
    private float yRotation = 0f;

    private bool isCrouching;
    private bool wasCrouching;
    private bool wasCrouchingForAudio = false;
    private bool isGrounded;
    private bool isDashing;
    private bool techActive;
    public bool canDash = true;
    private bool isJumpHeld;
    private bool isDashTechDownward;

    private float dashTimer;
    public float dashCooldownTimer;
    private float jumpBufferCounter;
    private float groundCoyoteCounter;
    private float jumpCooldownTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        yRotation = transform.eulerAngles.y;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        controls = new PlayerControls();

        controls.Player.Jump.performed += ctx => jumpBufferCounter = jumpBufferTime;
        controls.Player.Dash.performed += ctx => StartDash();
        controls.Player.Crouch.performed += ctx => isCrouching = true;
        controls.Player.Crouch.canceled += ctx => isCrouching = false;
    }

    void OnEnable() => controls.Enable();
    void OnDisable() => controls.Disable();

    void Update()
    {
        if (Time.timeScale == 0f) return;
        if (jumpCooldownTimer > 0) jumpCooldownTimer -= Time.deltaTime;
        if (dashCooldownTimer > 0) dashCooldownTimer -= Time.deltaTime;
        if (jumpBufferCounter > 0) jumpBufferCounter -= Time.deltaTime;
        if (dashTimer > 0) dashTimer -= Time.deltaTime;
        else isDashing = false;

        isJumpHeld = controls.Player.Jump.ReadValue<float>() > 0.5f;
        moveInput = controls.Player.Move.ReadValue<Vector2>();
        lookInput = controls.Player.Look.ReadValue<Vector2>();
        HandleLook();

        // Dash Reset
        if ((isGrounded || isWallrunning) && dashCooldownTimer <= 0) canDash = true;

        if (isDashing && isCrouching)
        {
            isDashTechDownward = true;
        }

        // Jump Execution
        bool validJumpInput = jumpBufferCounter > 0 || (isJumpHeld && isDashing);
        
        if (validJumpInput && groundCoyoteCounter > 0 && jumpCooldownTimer <= 0)
        {
            bool intentToHyper = isDashTechDownward || isCrouching;
            if (isGrounded || (isDashing && !intentToHyper)) ExecuteJumpLogic();
            
        }

        wasCrouching = isCrouching;
    }

    void FixedUpdate()
    {
        //rb.MoveRotation(Quaternion.Euler(0f, yRotation, 0f));
        CheckGrounded();
        CheckWallRun();
        ApplyMovementPhysics();
    }

    void HandleLook()
    {
        lookInput = controls.Player.Look.ReadValue<Vector2>();

        // Calculate the absolute rotation for both up/down and left/right
        xRotation -= lookInput.y * mouseSensitivity;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        yRotation += lookInput.x * mouseSensitivity;
        transform.rotation = Quaternion.Euler(0f, yRotation, 0f); // TEST FIXING
        // Handle Camera Tilt (Wallrunning)
        float targetTilt = 0f;
        if (isWallrunning)
        {
            if (wallLeft) targetTilt = -wallRunTilt;
            else if (wallRight) targetTilt = wallRunTilt;
        }
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSpeed);

        // Apply Up/Down and Tilt to the Camera
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, currentTilt);

        // Handle crouching camera height shifts
        float targetCamHeight = isCrouching ? slidingCamHeight : standingCamHeight;
        Vector3 camLocalPos = playerCamera.localPosition;
        camLocalPos.y = Mathf.Lerp(camLocalPos.y, targetCamHeight, Time.deltaTime * crouchTransitionSpeed);
        playerCamera.localPosition = camLocalPos;
    }

    void StartDash()
    {
        if (isDashing || !canDash || dashCooldownTimer > 0) return;

        isDashing = true;
        canDash = false;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;
        isDashTechDownward = false;

        Vector3 dashVelocity = playerCamera.forward * dashForce;

        if (playerCamera.forward.y > 0)
        {
            dashVelocity.y *= upwardDashMultiplier;
        }

        rb.linearVelocity = Vector3.zero;
        rb.linearVelocity = dashVelocity;
        AudioManager.Instance.PlaySFX(SFXType.Dash);
    }

    void ExecuteJumpLogic()
    {
        jumpBufferCounter = 0;
        groundCoyoteCounter = 0;
        jumpCooldownTimer = 0.2f;

        if (isDashing)
        {
            techActive = true;
            Vector3 forwardDir = new Vector3(playerCamera.forward.x, 0, playerCamera.forward.z).normalized;

            if (isDashTechDownward || isCrouching)
            {
                // HYPER / WAVEDASH
                transform.position += Vector3.up * 0.15f;
                rb.linearVelocity = (forwardDir * (dashForce * hyperMultiplier * 0.5f)) + (Vector3.up * jumpForce * hyperVerticalMultiplier);
                AudioManager.Instance.PlaySFX(SFXType.TechDash);
            }
            else
            {
                // SUPER JUMP
                rb.linearVelocity = (forwardDir * (dashForce * superMultiplier)) + (Vector3.up * jumpForce * superVerticalMultiplier);
                AudioManager.Instance.PlaySFX(SFXType.TechDash);
            }
            dashTimer = 0;
            isDashing = false;
            dashCooldownTimer = 0f;
            canDash = true;
        }
        else
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            AudioManager.Instance.PlaySFX(SFXType.Jump);
        }
    }

    void ExecuteWallJump()
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;

        // Calculate forward direction along the wall
        Vector3 wallForward = Vector3.ProjectOnPlane(flatVel, wallNormal).normalized;
        if (flatVel.magnitude < 0.1f) wallForward = Vector3.ProjectOnPlane(playerCamera.forward, wallNormal).normalized;

        float currentSpeed = flatVel.magnitude;

        Vector3 jumpDirection = (Vector3.up * jumpForce) + (wallNormal * wallJumpForce) + (wallForward * Mathf.Max(currentSpeed, walkSpeed));

        rb.linearVelocity = jumpDirection;

        // Reset Dash and tag as Tech so maintain landing speed
        canDash = true;
        dashCooldownTimer = 0f;
        jumpCooldownTimer = 0.2f;
        techActive = true;
    }

    void ApplyMovementPhysics()
    {
        Vector3 moveDirection = (transform.right * moveInput.x + transform.forward * moveInput.y).normalized;

        Vector3 targetVelocity = moveDirection * walkSpeed;
        Vector3 currentHorizontal = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        rb.useGravity = !isWallrunning;

        bool isTurning = moveInput.magnitude > 0.1f && Vector3.Dot(targetVelocity.normalized, currentHorizontal.normalized) < -0.1f;
        bool isLettingGo = moveInput.magnitude < 0.1f;

        if (isCrouching)
        {
            playerCollider.height = slideHeight;
            playerCollider.center = new Vector3(0, (slideHeight - normalHeight) / 2f, 0);
            if (!wasCrouchingForAudio) // only true on the entry frame
            {
                AudioManager.Instance.PlaySFX(SFXType.Slide);
                wasCrouchingForAudio = true;
            }
        }
        else
        {
            playerCollider.height = normalHeight;
            playerCollider.center = Vector3.zero;
            wasCrouchingForAudio = false;
        }

        if (!isDashing)
        {
            float appliedFriction;

            if (techActive)
            {
                if (isGrounded && (isLettingGo || isTurning) && !isCrouching)
                {
                    techActive = false;
                    appliedFriction = groundFriction;
                }
                else
                {
                    appliedFriction = groundFriction * techFrictionMultiplier;
                    if (isGrounded && currentHorizontal.magnitude <= walkSpeed + 1f && jumpCooldownTimer <= 0) techActive = false;
                }
            }
            else if (isWallrunning)
            {
                appliedFriction = 0f;

                rb.AddForce(Vector3.down * 15f, ForceMode.Acceleration);

                float yVel = rb.linearVelocity.y;
                if (yVel < -wallRunGravity) yVel = -wallRunGravity;
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, yVel, rb.linearVelocity.z);

                // Keep hugging the wall
                Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
                rb.AddForce(-wallNormal * 2f * Time.fixedDeltaTime, ForceMode.VelocityChange);

                // Generate forward speed
                Vector3 wallForward = Vector3.ProjectOnPlane(playerCamera.forward, wallNormal).normalized;
                if (currentHorizontal.magnitude < maxWallRunSpeed)
                {
                    rb.AddForce(wallForward * wallRunAcceleration, ForceMode.Acceleration);
                }
            }
            else
            {
                if (isCrouching && isGrounded && currentHorizontal.magnitude > walkSpeed * 0.5f)
                {
                    appliedFriction = slideFriction;
                    if (!wasCrouching) rb.AddForce(playerCamera.forward * slideBoost, ForceMode.Impulse);
                }
                else
                {
                    appliedFriction = isGrounded ? groundFriction : airDrag;
                }
            }

            if (!isWallrunning)
            {
                if (!isGrounded && currentHorizontal.magnitude > walkSpeed)
                {
                    if (isLettingGo)
                    {
                        targetVelocity = currentHorizontal;
                    }
                    else
                    {
                        targetVelocity = moveDirection * currentHorizontal.magnitude;
                    }
                }

                Vector3 velocityChange = (targetVelocity - currentHorizontal);
                bool isSliding = isCrouching && isGrounded && currentHorizontal.magnitude > walkSpeed * 0.5f;
                float currentAccelRate;

                if (techActive || isSliding)
                {
                    currentAccelRate = appliedFriction;
                }
                else if (!isLettingGo)
                {
                    float baseAccel = isGrounded ? acceleration : airAcceleration;
                    currentAccelRate = isTurning ? baseAccel * turnMultiplier : baseAccel;
                }
                else
                {
                    float overspeedMultiplier = isGrounded ? Mathf.Max(1f, currentHorizontal.magnitude / walkSpeed) : 1f;
                    currentAccelRate = appliedFriction * overspeedMultiplier;
                }

                float maxAccelThisFrame = currentAccelRate * 10f * Time.fixedDeltaTime;
                Vector3 finalForce = Vector3.ClampMagnitude(velocityChange, maxAccelThisFrame);

                rb.AddForce(finalForce, ForceMode.VelocityChange);
            }
        }
    }

    void CheckGrounded()
    {
        if (jumpCooldownTimer > 0)
        {
            isGrounded = false;
            groundCoyoteCounter = 0;
            return;
        }

        bool wasGrounded = isGrounded;
        if (groundCheckPoint != null) isGrounded = Physics.CheckSphere(groundCheckPoint.position, groundCheckRadius, groundLayer);

        if (isGrounded && !wasGrounded && isDashing && rb.linearVelocity.y < 0) rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (isGrounded) groundCoyoteCounter = groundCoyoteTime;
        else groundCoyoteCounter -= Time.deltaTime;
    }

    void CheckWallRun()
    {
        wallRight = Physics.Raycast(transform.position, transform.right, out rightWallHit, wallCheckDistance, wallLayer);
        wallLeft = Physics.Raycast(transform.position, -transform.right, out leftWallHit, wallCheckDistance, wallLayer);

        if (jumpCooldownTimer > 0)
        {
            if (isWallrunning) AudioManager.Instance.StopLoopingSFX(SFXType.WallRun); // ADD THIS
            isWallrunning = false;
            return;
        }

        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        bool isNextToWall = wallLeft || wallRight;

        if (isWallrunning)
        {
            // wallrun ending naturally
            if (!isNextToWall || isGrounded || flatVel.magnitude < (minWallRunSpeed * 0.5f))
            {
                isWallrunning = false;
                AudioManager.Instance.StopLoopingSFX(SFXType.WallRun); // was PlaySFX
            }
            else if (jumpBufferCounter > 0)
            {
                ExecuteWallJump();
                isWallrunning = false;
                jumpBufferCounter = 0;
                AudioManager.Instance.StopLoopingSFX(SFXType.WallRun);
                AudioManager.Instance.PlaySFX(SFXType.Jump);
            }
            else
            {
                Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
                Vector3 wallForward = Vector3.ProjectOnPlane(flatVel, wallNormal).normalized;

                float currentForwardSpeed = Vector3.Dot(flatVel, wallForward);
                rb.linearVelocity = new Vector3(wallForward.x * currentForwardSpeed, rb.linearVelocity.y, wallForward.z * currentForwardSpeed);
            }
        }
        else
        {
            // wallrun starting
            if (isNextToWall && !isGrounded && flatVel.magnitude >= minWallRunSpeed)
            {
                isWallrunning = true;
                canDash = true;
                dashCooldownTimer = 0f;

                Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
                Vector3 wallForward = Vector3.ProjectOnPlane(flatVel, wallNormal).normalized;

                rb.linearVelocity = new Vector3(wallForward.x * flatVel.magnitude, 0f, wallForward.z * flatVel.magnitude);
                AudioManager.Instance.PlaySFX(SFXType.WallRun); // was StopLoopingSFX
            }
        }
    }

    public void ResetDash()
    {
        canDash = true;
        dashCooldownTimer = 0f;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = wallRight ? Color.green : Color.red;
        Gizmos.DrawRay(transform.position, transform.right * wallCheckDistance);

        Gizmos.color = wallLeft ? Color.green : Color.red;
        Gizmos.DrawRay(transform.position, -transform.right * wallCheckDistance);
    }
}