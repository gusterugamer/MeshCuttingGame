using UnityEngine;
using UnityEngine.UI;

public class ScoreProgressBar : MonoBehaviour
{
    private Image _progressBar;
    [SerializeField] LevelManager _LM;

    private void Awake()
    {
        _progressBar = GetComponent<Image>();
        _LM.OnScoreChange += DefillBar;
    }

    private void DefillBar()
    {
        _progressBar.fillAmount = _LM.Score.progressPercent();
    }
    
}
