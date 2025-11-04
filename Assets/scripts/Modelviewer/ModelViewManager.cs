using System.Linq;
using UnityEngine;
using UnityEngine.UI;   // <-- slider
using TMPro;

public class ModelViewerManager : MonoBehaviour
{
    [Header("Scene Refs")]
    public Transform stage;                 // parent where models are placed
    public OrbitCamera orbitCam;
    public TMP_Text authorText;             // “Dit model is gemaakt door: {Tag}”

    [Header("Models")]
    public GameObject[] models;             // assign prefabs OR scene objects
    public int startIndex = 0;

    [Header("Rotation UI")]
    public Slider rotationSlider;           // assign your UI Slider here
    public Axis rotationAxis = Axis.Y;      // which axis to rotate around
    public float minAngle = -180f;
    public float maxAngle = 180f;
    public bool wholeDegrees = false;       // tick if you want whole numbers

    int current = -1;
    GameObject[] instances;

    public enum Axis { X, Y, Z }

    void Awake()
    {
        // Prepare instances (scene objects or prefabs)
        instances = new GameObject[models.Length];
        for (int i = 0; i < models.Length; i++)
        {
            if (!models[i]) continue;

            if (models[i].scene.IsValid())  // already in scene
            {
                instances[i] = models[i];
                if (stage) instances[i].transform.SetParent(stage, true);
            }
            else
            {
                instances[i] = Instantiate(models[i], stage ? stage : null);
                instances[i].name = models[i].name;
            }
            instances[i].SetActive(false);
            ZeroOutTransform(instances[i].transform);
        }

        // Setup slider (range + callback)
        if (rotationSlider)
        {
            rotationSlider.minValue = minAngle;
            rotationSlider.maxValue = maxAngle;
            rotationSlider.wholeNumbers = wholeDegrees;
            rotationSlider.onValueChanged.AddListener(OnRotationSliderChanged);
        }

        Show(startIndex);

        // After first model is visible, sync slider value to its current rotation
        SyncSliderToCurrentModel();
        // Or, if you always want slider to drive rotation from its current value,
        // call ApplyRotationFromSlider() instead of SyncSliderToCurrentModel().
    }

    void ZeroOutTransform(Transform t)
    {
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;
        t.localScale = Vector3.one;
    }

    public void Next() => Show((current + 1) % instances.Length);
    public void Prev() => Show((current - 1 + instances.Length) % instances.Length);

    void Show(int index)
    {
        if (instances.Length == 0) return;
        if (index < 0 || index >= instances.Length) index = 0;

        // deactivate previous
        if (current >= 0 && current < instances.Length && instances[current])
            instances[current].SetActive(false);

        current = index;
        var go = instances[current];
        if (!go) return;

        go.SetActive(true);

        // Focus camera to model bounds
        var b = CalculateBounds(go);
        if (orbitCam)
        {
            if (!orbitCam.target)
            {
                var t = new GameObject("CameraTarget").transform;
                t.position = b.center;
                orbitCam.target = t;
            }
            else orbitCam.target.position = b.center;

            orbitCam.Focus(b, 1.25f);

            float radius = Mathf.Max(b.extents.x, Mathf.Max(b.extents.y, b.extents.z));
            orbitCam.minDistance = Mathf.Max(0.5f, radius * 0.4f);
            orbitCam.maxDistance = Mathf.Max(5f, radius * 3.0f);
        }

        // Update author tag text
        if (authorText)
        {
            string tagName = go.tag;
            if (string.IsNullOrEmpty(tagName) || tagName == "Untagged")
                tagName = "Onbekend";
            authorText.text = $"Dit model is gemaakt door: {tagName}";
        }

        // When switching models, keep the same slider angle but apply it to the new model
        ApplyRotationFromSlider();
    }

    Bounds CalculateBounds(GameObject root)
    {
        var renderers = root.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0) return new Bounds(root.transform.position, Vector3.one);

        var b = new Bounds(renderers[0].bounds.center, Vector3.zero);
        foreach (var r in renderers) b.Encapsulate(r.bounds);
        return b;
    }

    // ---------- Rotation logic ----------

    void OnRotationSliderChanged(float angle)
    {
        ApplyRotation(angle);
    }

    void ApplyRotationFromSlider()
    {
        if (!rotationSlider) return;
        ApplyRotation(rotationSlider.value);
    }

    void ApplyRotation(float angle)
    {
        if (current < 0 || current >= instances.Length) return;
        var go = instances[current];
        if (!go) return;

        // Build new euler with the selected axis set to 'angle'
        Vector3 e = go.transform.localEulerAngles;

        // Convert current 0..360 to -180..180 when syncing, but here we set directly.
        switch (rotationAxis)
        {
            case Axis.X: e = new Vector3(angle, e.y, e.z); break;
            case Axis.Y: e = new Vector3(e.x, angle, e.z); break;
            case Axis.Z: e = new Vector3(e.x, e.y, angle); break;
        }

        // Unity stores euler angles as 0..360; feeding negative is fine—Unity normalizes it.
        go.transform.localEulerAngles = e;
    }

    void SyncSliderToCurrentModel()
    {
        if (!rotationSlider) return;
        if (current < 0 || current >= instances.Length) return;
        var go = instances[current];
        if (!go) return;

        Vector3 e = go.transform.localEulerAngles;

        // Convert 0..360 to a signed -180..180 angle for nicer slider mapping
        float Normalize360toSigned(float a) => (a > 180f) ? a - 360f : a;

        float currentAngle = 0f;
        switch (rotationAxis)
        {
            case Axis.X: currentAngle = Normalize360toSigned(e.x); break;
            case Axis.Y: currentAngle = Normalize360toSigned(e.y); break;
            case Axis.Z: currentAngle = Normalize360toSigned(e.z); break;
        }

        // Clamp to slider range and assign without triggering a loop
        currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);
        rotationSlider.SetValueWithoutNotify(currentAngle);
    }
}
