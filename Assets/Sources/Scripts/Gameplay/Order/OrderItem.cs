using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace FoodSort
{
    public class OrderItem : MonoBehaviour
    {
        [SerializeField] private GameObject[] _displayFoods;
        [SerializeField] private SpriteRenderer _maskDone;
        [SerializeField] private SpriteRenderer _vObj;

        private int _foodIdx;
        private bool _isDone;
        public int FoodIdx => _foodIdx;
        public bool IsDone => _isDone;

        private LevelManager LevelManager => LevelManager.Instance;

        public void Init(int orderFoodIdx)
        {
            _foodIdx = orderFoodIdx;

            FoodSO foodSO = LevelManager.GetFoodSO(orderFoodIdx);
            foreach (var displayFood in _displayFoods)
            {
                Debug.Assert(displayFood != null);
                Debug.Assert(displayFood.TryGetComponent<SpriteRenderer>(out var _));
                displayFood.GetComponent<SpriteRenderer>().sprite = foodSO.foodSprite;
                displayFood.transform.localScale = new Vector3(foodSO.scale, foodSO.scale, 1);
            }
            _maskDone.DOFade(0, 0);
            _vObj.DOFade(0, 0);

            _isDone = false;
        }

        public void ReceiveFood()
        {
            _isDone = true;

            _maskDone.DOFade(0.6f, 0.5f);
            _vObj.DOFade(1f, 0.5f);
        }
    }
}