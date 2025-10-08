using UnityEngine;
using UnityEngine.UI;

public class SliderWiggle : MonoBehaviour
{
    Slider s;
    void Awake() { s = GetComponent<Slider>(); s.minValue = 0f; s.maxValue = 1f; }
    void Update() { if (s) s.value = 0.5f + 0.5f * Mathf.Sin(Time.time * 2f); }
}
