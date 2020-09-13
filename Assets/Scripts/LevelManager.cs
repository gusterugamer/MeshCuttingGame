using UnityEngine;
using System;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private CustomBoundryBox cbm;
    [SerializeField] private CutAreaInput cai;

    private static Score score;

    public Score Score { get => score; private set => score = value; }

    public delegate void ScoreChangeDelegate();
    public event ScoreChangeDelegate OnScoreChange;
   
    void Start()
    {
        Score = new Score(cbm.Area);
        cai.OnCutDone += UpdateScore;
    } 

    private void UpdateScore()
    {
        Score.UpdateCurrentScore(cbm.Area);
        OnScoreChange?.Invoke();
    }
}
