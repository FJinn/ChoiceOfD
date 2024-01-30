using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TimeManager : Singleton<TimeManager>
{
    public static Action onDayEnded;
    public static Action onTimeOfDayChanged;

    public static int currentDay {private set; get;}
    public static ETimeOfDay currentTimeOfDay {private set; get;}

    [SerializeField] float dayInSeconds;
    [SerializeField, Range(0, 1f)] float dayPercentagePerDay = 0.5f;

    float dayNightSwapInSeconds;
    bool dayNightSwapped;

    static float gameElapsedTimePerDay;
    static int dayCount;

    public static float deltaTime {private set; get;}

    public enum ETimeOfDay
    {
        Daytime = 0,
        Night = 1
    }

    void Start()
    {
        dayNightSwapInSeconds = dayInSeconds * dayPercentagePerDay;
        dayNightSwapped = false;
    }

    void Update()
    {
        deltaTime = Time.deltaTime;
        gameElapsedTimePerDay += deltaTime;

        if(!dayNightSwapped && gameElapsedTimePerDay >= dayNightSwapInSeconds)
        {
            currentTimeOfDay = currentTimeOfDay == ETimeOfDay.Daytime ? ETimeOfDay.Night : ETimeOfDay.Daytime;
            onTimeOfDayChanged?.Invoke();
            dayNightSwapped = true;
        }

        if(gameElapsedTimePerDay >= dayInSeconds)
        {
            EndDay();
            gameElapsedTimePerDay = 0;
        }
    }

    void ResetGame()
    {
        currentDay = 0;
        dayCount = 0;
        gameElapsedTimePerDay = 0;
        onDayEnded?.Invoke();
    }

    void EndDay()
    {
        dayCount += 1;
        currentDay = dayCount;
        onDayEnded?.Invoke();
        dayNightSwapped = false;
    }
}
