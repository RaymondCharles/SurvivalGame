using UnityEngine;
using UnityEngine.Events;

[System.Serializable] public class FloatEvent : UnityEvent<float> { }
[System.Serializable] public class BoolEvent : UnityEvent<bool> { }

public class PlayerVitals : MonoBehaviour
{
    [Header("HP")]
    public float maxHP = 100f;
    [SerializeField] float hp = 100f;
    public float passiveRegenPerSecond = 0f;   // set >0 if you want auto-heal
    public float regenMinHunger = 60f;
    public float regenDelaySeconds = 5f;

    [Header("Hunger")]
    public float maxHunger = 100f;
    [SerializeField] float hunger = 100f;
    public float hungerDrainPerSecond = 1f;
    public float starvationDps = 5f;

    [Header("Events")]
    public FloatEvent onHPChanged;
    public FloatEvent onHungerChanged;
    public UnityEvent onDeath;
    public BoolEvent onStarvingChanged;

    float lastDamageTime;
    bool isDead;
    bool starving;

    public float HP => hp;
    public float Hunger => hunger;
    public bool IsDead => isDead;

    void Start()
    {
        hp = Mathf.Clamp(hp, 0, maxHP);
        hunger = Mathf.Clamp(hunger, 0, maxHunger);
        onHPChanged?.Invoke(hp / maxHP);
        onHungerChanged?.Invoke(hunger / maxHunger);
        onStarvingChanged?.Invoke(false);
    }

    void Update()
    {
        if (isDead) return;
        float dt = Time.deltaTime;

        // Hunger drain
        float prevH = hunger;
        hunger = Mathf.Clamp(hunger - hungerDrainPerSecond * dt, 0f, maxHunger);
        if (!Mathf.Approximately(prevH, hunger)) onHungerChanged?.Invoke(hunger / maxHunger);

        // Starvation vs regen
        bool nowStarving = hunger <= 0.0001f;
        if (nowStarving != starving) { starving = nowStarving; onStarvingChanged?.Invoke(starving); }

        if (starving)
        {
            Damage(starvationDps * dt);
        }
        else if (passiveRegenPerSecond > 0f && hunger >= regenMinHunger && (Time.time - lastDamageTime) >= regenDelaySeconds)
        {
            Heal(passiveRegenPerSecond * dt);
        }
    }

    public void Damage(float amount)
    {
        if (isDead || amount <= 0f) return;
        lastDamageTime = Time.time;
        float prev = hp;
        hp = Mathf.Max(0f, hp - amount);
        if (!Mathf.Approximately(prev, hp)) onHPChanged?.Invoke(hp / maxHP);
        if (hp <= 0f && !isDead) { isDead = true; onDeath?.Invoke(); }
    }

    public void Heal(float amount)
    {
        if (isDead || amount <= 0f) return;
        float prev = hp;
        hp = Mathf.Min(maxHP, hp + amount);
        if (!Mathf.Approximately(prev, hp)) onHPChanged?.Invoke(hp / maxHP);
    }

    public void Eat(float calories, float hpRestore = 0f)
    {
        if (isDead) return;
        float prev = hunger;
        hunger = Mathf.Clamp(hunger + calories, 0f, maxHunger);
        if (!Mathf.Approximately(prev, hunger)) onHungerChanged?.Invoke(hunger / maxHunger);
        if (hpRestore > 0f) Heal(hpRestore);
    }
}
