using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DG.Tweening;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.UI;

namespace FoodSort
{
	[RequireComponent(typeof(BoxCollider2D))]
	public class Skewer : MonoBehaviour
	{
		private const float TIME_ANIM_MOVE = 0.3f;
		private const float TIME_ANIM_ROTATE = 0.3f;
		private const float TIME_ANIM_SCALE = 0.3f;
		private const float POS_Y_INIT_FOOD_SPAWN = 0.8f;
		private const float POS_Y_INIT_FOOD_SPAWN_SPECIAL = 1.4f;
		private const float POS_Y_FOOD_MOVE_TO_OTHER_SKEWER = 0.8f;
		private const float TIME_ANIM_MOVE_FOR_SAME_FOOD = 0.05f;
		private const float TIME_ANIM_FLY_FOR_SAME_FOOD = 0.1f;
		private const float HEIGHT_PARABOLA = 1f;
		private const float TIME_ANIM_FLY = 0.6f;
		private const float TIME_ANIM_FADE = 0.1f;
		private const int SEGMENTS_POINT = 30;

		[SerializeField] private SpriteRenderer _background;
		[SerializeField] private Transform _foodStorage;
		[SerializeField] private Food _foodTemplete;
		[SerializeField] private ParticleSystem _skewerOnPlateVFX;

		private SoundManager _soundManager;
		private TutorialManager _tutorialManager;
		private LevelManager _levelManager;
		private BoosterManager _boosterManager;
		private UIManager _uIManager;

		private List<Food> _foodsInSkewer;
		private List<Food> _sameFoodsInSkewer = new List<Food>();
		private List<Vector2> _posFoods;
		private BoxCollider2D _boxColl;

		private Tween _tweenScale;
		private Tween _tweenMove;
		private Tween _tweenRotate;
		private Tween _tweenBackground;

		private Plate _plate;
		private Stove _stove;
		private SkewerData _skewerData;

		private int _inxSkewer;
		private int _slotCanUse;
		private int _numUndo;
		private bool _canSelect;
		private bool _isDone;
		private bool _onPlate;
		private bool _onTop;
		private bool _isDoneSpecial;
		private float _posTopFly;

		public int InxSkewer { get => _inxSkewer; set => _inxSkewer = value; }
		public bool CanSelect { get => _canSelect; set => _canSelect = value; }
		public bool OnTop { get => _onTop; set => _onTop = value; }
		public int NumUndo { get => _numUndo; set => _numUndo = value; }
		public bool IsDone { get => _isDone; set => _isDone = value; }
		public bool OnPlate { get => _onPlate; set => _onPlate = value; }

		void Awake()
		{
			_levelManager = LevelManager.Instance;
			_soundManager = SoundManager.Instance;
			_tutorialManager = TutorialManager.Instance;
			_boosterManager = BoosterManager.Instance;
			_uIManager = UIManager.Instance;

			_boxColl = GetComponent<BoxCollider2D>();
		}
		void Start()
		{
			_plate = _levelManager.Header.Plate;
		}

