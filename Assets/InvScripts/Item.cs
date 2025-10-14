using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName ="Item", menuName ="RumbledCode/Item", order =1)]
public class Item : ScriptableObject
{

    public string id;
    public string desc;
    public Sprite icon;
    public GameObject prefab;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
