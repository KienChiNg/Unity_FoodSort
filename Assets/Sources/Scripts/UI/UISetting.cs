using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace FoodSort
{
	public class UISettings : UICanvas
	{
		// [SerializeField] private GameObject _vfxHome;
		[SerializeField] private Transform _mainUI;
		[SerializeField] private UIClick _uIClickClose;
		[SerializeField] private UIClick _uIClickVolume;
		[SerializeField] private UIClick _uIClickHaptic;
		[SerializeField] private UIClick _uIClickMusic;
		[Header("Music")]
		[SerializeField] private Transform _adjustmentKnobMusic;
		[SerializeField] private Transform _stateDisplayMusic;
		[SerializeField] private Transform _stateDisplayColorMusic;
		[Header("Sound")]
		[SerializeField] private Transform _adjustmentKnobSound;
		[SerializeField] private Transform _stateDisplaySound;
		[SerializeField] private Transform _stateDisplayColorSound;

		[Header("Haptic")]
		[SerializeField] private Transform _adjustmentKnobHaptic;
		[SerializeField] private Transform _stateDisplayHaptic;
		[SerializeField] private Transform _stateDisplayColorHaptic;
		[SerializeField] private List<Sprite> _stateDisplayImage;
		[SerializeField] private List<Sprite> _stateDisplayColorImage;


		private Tween _tween;
		private Tween _tweenAdjMusic;
		private Tween _tweenColorDisplayMusic;
		private Tween _tweenAdjSound;
		private Tween _tweenColorDisplaySound;
		private Tween _tweenAdjHaptic;
		private Tween _tweenColorDisplayHaptic;

		private bool _isMuteMusic;
		private bool _isMuteSound;
		private bool _isHaptic;
		private float _posXKnob;
		private float _posXStateDisplay;
		void Awake()
		{
			_uIClickClose.ActionAfterClick += CloseUI;
			_uIClickVolume.ActionAfterClick += ChangeStateVolume;
			_uIClickHaptic.ActionAfterClick += ChangeStateHaptics;
			_uIClickMusic.ActionAfterClick += ChangeStateMusic;


		}
		void Start()
		{

			_posXKnob = _adjustmentKnobSound.localPosition.x;
			_posXStateDisplay = _stateDisplaySound.localPosition.x;

			_isMuteSound = SoundManager.Instance.IsMute;
			_isHaptic = SoundManager.Instance.IsHaptic;
			_isMuteMusic = SoundManager.Instance.IsMuteMusic;

			StateVolumeMusic(true);
			StateVolumeSound(true);
			StateHaptics(true);
		}
		protected override void OnEnable()
		{
			base.OnEnable();

			_tween?.Kill();

			_mainUI.localScale = Vector3.zero;

			_tween = _mainUI.DOScale(Vector3.one, 0.4f)
					  .SetEase(Ease.OutBack);
		}
		protected override void BeforeCloseUI(Action action)
		{
			base.BeforeCloseUI(action);

			_tween?.Kill();

			_tween = _mainUI.DOScale(Vector3.zero, 0.3f)
				  .SetEase(Ease.InBack);
		}
		public void OpenRateUs(GameObject gameObject)
		{
			BeforeCloseUI(() => gameObject.SetActive(true));
		}
		public void OpenRestartCurrentLv(GameObject gameObject)
		{
			BeforeCloseUI(() =>
			{
				gameObject.SetActive(true);
			});
		}
		public void ChangeStateMusic()
		{
			StateVolumeMusic(false);
		}
		public void ChangeStateVolume()
		{
			StateVolumeSound(false);
		}
		public void ChangeStateHaptics()
		{
			StateHaptics(false);
		}
		public void StateVolumeMusic(bool isCheck)
		{
			_tweenAdjMusic?.Kill();
			_tweenColorDisplayMusic?.Kill();
			if (!isCheck)
			{
				_isMuteMusic = !_isMuteMusic;
				SoundManager.Instance.IsMute = _isMuteMusic;
				if (_isMuteMusic)
				{
					PlayerPrefs.SetInt(Consts.MUSIC_FOODSORT, 0);
					PlayerPrefs.Save();
					SoundManager.Instance.HandleVolumeMusic(0);
				}
				else
				{
					PlayerPrefs.SetInt(Consts.MUSIC_FOODSORT, 1);
					PlayerPrefs.Save();
					SoundManager.Instance.HandleVolumeMusic(1);
				}
			}

			_tweenAdjMusic = _adjustmentKnobMusic.DOLocalMoveX(_isMuteMusic ? -_posXKnob : _posXKnob, 0.3f);
			_stateDisplayMusic.localPosition = new Vector3(_isMuteMusic ? -_posXStateDisplay : _posXStateDisplay, 0, 0);
			_stateDisplayMusic.GetComponent<Image>().sprite = _stateDisplayImage[_isMuteMusic ? 1 : 0];
			_tweenColorDisplayMusic = _stateDisplayColorMusic.GetComponent<Image>()
					.DOFade(0, 0.15f)
					.OnComplete(() =>
					{
						_stateDisplayColorMusic.GetComponent<Image>().sprite = _stateDisplayColorImage[_isMuteMusic ? 1 : 0];
						_stateDisplayColorMusic.GetComponent<Image>().DOFade(1, 0.15f);
					});
		}
		public void StateVolumeSound(bool isCheck)
		{
			_tweenAdjSound?.Kill();
			_tweenColorDisplaySound?.Kill();
			if (!isCheck)
			{
				_isMuteSound = !_isMuteSound;
				SoundManager.Instance.IsMute = _isMuteSound;
				if (_isMuteSound)
				{
					PlayerPrefs.SetInt(Consts.SOUND_FOODSORT, 0);
					PlayerPrefs.Save();
					SoundManager.Instance.HandleVolume(0);
				}
				else
				{
					PlayerPrefs.SetInt(Consts.SOUND_FOODSORT, 1);
					PlayerPrefs.Save();
					SoundManager.Instance.HandleVolume(1);
				}
			}

			_tweenAdjSound = _adjustmentKnobSound.DOLocalMoveX(_isMuteSound ? -_posXKnob : _posXKnob, 0.3f);
			_stateDisplaySound.localPosition = new Vector3(_isMuteSound ? -_posXStateDisplay : _posXStateDisplay, 0, 0);
			_stateDisplaySound.GetComponent<Image>().sprite = _stateDisplayImage[_isMuteSound ? 1 : 0];
			_tweenColorDisplaySound = _stateDisplayColorSound.GetComponent<Image>()
					.DOFade(0, 0.15f)
					.OnComplete(() =>
					{
						_stateDisplayColorSound.GetComponent<Image>().sprite = _stateDisplayColorImage[_isMuteSound ? 1 : 0];
						_stateDisplayColorSound.GetComponent<Image>().DOFade(1, 0.15f);
					});
		}
		public void StateHaptics(bool isCheck)
		{
			_tweenAdjHaptic?.Kill();
			_tweenColorDisplayHaptic?.Kill();

			if (!isCheck)
			{
				_isHaptic = !_isHaptic;
				SoundManager.Instance.IsHaptic = _isHaptic;
				if (_isHaptic)
				{
					PlayerPrefs.SetInt(Consts.HAPTIC_FOODSORT, 0);
					PlayerPrefs.Save();
				}
				else
				{
					PlayerPrefs.SetInt(Consts.HAPTIC_FOODSORT, 1);
					PlayerPrefs.Save();
				}
			}

			_tweenAdjSound = _adjustmentKnobHaptic.DOLocalMoveX(_isHaptic ? -_posXKnob : _posXKnob, 0.3f);
			_stateDisplayHaptic.localPosition = new Vector3(_isHaptic ? -_posXStateDisplay : _posXStateDisplay, 0, 0);
			_stateDisplayHaptic.GetComponent<Image>().sprite = _stateDisplayImage[_isHaptic ? 1 : 0];
			_tweenColorDisplaySound = _stateDisplayColorHaptic.GetComponent<Image>()
					.DOFade(0, 0.15f)
					.OnComplete(() =>
					{
						_stateDisplayColorHaptic.GetComponent<Image>().sprite = _stateDisplayColorImage[_isHaptic ? 1 : 0];
						_stateDisplayColorHaptic.GetComponent<Image>().DOFade(1, 0.15f);
					});
		}
		public void OpenTermsOfUse()
		{
			Application.OpenURL("https://zegostudio.com/terms.html");
		}
		public void OpenPrivacyPolicy()
		{
			Application.OpenURL("https://zegostudio.com/privacy-policy.html");
		}
	}
}
