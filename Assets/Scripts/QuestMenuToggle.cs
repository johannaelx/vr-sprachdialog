using UnityEngine;
using UnityEngine.InputSystem;

public class QuestMenuToggle : MonoBehaviour
{
    public GameObject menuPanel;
    public InputActionReference toggleMenuAction;
    public Transform head;
    public float distanceInFront = 1.5f;

    private void OnEnable()
    {
        toggleMenuAction.action.Enable();
        toggleMenuAction.action.performed += OnTogglePressed;
    }

    private void OnDisable()
    {
        toggleMenuAction.action.performed -= OnTogglePressed;
        toggleMenuAction.action.Disable();
    }

    private void OnTogglePressed(InputAction.CallbackContext context)
    {
        bool newState = !menuPanel.activeSelf;
        menuPanel.SetActive(newState);

        if (newState && head != null)
        {
            menuPanel.transform.position = head.position + head.forward * distanceInFront;
            menuPanel.transform.LookAt(head);
            menuPanel.transform.Rotate(0, 180f, 0);
        }
    }
}