		public void SetupSkewer(int skewerFoodCount, SkewerData skewerData, Stove stove, int inx)
		{
			_slotCanUse = skewerFoodCount;
			this._stove = stove;
			this._inxSkewer = inx;
			this._skewerData = skewerData;

			SetupSizeSkewer(skewerFoodCount);
			CreateFood(skewerFoodCount);

			List<Func<Task>> actions = new List<Func<Task>>();

			actions.Add(() => SkewerAnimMove(Vector2.zero, Vector2.zero));
			actions.Add(() => FoodDrop());

			_ = RunSequence(actions);
		}
		void SetupSizeSkewer(int skewerFoodCount)
		{
			float skewerLen = skewerFoodCount * Consts.EXTRA_SKEWER_LENGTH + Consts.INIT_SKEWER_LENGTH;

			//* Xét kích thước của xiên nướng
			Vector2 size = _background.size;
			size.y = skewerLen;
			_background.size = size;

			//* Xét kích thước của boxCol
			_boxColl.size = new Vector2(_boxColl.size.x, size.y / Consts.SCALE_IMAGE);
		}
		void CreateFood(int skewerFoodCount)
		{
			_posTopFly = _levelManager.IsLevelSpecial ? POS_Y_INIT_FOOD_SPAWN_SPECIAL : POS_Y_INIT_FOOD_SPAWN;
			_foodsInSkewer = new List<Food>();
			for (int i = 0; i < _skewerData.foods.Length; i++)
			{
				if (_skewerData.foods[i] <= 0) continue;

				Food food = Instantiate(_foodTemplete, _foodStorage);

				bool isHide = false;
				if (_levelManager.CheckLvHidden() && i < _skewerData.foods.Length - 1)
					isHide = true;

				food.SetupFood(_skewerData.foods[i], isHide);
				AddFoodOnSkewer(food);
				SetParentStorage(food);
				food.transform.localPosition = new Vector3(0, _posTopFly, 0);
				food.gameObject.SetActive(false);
				food.SetOrderBackground(_slotCanUse + 1 - _foodsInSkewer.Count);
			}

			SetupInitPosFoodOnSkewer(skewerFoodCount);
		}

		void SetupInitPosFoodOnSkewer(int skewerFoodCount)
		{
			_posFoods = new List<Vector2>();

			//* Vị trí khởi đầu
			float yFoodBottomInSkewer = -(Mathf.Floor((skewerFoodCount - 1) / 2) * Consts.FOOD_SPACING_ON_SKEWER + (skewerFoodCount % 2 == 0 ? Consts.FOOD_SPACING_ROOT : 0));
			for (int i = 0; i < skewerFoodCount; i++)
			{
				Vector3 pos = new Vector3(0, i * Consts.FOOD_SPACING_ON_SKEWER + yFoodBottomInSkewer, 0);

				_posFoods.Add(pos);
			}
		}
		async Task RunSequence(List<Func<Task>> actions)
		{
			foreach (var act in actions)
			{
				await act();
			}
		}
		async Task SkewerScaleIn(Vector2 targetScale)
		{
			_tweenScale?.Kill();

			_tweenScale = this.transform.DOScale(new Vector3(targetScale.x, targetScale.y, 0), TIME_ANIM_SCALE)
									.SetEase(Ease.OutBack)
									.OnComplete(() => this.transform.localScale = new Vector3(targetScale.x, targetScale.y, 0));

			await _tweenScale.AsyncWaitForCompletion();
		}
		async Task SkewerScaleOut(Vector2 targetScale)
		{
			_tweenScale?.Kill();

			_tweenScale = this.transform.DOScale(Vector3.one, TIME_ANIM_SCALE)
										.SetEase(Ease.OutBack)
										.OnComplete(() => this.transform.localScale = Vector3.one);

			await _tweenScale.AsyncWaitForCompletion();
		}
		public async Task SkeweFade(float alpha, float time = TIME_ANIM_FADE)
		{
			_tweenBackground?.Kill();
			_tweenBackground = _background.DOFade(alpha, time);

			await _tweenBackground.AsyncWaitForCompletion();
		}
		async Task SkewerAnimMove(Vector2 targetPoint, Vector2 targetRotation)
		{
			_tweenMove?.Kill();
			_tweenRotate?.Kill();

			_tweenMove = transform.DOLocalMove(
				new Vector3(targetPoint.x, targetPoint.y, 0),
				TIME_ANIM_MOVE
			)
			.OnComplete(() => transform.localPosition = new Vector3(targetPoint.x, targetPoint.y, 0));

			_tweenRotate = transform.DOLocalRotate(
				new Vector3(targetRotation.x, targetRotation.y, 0),
				TIME_ANIM_ROTATE
			);

			await Task.WhenAll(
				_tweenMove.AsyncWaitForCompletion(),
				_tweenRotate.AsyncWaitForCompletion()
			);
		}
		public async Task SkewerAnimMoveInPlate(Vector2 targetPoint, bool undo)
		{
			_onPlate = true;

			// _stove.AfterSkewerDone();
			_tweenMove?.Kill();

			// await Task.Delay(400);
			// _uIManager.ShowTexFirer();
			_soundManager.PlayFoodMove();
			foreach (Food food in _foodsInSkewer)
				food.PlayCompleteInStove();

			await Task.Delay(600);

			Vector3[] path = GenerateParabola(this.transform.position, new Vector3(targetPoint.x, targetPoint.y, 0), targetPoint.y + HEIGHT_PARABOLA, SEGMENTS_POINT);

			_ = SkewerScaleOut(Vector2.one);
			_tweenMove = this.transform.DOPath(path, TIME_ANIM_FLY, PathType.CatmullRom, PathMode.Full3D, 30)
					 .SetUpdate(UpdateType.Fixed)
					 .SetEase(Ease.InOutSine)
					 .OnComplete(() =>
					 {
						 List<Func<Task>> actions = new List<Func<Task>>();

						 actions.Add(() => SkewerScaleIn(new Vector2(1.25f, 0.9f)));
						 actions.Add(() => SkewerScaleOut(Vector2.one));

						 _ = RunSequence(actions);

					 });
			await Task.Delay(Mathf.FloorToInt(TIME_ANIM_FLY * 1000 / 4));

			if (!undo)
				_ = _stove.CloseLid();

			_skewerOnPlateVFX?.Stop();
			_skewerOnPlateVFX?.Play();

			await _tweenMove.AsyncWaitForCompletion();
		}
		async Task FoodDrop()
		{
			for (int i = 0; i < _foodsInSkewer.Count; i++)
			{
				_foodsInSkewer[i].gameObject.SetActive(true);
				await _foodsInSkewer[i].FoodFade();
				await _foodsInSkewer[i].FoodMove(_posFoods[i]);
				for (int j = 0; j < _foodsInSkewer.Count; j++)
					_foodsInSkewer[j].SetScaleFood();
			}
			await Task.Delay(400);
			_levelManager.OpenSpecialLevel();

			if (!_tutorialManager.IsTutorial)
				_canSelect = true;
			else
				_canSelect = _tutorialManager.SetStoveTutorial(_inxSkewer);


			await Task.CompletedTask;
		}
		public void MouseDown()
		{
			if (!_canSelect || _isDone) return;

			_ = SkewerScaleIn(new Vector2(1.25f, 0.9f));
		}
		public void MouseUp()
		{
			Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);

