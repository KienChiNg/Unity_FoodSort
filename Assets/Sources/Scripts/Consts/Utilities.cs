using System;
using System.Collections.Generic;
using UnityEngine;

public static class Utilities
{
    public static string GetStringMMSSFromInt(int number)
    {
        int totalSeconds = number;
        TimeSpan time = TimeSpan.FromSeconds(totalSeconds);
        string formattedTime = string.Format("{0:D2}:{1:D2}", time.Minutes, time.Seconds);
        return formattedTime;
    }
}