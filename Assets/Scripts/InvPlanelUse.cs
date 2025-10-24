using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class InvPlanelUse : MonoBehaviour
{
    public Button yesButton;
    public Button noButton;

    private Action onConfirm;
    public GameObject invDropPanel;

    public VitalsBarBinder playerVitals;
    private Item currentItem;

    //public float hungerRestoreAmount = 0.25f;
    //public float hpRestoreAmount = 0.25f;


    private void Start()
    {
        yesButton.onClick.AddListener(OnYesClicked);
        noButton.onClick.AddListener(OnNoClicked);

        if (!playerVitals)
        {
            playerVitals = FindFirstObjectByType<VitalsBarBinder>();
        }

    }

    public void Show(Item item, Action onConfirmAction)
    {
        currentItem = item;
        onConfirm = onConfirmAction;
        gameObject.SetActive(true);
    }

    private void OnYesClicked()
    {
        if (onConfirm != null)
        {
            onConfirm();
        }

        if (playerVitals != null && currentItem != null && currentItem.isConsumable)
        {
            if (currentItem.hpRestoreAmount > 0)
            {
                playerVitals.AddHealth(currentItem.hpRestoreAmount / 100f);
            }
        }

        if (currentItem.hungerRestoreAmount > 0)
        {
            playerVitals.AddHunger(currentItem.hungerRestoreAmount / 100f);
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