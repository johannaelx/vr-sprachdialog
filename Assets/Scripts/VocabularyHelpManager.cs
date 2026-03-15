using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class VocabularyHelpManager : MonoBehaviour
{
    public InputActionReference helpToggleAction;
    public XRRayInteractor rayInteractor;

    public GameObject helpPanel;
    public TextMeshProUGUI vocabularyText;

    public GameObject questMenu;

    private bool helpModeActive = false;

    private void OnEnable()
    {
        if (helpToggleAction != null && helpToggleAction.action != null)
        {
            helpToggleAction.action.Enable();
            helpToggleAction.action.performed += ToggleHelpMode;
        }
    }

    private void OnDisable()
    {
        if (helpToggleAction != null && helpToggleAction.action != null)
        {
            helpToggleAction.action.performed -= ToggleHelpMode;
            helpToggleAction.action.Disable();
        }
    }

    private void Start()
    {
        if (helpPanel != null)
            helpPanel.SetActive(false);
    }

    private void Update()
    {
        if (questMenu != null && questMenu.activeSelf)
        {
            helpModeActive = false;

            if (helpPanel != null)
                helpPanel.SetActive(false);

            return;
        }

        if (!helpModeActive)
            return;

        if (rayInteractor != null && rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            VocabularyObject vocabObject = hit.collider.GetComponentInParent<VocabularyObject>();

            if (vocabObject != null)
            {
                if (helpPanel != null && !helpPanel.activeSelf)
                    helpPanel.SetActive(true);

                if (vocabularyText != null)
                    vocabularyText.text = vocabObject.GetDisplayText();
            }
            else
            {
                if (helpPanel != null)
                    helpPanel.SetActive(false);
            }
        }
        else
        {
            if (helpPanel != null)
                helpPanel.SetActive(false);
        }
    }

    private void ToggleHelpMode(InputAction.CallbackContext context)
    {
        if (questMenu != null && questMenu.activeSelf)
            return;

        helpModeActive = !helpModeActive;

        Debug.Log("Help mode active: " + helpModeActive);

        if (!helpModeActive && helpPanel != null)
        {
            helpPanel.SetActive(false);
        }
    }
}