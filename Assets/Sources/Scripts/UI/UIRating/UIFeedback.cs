using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FoodSort
{
    public class UIFeedback : UICanvas
    {
        [SerializeField] private UIClick _uIClickSubmit;
        [SerializeField] private UIClick _uIClickClose;
        [SerializeField] private Transform mainUI;
        [SerializeField] private List<Image> _starList;
        [SerializeField] private Sprite _starFill;
        [SerializeField] private Sprite _starEmpty;
        [SerializeField] private TMP_InputField _feedback;
        [SerializeField] private UIThankYouRate _uIThank;

        private Tween _tween;

        void Awake()
        {
            _uIClickSubmit.ActionAfterClick += Submit;
            _uIClickClose.ActionAfterClick += CloseUI;
        }
        protected override void OnEnable()
        {
            base.OnEnable();

            _tween?.Kill();

            mainUI.localScale = Vector3.zero;

            _tween = mainUI.DOScale(Vector3.one, 0.4f)
                      .SetEase(Ease.OutBack);

            StarRating(0);
            _feedback.text = "";
        }
        protected override void BeforeCloseUI(Action action)
        {
            base.BeforeCloseUI(action);

            _tween?.Kill();

            _tween = mainUI.DOScale(Vector3.zero, 0.3f)
                  .SetEase(Ease.InBack);
        }
        public void StarRating(int star)
        {
            // Debug.Log(star);
            for (int i = 0; i < _starList.Count; i++)
            {
                _starList[i].sprite = _starEmpty;
            }

            for (int i = 0; i < star; i++)
            {
                _starList[i].sprite = _starFill;
            }
        }
        public void OpenThankYouUI()
        {
            BeforeCloseUI(() => _uIThank.gameObject.SetActive(true));
        }
        public void Submit()
        {
            OpenThankYouUI();
        }
    }
}
