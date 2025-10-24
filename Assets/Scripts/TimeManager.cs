using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    // Import Skybox Textures
    [SerializeField] private Material daySkybox;
    [SerializeField] private Material nightSkybox;

    // Time variables
    private int minutes;
    public int Minutes { get { return minutes; } set { minutes = value; OnMinutesChanged(value); } }
    private int hours;
    public int Hours { get { return hours; } set { hours = value; OnHoursChanged(value); } }
    private int days;
    public int Days { get { return days; } set { days = value; OnDaysChanged(value); } }

    private float tempSecond;
    public string timeOfDay => $"{Hours:D2}:{Minutes:D2}";
    public string currDay => $"Day {Days}";

    public GameManager GM;

    public float dayDurationInSeconds = 600f; // 2 minutes for full 24h in game cycle
    private float minutesPerSecond;

    void Start()
    {
        minutesPerSecond = 1440f / dayDurationInSeconds;
        Hours = 6;
        Days = 1;
        GM.SwitchTime();
    }

    public void Update()
    {
        // Increments tempSecond by the time passed since the last frame, and if it reaches 1 second, increments minutes - set to 0.005f for faster testing
        tempSecond += Time.deltaTime * minutesPerSecond;
        while (tempSecond >= 1f)
        {
            Minutes += 1;
            tempSecond -= 1f;
        }
    }

    // Handles minute and hour overflows if necessary
    private void OnMinutesChanged(int min)
    {
        if (min >= 60)
        {
            Minutes = 0;
            Hours += 1;
        }
    }

    private void OnHoursChanged(int hr)
    {
        if (hr >= 24)
        {
            Hours = 0;
            Days += 1;
            GM.day+=1;
        }
        // Handle hour changes (just changing Skybox for now)
        if (hr >= 6 && hr < 18)
        {
            if (RenderSettings.skybox != daySkybox)
            {
                GM.SwitchTime();
            }
            RenderSettings.skybox = daySkybox;
        }
        else
        {
            if (RenderSettings.skybox != nightSkybox)
            {
                GM.SwitchTime();
            }
            RenderSettings.skybox = nightSkybox;
        }
    }
    private void OnDaysChanged(int days)
    {
        // Handle day changes (e.g., update UI, trigger events)
    }
    private void OnGUI()
    {
        // Simple on-screen display for time and player status - update once own UI system is in place
        GUI.skin.label.fontSize = 60;

        GUI.Label(new Rect(30, 10, 300, 70), timeOfDay);
        GUI.Label(new Rect(30, 80, 300, 70), currDay);
        // Show TempSecs label for the first few seconds after the scene loads
        float introLabelDuration = 10f; // seconds to show the label
        if (Time.timeSinceLevelLoad < introLabelDuration)
        {
            GUI.Label(new Rect(800, 300, 300, 70), "SURVIVE!");
            GUI.Label(new Rect(800, 380, 300, 70), "DAY 1/10");
        }else if (Time.timeSinceLevelLoad > introLabelDuration && Time.timeSinceLevelLoad < introLabelDuration + 10f)
        {
            GUI.Label(new Rect(800, 100, 1000, 70), "Collect materials from monsters and the ");
            GUI.Label(new Rect(800, 80, 1000, 70), "environment and craft better gear to survive!");
        }else if (Days >= 10)
        {
            GUI.Label(new Rect(800, 300, 300, 70), "YOU SURVIVED!");
            GUI.Label(new Rect(800, 380, 300, 70), "CONGRATULATIONS!");
        }
    }#
}
