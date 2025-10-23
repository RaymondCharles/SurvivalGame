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



    public void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            if  (isBlocking)
            {
                Debug.Log("Collided with enemy, BLOCKING");
                if (other.gameObject.GetComponent<EnemyAI>() != null)
                {
                    other.gameObject.GetComponent<EnemyAI>().meleeBlocked = true;
                }
            }
            else
            {
                Debug.Log("Collided with enemy, BLOCKING");
                if (other.gameObject.GetComponent<EnemyAI>() != null)
                {
                    other.gameObject.GetComponent<EnemyAI>().meleeBlocked = false;
                }
            }
        }
    }
    
    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("Collided with enemy, UNBLOCKING");
            if (other.gameObject.GetComponent<EnemyAI>() != null)
            {
                other.gameObject.GetComponent<EnemyAI>().meleeBlocked = false;
            }
        }
    }
}
