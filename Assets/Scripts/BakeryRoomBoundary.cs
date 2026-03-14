using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.XR.CoreUtils;

public sealed class BakeryRoomBoundary : MonoBehaviour
{
    private const string BakerySceneName = "BakeryScene";

    // Tuned to the current bakery layout so the player stays inside the walkable room.
    private static readonly Vector3 BoundaryCenter = new(9.0f, 1.5f, -3.9f);
    private static readonly Vector3 BoundarySize = new(16.0f, 3.0f, 7.0f);

    [SerializeField] private XROrigin xrOrigin;
    [SerializeField] private Transform headTransform;
    [SerializeField] private float wallPadding = 0.35f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureBoundaryExists()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.name != BakerySceneName)
        {
            return;
        }

        if (FindFirstObjectByType<BakeryRoomBoundary>() != null)
        {
            return;
        }

        XROrigin origin = FindFirstObjectByType<XROrigin>();
        if (origin == null || origin.Camera == null)
        {
            return;
        }

        GameObject boundaryObject = new("BakeryRoomBoundary");
        BakeryRoomBoundary boundary = boundaryObject.AddComponent<BakeryRoomBoundary>();
        boundary.xrOrigin = origin;
        boundary.headTransform = origin.Camera.transform;
    }

    private void LateUpdate()
    {
        if (xrOrigin == null || headTransform == null)
        {
            return;
        }

        Bounds bounds = new(BoundaryCenter, BoundarySize);
        float minX = bounds.min.x + wallPadding;
        float maxX = bounds.max.x - wallPadding;
        float minZ = bounds.min.z + wallPadding;
        float maxZ = bounds.max.z - wallPadding;

        Vector3 headPosition = headTransform.position;
        float clampedX = Mathf.Clamp(headPosition.x, minX, maxX);
        float clampedZ = Mathf.Clamp(headPosition.z, minZ, maxZ);

        if (Mathf.Approximately(clampedX, headPosition.x) && Mathf.Approximately(clampedZ, headPosition.z))
        {
            return;
        }

        Vector3 correction = new(clampedX - headPosition.x, 0f, clampedZ - headPosition.z);
        xrOrigin.transform.position += correction;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.35f);
        Gizmos.DrawWireCube(BoundaryCenter, BoundarySize);
    }
}
