using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Score
{
    float _startArea;
    float _currentArea;

    public Score(float startArea)
    {
        _startArea = startArea;
    }

    public float StartArea { get => _startArea; }
    public float CurrentArea { get => _currentArea; }

    public float progressPercent()
    {
        return (CurrentArea / StartArea);
    }

    public void UpdateCurrentScore(float currentArea)
    {
        _currentArea = currentArea;
    }
}
