using UnityEngine;
using Unity.XR.CoreUtils;

public class SetXRStartPosition : MonoBehaviour
{
    public Transform startPoint;
    public XROrigin xrOrigin;
    public Camera xrCamera;

    void Start()
    {
        if (startPoint == null || xrOrigin == null || xrCamera == null)
            return;

        xrOrigin.transform.position = startPoint.position;

        Vector3 headForward = xrCamera.transform.forward;
        headForward.y = 0f;
        headForward.Normalize();

        Vector3 targetForward = startPoint.forward;
        targetForward.y = 0f;
        targetForward.Normalize();

        if (headForward.sqrMagnitude > 0.001f && targetForward.sqrMagnitude > 0.001f)
        {
            float angle = Vector3.SignedAngle(headForward, targetForward, Vector3.up);
            xrOrigin.transform.Rotate(0f, angle, 0f);
        }
    }
}