using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace FoodSort
{
	public class UICanvas : MonoBehaviour
	{
		private const float ALPHA_ONENABLE = .98f;
		private const float TIME_ANIM_FADE = 0.4f;

		[SerializeField] private Image _backGround;

		private Tween _tweenBackground;



		protected virtual void OnEnable()
		{
			if (_backGround == null) return;

			Color color = _backGround.color;
			color.a = 0;
			_backGround.color = color;

			_tweenBackground?.Kill();
			_tweenBackground = _backGround.DOFade(ALPHA_ONENABLE, TIME_ANIM_FADE);

			GameManager.Instance.SetPauseGame(true);
		}
		void Awake()
		{
		}
		protected virtual void OnDisable()
		{
			_tweenBackground?.Kill();
		}
		public void CloseUI()
		{
			BeforeCloseUI(null);
		}
		protected virtual void BeforeCloseUI(Action action)
		{
			Color color = _backGround.color;
			color.a = 1;
			_backGround.color = color;

			_tweenBackground?.Kill();

			_tweenBackground = _backGround.DOFade(0, 0.5f).OnComplete(() =>
			{
				if (action != null)
					action();
				else
					GameManager.Instance.SetPauseGame(false);

				gameObject.SetActive(false);
			});
		}
	}
}
