using UnityEngine;

public class ConstrainCameraRotation : MonoBehaviour
{
    [Header("References")]
    public Transform boat;

    [Header("Orbit Settings")]
    public float yaw = 0f;
    public float pitch = 20f;
    public float minPitch = 5f;
    public float maxPitch = 80f;

    [Header("Sensitivity")]
    public float lookSensitivity = 0.2f;     // mouse delta multiplier
    public float zoomSensitivity = 0.02f;    // % of radius per “scroll step”

    [Header("Zoom")]
    public float minRadius = 5f;
    public float maxRadius = 20f;
    public float zoomSmoothTime = 0.12f;     // SmoothDamp time

    // internal
    float currentRadius;
    float targetRadius;
    float zoomVel; // SmoothDamp velocity
    Vector3 lastMousePos;
    bool dragging;

    void Start()
    {
        if (boat == null)
        {
            Debug.LogWarning("[ConstrainCameraRotation] Boat reference not assigned!");
            enabled = false;
            return;
        }

        currentRadius = targetRadius = Mathf.Clamp(12f, minRadius, maxRadius);
        Cursor.lockState = CursorLockMode.None; // always free
        Cursor.visible = true;
    }

    void LateUpdate()
    {
        if (boat == null) return;

        // ----- Orbit (click-drag to rotate) -----
        if (Input.GetMouseButtonDown(0))
        {
            dragging = true;
            lastMousePos = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            dragging = false;
        }

        if (dragging)
        {
            Vector3 mouseDelta = Input.mousePosition - lastMousePos;
            lastMousePos = Input.mousePosition;

            yaw += mouseDelta.x * lookSensitivity * 0.2f;
            pitch -= mouseDelta.y * lookSensitivity * 0.2f;
        }

        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // ----- Zoom (scroll wheel) -----
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.0001f)
        {
            float steps = scroll * 10f;
            float radiusDelta = -steps * zoomSensitivity * Mathf.Max(1f, targetRadius);
            targetRadius = Mathf.Clamp(targetRadius + radiusDelta, minRadius, maxRadius);
        }

        // Smooth zoom
        currentRadius = Mathf.SmoothDamp(currentRadius, targetRadius, ref zoomVel, zoomSmoothTime);

        // ----- Place camera -----
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 offset = rot * (Vector3.back * currentRadius);

        transform.position = boat.position + offset;
        transform.LookAt(boat.position);
    }
}
