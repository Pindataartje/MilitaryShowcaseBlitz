using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Speeds")]
    public float walkSpeed = 4f;
    public float sprintSpeed = 8f;

    [Header("Acceleration")]
    public float accel = 20f;          // how fast you reach target speed while pressing input
    public float decel = 25f;          // how fast you stop when releasing input
    public float airAccel = 6f;        // limited air steering
    [Range(0f, 1f)] public float airControl = 0.35f; // how much you can re-aim mid-air (0..1)

    [Header("Jump & Gravity")]
    public float jumpHeight = 1.6f;
    public float gravity = -24f;
    public float groundedGraceTime = 0.12f;   // coyote time
    public float jumpBufferTime = 0.15f;      // jump pressed slightly before landing
    public float groundStickForce = 5f;       // keeps you glued to slopes when grounded

    [Header("References")]
    public Transform cameraTransform;

    CharacterController cc;

    // Horizontal velocity we control (x,z). We do NOT rotate this with the camera.
    Vector3 horizontalVel; // y=0
    float verticalVel;

    float lastGroundedTime;
    float lastJumpPressedTime;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (!cameraTransform && Camera.main) cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        // --- INPUT ---
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 input = new Vector3(h, 0f, v);
        input = Vector3.ClampMagnitude(input, 1f);

        // Camera-relative desired direction (world space)
        Vector3 camF = Vector3.forward, camR = Vector3.right;
        if (cameraTransform)
        {
            camF = cameraTransform.forward; camF.y = 0f; camF.Normalize();
            camR = cameraTransform.right; camR.y = 0f; camR.Normalize();
        }
        Vector3 desiredDir = (camF * input.z + camR * input.x); // world dir, y=0

        float targetSpeed = (Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed);
        Vector3 targetHorizontalVel = desiredDir * targetSpeed; // desired horizontal velocity (y=0)

        // Jump input buffer
        if (Input.GetButtonDown("Jump"))
            lastJumpPressedTime = Time.time;

        // Grounded bookkeeping / coyote time
        bool grounded = cc.isGrounded;
        if (grounded) lastGroundedTime = Time.time;

        // --- HORIZONTAL VELOCITY UPDATE ---
        // We explicitly move toward the target velocity. No SmoothDamp, no camera “re-aim” of existing velocity.
        float usedAccel;
        if (grounded)
        {
            // accelerate when input present, decelerate faster when no input
            bool hasInput = input.sqrMagnitude > 0.0001f;
            usedAccel = hasInput ? accel : decel;
        }
        else
        {
            usedAccel = airAccel;
            // In air, only allow partial steering towards the desired velocity, preserving momentum.
            // Blend the *direction* a bit to avoid instant heading snaps mid-air.
            if (targetHorizontalVel.sqrMagnitude > 0.0001f)
            {
                Vector3 aim = Vector3.Lerp(horizontalVel, targetHorizontalVel, airControl);
                targetHorizontalVel = aim;
            }
        }
        horizontalVel = Vector3.MoveTowards(horizontalVel, targetHorizontalVel, usedAccel * Time.deltaTime);

        // --- VERTICAL VELOCITY / JUMP ---
        // Gravity
        if (grounded && verticalVel < 0f)
        {
            // small negative keeps contact; also apply extra stick to avoid micro bounces on slopes
            verticalVel = -2f;
            verticalVel += gravity * groundStickForce * Time.deltaTime;
        }
        else
        {
            verticalVel += gravity * Time.deltaTime;
        }

        // Jump (coyote + buffer)
        bool canJump = (Time.time - lastGroundedTime) <= groundedGraceTime;
        bool buffered = (Time.time - lastJumpPressedTime) <= jumpBufferTime;
        if (buffered && canJump)
        {
            verticalVel = Mathf.Sqrt(jumpHeight * -2f * gravity);
            lastJumpPressedTime = -999f; // consume buffer
            lastGroundedTime = -999f;    // consume coyote
        }

        // --- MOVE ---
        Vector3 move = horizontalVel;
        move.y = verticalVel;
        cc.Move(move * Time.deltaTime);
    }

    void OnDrawGizmosSelected()
    {
        if (!cc) cc = GetComponent<CharacterController>();
        if (!cc) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position + cc.center - new Vector3(0, cc.height / 2 - cc.radius, 0), cc.radius);
    }
}
