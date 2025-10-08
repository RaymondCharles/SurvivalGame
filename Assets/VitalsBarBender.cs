using UnityEngine;
using UnityEngine.UI;

public class VitalsBarBinder : MonoBehaviour
{
    public PlayerVitals vitals;
    public Slider hpBar, hungerBar;

    void Awake()
    {
        if (!vitals) vitals = FindObjectOfType<PlayerVitals>();
        hpBar.maxValue = 1f; hungerBar.maxValue = 1f;
        vitals.onHPChanged.AddListener(v => hpBar.SetValueWithoutNotify(v));
        vitals.onHungerChanged.AddListener(v => hungerBar.SetValueWithoutNotify(v));
    }
}
