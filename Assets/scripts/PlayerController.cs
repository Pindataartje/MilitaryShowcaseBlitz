using UnityEngine;

/// <summary>
/// Simple, robust player controller using CharacterController:
/// - WASD / Arrow keys to move
/// - Hold Left Shift to sprint
/// - Space to jump
/// - Handles gravity & smooth falling
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 4f;
    public float sprintSpeed = 8f;
    [Range(0f, 1f)] public float movementSmoothTime = 0.08f;

    [Header("Jump & Gravity")]
    public float jumpHeight = 1.6f;          // peak jump height (meters)
    public float gravity = -24f;             // stronger than default for snappier feel
    public float groundedGraceTime = 0.1f;   // allow small forgiveness for jump input

    [Header("References")]
    public Transform cameraTransform;        // used to rotate movement with camera

    CharacterController cc;
    Vector3 velocity;             // vertical velocity (y)
    Vector3 currentMovement;      // smoothed horizontal movement
    Vector3 movementVelocityRef;  // used by SmoothDamp

    float lastGroundedTime;
    float jumpInputTime;
    bool jumpRequested;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        ReadInput();
        HandleMovement();
    }

    void ReadInput()
    {
        // Horizontal & vertical input (WASD / arrows)
        float h = Input.GetAxisRaw("Horizontal"); // raw for snappy input
        float v = Input.GetAxisRaw("Vertical");

        // Convert input to camera-relative direction
        Vector3 inputDir = new Vector3(h, 0f, v);
        inputDir = Vector3.ClampMagnitude(inputDir, 1f);

        if (cameraTransform != null)
        {
            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();
            Vector3 worldDir = forward * inputDir.z + right * inputDir.x;
            // smoothly interpolate movement
            float targetSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;
            Vector3 targetMovement = worldDir * targetSpeed;
            currentMovement = Vector3.SmoothDamp(currentMovement, targetMovement, ref movementVelocityRef, movementSmoothTime);
        }
        else
        {
            // fallback: local-space movement
            Vector3 targetMovement = transform.TransformDirection(inputDir) * (Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed);
            currentMovement = Vector3.SmoothDamp(currentMovement, targetMovement, ref movementVelocityRef, movementSmoothTime);
        }

        // Jump input
        if (Input.GetButtonDown("Jump"))
        {
            jumpRequested = true;
            jumpInputTime = Time.time;
        }

        // Track grounded time for jump grace
        if (cc.isGrounded)
            lastGroundedTime = Time.time;
    }

    void HandleMovement()
    {
        // Apply gravity
        if (cc.isGrounded && velocity.y <= 0f)
        {
            velocity.y = -2f; // small negative to keep contact with ground
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        // Jump logic with small grace time
        bool canJump = (Time.time - lastGroundedTime) <= groundedGraceTime;
        if (jumpRequested && canJump)
        {
            // v = sqrt(2 * g * h) but gravity is negative
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpRequested = false;
        }
        // Clear jump request if too old (optional)
        if (jumpRequested && (Time.time - jumpInputTime) > 0.2f)
            jumpRequested = false;

        // Combine horizontal movement and vertical velocity
        Vector3 move = currentMovement * Time.deltaTime;
        move += new Vector3(0f, velocity.y * Time.deltaTime, 0f);

        cc.Move(move);
    }

    // Optional: visualize ground check in editor (CharacterController center/radius)
    void OnDrawGizmosSelected()
    {
        if (cc == null) cc = GetComponent<CharacterController>();
        if (cc == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position + cc.center - new Vector3(0, cc.height / 2 - cc.radius, 0), cc.radius);
    }
}
