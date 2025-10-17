using UnityEngine;
using UnityEngine.UI;
using System;

public class InvDropPanel : MonoBehaviour
{
    public Button yesButton;
    public Button noButton;

    // Reference to the Use panel GameObject
    public GameObject invUsePanel;

    private Action onConfirm;

    private void Start()
    {
        yesButton.onClick.AddListener(OnYesClicked);
        noButton.onClick.AddListener(OnNoClicked);
    }

    public void Show(Action onConfirmAction)
    {
        onConfirm = onConfirmAction;
        gameObject.SetActive(true);
    }

    private void OnYesClicked()
    {
        Debug.Log("DropPanel Yes clicked.");

        if (onConfirm != null)
        {
            onConfirm();
        }

        gameObject.SetActive(false);
        Debug.Log("Drop panel set inactive.");

        if (invUsePanel != null)
        {
            if (invUsePanel.activeSelf)
            {
                Debug.Log("Use panel is active, hiding it now.");
                invUsePanel.SetActive(false);
            }
            else
            {
                Debug.Log("Use panel was already inactive.");
            }
        }
        else
        {
            Debug.LogWarning("invUsePanel reference is not assigned!");
        }
    }

    private void OnNoClicked()
    {
        gameObject.SetActive(false);
    }
}
