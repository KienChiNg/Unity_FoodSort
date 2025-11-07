using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace FoodSort
{
    public class UIThankYouRate : UICanvas
    {
        [SerializeField] private Transform mainUI;
        [SerializeField] private UIClick _uIClickClose;
        private Tween _tween;
        protected override void OnEnable()
        {
            base.OnEnable();

            _tween?.Kill();

            mainUI.localScale = Vector3.zero;

            _tween = mainUI.DOScale(Vector3.one, 0.4f)
                      .SetEase(Ease.OutBack);
        }
        private void Awake() {
            _uIClickClose.ActionAfterClick += CloseUI;
        }
        protected override void BeforeCloseUI(Action action)
        {
            base.BeforeCloseUI(action);

            _tween?.Kill();

            _tween = mainUI.DOScale(Vector3.zero, 0.3f)
                  .SetEase(Ease.InBack);
        }
    }
}
