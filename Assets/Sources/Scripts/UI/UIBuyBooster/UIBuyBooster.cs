using System;
using DG.Tweening;
using UnityEngine;

namespace FoodSort
{
	public abstract class UIBuyBooster : UICanvas
	{
		[SerializeField] private Transform _mainUI;
		[SerializeField] private UIClick _uIClickClose;
		[SerializeField] private UIClick _uIClickUseCoin;
		[SerializeField] private UIClick _uIClickUseAds;

		private Tween _tween;

		protected override void OnEnable()
		{
			base.OnEnable();

			_tween?.Kill();

			_mainUI.localScale = Vector3.zero;

			_tween = _mainUI.DOScale(Vector3.one, 0.4f)
					  .SetEase(Ease.OutBack);

			UIManager.Instance.ShowCoin();
		}
		void Awake()
		{

			_uIClickClose.ActionAfterClick += CloseUI;
			_uIClickUseCoin.ActionAfterClick += UseCoinBooster;
			_uIClickUseAds.ActionAfterClick += UseAdsBooster;
		}
		protected abstract void UseCoinBooster();
		protected abstract void UseAdsBooster();

		protected override void BeforeCloseUI(Action action)
		{
			base.BeforeCloseUI(action);

			_tween?.Kill();

			_tween = _mainUI.DOScale(Vector3.zero, 0.3f)
				  .SetEase(Ease.InBack);

			UIManager.Instance.HideCoin();
		}
	}
}
