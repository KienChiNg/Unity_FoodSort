using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;


namespace FoodSort
{
    public class UIRateUs : UICanvas
    {
        [SerializeField] private UIClick _uIClickClose;
        [SerializeField] private Transform mainUI;
        [SerializeField] private List<Image> _starList;
        [SerializeField] private Sprite _starFill;
        [SerializeField] private Sprite _starEmpty;
        [SerializeField] private UIThankYouRate _uIThank;
        [SerializeField] private UIFeedback _uIFeedback;
        private Tween _tween;
        private int _starVote;
        protected override void OnEnable()
        {
            base.OnEnable();

            _tween?.Kill();

            mainUI.localScale = Vector3.zero;

            _tween = mainUI.DOScale(Vector3.one, 0.4f)
                      .SetEase(Ease.OutBack);

            StarRating(0);
        }
        void Awake()
        {
            _uIClickClose.ActionAfterClick += CloseUI;
        }
        protected override void BeforeCloseUI(Action action)
        {
            base.BeforeCloseUI(action);

            _tween?.Kill();

            _tween = mainUI.DOScale(Vector3.zero, 0.3f)
                  .SetEase(Ease.InBack);
        }
        public void OpenSetting()
        {
            BeforeCloseUI(null);
        }
        public void OpenThankYouUI()
        {
            BeforeCloseUI(() => _uIThank.gameObject.SetActive(true));
        }
        public void OpenFeedback()
        {
            BeforeCloseUI(() =>
            {
                _uIFeedback.gameObject.SetActive(true);
                _uIFeedback.StarRating(_starVote);
            });
        }
        public void StarRating(int star)
        {
            // Debug.Log(star);
            for (int i = 0; i < _starList.Count; i++)
            {
                _starList[i].sprite = _starEmpty;
            }

            if (star > 0)
            {
                for (int i = 0; i < star; i++)
                {
                    _starList[i].sprite = _starFill;
                }
                if (star == 5)
                {
                    PlayerPrefs.SetInt("Feedback", 1);
                    PlayerPrefs.Save();
                    Application.OpenURL($"market://details?id={Application.identifier}");
                    OpenThankYouUI();
                }
                else
                {
                    _starVote = star;
                    OpenFeedback();
                }
            }
        }
        // private void GoRatingAndroid()
        //     => RequestReview().Forget();

        // private async UniTask RequestReview()
        // {
        //     var reviewManager = new ReviewManager();

        //     var requestFlowOperation = reviewManager.RequestReviewFlow();
        //     await requestFlowOperation;
        //     if (requestFlowOperation.Error != ReviewErrorCode.NoError)
        //     {
        //         Debug.LogError($"Android rating Request Flow Operation error: {requestFlowOperation.Error.ToString()}");
        //         Application.OpenURL($"market://details?id={Application.identifier}");
        //         await UniTask.CompletedTask;
        //         return;
        //     }

        //     var playReviewInfo = requestFlowOperation.GetResult();

        //     var launchFlowOperation = reviewManager.LaunchReviewFlow(playReviewInfo);
        //     await launchFlowOperation;

        //     if (launchFlowOperation.Error != ReviewErrorCode.NoError)
        //     {
        //         Debug.LogError($"Android rating Launch Flow Operation error: {requestFlowOperation.Error.ToString()}");
        //         Application.OpenURL($"market://details?id={Application.identifier}");
        //         await UniTask.CompletedTask;
        //     }
        // }
    }
}
