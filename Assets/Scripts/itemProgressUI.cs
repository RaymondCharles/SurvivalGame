using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class itemProgressUI : MonoBehaviour
{
    [System.Serializable]
    public class ItemProgress
    {
        public string itemName;
        public int currentAmt;
        public int requiredAmt;
        public TMP_Text progressText;
    }

    public List<ItemProgress> trackedItems = new List<ItemProgress>();
    public Dictionary<string, List<ItemProgress>> progressDict;

    private void Start()
    {
        progressDict = new Dictionary<string, List<ItemProgress>>();

        foreach (var item in trackedItems)
        {
            string key = item.itemName.ToLower();
            if (!progressDict.ContainsKey(key))
            {
                progressDict[key] = new List<ItemProgress>();
            }

            progressDict[key].Add(item);
            UpdateUI(key);
        }
    }

    public void AddItem(string itemName, int amt)
    {
        itemName = itemName.ToLower();
        if (progressDict.TryGetValue(itemName, out var progressList))
        {
            foreach (var progress in progressList)
            {
                progress.currentAmt = Mathf.Min(progress.currentAmt + amt, progress.requiredAmt);
            }
            UpdateUI(itemName);
        }
    }

    public void RemoveItem(string itemName, int amt)
    {
        itemName = itemName.ToLower();
        if (progressDict.TryGetValue(itemName, out var progressList))
        {
            foreach (var progress in progressList)
            {
                // Fix: use Mathf.Max to avoid going below zero
                progress.currentAmt = Mathf.Max(progress.currentAmt - amt, 0);
            }
            UpdateUI(itemName);
        }
    }

    public void UpdateUI(string itemName)
    {
        string key = itemName.ToLower();

        if (progressDict.ContainsKey(key))
        {
            var progressList = progressDict[key];

            foreach (var progress in progressList)
            {
                if (progress.progressText != null)
                {
                    progress.progressText.text = progress.currentAmt + "/" + progress.requiredAmt;
                }
            }
        }
    }

    public void SyncWInventory(Dictionary<string, int> inventoryCount)
    {
        foreach (var x in progressDict)
        {
            string itemName = x.Key;
            int countInventory = 0;

            // Fix: check if key exists before accessing
            if (inventoryCount.ContainsKey(itemName))
            {
                countInventory = inventoryCount[itemName];
            }

            foreach (var progress in x.Value)
            {
                progress.currentAmt = Mathf.Min(countInventory, progress.requiredAmt);
            }
            UpdateUI(itemName);
        }
    }
}
