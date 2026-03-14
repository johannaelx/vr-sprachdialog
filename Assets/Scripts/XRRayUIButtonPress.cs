using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRRayInteractor))]
public class XRRayUIButtonPress : MonoBehaviour
{
    private XRRayInteractor rayInteractor;
    private InputAction pressAction;

    void Awake()
    {
        rayInteractor = GetComponent<XRRayInteractor>();
    }

    void OnEnable()
    {
        pressAction ??= new InputAction(
            name: "QuestAButtonPress",
            type: InputActionType.Button,
            binding: "<XRController>{RightHand}/primaryButton");

        pressAction.Enable();
    }

    void OnDisable()
    {
        pressAction?.Disable();
    }

    void Update()
    {
        if (rayInteractor == null || pressAction == null || !pressAction.WasPressedThisFrame())
        {
            return;
        }

        if (!rayInteractor.TryGetCurrentUIRaycastResult(out RaycastResult raycastResult))
        {
            return;
        }

        Button button = raycastResult.gameObject != null
            ? raycastResult.gameObject.GetComponentInParent<Button>()
            : null;

        if (button == null || !button.IsInteractable())
        {
            return;
        }

        button.onClick.Invoke();
    }
}