			_ = SkewerScaleOut(Vector2.one);

			if (!_canSelect || _isDone) return;

			if (_boxColl.OverlapPoint(worldPoint))
			{
				_levelManager.CheckFoodValue();
			}
		}
		public async void SelectFood(int status, Skewer skewer = null)
		{
			_canSelect = false;
			if (skewer != null)
				skewer.CanSelect = false;

			switch (status)
			{
				case Consts.STATUS_ONTOP:
					await GetFoodOnTop()?.FoodMove(new Vector2(0, _posTopFly));

					_canSelect = true;
					_onTop = true;

					break;
				case Consts.STATUS_BACK:
					await GetFoodOnTop()?.FoodMove(_posFoods[_foodsInSkewer.Count - 1]);

					_canSelect = true;
					_onTop = false;

					break;

				case Consts.STATUS_FLY:
					_soundManager.PlayFoodMove();
					_onTop = false;

					_tweenMove?.Kill();

					await FoodFly(skewer);

					int maxFood = Math.Min(_sameFoodsInSkewer.Count, skewer.GetSlotEmpty());
					for (int i = 0; i < maxFood; i++)
					{
						_soundManager.PlayFoodMove();
						await SameFoodFly(skewer, (i == maxFood - 1) ? true : false);
					}
					await Task.Delay(Mathf.FloorToInt(TIME_ANIM_FLY_FOR_SAME_FOOD * 1000));

					_boosterManager.AddStep(this._inxSkewer, skewer.InxSkewer, maxFood);
					break;

				case Consts.STATUS_UNDO:
					// _soundManager.PlayFoodMove();
					_onTop = false;

					_tweenMove?.Kill();

					if (skewer.OnTop)
						skewer.SelectFood(Consts.STATUS_BACK);

					await GetFoodOnTop()?.FoodMove(new Vector2(0, _posTopFly));
					await FoodFly(skewer, true);

					for (int i = 0; i < _numUndo; i++)
					{
						// _soundManager.PlayFoodMove();
						await SameFoodFly(skewer, (i == _numUndo - 1) ? true : false);
					}
					await Task.Delay(Mathf.FloorToInt(TIME_ANIM_FLY_FOR_SAME_FOOD * 1000));
					_isDoneSpecial = false;

					if (_levelManager.IsLevelSpecial)
						_isDone = false;

					break;

				case Consts.STATUS_UNDO_FLY_PLATE:

					// _soundManager.PlayFoodMove();
					_onTop = false;

					_tweenMove?.Kill();

					Vector2 vec2 = new Vector2(_stove.SkewerStorage.position.x, _stove.SkewerStorage.position.y);
					this.transform.parent = _stove.SkewerStorage;

					await _stove.OpenLid();
					await SkewerAnimMoveInPlate(vec2, true);

					_plate.SubtractSkewerOnPlate();
					if (skewer.OnTop)
						skewer.SelectFood(Consts.STATUS_BACK);

					await GetFoodOnTop()?.FoodMove(new Vector2(0, _posTopFly));
					await FoodFly(skewer, true);

					for (int i = 0; i < _numUndo; i++)
					{
						// _soundManager.PlayFoodMove();
						await SameFoodFly(skewer, (i == _numUndo - 1) ? true : false);
					}
					await Task.Delay(Mathf.FloorToInt(TIME_ANIM_FLY_FOR_SAME_FOOD * 1000));
					_isDone = false;
					_onPlate = false;
					break;
			}

			if (skewer != null)
				skewer.CanSelect = true;

			if (_tutorialManager.IsTutorial)
				_tutorialManager.CheckStep();

		}
		async Task FoodMoveDownInSkewer(Food food)
		{
			await GetFoodOnTop()?.FoodMove(_posFoods[_foodsInSkewer.Count - 1]);

			food.SetOrderBackground(_slotCanUse + 1 - _foodsInSkewer.Count);

			for (int i = 0; i < _foodsInSkewer.Count; i++)
				_foodsInSkewer[i].SetScaleFood();

			food.PlayNewFoodInStove();

			if (CheckFullSameFood() && !_onPlate)
			{

				_isDone = true;
				if (_levelManager.IsLevelSpecial)
				{
					_isDoneSpecial = true;
					await CheckFullFood();
					_levelManager.WinLevel();
				}
				else
				{
					AddSkewerOnPlate();
				}
			}
		}
		public async Task CheckFullFood()
		{
			await Task.Delay(400);
			_uIManager.ShowTexFirer();
			_soundManager.PlayFoodMove();
			foreach (Food food in _foodsInSkewer)
				food.PlayCompleteInStove();
		}
		async Task FoodFly(Skewer skewer, bool undo = false)
		{
			Food foodOnTop = GetFoodOnTop();

			Vector2 worldPos2D = TransformPointLocalToWorld(skewer, skewer.GetPosSpawnFoodTopSkewer());

			skewer.AddFoodOnSkewer(foodOnTop);
			RemoveFoodTopOnSkewer();

			if (!CheckEmptySlotOnSkewer() && GetFoodOnTop().IsHide)
				_ = GetFoodOnTop().FoodOpenHidden();

			int maxFood = Math.Min(_sameFoodsInSkewer.Count, skewer.GetSlotEmpty());
			if (maxFood == 0 || (undo && _numUndo == 0))
				_canSelect = true;

			await foodOnTop?.FoodFly(worldPos2D);
			_ = skewer.FoodMoveDownInSkewer(foodOnTop);
			skewer.SetParentStorage(foodOnTop);
		}
		async Task SameFoodFly(Skewer skewer, bool isCheck)
		{
			Food foodOnTop = GetFoodOnTop();

			Vector2 worldPos2D = TransformPointLocalToWorld(skewer, skewer.GetPosSpawnFoodTopSkewer());

			await GetFoodOnTop()?.FoodMove(new Vector2(0, _posTopFly), TIME_ANIM_MOVE_FOR_SAME_FOOD);

			skewer.AddFoodOnSkewer(foodOnTop);
			RemoveFoodTopOnSkewer();

			if (!CheckEmptySlotOnSkewer() && GetFoodOnTop().IsHide)
				_ = GetFoodOnTop().FoodOpenHidden();

			if (isCheck)
				_canSelect = true;

			await foodOnTop?.FoodFly(worldPos2D, TIME_ANIM_FLY_FOR_SAME_FOOD);
			_ = skewer.FoodMoveDownInSkewer(foodOnTop);
			skewer.SetParentStorage(foodOnTop);
		}

