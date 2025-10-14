using System;
using UnityEngine;
using UnityEngine.UI;

public class ItemUi : MonoBehaviour
{

    public Image image;
    public Button button;

    public void Initialize(string inventoryId, Item item, Action<string> removeItemAction)
    {
        image.sprite = item.icon;
        transform.localScale = Vector3.one;
        button.onClick.AddListener(() => removeItemAction.Invoke(inventoryId));
    }
    void OnDestroy()
    {
        button.onClick.RemoveAllListeners();

    }



    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
