using UnityEngine;
using System;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private CustomBoundryBox cbm;
    [SerializeField] private CutAreaInput cai;

    private static Score score;   
   
    void Start()
    {
        score = new Score(cbm.Area);
        cai.OnCutDone += UpdateScore;
    } 

    private void UpdateScore (object sender, EventArgs e)
    {
        score.UpdateCurrentScore(cbm.Area);
        Debug.Log(score.progressPercent());
    }
}