		public Vector2 TransformPointLocalToWorld(Skewer skewer, Vector2 input)
		{
			Vector2 localPos2D = input;
			Vector3 localPos3D = new Vector3(localPos2D.x, localPos2D.y, 0);
			Vector3 worldPos3D = skewer.transform.TransformPoint(localPos3D);
			Vector2 worldPos2D = new Vector2(worldPos3D.x, worldPos3D.y);

			return worldPos2D;
		}
		public Vector2 InverseTransformPointWorldToLocal(Skewer skewer, Vector2 input)
		{
			Vector2 worldPos2D = input;
			Vector3 worldPos3D = new Vector3(worldPos2D.x, worldPos2D.y, 0);
			Vector3 localPos3D = skewer.transform.InverseTransformPoint(worldPos3D);
			Vector2 localPos2D = new Vector2(localPos3D.x, localPos3D.y);

			return localPos2D;
		}
		public void DeselectFood()
		{
			_ = GetFoodOnTop().FoodMove(new Vector2(0, _posTopFly));
		}
		void AddSkewerOnPlate()
		{
			_plate.AddSkewerOnPlate(this);
		}
		public void AddFoodOnSkewer(Food food)
		{
			_foodsInSkewer.Add(food);

			GetSameFood();
		}
		public void RemoveFoodTopOnSkewer()
		{
			GetSameFood();

			_foodsInSkewer.RemoveAt(_foodsInSkewer.Count - 1);
		}
		public void SetParentStorage(Food food)
		{
			food.transform.parent = _foodStorage;
			food.transform.localScale = Vector3.one;
		}
		public void GetSameFood()
		{
			_sameFoodsInSkewer.Clear();
			for (int i = _foodsInSkewer.Count - 1; i >= 0; i--)
			{
				if (_foodsInSkewer[i] == GetFoodOnTop()) continue;
				if (_foodsInSkewer[i].Value == GetFoodOnTop().Value && !_foodsInSkewer[i].IsHide)
					_sameFoodsInSkewer.Add(_foodsInSkewer[i]);
				else
					break;
			}
		}
		public bool IsOnPlate()
		{
			return _onPlate;
		}
		public bool CheckIsFullSpecial()
		{
			return _isDoneSpecial;
		}
		public bool CheckFullSameFood()
		{
			return _foodsInSkewer.Count == _slotCanUse && _sameFoodsInSkewer.Count == _slotCanUse - 1;
		}
		public int GetSlotEmpty()
		{
			return _slotCanUse - _foodsInSkewer.Count;
		}
		public bool CheckFullSlotOnSkewer()
		{
			return _foodsInSkewer.Count == _slotCanUse;
		}
		public bool CheckEmptySlotOnSkewer()
		{
			return _foodsInSkewer.Count == 0;
		}
		public Vector2 GetPosSpawnFoodTopSkewer()
		{
			return new Vector2(0, _posTopFly);
		}
		public Vector2 GetPosOnTop()
		{
			return CheckFullSlotOnSkewer() ? Vector2.zero : _posFoods[_foodsInSkewer.Count];
		}
		public List<Vector2> GetListPosFoods()
		{
			return _posFoods;
		}
		public List<Food> GetFoodsInSkewer()
		{
			return _foodsInSkewer;
		}
		public Food GetFoodOnTop()
		{
			if (_foodsInSkewer.Count > 0)
				return _foodsInSkewer[_foodsInSkewer.Count - 1];
			return null;
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
