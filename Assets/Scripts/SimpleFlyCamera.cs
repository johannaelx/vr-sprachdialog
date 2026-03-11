using UnityEngine;
using UnityEngine.InputSystem;   // <<< neues Input System

public class SimpleFlyCamera : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float lookSensitivity = 0.1f;

    float yaw;
    float pitch;

    void Start()
    {
        var angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Wenn kein Keyboard/Mouse da ist (z.B. im Editor ohne Fokus), nichts tun
        if (Keyboard.current == null || Mouse.current == null)
            return;

        // --- Maus: Blickrichtung ---
        Vector2 mouseDelta = Mouse.current.delta.ReadValue() * lookSensitivity;
        yaw   += mouseDelta.x;
        pitch -= mouseDelta.y;
        pitch  = Mathf.Clamp(pitch, -80f, 80f);
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);

        // --- Tastatur: Bewegung ---
        Vector3 dir = Vector3.zero;

        if (Keyboard.current.wKey.isPressed) dir += Vector3.forward;
        if (Keyboard.current.sKey.isPressed) dir += Vector3.back;
        if (Keyboard.current.aKey.isPressed) dir += Vector3.left;
        if (Keyboard.current.dKey.isPressed) dir += Vector3.right;
        if (Keyboard.current.eKey.isPressed) dir += Vector3.up;
        if (Keyboard.current.qKey.isPressed) dir += Vector3.down;

        if (dir.sqrMagnitude > 0f)
        {
            dir = dir.normalized;
            transform.position += transform.TransformDirection(dir) * moveSpeed * Time.deltaTime;
        }
    }
}