using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetScreen : MonoBehaviour
{
    [SerializeField] LevelManager _LM;
    [SerializeField] GameObject _resetPanel;

    private void Awake()
    {
        _resetPanel.SetActive(false);
        _LM.OnCuttingObject += Activate;
        _LM.OnResetScene += DeActivate;
    }

    private void Activate()
    {
        _resetPanel.SetActive(true);
    }
     
    private void DeActivate()
    {
        _resetPanel.SetActive(false);
    }
}
