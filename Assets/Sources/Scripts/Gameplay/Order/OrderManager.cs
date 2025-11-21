using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using FoodSort;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace FoodSort
{
    public class OrderManager : MonoBehaviour
    {
        [SerializeField] private GameObject _orderPlace;
        [SerializeField] private NormalCustomer _currentCustomer;
        [SerializeField] private OrderPack _orderPack;

		public Action OnCompleteIncorrectOrder;
        public Action OnOrderTimerWarning;

        public static OrderManager Instance;

        void Awake()
        {
            Instance = this;
            _orderPlace.gameObject.SetActive(false);
        }

        void OnEnable()
        {
            OnCompleteIncorrectOrder += MakeCustomerSad;
            OnOrderTimerWarning += MakeCustomerAnrgy;
        }

        public void Init(OrderData orderData)
        {
            Debug.Assert(_currentCustomer != null && _orderPack != null);

            _orderPlace.gameObject.SetActive(true);
            _orderPack.Init(orderData);
        }

        public bool TryDeliverFood(int foodIdx)
        {
            if (_orderPack.CanReceiveFood(foodIdx))
            {
                _orderPack.ReceiveFood(foodIdx);
                return true;
            }
            return false;
        }

        public void AddSkewerOnCustomer(Skewer skewer)
        {
            _currentCustomer.AddSkewerOnCustomer(skewer);
        }

        public void MakeCustomerSad()
        {
            _currentCustomer.MakeCustomerSad();
        }

        public void MakeCustomerAnrgy()
        {
            _currentCustomer.MakeCustomerAnrgy();
        }

        public void AfterWinGame()
        {
            _currentCustomer.StopCustomerAngry();
        }

        public bool IsCompletedAllOrders()
        {
            return _orderPack.IsCompletedAllOrders();
        }
    }
}