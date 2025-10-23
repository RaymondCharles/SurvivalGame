using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEquipment : MonoBehaviour
{
    public GameObject playerSword = null;
    public GameObject playerShield = null;
    public GameObject playerArmor = null;
    public swordBehaviour swordScript = null;
    public shieldBehaviour shieldScript = null;



    // Update is called once per frame
    void Update()
    {
        if (playerSword != null)
        {
            if (Input.GetMouseButtonDown(0) && !shieldScript.isBlocking)
            {
                swordScript.Attack();
            }
        }
        if (playerShield != null)
        {
            if (Input.GetMouseButton(1) && swordScript.canAttack)
            {
                shieldScript.Block();
            }
            else if (Input.GetMouseButtonUp(1) && shieldScript.isBlocking)
            {
                shieldScript.StopBlock();
            }
        }
    }

    public void AddSword(GameObject Sword)
    {
        playerSword = Sword;
        swordScript = playerSword.GetComponent<swordBehaviour>();
    }

    public void AddShield(GameObject Shield)
    {
        playerShield = Shield;
        shieldScript = playerShield.GetComponent<shieldBehaviour>();
    }

    public void AddArmor(GameObject Armor)
    {
        playerArmor = Armor;
    }
}
