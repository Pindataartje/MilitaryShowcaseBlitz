using UnityEngine;
using UnityEngine.EventSystems;

public class OrbitCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;                 // Set at runtime by the manager
    public float distance = 5f;
    public float minDistance = 1.5f;
    public float maxDistance = 20f;

    [Header("Orbit")]
    public float orbitSpeed = 180f;          // deg/sec
    public float panSpeed = 0.5f;            // (optional) middle-mouse pan
    public float zoomSpeed = 5f;             // scroll wheel
    public float minPitch = -80f;
    public float maxPitch = 80f;

    [Header("Smoothing")]
    public float followSmoothing = 0.08f;

    float yaw, pitch;
    Vector3 panOffset;

    void Start()
    {
        var eul = transform.eulerAngles;
        yaw = eul.y; pitch = eul.x;
        Cursor.lockState = CursorLockMode.None;
    }

    void LateUpdate()
    {
        if (!target) return;

        bool overUI = EventSystem.current && EventSystem.current.IsPointerOverGameObject();

        // Orbit (RMB or LMB) — pick one; using LMB here
        if (!overUI && Input.GetMouseButton(0))
        {
            yaw += Input.GetAxis("Mouse X") * orbitSpeed * Time.deltaTime;
            pitch -= Input.GetAxis("Mouse Y") * orbitSpeed * Time.deltaTime;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        // Pan (MMB)
        if (!overUI && Input.GetMouseButton(2))
        {
            Vector3 right = transform.right;
            Vector3 up = Vector3.up; // keep world up to avoid weird tilt
            Vector2 delta = new Vector2(-Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y"));
            panOffset += (right * delta.x + up * delta.y) * panSpeed * (distance * 0.1f);
        }

        // Zoom
        float scroll = Input.mouseScrollDelta.y;
        distance = Mathf.Clamp(distance - scroll * zoomSpeed, minDistance, maxDistance);

        // Compose camera transform
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desiredPos = target.position + panOffset - rot * Vector3.forward * distance;

        transform.position = Vector3.Lerp(transform.position, desiredPos, 1f - Mathf.Exp(-Time.deltaTime / followSmoothing));
        transform.rotation = rot;
    }

    public void Focus(Bounds b, float padding = 1.1f)
    {
        panOffset = Vector3.zero;

        // Compute a distance that frames the model well
        float radius = Mathf.Max(b.extents.x, Mathf.Max(b.extents.y, b.extents.z));
        float fov = GetComponent<Camera>().fieldOfView * Mathf.Deg2Rad;
        float fitDist = (radius * padding) / Mathf.Sin(fov * 0.5f);

        distance = Mathf.Clamp(fitDist, minDistance, maxDistance);
        // Point camera at model center
        transform.LookAt(b.center);
        yaw = transform.eulerAngles.y;
        pitch = Mathf.Clamp(transform.eulerAngles.x, minPitch, maxPitch);
    }
}
