using UnityEngine;

/// <summary>
/// Simple first-person mouse look. Attach to the camera.
/// </summary>
public class SimpleMouseLook : MonoBehaviour
{
    public Transform playerBody;       // usually the capsule transform
    public float mouseSensitivity = 200f;
    public float smoothTime = 0.02f;
    public bool lockCursor = true;
    public float minY = -85f;
    public float maxY = 85f;

    float xRotation = 0f;
    Vector2 currentMouseDelta;
    Vector2 mouseDeltaVelocity;

    void Start()
    {
        if (playerBody == null && transform.parent != null)
            playerBody = transform.parent;
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Update()
    {
        Vector2 targetDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * mouseSensitivity * Time.deltaTime;
        currentMouseDelta = Vector2.SmoothDamp(currentMouseDelta, targetDelta, ref mouseDeltaVelocity, smoothTime);

        // rotate camera pitch
        xRotation -= currentMouseDelta.y;
        xRotation = Mathf.Clamp(xRotation, minY, maxY);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // rotate player yaw
        if (playerBody != null)
            playerBody.Rotate(Vector3.up * currentMouseDelta.x);
    }
}
