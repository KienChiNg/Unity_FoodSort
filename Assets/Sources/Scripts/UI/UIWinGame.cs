using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace FoodSort
{
	public class UIWinGame : UICanvas
	{
		private const float TIME_ANIM_SCALE = 0.7f;
		private const float TIME_ANIM_SCALE_TEXT_LEVEL = 0.3f;
		private const float TIME_ANIM_SCALE_SLIDER_LEVEL = 0.3f;
		private const float TIME_ANIM_SCALE_GIFT_LEVEL = 0.2f;
		private const float TIME_ANIM_SCALE_BUTTONX3_LEVEL = 0.2f;
		private const float TIME_ANIM_SCALE_BUTTONCONTINUE_LEVEL = 0.5f;
		private const int TIME_ANIM_NEXT_LEVEL = 300;
		private const float TIME_ANIM_SLIDER = 1.5f;
		private const float HEIGHT_PARABOLA = 1f;
		private const int SEGMENTS_POINT = 30;

		#region SEQUENCE
		[SerializeField] private Transform _mainUI;
		[SerializeField] private Transform _coinHeader;
		[SerializeField] private Transform _coinGain;
		[SerializeField] private Transform _victoryImg;
		[SerializeField] private Transform _sliderImg;
		[SerializeField] private Transform _giftImg;
		[SerializeField] private Transform _buttonX3;
		[SerializeField] private Transform _buttonContinue;
		[SerializeField] private Transform _coveringSheet;
		[SerializeField] private Transform _giftClaim;
		[SerializeField] private Transform _valueGift;
		[SerializeField] private Transform _buttonGift;
		[SerializeField] private Transform _giftChild;
		[SerializeField] private Animator _giftAnim;
		#endregion

		[SerializeField] private RawImage _rawImage;
		[SerializeField] private ParticleSystem _VFXconfetti;
		[SerializeField] private ParticleSystem _VFXfocus;
		[SerializeField] private ParticleSystem _VFXcoin;
		[SerializeField] private TMP_Text _levelDisplay;

		#region SLIDER
		[SerializeField] private RectTransform _slider;
		[SerializeField] private TMP_Text _levelDisplayUnlockPre;
		[SerializeField] private TMP_Text _levelDisplayUnlockNext;
		#endregion

		[SerializeField] private UIClick _uIClickNextLevel;
		[SerializeField] private UIClick _uIClickClaim;
		[SerializeField] private Image _newFood;
		[SerializeField] private Image _victoryImgAva;
		[SerializeField] private TMP_Text _newFoodTxt;

		int _level;
		int _prelUnlock;
		int _nextLevelUnlock;
		private bool _isClick;
		private bool _isOpen;

		private Tween _tween;
		private Tween _tweenScale;
		private Tween _tweenSlider;
		private Tween _tweenRawImage;
		private Tween _tweenFly;

		Sequence seq;


		protected override void OnEnable()
		{
			base.OnEnable();

			_tween?.Kill();
			_tweenRawImage?.Kill();

			_mainUI.localScale = Vector3.zero;
			Color color = _rawImage.color;
			color.a = 0;
			_rawImage.color = color;

			_tween = _mainUI.DOScale(Vector3.one, 0.6f)
					  .SetEase(Ease.OutBack);

			_tweenRawImage = _rawImage.DOFade(0.8f, 0.6f);

			UIManager.Instance.ShowCoin();

			_ = DoSequence();
		}
		void Awake()
		{

			_uIClickNextLevel.ActionAfterClick += CheckAdsX3;
		}
		void Start()
		{
			// _ = DoSequence();
		}
		async Task DoSequence()
		{
			_victoryImgAva.sprite = GameManager.Instance.avatarSOs[GameManager.Instance.inxProgess].avatar;
			_coinHeader.localScale = Vector3.zero;
			_coinGain.localScale = Vector3.zero;
			_victoryImg.localScale = Vector3.zero;
			_sliderImg.localScale = Vector3.zero;
			_giftImg.localScale = Vector3.zero;
			_buttonX3.localScale = Vector3.zero;
			_buttonContinue.localScale = Vector3.zero;
			_levelDisplay.transform.localScale = Vector3.zero;

			LoadLevelDisplayUnlockGift();
			await Task.Delay(500);
			await ImageScale(_victoryImg, TIME_ANIM_SCALE);
			await ImageScale(_levelDisplay.transform, TIME_ANIM_SCALE_TEXT_LEVEL);
			await Task.Delay(400);
			_VFXconfetti?.Stop();
			_VFXconfetti?.Play();
			await Task.Delay(500);
			if (!LevelManager.Instance.MaxFood)
			{
				await ImageScale(_sliderImg, TIME_ANIM_SCALE_SLIDER_LEVEL);
				await LoadUnlockNewFoodSlider();
				await ImageScale(_giftImg, TIME_ANIM_SCALE_SLIDER_LEVEL);
			}
			_VFXcoin?.Stop();
			_VFXcoin?.Play();
			CheckButtonDisplay();
		}
		public void CheckAds()
		{
			if (_isClick) return;

			// Debug.Log(GameManager.Instance.CountInterPerAds + " " + AnalyticHandle.RemoteConfig.InterPerLevel);
			if (GameManager.Instance.CountLevelFeedback != Consts.LEVEL_RATE_US
				&& GameManager.Instance.OnAds
				&& (PlayerPrefs.GetInt(Consts.LEVEL_SAVE, 1) >= AnalyticHandle.RemoteConfig.InterLevel))
				MaxMediationController.instance.ShowInterstitial("_finish_level", () => { NextLevel(); });
			else
				NextLevel();
		}
		public void CheckAdsx0()
		{
			if (_isClick) return;

			// Debug.Log(GameManager.Instance.CountInterPerAds + " " + AnalyticHandle.RemoteConfig.InterPerLevel);
			if (GameManager.Instance.CountLevelFeedback != Consts.LEVEL_RATE_US
				&& GameManager.Instance.OnAds
				&& (PlayerPrefs.GetInt(Consts.LEVEL_SAVE, 1) >= AnalyticHandle.RemoteConfig.InterLevel))
				MaxMediationController.instance.ShowInterstitial("_finish_level", () =>
				{
					NextLevel(0);
				});
			else
				NextLevel(0);
		}
		public void CheckAdsX3()
		{
			if (_isClick) return;

			MaxMediationController.instance.ShowRewardedAd("_x3_coin", () =>
			{
				NextLevel(3);
			});
		}
		public async Task ImageScale(Transform tf, float time, bool res = false)
		{
			// _tweenScale?.Kill();

			tf.localScale = res ? Vector3.one : Vector3.zero;

			_tweenScale = tf.DOScale(!res ? Vector3.one : Vector3.zero, time)
										.SetEase(!res ? Ease.OutBack : Ease.Linear)
										.OnComplete(() =>
										{
											tf.localScale = !res ? Vector3.one : Vector3.zero;
										});
			await _tweenScale.AsyncWaitForCompletion();
		}
		public async void ClaimNewFood()
		{
			_uIClickClaim.ActionAfterClick -= ClaimNewFood;
			GameManager.Instance.StateCoin(true, Consts.COIN_AFTER_WIN * 2);
			await ImageScale(_valueGift, 0.3f, true);
			await ImageScale(_buttonGift, 0.2f, true);

			_coveringSheet.gameObject.SetActive(false);

			// CheckAdsx0();
			await SequenceContinue();
		}
		public async void NextLevel(int xCoin = 1)
		{
			_isClick = true;
			_uIClickNextLevel.ActionAfterClick = null;
			GameManager.Instance.StateCoin(true, Consts.COIN_AFTER_WIN * xCoin);
			await Task.Delay(TIME_ANIM_NEXT_LEVEL);
			LevelManager.Instance.Replay();
		}
		public void SetLevelDisplay(int level)
		{
			_levelDisplay.text = $"Level {level}";
		}
		public void LoadLevelDisplayUnlockGift()
		{
			_level = LevelManager.Instance.LevelDisplay;
			_prelUnlock = LevelManager.Instance.ValueUnlockNewFood + 1;
			_nextLevelUnlock = LevelManager.Instance.NextUnlockNewFood;

			_levelDisplayUnlockPre.text = $"{_prelUnlock}";
			_levelDisplayUnlockNext.text = $"{_nextLevelUnlock}";

			float preValue = Consts.SLIDER_WIDTH_MAX * (_level - _prelUnlock) / (_nextLevelUnlock + 1 - _prelUnlock);
			Vector2 sizeX = _slider.sizeDelta;
			sizeX.x = preValue;
			_slider.sizeDelta = sizeX;
		}
		public async void CheckButtonDisplay()
		{
			if (_level == _nextLevelUnlock)
			{
				_isOpen = true;
				_coveringSheet.gameObject.SetActive(true);
				Vector3 originalPos = _giftImg.localEulerAngles;

				seq = DOTween.Sequence();

				seq.Append(_giftImg.DOLocalRotate(new Vector3(0, 0, 10), 0.2f))
				   .Append(_giftImg.DOLocalRotate(new Vector3(0, 0, -10), 0.2f))
				   .Append(_giftImg.DOLocalRotate(new Vector3(0, 0, 10), 0.2f))
				   .Append(_giftImg.DOLocalRotate(new Vector3(0, 0, -10), 0.2f))
				   .Append(_giftImg.DOLocalRotate(originalPos, 0.1f))
				   .SetEase(Ease.InOutSine)
				   .SetLoops(-1, LoopType.Restart);

				_newFood.sprite = LevelManager.Instance.UnlockFoodSO.unlockLevelFoodSOs[LevelManager.Instance.InxUnlock + 1].foodSO.foodSprite;
				_newFoodTxt.text = LevelManager.Instance.UnlockFoodSO.unlockLevelFoodSOs[LevelManager.Instance.InxUnlock + 1].foodSO.foodName;
			}
			else
			{
				_ = ImageScale(_coinHeader, TIME_ANIM_SCALE_SLIDER_LEVEL);
				await SequenceContinue();
			}
		}
		public async Task SequenceContinue()
		{
			_ = ImageScale(_coinGain, TIME_ANIM_SCALE_SLIDER_LEVEL);

			await ImageScale(_buttonX3, TIME_ANIM_SCALE_BUTTONX3_LEVEL);
			await Task.Delay(800);
			await ImageScale(_buttonContinue, TIME_ANIM_SCALE_BUTTONCONTINUE_LEVEL);
		}
		public async Task LoadUnlockNewFoodSlider()
		{
			_tweenSlider?.Kill();

			float nextValue = Consts.SLIDER_WIDTH_MAX * (_level + 1 - _prelUnlock) / (_nextLevelUnlock + 1 - _prelUnlock);
			_tweenSlider = DOTween.To(
							() => _slider.sizeDelta.x,
							x => _slider.sizeDelta = new Vector2(x, _slider.sizeDelta.y),
							nextValue,
							TIME_ANIM_SLIDER
						).SetEase(Ease.OutQuad);

			await _tweenSlider.AsyncWaitForCompletion();
		}
		public async void LoadUnlockGift()
		{
			if (!_isOpen) return;
			_isOpen = false;

			_VFXfocus?.Stop();
			seq.Kill();
			_giftImg.localEulerAngles = Vector3.zero;
			_valueGift.localScale = Vector3.zero;
			_buttonGift.localScale = Vector3.zero;

			Vector3[] path = GenerateParabola(_giftImg.position, _coinGain.position, HEIGHT_PARABOLA, SEGMENTS_POINT);

			_tweenFly?.Kill();
			_tweenFly = _giftImg.DOPath(path, 0.8f, PathType.CatmullRom, PathMode.Full3D, 30)
					 .SetUpdate(UpdateType.Fixed)
					 .SetEase(Ease.InSine)
					 .OnComplete(() =>
					 {
						 _giftImg.position = path[path.Length - 1];
					 });

			_tweenScale = _giftChild.DOScale(new Vector3(1, 1, 1), 0.8f)
										.SetEase(Ease.OutBack)
										.OnComplete(() =>
										{
											_giftChild.localScale = new Vector3(1, 1, 1);
										});

			await _tweenFly.AsyncWaitForCompletion();

			_giftClaim.gameObject.SetActive(true);

			_giftAnim.Play("giftbox open");

			await Task.Delay(1000);

			await ImageScale(_giftImg, 0.15f, true);
			_ = ImageScale(_coinHeader, TIME_ANIM_SCALE_SLIDER_LEVEL);
			await ImageScale(_valueGift, 0.6f);
			await ImageScale(_buttonGift, TIME_ANIM_SCALE_BUTTONCONTINUE_LEVEL);

			_uIClickClaim.ActionAfterClick += ClaimNewFood;
		}
		Vector3[] GenerateParabola(Vector3 start, Vector3 end, float _height, int _segments)
		{
			Vector3[] points = new Vector3[_segments + 1];
			for (int i = 0; i <= _segments; i++)
			{
				float t = i / (float)_segments;
				float yOffset = 4 * _height * t * (1 - t);
				points[i] = Vector3.Lerp(start, end, t) + Vector3.up * yOffset;
			}
			return points;
		}
	}
}
