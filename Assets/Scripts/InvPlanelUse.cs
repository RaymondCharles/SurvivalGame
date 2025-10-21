using UnityEngine;
using UnityEngine.UI;
using System;

public class InvPlanelUse : MonoBehaviour
{
    public Button yesButton;
    public Button noButton;

    private Action onConfirm;

    // Reference to the Drop Panel GameObject to hide it
    public GameObject invDropPanel;

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
        if (onConfirm != null)
        {
            onConfirm();
        }

        gameObject.SetActive(false);

        if (invDropPanel != null)
        {
            invDropPanel.SetActive(false);
        }
    }

    private void OnNoClicked()
    {
        gameObject.SetActive(false);
    }
}
