using UnityEngine;

public class MoveToPosition : MonoBehaviour
{
    public Transform targetPosition;

    public void MoveHere()
    {
        if (targetPosition != null)
            transform.position = targetPosition.position;
    }

    public void MoveToCustomPosition(Vector3 newPos)
    {
        transform.position = newPos;
    }
}
