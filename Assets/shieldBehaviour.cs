using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shieldBehaviour : MonoBehaviour
{

    public bool isBlocking = false;
    [SerializeField] private Animator shieldAnimator;
    public float shieldBlock = 0;


    // Update is called once per frame
    public void Block()
    {
        isBlocking = true;
        shieldAnimator.SetBool("Block", isBlocking);
    }
    
    public void StopBlock()
    {
        isBlocking = false;
        shieldAnimator.SetBool("Block", isBlocking);
    }
}
