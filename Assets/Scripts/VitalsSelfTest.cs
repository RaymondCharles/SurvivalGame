using System.Collections;
using UnityEngine;

public class VitalsSelfTest : MonoBehaviour
{
    public PlayerVitals vitals;

    //IEnumerator Start()
    //{
    //    if (!vitals) vitals = FindObjectOfType<PlayerVitals>();
    //    yield return new WaitForSeconds(0.5f);

    //    Debug.Log("[VitalsSelfTest] Damage 25");
    //    vitals.Damage(25f);   // HP should drop

    //    yield return new WaitForSeconds(0.8f);
    //    Debug.Log("[VitalsSelfTest] Heal 10 via food");
    //    vitals.Eat(0f, 10f);  // HP should rise a bit

    //    yield return new WaitForSeconds(0.8f);
    //    Debug.Log("[VitalsSelfTest] Eat 30 hunger");
    //    vitals.Eat(30f);      // Hunger should go up
    //}
}
