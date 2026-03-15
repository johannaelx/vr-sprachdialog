using UnityEngine;

public class SetXRStartPosition : MonoBehaviour
{
    public Transform startPoint;

    void Start()
    {
        if (startPoint != null)
        {
            transform.position = startPoint.position;
            transform.rotation = startPoint.rotation;
        }
    }
}