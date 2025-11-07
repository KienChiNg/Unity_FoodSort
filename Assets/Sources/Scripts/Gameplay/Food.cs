using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace FoodSort
{
	[System.Serializable]
	public class FoodData
	{
		public List<FoodSO> foodSOs;
	}
	public class Food : MonoBehaviour
	{
		private const float TIME_ANIM_MOVE = 0.2f;
		private const float TIME_ANIM_FLY = 0.4f;
		private const float TIME_ANIM_SCALE = 0.1f;
		private const float TIME_ANIM_FADE = 0.1f;
		private const float TIME_ANIM_FADE_OPENHIDDEN = 0.4f;
		private const float ALPHA_ONENABLE = 1f;
		private const float HEIGHT_PARABOLA = 1f;
		private const int SEGMENTS_POINT = 30;
		private const int SORTING_ORDER_INIT = 0;
		private const int SORTING_ORDER_FLY = 10;

		[SerializeField] private SpriteRenderer _background;
		[SerializeField] private SpriteRenderer _backgroundHidden;
		// [SerializeField] private List<FoodData> _foodSOList;
		[SerializeField] private ParticleSystem _stoveHit;
		[SerializeField] private ParticleSystem _stoveHitComplete;

		private SoundManager _soundManager;
		private LevelManager _levelManager;

		private Tween _tweenFly;
		private Tween _tweenMove;
		private Tween _tweenScale;
		private Tween _tweenRotate;
		private Tween _tweenBackground;

		private int _inxFood;
		private bool _isMove;
		private bool _isHide;

		public bool IsMove { get => _isMove; set => _isMove = value; }
		public int Value { get => _inxFood; set => _inxFood = value; }
		public bool IsHide { get => _isHide; set => _isHide = value; }

		void Awake()
		{
			_soundManager = SoundManager.Instance;
			_levelManager = LevelManager.Instance;
		}
		void Start()
		{
		}

		public void SetupFood(int inxFood, bool isHide = false)
		{
			this._inxFood = inxFood;
			FoodSO foodSO = _levelManager.GetFoodSO(inxFood);
			float scale = foodSO.scale;
			_background.transform.localScale = new Vector3(scale, scale, scale);
			_background.sprite = foodSO.foodSprite;

			_isHide = isHide;

			TurnOffBackground(isHide);
			TurnOffBackground(!isHide);
		}
		public void TurnOffBackground(bool isHide)
		{
			SpriteRenderer bg = isHide ? _backgroundHidden : _background;
			Color c = bg.color;
			c.a = 0f;
			bg.color = c;
		}
		public void TurnOnBackground(bool isHide)
		{
			SpriteRenderer bg = isHide ? _backgroundHidden : _background;
			Color c = bg.color;
			c.a = 1f;
			bg.color = c;
		}
		public void StatusBGHide(bool isHide)
		{
			TurnOffBackground(!isHide);
			TurnOnBackground(isHide);
		}
		public void SetOrderBackground(int order)
		{
			_background.sortingOrder = order;
		}
		public async Task FoodFade()
		{
			_tweenBackground?.Kill();
			SpriteRenderer bg = _isHide ? _backgroundHidden : _background;
			_tweenBackground = bg.DOFade(ALPHA_ONENABLE, TIME_ANIM_FADE)
											.OnComplete(() =>
											{
												Color c = bg.color;
												c.a = 1;
												bg.color = c;
											});

			await _tweenBackground.AsyncWaitForCompletion();
		}
		public async Task FoodOpenHidden()
		{
			_tweenBackground?.Kill();
			_isHide = false;
			_tweenBackground = _backgroundHidden.DOFade(0, TIME_ANIM_FADE_OPENHIDDEN);
			_background.DOFade(1, TIME_ANIM_FADE_OPENHIDDEN);

			await _tweenBackground.AsyncWaitForCompletion();
		}
		public void FoodScale(Vector2 targetScale)
		{
			_tweenScale?.Kill();
			_tweenScale = this.transform.DOScale(new Vector3(targetScale.x, targetScale.y, 0), TIME_ANIM_SCALE)
										.SetEase(Ease.OutBack)
										.OnComplete(() =>
										{
											_tweenScale = this.transform.DOScale(Vector3.one, TIME_ANIM_SCALE)
																		.SetEase(Ease.OutBack)
																		.OnComplete(() =>
																		{
																			this.transform.localScale = Vector3.one;
																		});
										});
		}
		public async Task FoodMove(Vector2 targetPoint, float time = TIME_ANIM_MOVE)
		{
			_tweenMove?.Kill();

			_isMove = true;

			_tweenMove = this.transform.DOLocalMove(new Vector3(targetPoint.x, targetPoint.y, 0), time)
										.SetEase(Ease.Linear)

										.OnComplete(() =>
										{
											this.transform.localPosition = new Vector3(targetPoint.x, targetPoint.y, 0);
											_isMove = false;
										});

			await _tweenMove.AsyncWaitForCompletion();
		}
		public async Task FoodFly(Vector2 targetPoint, float time = TIME_ANIM_FLY)
		{
			_tweenFly?.Kill();
			_tweenRotate?.Kill();

			SetOrderBackground(SORTING_ORDER_FLY);

			Vector3[] path = GenerateParabola(this.transform.position, new Vector3(targetPoint.x, targetPoint.y, 0), HEIGHT_PARABOLA, SEGMENTS_POINT);

			_tweenFly = this.transform.DOPath(path, time, PathType.CatmullRom, PathMode.Full3D, 30)
					 .SetUpdate(UpdateType.Fixed)
					 .SetEase(Ease.InSine)
					 .OnComplete(() =>
					 {
						 this.transform.position = path[path.Length - 1];
					 });

			// float timeDelay = time * 0.9f;

			// await Task.Delay((int)timeDelay * 1000);

			float rot = (transform.position.x > targetPoint.x) ? 720f : -720f;

			_tweenRotate = _background.transform.DORotate(new Vector3(0, 0, rot), time, RotateMode.WorldAxisAdd).SetEase(Ease.Linear);

			await Task.WhenAll(
				_tweenFly.AsyncWaitForCompletion(),
				_tweenRotate.AsyncWaitForCompletion()
			);

			SetOrderBackground(SORTING_ORDER_INIT);
		}
		public void PlayNewFoodInStove()
		{
			_stoveHit.Stop();
			_stoveHit.Play();
		}
		public void PlayCompleteInStove()
		{
			_stoveHitComplete.Stop();
			_stoveHitComplete.Play();
		}
		public void SetScaleFood()
		{
			FoodScale(new Vector2(1.2f, 0.9f));
		}
		// void OnTriggerEnter2D(Collider2D other)
		// {
		// 	if (other.CompareTag("Food") && (other.GetComponent<Food>().IsMove || _isMove))
		// 	{
		// 		FoodScale(new Vector2(1.2f, 0.9f));
		// 	}
		// }
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
