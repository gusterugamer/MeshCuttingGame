using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] CustomBoundryBox cbm;

    private static Score score;
   
    void Start()
    {
        score = new Score(cbm.Area);
    }    
}
