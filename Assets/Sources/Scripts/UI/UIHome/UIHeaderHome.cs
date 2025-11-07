using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace FoodSort
{
    public class UIHeaderHome : MonoBehaviour
    {
        [SerializeField] private UIClick _uIClickSetting;
        [SerializeField] private UIClick _uIClickCoin;
        [SerializeField] private UIBottomBarElement _uIBottomBarElementShop;
        [SerializeField] private UISettings _uISettings;
        [SerializeField] private TMP_Text _txtCoin;

        void Awake()
        {
            _uIClickSetting.ActionAfterClick += OpenSetting;
            _uIClickCoin.ActionAfterClick += _uIBottomBarElementShop.Select;

            SetCoinValue(GameManager.Instance.Coin);
            GameManager.Instance.OnCoinChange += SetCoinValue;
        }
        void OpenSetting()
        {
            _uISettings.gameObject.SetActive(true);
        }
        public void SetCoinValue(int value)
        {
            _txtCoin.text = value.ToString();
        }
    }
}
