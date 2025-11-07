using System.Collections;
using System.Collections.Generic;
using FoodSort;
using UnityEngine;

public class UISpecialLevel : UICanvas
{
    [SerializeField] private UIClick _uIClickPlay;


    void Awake()
    {
        _uIClickPlay.ActionAfterClick += PlaySpecial;
    }
    public async void PlaySpecial()
    {
        GameManager.Instance.IsSpecialLevel = true;
        LevelManager.Instance.Replay();
    }
    public async void SkipSpecial()
    {
        GameManager.Instance.BreakSpecialLevel = true;
        LevelManager.Instance.StartAnalytic();
    }
}
