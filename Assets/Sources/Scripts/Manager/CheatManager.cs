using System.Collections;
using System.Collections.Generic;
using FoodSort;
using TMPro;
using UnityEngine;

public class CheatManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField _inputF;
    [SerializeField] private TMP_InputField _valueLTV;
    [SerializeField] private GameObject _cheatUI;

    public float clickInterval = 2f;
    private int clickCount = 0;
    private float firstClickTime = 0f;

    public void SelectLV()
    {
        if (_inputF.text != "" && int.Parse(_inputF.text) > 0)
        {
            PlayerPrefs.SetInt(Consts.LEVEL_SAVE, int.Parse(_inputF.text));
            PlayerPrefs.Save();

            LevelManager.Instance.Replay();
        }
    }
    public void ApplyValueLTV()
    {
        AnalyticHandle.Instance.AdIncremental(float.Parse(_valueLTV.text));
    }
    public void Click()
    {
        float now = Time.time;

        if (clickCount == 0)
        {
            firstClickTime = now;
        }

        if (now - firstClickTime <= clickInterval)
        {
            clickCount++;

            if (clickCount >= 3)
            {
                _cheatUI.SetActive(true);
                clickCount = 0;
            }
        }
        else
        {
            firstClickTime = now;
            clickCount = 1;
        }
    }
}
