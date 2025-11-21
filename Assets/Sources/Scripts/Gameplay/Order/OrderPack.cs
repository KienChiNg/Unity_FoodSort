using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
// using MoreMountains.Feedbacks;
// using Spine;
// using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;


namespace FoodSort
{
    public class OrderPack : MonoBehaviour
    {
        [SerializeField] private OrderItem[] _orderItems;

        public void Init(OrderData orderData)
        {
            Debug.Assert(orderData?.orderFoodsIdx?.Length == 3);

            Debug.Assert(_orderItems != null && _orderItems.Length == 3);
            for (int iFood = 0; iFood < 3; iFood++)
            {
                Debug.Assert(_orderItems[iFood] != null);
                _orderItems[iFood].Init(orderData.orderFoodsIdx[iFood]);
            }
        }

        public bool CanReceiveFood(int foodIdx)
        {
            return _orderItems.Any(x => x.FoodIdx == foodIdx && !x.IsDone);
        }

        public void ReceiveFood(int foodIdx)
        {
            OrderItem order = _orderItems.First(x => x.FoodIdx == foodIdx && !x.IsDone);
            Debug.Assert(order != null);
            order.ReceiveFood();
        }

        public bool IsCompletedAllOrders()
        {
            return _orderItems.All(x => x.IsDone);
        }
    }
}
