using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class XRHoverPressButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Button button;
    private InputAction pressAction;
    private bool isHovered;

    void Awake()
    {
        button = GetComponent<Button>();
    }

    void OnEnable()
    {
        pressAction ??= new InputAction(
            name: "QuestPrimaryButtonPress",
            type: InputActionType.Button,
            binding: "<XRController>{RightHand}/primaryButton");

        pressAction.Enable();
    }

    void OnDisable()
    {
        pressAction?.Disable();
        isHovered = false;
    }

    void Update()
    {
        if (!isHovered || button == null || !button.IsInteractable() || pressAction == null)
        {
            return;
        }

        if (pressAction.WasPressedThisFrame())
        {
            button.onClick.Invoke();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
    }
}
