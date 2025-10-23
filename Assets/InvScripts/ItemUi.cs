using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class ItemUi : MonoBehaviour
{
    public Image image;
    public Button button;
    public GameObject selectionBorder;
    public TextMeshProUGUI countText;

    public string itemId;
    public Action<string> onClickAction;
    public int count = 1;

    // Initialize assigns sprite and onClick callback
    public void Initialize(string inventoryId, Item item, Action<string> onClickAction)
    {
        itemId = inventoryId;
        this.onClickAction = onClickAction;

        image.sprite = item.icon;
        transform.localScale = Vector3.one;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => {
        if (onClickAction != null)
       
            {
                onClickAction(inventoryId);
            }
        });

        if (selectionBorder != null) {
            selectionBorder.gameObject.SetActive(false); 
        }

        updateCountUI();
                   
    }
    public void SetSelected(bool isSelected)
    {
        if (selectionBorder != null)
        {
            selectionBorder.SetActive(isSelected);
        }
    }
    public void SetCount(int newCount)
    {
        count = newCount;
        updateCountUI();
    }
    public void IncreaseCount(int amt)
    {
        amt = 1;
        count += amt;
        updateCountUI();
    }
    public void DecreaseCount(int amt)
    {
        amt = 1;
        count = Mathf.Max(0, count - amt);
        updateCountUI();
    }
    public void updateCountUI()
    {
        if (countText == null)
        {
            return;
        }
        if (count > 1)
        {
            countText.text = count.ToString();
        }
        else
        {
            countText.text = string.Empty;
        }
    }
    private void OnDestroy()
    {
        button.onClick.RemoveAllListeners();
    }
}


