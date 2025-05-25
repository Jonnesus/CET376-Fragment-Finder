using System.Collections;
using UnityEngine;

[RequireComponent(typeof(InputManager))]

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Transform cameraOrientation;

    [Header("Movement")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float sprintSpeed;
    [SerializeField] private float slideSpeed;
    [SerializeField] private float maxSlopeAngle;
    [SerializeField] private float speedIncreaseMultiplier;
    [SerializeField] private float slopeIncreaseMultiplier;
    [SerializeField] private float groundDrag;

    private float moveSpeed;
    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;

    [Header("Jumping")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpCooldown;
    [SerializeField] private float airMultiplier;

    private bool readyToJump;

    [Header("Crouching")]
    [SerializeField] private float crouchSpeed;
    [SerializeField] private float crouchYScale;

    private float startYScale;

    [Header("Ground Check")]
    [SerializeField] private float playerHeight;
    [SerializeField] private float maxGroundDistance;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask ground;

    [Header("Misc")]
    public bool grounded;
    public bool freeze;
    public bool sliding;
    private bool exitingSlope;
    private bool enableMovementOnNextTouch;

    private InputManager IM;

    private RaycastHit slopeHit;

    private Vector3 moveDirection;

    private Rigidbody rb;

    public MovementState state;
    public enum MovementState
    {
        //List of possible player states
        freeze,
        walking,
        sprinting,
        crouching,
        sliding,
        air
    }

    private void Start()
    {
        IM = GetComponent<InputManager>();
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;

        startYScale = transform.localScale.y;
    }

    private void Update()
    {
        //Checks if player is in the ground
        grounded = GroundCheck();

        OnSlope();
        MyInput();
        SpeedControl();
        StateHandler();

        //Adds drag if on the ground
        if (grounded)
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = 0;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private bool GroundCheck()
    {
        //return Physics.CheckSphere(groundCheck.position, 0.1f, ground);
        RaycastHit hit;
        return Physics.Raycast(groundCheck.position, -groundCheck.up, out hit, maxGroundDistance, ground);
    }

    private void MyInput()
    {
        //Allows the player to jump when key is pressed, on ground and cooldown timer is off
        if (IM.jump && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        //Allows the player to crouch when key is pressed
        if (IM.crouch)
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.mass = 5;
        }

        //Stops crouching when key is released
        if (IM.crouch == false)
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
            rb.mass = 1;
        }
    }

    private void StateHandler()
    {
        //State checks ordered this way to prevent actions such as sprinting whilst crouching

        //Handles movement speed whilst frozen
        if (freeze)
        {
            state = MovementState.freeze;
            moveSpeed = 0;
            rb.linearVelocity = Vector3.zero;
        }

        //Handles movemment speed while sliding
        if (sliding)
        {
            state = MovementState.sliding;

            if (OnSlope() && rb.linearVelocity.y < 0.1f)
                desiredMoveSpeed = slideSpeed;

            else
                desiredMoveSpeed = sprintSpeed;
        }

        //Handles movemment speed while crouching
        else if (IM.crouch)
        {
            state = MovementState.crouching;
            desiredMoveSpeed = crouchSpeed;
        }

        //Handles movemment speed while sprinting
        else if (grounded && IM.sprint)
        {
            state = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed;
        }

        //Handles movemment speed while walking
        else if (grounded)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }

        //Handles movemment speed while in air/jumping
        else
        {
            state = MovementState.air;
        }

        //Checks if desiredMoveSpeed has changed >8f, if not changes speed instantly
        if (Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 8f && moveSpeed != 0)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed());
        }
        else
        {
            moveSpeed = desiredMoveSpeed;
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
    }

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        //Gradually changes moveSpeed to desiredMoveSpeed, e.g. sliding down slope to walking speed decreases gradually instead of instant change
        //Velocity increases quicker at greater slope angle
        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

            if (OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);

                time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
            }
            else
                time += Time.deltaTime * speedIncreaseMultiplier;

            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
    }

    private void MovePlayer()
    {
        //Calculates movement direction
        moveDirection = cameraOrientation.forward * IM.move.y + cameraOrientation.right * IM.move.x;
        moveDirection.y = 0f;

        //Movement whilst on a slope
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);

            if (rb.linearVelocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }

        //Movement whilst on the ground (AddForce stops floating while crouching)
        else if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        //Movement whilst in the air/jumping
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

        //Disables gravity whilst on a slope to stop perpetually sliding down
        rb.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {
        //Limits speed on a slope
        if (OnSlope() && !exitingSlope)
        {
            if (rb.linearVelocity.magnitude > moveSpeed)
                rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
        }

        //Limits speed on the ground or in the air
        else
        {
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            //Limits velocity
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            }
        }
    }

    private void Jump()
    {
        //Allows player to jump on slopes
        exitingSlope = true;

        //Resets y velocity when jumping
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
    private void ResetJump()
    {
        readyToJump = true;

        exitingSlope = false;
    }

    public void JumpToPosition(Vector3 targetPosition, float trajectoryHeight)
    {

        velocityToSet = CalculateJumpVelocity(transform.position, targetPosition, trajectoryHeight);
        Invoke(nameof(SetVelocity), 0.1f);
    }

    private Vector3 velocityToSet;
    private void SetVelocity()
    {
        enableMovementOnNextTouch = true;
        rb.linearVelocity = velocityToSet;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (enableMovementOnNextTouch)
        {
            enableMovementOnNextTouch = false;
        }
    }

    public Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight)
    {
        float gravity = Physics.gravity.y;
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity)
            + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity));

        return velocityXZ + velocityY;
    }

    public bool OnSlope()
    {
        //Checks if player is on a slope. If yes + below maximum slope angle, returns slope angle
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        //returns Vector3 direction relative to slope angle
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }
}