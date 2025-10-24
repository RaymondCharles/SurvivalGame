using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class swordBehaviour : MonoBehaviour
{
    private int attackCombo= 0;
    private float prevAttack = 0;
    public bool canAttack = true;
    private float attackCooldown = 1.0f;
    private float attackComboTimer = 3.0f;
    public float swordDamage = 20.0f;
    [SerializeField] private Animator swordAnimator;
    private List<GameObject> enemiesHit = new List<GameObject>();
    private List<GameObject> objectsHit = new List<GameObject>();



    public void Attack()
    {
        if (canAttack)
        {
            prevAttack = Time.time;
            attackCombo +=1;
            if (attackCombo > 3)
            {
                attackCombo = 1;
            }
            canAttack = false;
            Invoke(nameof(ResetAttack), attackCooldown);
            Invoke(nameof(CheckAttackCombo), attackComboTimer);
        }
        swordAnimator.SetInteger("attackCombo", attackCombo);
    }

    void ResetAttack()
    {
        canAttack = true;
        enemiesHit.Clear();
        objectsHit.Clear();
    }
    
    void CheckAttackCombo()
    {
        if ((Time.time - prevAttack >= 2.9))
        {
            attackCombo = 0;
            swordAnimator.SetInteger("attackCombo", attackCombo);
        }
    }

/*
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            GameObject enemy = collision.gameObject;
            if (!canAttack && !enemiesHit.Contains(enemy))
            {
                enemiesHit.Add(enemy);
                collision.gameObject.GetComponent<EnemyHealth>().currentHealth -= 20;
            }
        }
    }*/


    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            GameObject enemy = other.gameObject;
            if (!canAttack && !enemiesHit.Contains(enemy))
            {
                enemiesHit.Add(enemy);
                enemy.GetComponent<EnemyHealth>().TakeDamage(swordDamage);
            }
        }
        else if (other.CompareTag("Mineable"))
        {
            GameObject envObject = other.gameObject;
            if (!canAttack && !objectsHit.Contains(envObject))
            {
                objectsHit.Add(envObject);
                envObject.GetComponent<mineableBehaviour>().SpawnMaterials((int)swordDamage);
            }
        }
    }
/*
    void OnCollisionStay(Collision collision)
    {
        Debug.Log(collision.gameObject);
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Colliding with enemy");
            GameObject enemy = collision.gameObject;
            if (!canAttack && !enemiesHit.Contains(enemy))
            {
                Debug.Log("Enemy Hit");
                enemiesHit.Add(enemy);
                collision.gameObject.GetComponent<EnemyHealth>().currentHealth -= 20;
            }
        }
    }*/
}
