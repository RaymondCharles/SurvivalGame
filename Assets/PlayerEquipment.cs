using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerEquipment : MonoBehaviour
{
    //GameObject references
    public GameObject playerSword = null;
    public GameObject playerShield = null;
    public GameObject playerArmor = null;
    public swordBehaviour swordScript = null;
    public shieldBehaviour shieldScript = null;

    //Input System References
    private PlayerInputActions playerInputActions;
    private InputAction attackAction;
    private InputAction blockAction;

    private void Awake()
    {
        playerInputActions = new PlayerInputActions();
        attackAction = playerInputActions.Player.Attack;
        blockAction = playerInputActions.Player.Block;
    }

    private void OnEnable()
    {
        playerInputActions?.Player.Enable();
    }

    private void OnDisable()
    {
        playerInputActions?.Player.Disable();
    }

    void Update()
    {
        // Check Sword Attack
        if (playerSword != null && swordScript != null)
        {
            if (attackAction.triggered && (playerShield == null || !shieldScript.isBlocking)) // Check if player equipped shield and isn't in the middle of blocking
            {
                swordScript.Attack();
            }
        }

        // Check Shield Block
        if (playerShield != null && shieldScript != null)
        {
            if (blockAction.IsPressed() && (playerSword == null || swordScript.canAttack)) // Check if player equipped sword and isn't mid attack
            {
                // Ensure Block() is only called once when starting
                if (!shieldScript.isBlocking)
                {
                    shieldScript.Block();
                }
            }
            else if (blockAction.WasReleasedThisFrame() && shieldScript.isBlocking)
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