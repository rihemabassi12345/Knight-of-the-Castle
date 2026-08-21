using System;
using UnityEngine;

public interface IBuildPhaseListener
{
    void OnPrepPhaseStarted(float duration);
    void OnDayPhaseStarted(float duration);
}

public static class GameEvents
{
    // Event triggered when preparation/building phase begins
    public static event Action<float> OnPrepPhaseStart;

    // Event triggered when active survival/day phase begins
    public static event Action<float> OnDayPhaseStart;

    public static event Action<int> OnCoinsChanged;

    public static void TriggerPrepPhaseStart(float duration) => OnPrepPhaseStart?.Invoke(duration);
    public static void TriggerDayPhaseStart(float duration) => OnDayPhaseStart?.Invoke(duration);
    public static void TriggerCoinsChanged(int newAmount) => OnCoinsChanged?.Invoke(newAmount);
}