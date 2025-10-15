using UnityEngine;
using TMPro;

public class RaycastTagDisplay : MonoBehaviour
{
    [SerializeField] TMP_Text infoText;
    [SerializeField] float rayDistance = 5f;

    void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance))
        {
            string tagName = hit.collider.tag;
            infoText.text = "Dit object is gemaakt door: " + tagName;
        }
        else
        {
            infoText.text = "";
        }
    }
}
