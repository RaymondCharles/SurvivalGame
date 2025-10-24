using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

public class VitalsBarBinder : MonoBehaviour
{
    public float hp = 1.0f;
    public float hunger = 1.0f;

    public float hungerDec = 0.01f;
    public float healthDec = 0.1f;

    public UnityEvent<float> onHpChanged;
    public UnityEvent<float> onHungerChanged;

    private void Start()
    {
        // Set initial values first
        hp = 1.0f;
        hunger = 1.0f;

        // Then invoke events so UI updates immediately
        if (onHpChanged != null) onHpChanged.Invoke(hp);
        if (onHungerChanged != null) onHungerChanged.Invoke(hunger);
    }

    void Update()
    {
        hunger -= hungerDec * Time.deltaTime;
        hunger = Mathf.Clamp01(hunger);
        onHungerChanged.Invoke(hunger);

        if (hunger <= 0f)
        {
            hp -= 0.05f * Time.deltaTime;
            hp = Mathf.Clamp01(hp);
            onHpChanged.Invoke(hp);
        }

    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            hp -= healthDec;
            hp = Mathf.Clamp01(hp);
            onHpChanged.Invoke(hp);
        }


    }
    public void AddHunger(float amount)
    {
        hunger += amount;
        hunger = Mathf.Clamp01(hunger);
        onHungerChanged.Invoke(hunger);
    }
    public void AddHealth(float amount)
    {
        hp += amount;
        hp = Mathf.Clamp01(hp);
        onHpChanged.Invoke(hp);
    }
}
