using System;
using UnityEngine;
using UnityEngine.UI;

public class ItemUi : MonoBehaviour
{
    public Image image;
    public Button button;

    // Initialize assigns sprite and onClick callback
    public void Initialize(string inventoryId, Item item, Action<string> onClickAction)
    {
        image.sprite = item.icon;
        transform.localScale = Vector3.one;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            onClickAction?.Invoke(inventoryId);
        });
    }

    private void OnDestroy()
    {
        button.onClick.RemoveAllListeners();
    }
}


//using System;
//using UnityEngine;
//using UnityEngine.UI;

//public class ItemUi : MonoBehaviour
//{

//    public Image image;
//    public Button button;

//    public void Initialize(string inventoryId, Item item, Action<string> removeItemAction)
//    {
//        image.sprite = item.icon;
//        transform.localScale = Vector3.one;
//        button.onClick.AddListener(() => removeItemAction.Invoke(inventoryId));
//    }
//    void OnDestroy()
//    {
//        button.onClick.RemoveAllListeners();
//        Debug.Log("Item dropped");

//    }



//    // Start is called before the first frame update
//    void Start()
//    {

//    }

//    // Update is called once per frame
//    void Update()
//    {

//    }
//}
