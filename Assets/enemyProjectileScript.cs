using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyProjectileScript : MonoBehaviour
{

    public int Damage;
    public string targetLayer = "whatIsPlayer";

    //Script variables for future potential implementation of projectiles to be done.

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.gameObject.name);
        if (collision.gameObject.layer == LayerMask.NameToLayer(targetLayer))
        {
            //Here is where armor is checked and damage is reduced.
            //TO DO.


            //Script variable would be used here instead of only playerStats so that this script isn't just for enemies but can be used universally.
            collision.gameObject.GetComponentInParent<PlayerStats>().TakeDamage(Damage);
            Debug.Log("Player Hit! New HP is " + collision.gameObject.GetComponentInParent<PlayerStats>().hp);
        }
        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Shield"))
        {
            if (other.gameObject.GetComponent<shieldBehaviour>().isBlocking)
            {
                Destroy(gameObject);
            }
        }
    }
}
