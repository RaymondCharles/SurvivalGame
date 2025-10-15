using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    // Time variables
    private int minutes;
    public int Minutes { get { return minutes; } set { minutes = value; OnMinutesChanged(value); } }
    private int hours;
    public int Hours { get { return hours; } set { hours = value; OnHoursChanged(value); } }
    private int days;
    public int Days { get { return days; } set { days = value; OnDaysChanged(value); } }

    private float tempSecond;

    public void Update()
    {
        // Increments tempSecond by the time passed since the last frame, and if it reaches 1 second, increments minutes - current time scale is 1 real time second = 1 game minute
        tempSecond += Time.deltaTime;
        if (tempSecond >= 1f)
        {
            Minutes += 1;
            tempSecond = 0f;
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
        }
        // Handle hour changes (just Skybox for now)
        if (hr >= 6 && hr < 18)
        {
            
        }
        else
        {
            
        }
    }
    private void OnDaysChanged(int days)
    {
        // Handle day changes (e.g., update UI, trigger events)
    }
}
