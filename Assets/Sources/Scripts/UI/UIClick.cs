using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FoodSort
{
	public class UIClick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
	{
		private const float ALPHA_MOUSEDOWN = 0.8f;
		private const float ALPHA_MOUSEUP = 1f;
		private const float SCALE_MOUSEDOWN = 1.1f;
		private const float SCALE_MOUSEUP = 1f;
		private const float TIME_ANIM_FADE = 0.1f;

		private Action _action;
		[SerializeField] private Image _uiMain;

		private SoundManager _soundManager;

		private Tween _tweenFade;
		private Tween _tweenScale;

		public Action ActionAfterClick { get => _action; set => _action = value; }
		void Awake()
		{
			_soundManager = SoundManager.Instance;
		}
		public void OnPointerDown(PointerEventData eventData)
		{
			_tweenFade?.Kill();
			_tweenScale?.Kill();

			_tweenFade = _uiMain.DOFade(ALPHA_MOUSEDOWN, TIME_ANIM_FADE);
			_tweenScale = transform.DOScale(SCALE_MOUSEDOWN, TIME_ANIM_FADE);

			_soundManager.PlayBtnClick();
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			_tweenFade?.Kill();
			_tweenScale?.Kill();

			_tweenFade = _uiMain.DOFade(ALPHA_MOUSEUP, TIME_ANIM_FADE);
			_tweenScale = transform.DOScale(SCALE_MOUSEUP, TIME_ANIM_FADE);

			if (RectTransformUtility.RectangleContainsScreenPoint(
				transform as RectTransform,
				eventData.position,
				eventData.pressEventCamera))
			{
				if (_action != null)
					_action();
			}
		}
	}
}
