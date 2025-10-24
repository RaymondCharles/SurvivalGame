using UnityEngine;
using UnityEngine.UI;

public class HpHungerUI : MonoBehaviour
{
    public VitalsBarBinder vitalsBar;
    public bool isHp = true;
    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();

        // Set slider range 0–1 since VitalsBarBinder uses floats
        slider.minValue = 0f;
        slider.maxValue = 1f;
    }

    private void OnEnable()
    {
        if (isHp)
        {
            vitalsBar.onHpChanged.AddListener(UpdateSlider);
            UpdateSlider(vitalsBar.hp);
        }
        else
        {
            vitalsBar.onHungerChanged.AddListener(UpdateSlider);
            UpdateSlider(vitalsBar.hunger);
        }
    }

    private void OnDisable()
    {
        if (isHp)
            vitalsBar.onHpChanged.RemoveListener(UpdateSlider);
        else
            vitalsBar.onHungerChanged.RemoveListener(UpdateSlider);
    }

    // Float version
    private void UpdateSlider(float value)
    {
        slider.value = Mathf.Clamp01(value);
    }
}
