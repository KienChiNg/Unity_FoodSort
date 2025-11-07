using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


namespace FoodSort
{
    public class UIStarRating : MonoBehaviour, IPointerDownHandler, IPointerExitHandler
    {
        [SerializeField] private UIRateUs _uIRateUs;
        [SerializeField] private int _starInx;

        public void OnPointerDown(PointerEventData eventData)
        {
            // throw new System.NotImplementedException();
            _uIRateUs.StarRating(_starInx);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            // _uIRateUs.StarRating(_starInx);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // _uIRateUs.StarRating(0);
        }
    }
}