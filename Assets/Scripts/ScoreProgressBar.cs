using Packages.Rider.Editor;
using UnityEngine;
using UnityEngine.UI;

public class ScoreProgressBar : MonoBehaviour
{
    private Image progressBar;
    [SerializeField] LevelManager LM;

    private void Awake()
    {
        progressBar = GetComponent<Image>();
        LM.OnScoreChange += DefillBar;
    }

    private void DefillBar()
    {
        progressBar.fillAmount = LM.Score.progressPercent();
    }
    
}
