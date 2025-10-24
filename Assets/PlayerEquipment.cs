using UnityEngine;
using UnityEngine.InputSystem; // <<< Added

public class PlayerEquipment : MonoBehaviour
{
    public GameObject playerSword = null;
    public GameObject playerShield = null;
    public GameObject playerArmor = null;
    public swordBehaviour swordScript = null;
    public shieldBehaviour shieldScript = null;

    // --- New Input System References ---
    private PlayerInputActions playerInputActions;
    private InputAction attackAction;
    private InputAction blockAction;
    // ---

    private void Awake() // Changed from Update to Awake for setup
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
            // Use attackAction.triggered for button down equivalent
            if (attackAction.triggered && (shieldScript == null || !shieldScript.isBlocking)) // Check if shieldScript exists
            {
                swordScript.Attack();
            }
        }

        // Check Shield Block
        if (playerShield != null && shieldScript != null)
        {
            // Use blockAction.IsPressed() for button held equivalent
            if (blockAction.IsPressed() && (swordScript == null || swordScript.canAttack)) // Check if swordScript exists
            {
                // Ensure Block() is only called once when starting
                if (!shieldScript.isBlocking)
                {
                    shieldScript.Block();
                }
            }
            // Use blockAction.WasReleasedThisFrame() for button up equivalent
            else if (blockAction.WasReleasedThisFrame() && shieldScript.isBlocking)
            {
                shieldScript.StopBlock();
            }
            // Failsafe: If button is not held but script thinks it's blocking
            else if (!blockAction.IsPressed() && shieldScript.isBlocking)
            {
                shieldScript.StopBlock();
            }
        }
    }

    // (AddSword, AddShield, AddArmor methods remain unchanged)
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