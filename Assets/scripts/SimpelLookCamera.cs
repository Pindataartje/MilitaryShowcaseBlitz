using UnityEngine;

public class SimpleMouseLook : MonoBehaviour
{
    [SerializeField] Transform playerBody;
    [SerializeField] float sensitivity = 1.5f;
    [SerializeField] float smoothingTime = 0.05f;
    [SerializeField] bool lockCursor = true;
    [SerializeField] float minY = -85f;
    [SerializeField] float maxY = 85f;

    float yaw;
    float pitch;
    Vector2 smoothedDelta;
    Transform camTr;
    Transform bodyTr;

    void Awake()
    {
        camTr = transform;
        bodyTr = playerBody != null ? playerBody : (transform.parent != null ? transform.parent : null);
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        if (bodyTr) yaw = bodyTr.eulerAngles.y;
        pitch = camTr.localEulerAngles.x;
        if (pitch > 180f) pitch -= 360f;
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus) smoothedDelta = Vector2.zero;
    }

    void Update()
    {
        float dx = Input.GetAxisRaw("Mouse X");
        float dy = Input.GetAxisRaw("Mouse Y");
        if (dx == 0f && dy == 0f) return;

        Vector2 raw = new Vector2(dx, dy) * sensitivity;

        if (smoothingTime > 0f)
        {
            float t = 1f - Mathf.Exp(-Time.unscaledDeltaTime / smoothingTime);
            smoothedDelta += (raw - smoothedDelta) * t;
        }
        else
        {
            smoothedDelta = raw;
        }

        pitch = Mathf.Clamp(pitch - smoothedDelta.y, minY, maxY);
        yaw += smoothedDelta.x;

        camTr.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        if (bodyTr) bodyTr.rotation = Quaternion.Euler(0f, yaw, 0f);
    }
}
