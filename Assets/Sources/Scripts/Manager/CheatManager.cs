using System.Collections;
using System.Collections.Generic;
using FoodSort;
using TMPro;
using UnityEngine;

public class CheatManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField _inputF;

    public void SelectLV()
    {
        if (_inputF.text != "" && int.Parse(_inputF.text) > 0)
        {
            PlayerPrefs.SetInt(Consts.LEVEL_SAVE, int.Parse(_inputF.text));
            PlayerPrefs.Save();

            LevelManager.Instance.Replay();
        }
    }
}
