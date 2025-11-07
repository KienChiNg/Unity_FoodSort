using System.Collections;
using System.Collections.Generic;
using FoodSort;
using UnityEngine;


namespace FoodSort
{
    public class UIBottomBar : Singleton<UIBottomBar>
    {
        [SerializeField] private List<UIBottomBarElement> uIBottomBarElements;
        [SerializeField] private GameObject _vfxHome;

        public GameObject VfxHome { get => _vfxHome; set => _vfxHome = value; }

        public void UnSelectAll()
        {
            foreach (UIBottomBarElement uIBottomBarElement in uIBottomBarElements)
            {
                uIBottomBarElement.UnSelect();
            }
        }
    }
}
