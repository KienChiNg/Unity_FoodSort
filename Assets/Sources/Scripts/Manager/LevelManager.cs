using System;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.Linq;
using Random = UnityEngine.Random;
using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine.UI;

namespace FoodSort
{
	public class LevelResult
	{
		public int step;
		public int level = 0;
		public string result;
		public float duration;
		public string action_name;
		public int is_use_booster = 0;
		public int is_view_ads = 0;
		public float startTime;
	}
	public class LevelManager : MonoBehaviour
	{
		private const float TIME_DEFAULT_ANIM_LOADING = 2f;
		public static LevelManager Instance;

		private const int MAX_STOVE_SMOKE_LIGHT = 3;
		private const int TIME_ANIM_MOVE_STOVE_COVER = 1000;
		private const int TIME_WAIT_REPLAY = 3;
		private const float TIME_ANIM_SCALE = 0.2f;
		private const float STOVE_STORAGE_SCALE_INIT = 1f;

		[SerializeField] private Transform _stoveStorage;
		[SerializeField] private Stove _stoveTemplete;
		[SerializeField] private Header _header;
		[SerializeField] private UIWinGame _uIWinGame;
		[SerializeField] private UIRateUs _uIRateUs;
		[SerializeField] private UISpecialLevel _uISpecialLevel;
		[SerializeField] private UnlockFoodSO _unlockFoodSO;
		[SerializeField] private List<FoodData> _foodSOList;
		[SerializeField] private List<GameObject> _backGround;

		#region LOADING
		[SerializeField] private GameObject _loading;
		[SerializeField] private Transform _iconLoading;
		[SerializeField] private LoadingAnim _loadingAnim;
		[SerializeField] private Image _backgroundLoading;
		#endregion


		public Action<int> OnLevelStart;
		public Action<int> OnNumStove;

		private SoundManager _soundManager;
		private GameManager _gameManager;
		private UIManager _uIManager;

		private int _levelDisplay;
		private int _inxUnlock;
		private int _levelDisplaySpecial;
		private int _valueUnlockNewFood;
		private int _nextUnlockNewFood;
		private int _stoveSmokeLight;
		private int _skewerHaveFood;
		private int _skewerCanSelect;
		private int _skekwerFoodCount;
		private bool _isUnlockNewFood;
		private float _currentTime;
		private bool _isLevelSpecial;
		private bool _maxFood;

		private LevelData _levelData;
		public LevelResult levelResult;

		private Skewer _skewerSelect;
		private Skewer _skewerSelectOut;

		private List<Stove> _stoves = new List<Stove>();
		private List<FoodData> _foodHave = new List<FoodData>();

		private Tween _tweenScale;
		private Tween _tweenFade;
		private Tween _tweenScaleLoading;

		public LevelData LevelData { get => _levelData; }
		public Header Header { get => _header; set => _header = value; }
		public List<Stove> Stoves { get => _stoves; set => _stoves = value; }
		public int StoveSmokeLight { get => _stoveSmokeLight; set => _stoveSmokeLight = value; }
		public int SkewerHaveFood { get => _skewerHaveFood; set => _skewerHaveFood = value; }
		public int LevelDisplay { get => _levelDisplay; set => _levelDisplay = value; }
		public bool IsLevelSpecial { get => _isLevelSpecial; set => _isLevelSpecial = value; }
		public int SkekwerFoodCount { get => _skekwerFoodCount; set => _skekwerFoodCount = value; }
		public int ValueUnlockNewFood { get => _valueUnlockNewFood; set => _valueUnlockNewFood = value; }
		public int NextUnlockNewFood { get => _nextUnlockNewFood; set => _nextUnlockNewFood = value; }
		public UnlockFoodSO UnlockFoodSO { get => _unlockFoodSO; set => _unlockFoodSO = value; }
		public int InxUnlock { get => _inxUnlock; set => _inxUnlock = value; }
		public bool MaxFood { get => _maxFood; set => _maxFood = value; }
#if UNITY_EDITOR
		[Button("Food Checker")]
		private void FoodChecker()
		{
			foreach (var foodSOLst in _foodSOList)
			{
				for (int i = 0; i < foodSOLst.foodSOs.Count;)
				{
					if (foodSOLst.foodSOs[i] == null)
					{
						foodSOLst.foodSOs.RemoveAt(i);
						Debug.LogError("Tron");
					}
					else i++;
				}
			}
		}
#endif
		void Awake()
		{
			if (Instance == null)
			{
				Instance = this;
			}

			_soundManager = SoundManager.Instance;
			_gameManager = GameManager.Instance;
			_uIManager = UIManager.Instance;

		}
		void Start()
		{

			StartGame();

			_currentTime = Time.time;
			MaxMediationController.instance.SetUpBanner();

			if (_isLevelSpecial) return;

			// if (!_gameManager.IsSpecialLevel
			// 		&& !_gameManager.BreakSpecialLevel
			// 		&& _levelDisplay > Consts.LEVEL_SPECIAL_SPACE
			// 		&& (_levelDisplay - 1) % Consts.LEVEL_SPECIAL_SPACE == 0) return;

			if (_gameManager.CountLevelFeedback >= Consts.LEVEL_RATE_US && PlayerPrefs.GetInt("Feedback", 0) == 0)
			{
				_uIRateUs.gameObject.SetActive(true);
				_gameManager.CountLevelFeedback = 1;
				return;
			}
		}
		void StartGame()
		{
			SoundManager.Instance.Play(Consts.SCENE_GAMEPLAY);
			_gameManager.IsPauseGame = false;
			_soundManager.PlaySceneStart();
			CreateLevel();
		}
		private void CheckNewFood()
		{
			Dictionary<int, List<FoodSO>> keyValuePairsFood = new Dictionary<int, List<FoodSO>>();
			List<UnlockLevelFoodSO> unlockLevelFoodSOs = _unlockFoodSO.unlockLevelFoodSOs;

			unlockLevelFoodSOs = unlockLevelFoodSOs.OrderBy(f => f.lvUnlock).ToList();
			_inxUnlock = unlockLevelFoodSOs.Count - 1;
			_valueUnlockNewFood = 0;
			for (int i = 0; i < unlockLevelFoodSOs.Count; i++)
			{
				UnlockLevelFoodSO unlockFood = unlockLevelFoodSOs[i];
				FoodSO foodSO = unlockFood.foodSO;
				if (_levelDisplay <= unlockLevelFoodSOs[i].lvUnlock)
				{
					if (_levelDisplay - 1 == unlockLevelFoodSOs[i - 1].lvUnlock)
						_isUnlockNewFood = true;

					_inxUnlock = i - 1;
					break;
				}
				if (!keyValuePairsFood.ContainsKey(foodSO.typeFood))
				{
					keyValuePairsFood[foodSO.typeFood] = new List<FoodSO>();
				}
				keyValuePairsFood[foodSO.typeFood].Add(foodSO);
			}

			if (_inxUnlock < 0) return;

			if (_inxUnlock == unlockLevelFoodSOs.Count - 1)
				_maxFood = true;
			else
			{
				_valueUnlockNewFood = unlockLevelFoodSOs[_inxUnlock].lvUnlock;
				_nextUnlockNewFood = unlockLevelFoodSOs[_inxUnlock + 1].lvUnlock;
			}

			PlayerPrefs.SetInt(Consts.LEVEL_NEW_FOOD, _inxUnlock);
			PlayerPrefs.Save();

			foreach (KeyValuePair<int, List<FoodSO>> pair in keyValuePairsFood)
			{
				FoodData foodData = new FoodData();
				List<FoodSO> foodSOs = new List<FoodSO>();
				foreach (FoodSO food in pair.Value)
				{
					foodSOs.Add(food);
				}
				foodData.foodSOs = foodSOs;
				_foodHave.Add(foodData);
			}
		}
		private int CalculateLevel(int rawLevel, int levelMax, int levelLoop)
		{
			if (rawLevel <= levelMax)
				return rawLevel;

			int loopCount = rawLevel - levelMax;
			return levelLoop + (loopCount - 1) % (levelMax - levelLoop + 1);
		}
		void SetBackground(int level)
		{
			int inx = GameManager.Instance.GetAvatarInx(level);
			for (int i = 0; i < _backGround.Count; i++)
				_backGround[i].SetActive(false);

			if (_backGround.Count - 1 < inx)
				_backGround[_backGround.Count - 1].SetActive(true);
			else
				_backGround[inx].SetActive(true);
		}
		void CreateLevel()
		{
			// _isLevelSpecial = false;
			_levelDisplay = PlayerPrefs.GetInt(Consts.LEVEL_SAVE, 1);
			// _levelDisplay = 1500;

			CheckNewFood();
			SetBackground(_levelDisplay);
			// _levelDisplaySpecial = PlayerPrefs.GetInt(Consts.LEVEL_SAVE_SPECIAL, 1);

			// if (_gameManager.IsSpecialLevel)
			// 	_isLevelSpecial = true;

			TextAsset[] allTextAsset = Resources.LoadAll<TextAsset>($"Data");

			int realLv = CalculateLevel(_levelDisplay, 1000, 50);

			// if (_isLevelSpecial)
			// {
			// 	allTextAsset = Resources.LoadAll<TextAsset>($"DataSpecial");
			// 	realLv = CalculateLevel(_levelDisplaySpecial, 2596, 100);
			// 	_uIManager.SetupSpecial();

			// 	StartAnalytic();
			// }

			// for (int i = 0; i < allTextAsset.Length; i++)
			// {
			// 	TextAsset jsonFile222 = allTextAsset[i];
			// 	if (jsonFile222 != null)
			// 	{
			// 		var rootNode = JSON.Parse(jsonFile222.text);
			// 		JSONArray levels = rootNode["levels"].AsArray;

			// 		for (int j = 0; j < levels.Count; j++)
			// 		{
			// 			JSONArray foodDatas = levels[j]["tubes"].AsArray;
			// 			Debug.Log((j + 1) + " " +foodDatas.Count);

			// _levelData = new LevelData();
			// StoveData[] stoveDatas = new StoveData[foodDatas.Count];
			// for (int k = 0; k < foodDatas.Count; k++)
			// {
			// 	JSONArray skewer = foodDatas[k].AsArray;

			// 	SkewerData skewerData = new SkewerData();

			// 	int[] foods = new int[skewer.Count];

			// 	for (int h = 0; h < skewer.Count; h++)
			// 	{
			// 		foods[h] = skewer[h];

			// 		if (foods[h] > 0) _skewerHaveFood++;
			// 	}
			// 	skewerData.foods = foods;
			// 	stoveDatas[k] = new StoveData();
			// 	stoveDatas[k].skewerData = skewerData;
			// }
			// _levelData.stoveDatas = stoveDatas;
			// 		}
			// 	}
			// }

			TextAsset jsonFile = allTextAsset[Mathf.FloorToInt((float)(realLv - 1) / 100)];

			if (jsonFile != null)
			{
				var rootNode = JSON.Parse(jsonFile.text);
				JSONArray levels = rootNode["levels"].AsArray;

				var level = levels[(realLv - 1) % 100];
				JSONArray skewerDatas = level["tubes"].AsArray;

				_levelData = new LevelData();
				StoveData[] stoveDatas = new StoveData[skewerDatas.Count];

				for (int i = 0; i < skewerDatas.Count; i++)
				{
					JSONArray skewer = skewerDatas[i].AsArray;

					SkewerData skewerData = new SkewerData();

					int[] foods = new int[skewer.Count];

					for (int j = 0; j < skewer.Count; j++)
					{
						foods[j] = skewer[j];

						if (foods[j] > 0) _skewerHaveFood++;
					}
					skewerData.foods = foods;
					stoveDatas[i] = new StoveData();
					stoveDatas[i].skewerData = skewerData;
				}
				_levelData.stoveDatas = stoveDatas;

				_skekwerFoodCount = CountSameFood(_levelData);
				RandomSprite(_levelData);
				CreateStove(_levelData.stoveDatas.Length);

				_skewerHaveFood /= _skekwerFoodCount;
				OnLevelStart?.Invoke(_levelDisplay);
				// OnNumStove?.Invoke(_skewerHaveFood);
				_header.Plate.SpritePointsOnX(_skewerHaveFood);


				levelResult = new LevelResult();
				levelResult.step = 0;
				levelResult.startTime = Time.time;
				levelResult.level = _levelDisplay;
			}

			StartAnalytic();
		}
		public void StartAnalytic()
		{
			AnalyticHandle.Instance.OnLevelStart(_levelDisplay);
		}
		public void OpenSpecialLevel()
		{
			// _skewerCanSelect++;

			// if (_skewerCanSelect == _stoves.Count)
			// {
			// 	if (!_gameManager.IsSpecialLevel
			// 		&& !_gameManager.BreakSpecialLevel
			// 		&& _levelDisplay > Consts.LEVEL_SPECIAL_SPACE
			// 		&& (_levelDisplay - 1) % Consts.LEVEL_SPECIAL_SPACE == 0
			// 		)
			// 	{
			// 		// _isLevelSpecial = true;
			// 		_uISpecialLevel?.gameObject.SetActive(true);
			// 	}

			// 	if (_levelDisplay < Consts.LEVEL_SPECIAL_SPACE || (_levelDisplay - 1) % Consts.LEVEL_SPECIAL_SPACE != 0 || _gameManager.BreakSpecialLevel)
			// 		StartAnalytic();
			// }
		}
		public bool CheckMaxStove()
		{
			if (_isLevelSpecial)
				return _stoves.Count >= Consts.STOVE_MAX_SPECIAL;
			else
				return _stoves.Count >= Consts.STOVE_MAX;
		}
		public bool CheckLvHidden()
		{
			if (_levelDisplay >= Consts.LEVEL_HIDDEN_START && (_levelDisplay - Consts.LEVEL_HIDDEN_START) % Consts.LEVEL_HIDDEN_SPACE == 0 && !_isLevelSpecial)
				return true;
			return false;
		}
		int CountSameFood(LevelData levelData)
		{
			int count = 0;
			int inxFood = 0;

			for (int i = 0; i < levelData.stoveDatas.Length; i++)
			{
				int[] foodDatas = levelData.stoveDatas[i].skewerData.foods;
				for (int j = 0; j < foodDatas.Length; j++)
				{
					if (foodDatas[j] != 0)
					{
						inxFood = foodDatas[j];
						break;
					}
				}
				if (inxFood != 0) break;
			}
			for (int i = 0; i < levelData.stoveDatas.Length; i++)
			{
				int[] foodDatas = levelData.stoveDatas[i].skewerData.foods;
				for (int j = 0; j < foodDatas.Length; j++)
				{
					if (foodDatas[j] != 0 && inxFood <= 0) inxFood = foodDatas[j];
					if (foodDatas[j] == inxFood) count++;
				}
			}

			return count;
		}
		void RandomSprite(LevelData levelData)
		{
			int maxFood = _foodHave.Count;
			List<int> foodInx = new List<int>();
			List<int> foodInxQueue = new List<int>();
			List<int> queueInx = new List<int>();
			List<int> foodInxRand = new List<int>();
			List<int> foodInxInTypeRand = new List<int>();
			Dictionary<int, List<int>> foodCountInDicQueue = new Dictionary<int, List<int>>();
			Dictionary<int, int> foodCountInDic = new Dictionary<int, int>();
			Dictionary<int, int> foodConverInDic = new Dictionary<int, int>();

			for (int i = 0; i < maxFood; i++)
				foodInx.Add(i);

			foodInxRand = Shuffle(foodInx).ToList(); //* Ramdom loại thức ăn (theo 4 loại)

			for (int i = 0; i < levelData.stoveDatas.Length; i++) //*Lấy ra số lượng thức ăn khác nhau và số lượng của chúng
			{
				int[] foodDatas = levelData.stoveDatas[i].skewerData.foods;
				for (int j = foodDatas.Length - 1; j >= 0; j--)
				{
					if (foodDatas[j] == 0) continue;

					if (!foodCountInDic.ContainsKey(foodDatas[j]))
					{
						foodCountInDic[foodDatas[j]] = 0;
					}
					foodCountInDic[foodDatas[j]]++;
				}
			}
			int typeNewFood = _unlockFoodSO.unlockLevelFoodSOs[_inxUnlock].foodSO.typeFood;
			for (int i = 0; i < foodInxRand.Count; i++) //* Ramdom loại thức ăn cụ thể trong 4 loại đã được phân loại
			{
				foodInx.Clear();
				queueInx.Clear();
				foodInxQueue.Clear();

				for (int j = 0; j < _foodHave[foodInxRand[i]].foodSOs.Count; j++)
					foodInx.Add(j);

				if (foodInxRand[i] == typeNewFood && _isUnlockNewFood)//*KO shuffle để lấy ra giá trị topping mới nhất
					foodInxQueue = foodInx.ToList();
				else
					foodInxQueue = Shuffle(foodInx);

				for (int j = 0; j < foodInxQueue.Count; j++)
				{
					if (!foodCountInDicQueue.ContainsKey(foodInxRand[i]))
						foodCountInDicQueue[foodInxRand[i]] = new List<int>();
					foodCountInDicQueue[foodInxRand[i]].Add(foodInxQueue[j]);
				}

			}

			int count = 0;
			int countCheck = 0;
			for (int i = 0; i < foodCountInDic.Count; i++)//* Chuyển về inx cho list thay thế
			{
				int inxFood = 0;
				int rand = 0;
				if (i == 0 && _isUnlockNewFood)//*lấy ra giá trị cuối cùng là giá trị topping mới unlock 
				{
					// for (int j = 0; j < foodCountInDicQueue[typeNewFood].Count; j++)
					// {
					// 	Debug.Log(foodCountInDicQueue[typeNewFood][j]);
					// }
					rand = foodCountInDicQueue[typeNewFood].Count - 1;
					foodCountInDicQueue[typeNewFood].RemoveAt(foodCountInDicQueue[typeNewFood].Count - 1);
					inxFood = CalInxFood(typeNewFood, rand) + 1;
					// Debug.Log(typeNewFood + " " + rand + " " + inxFood);
				}
				else
				{
					countCheck = 0;
					while (true)
					{
						if (foodCountInDicQueue[foodInxRand[count]].Count > 0)
						{
							//125 205
							rand = foodCountInDicQueue[foodInxRand[count]][0];
							foodCountInDicQueue[foodInxRand[count]].RemoveAt(0);
							// Debug.Log(foodCountInDicQueue[foodInxRand[count]].Count);
							break;
						}
						else
						{
							count++;
							countCheck++;
						}

						if (count >= foodInxRand.Count) count = 0;
						if (countCheck >= foodInxRand.Count) break;
					}

					inxFood = CalInxFood(foodInxRand[count], rand) + 1;
					// Debug.Log(foodInxRand[count] + " " + rand + " " + inxFood);

					count++;
					if (count >= foodInxRand.Count) count = 0;
				}

				foodInxInTypeRand.Add(inxFood);
			}

			foodConverInDic = foodCountInDic;

			int type = 0;
			foreach (var key in foodCountInDic.Keys.ToList())
			{
				foodConverInDic[key] = foodInxInTypeRand[type];
				type++;
			}

			for (int i = 0; i < levelData.stoveDatas.Length; i++) //* Thay thể để random thức ăn
			{
				int[] foodDatas = levelData.stoveDatas[i].skewerData.foods;
				for (int j = 0; j < foodDatas.Length; j++)
				{
					if (foodDatas[j] == 0) continue;

					foodDatas[j] = foodConverInDic[foodDatas[j]];
				}
			}
		}
		public int CalInxFood(int type, int inx) //* Chuyển về index cụ thể
		{
			int count = 0;
			for (int i = 0; i < type; i++)
			{
				count += _foodHave[i].foodSOs.Count;
			}
			return count + inx;
		}
		public FoodSO GetFoodSO(int inxFood) //* Lấy ra hình ảnh thức ăn từ index cụ thể
		{
			int count = inxFood;
			int type = 0;
			while (true)
			{
				count -= _foodHave[type].foodSOs.Count;

				if (count <= 0) break;

				type++;
			}
			int inxFoodInType = _foodHave[type].foodSOs.Count + count - 1;

			return _foodHave[type].foodSOs[inxFoodInType];
		}
		public List<int> Shuffle(List<int> list)
		{
			int n = list.Count;
			while (n > 1)
			{
				n--;
				int k = UnityEngine.Random.Range(0, n);
				int value = list[k];
				list[k] = list[n];
				list[n] = value;
			}
			return list;
		}
		void CreateStove(int totalObjects)
		{
			int inx = 0;
			List<Vector2> vec2s = GetPosStove(totalObjects);

			foreach (Vector2 vec2 in vec2s)
			{
				float x = vec2.x;
				float y = vec2.y;

				Stove stove = Instantiate(_stoveTemplete, _stoveStorage);
				stove.SetupStove(new Vector2(x, y), inx, _levelData.stoveDatas[inx]);
				_stoves.Add(stove);

				inx++;
			}
		}
		public void AddStove(int numExtra = 1)
		{
			int stoveExtraAll = _stoves.Count + numExtra;

			if (stoveExtraAll > Consts.STOVE_MAX) return;

			int inx = 0;
			List<Vector2> vec2s = GetPosStove(stoveExtraAll);

			foreach (Vector2 vec2 in vec2s)
			{
				float x = vec2.x;
				float y = vec2.y;

				if (inx < _stoves.Count)
					_ = _stoves[inx].StoveMove(new Vector2(x, y));
				else
				{
					SkewerData skewerData = new SkewerData();

					int[] foods = new int[0];
					skewerData.foods = foods;
					StoveData stoveData = new StoveData();
					stoveData.skewerData = skewerData;

					Stove stove = Instantiate(_stoveTemplete, _stoveStorage);
					stove.SetupStove(new Vector2(x, y), inx, stoveData);
					_stoves.Add(stove);
				}
				inx++;
			}
		}
		List<Vector2> GetPosStove(int totalObjects)
		{
			List<Vector2> posList = new List<Vector2>();
			Vector3 center = _stoveStorage.position;

			List<int> objInRow = DivideObjects(totalObjects);

			//* Số hàng
			int rows = objInRow.Count;
			float valueExtraSpecial = _isLevelSpecial ? Consts.SCALE_STOVE_DELTA * rows : 0;

			float v3ScaleMax = Consts.SCALE_STOVE_DELTA * (Consts.MAX_PER_ROW - rows) + STOVE_STORAGE_SCALE_INIT - valueExtraSpecial * 1.6f;
			float spaceXDefault = Consts.SPACING_STOVE_X * v3ScaleMax + valueExtraSpecial;
			float spaceYDefault = Consts.SPACING_STOVE_Y * v3ScaleMax + (_isLevelSpecial ? 2f : 0) - valueExtraSpecial * 1.6f;

			StoveScale(new Vector3(v3ScaleMax, v3ScaleMax, v3ScaleMax));

			//* Tổng chiều cao grid
			float totalHeight = (rows - 1) * spaceYDefault;


			for (int row = 0; row < objInRow.Count; row++)
			{
				//* Số object trong hàng này
				int countInRow = objInRow[row];

				//* Tổng chiều rộng của hàng
				float totalWidth = (countInRow - 1) * spaceXDefault;

				//* X bắt đầu để căn giữa
				float startX = center.x - totalWidth / 2f;

				//* Y cho hàng này, căn giữa theo chiều dọc
				float startY = center.y + totalHeight / 2f - row * spaceYDefault;

				for (int i = 0; i < countInRow; i++)
				{
					float x = startX + i * spaceXDefault;
					float y = startY;

					posList.Add(new Vector2(x, y));
				}
			}


			return posList;
		}
		public List<int> DivideObjects(int total)
		{
			List<int> result = new List<int>();

			int maxPerRow = Consts.MAX_PER_ROW;

			if (total == 5)
				return new List<int> { 3, 2 };

			if (total < Consts.MAX_PER_ROW)
			{
				result.Add(total);
				return result;
			}

			int numRows = Mathf.CeilToInt(total / (float)maxPerRow);
			int remaining = total;

			for (int i = 0; i < numRows; i++)
			{
				int countThisRow = Mathf.CeilToInt(remaining / (float)(numRows - i));
				result.Add(countThisRow);
				remaining -= countThisRow;
			}

			return result;
		}
		void StoveScale(Vector3 targetScale)
		{
			_tweenScale?.Kill();

			_tweenScale = _stoveStorage.transform
									.DOScale(new Vector3(targetScale.x, targetScale.y, targetScale.z), TIME_ANIM_SCALE)
									.SetEase(Ease.Linear)
									.OnComplete(() => _stoveStorage.transform.localScale = new Vector3(targetScale.x, targetScale.y, 0));
		}
		void Update()
		{
			if (_gameManager.IsPauseGame) return;
			if (Input.GetMouseButtonDown(0))
			{
				Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
				LayerMask mask = LayerMask.GetMask(Consts.LAYER_SKEWER);

				GameObject hit = Physics2D.Raycast(worldPoint, Vector2.zero, Mathf.Infinity, mask).transform?.gameObject;

				if (hit == null) return;

				_skewerSelect = hit.GetComponent<Skewer>();

				_skewerSelect.MouseDown();
			}
			if (Input.GetMouseButtonUp(0))
			{
				_skewerSelect?.MouseUp();
			}
		}
		public void ResetSkewerSelect()
		{
			_skewerSelectOut = null;
		}
		public void CheckFoodValue()
		{
			TutorialManager.Instance.ResetTimeCheck(_levelDisplay);

			if (levelResult != null)
				levelResult.step++;

			if (_skewerSelect == _skewerSelectOut) //* 2 lựa chọn giống nhau
			{
				_soundManager.PlayFoodSelect();

				_skewerSelect.SelectFood(Consts.STATUS_BACK);
				_skewerSelectOut = null;
			}
			else //* 2 lựa chọn khác nhau
			{
				if (_skewerSelectOut) //* Đã có lựa chọn ban đầu
				{
					if (_skewerSelect.CheckEmptySlotOnSkewer()
					|| (_skewerSelect.GetFoodOnTop().Value == _skewerSelectOut.GetFoodOnTop().Value
					&& !_skewerSelect.CheckFullSlotOnSkewer())
					)//* Kiểm tra 2 food có giá trị bằng nhau hoặc xiên rỗng
					{
						_skewerSelectOut.SelectFood(Consts.STATUS_FLY, _skewerSelect);
						_skewerSelectOut = null;
					}
					else
					{
						_soundManager.PlayFoodSelect();

						_skewerSelect.SelectFood(Consts.STATUS_ONTOP);
						_skewerSelectOut.SelectFood(Consts.STATUS_BACK);

						_skewerSelectOut = _skewerSelect;
					}
				}
				else    //* Chưa có lựa chọn ban đầu
				{
					if (!_skewerSelect.CheckEmptySlotOnSkewer())
					{
						_soundManager.PlayFoodSelect();

						_skewerSelect.SelectFood(Consts.STATUS_ONTOP);
						_skewerSelectOut = _skewerSelect;
					}
				}
			}
		}
		public bool FullStoveSmokeLight()
		{
			return _stoveSmokeLight >= MAX_STOVE_SMOKE_LIGHT;
		}
		public void HandleNumStoveSmokeLight(bool state)
		{
			if (state && _stoveSmokeLight < MAX_STOVE_SMOKE_LIGHT)
				_stoveSmokeLight++;
			else if (!state && _stoveSmokeLight > 0)
				_stoveSmokeLight--;
		}
		public List<Skewer> GetSkewerDone()
		{
			List<Skewer> skewers = new List<Skewer>();
			for (int i = 0; i < _stoves.Count; i++)
			{
				Skewer skewer = _stoves[i].SkewerOnStove;
				if (skewer && skewer.CheckFullSameFood() && !skewer.IsOnPlate()) skewers.Add(skewer);
			}
			return skewers;
		}
		public bool CheckWin()
		{
			for (int i = 0; i < _stoves.Count; i++)
			{
				Skewer skewer = _stoves[i].SkewerOnStove;
				if (skewer.IsOnPlate()) continue;
				if (skewer.CheckEmptySlotOnSkewer()) continue;
				if (!_isLevelSpecial && skewer.GetFoodsInSkewer().Count > 0) return false;
				if (!skewer.CheckFullSameFood()) return false;
				if (_isLevelSpecial && !skewer.CheckIsFullSpecial()) return false;
			}
			return true;
		}
		public async void WinLevel()
		{
			if (CheckWin())
			{
				_soundManager.TurnOffMusicBackground(1);

				if (!_isLevelSpecial)
					for (int i = 0; i < _stoves.Count; i++)
					{
						_ = _stoves[i].AfterWinGame();
					}
				await Task.Delay(TIME_ANIM_MOVE_STOVE_COVER);

				OnLevelStart?.Invoke(_levelDisplay);

				_uIWinGame?.gameObject.SetActive(true);
				_uIWinGame.SetLevelDisplay(_levelDisplay);

				if (_isLevelSpecial)
				{
					PlayerPrefs.SetInt(Consts.LEVEL_SAVE_SPECIAL, ++_levelDisplaySpecial);
					_gameManager.IsSpecialLevel = false;
					_gameManager.BreakSpecialLevel = true;
				}
				else
				{
					PlayerPrefs.SetInt(Consts.LEVEL_SAVE, ++_levelDisplay);
					_gameManager.BreakSpecialLevel = false;
				}

				PlayerPrefs.Save();

				_soundManager.PlayWin();

				_gameManager.OnAds = true;
				_gameManager.CountLevelFeedback++;

				if (levelResult != null)
				{
					levelResult.duration = Time.time - levelResult.startTime;
					levelResult.action_name = "end";
					levelResult.result = "win";

					AnalyticHandle.Instance.OnLevelFinish(levelResult);
				}



				Debug.Log("WinGame");
			}
		}
		public bool CheckTimeReplay()
		{
			return Time.time - _currentTime >= TIME_WAIT_REPLAY;
		}
		public void Replay()
		{
			if (CheckTimeReplay())
			{
				_gameManager.IsPauseGame = true;
				_gameManager.OnAds = true;
				// DOTween.KillAll(complete: false, idsOrTargetsToExclude: Consts.GAME_NAME);
				// SceneManager.LoadScene(Consts.SCENE_GAMEPLAY);
				_ = LoadSceneWithLoading(Consts.SCENE_GAMEPLAY);
			}
		}
		public void BackToHome()
		{
			if (CheckTimeReplay())
			{
				_gameManager.OnAds = true;
				// DOTween.KillAll(complete: false, idsOrTargetsToExclude: Consts.GAME_NAME);
				// SceneManager.LoadScene(Consts.SCENE_GAMEPLAY);
				_ = LoadSceneWithLoading(Consts.SCENE_HOME);
			}
		}
		public async Task LoadSceneWithLoading(string sceneName)
		{
			_soundManager.TurnOffMusicBackground(1);
			_loading.SetActive(true);

			Color c = _backgroundLoading.color;
			c.a = 0;
			_backgroundLoading.color = c;

			_iconLoading.localScale = Vector3.zero;

			_tweenFade?.Kill();
			_tweenScale?.Kill();

			_tweenFade = _backgroundLoading.DOFade(1, 0.3f);
			_tweenScaleLoading = _iconLoading.DOScale(Vector3.one, 0.3f);

			await Task.WhenAll(
				_tweenFade.AsyncWaitForCompletion(),
				_tweenScaleLoading.AsyncWaitForCompletion()
			);
			StartCoroutine(LoadAsync(sceneName));
		}

		private IEnumerator LoadAsync(string sceneName)
		{
			AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

			operation.allowSceneActivation = false;

			while (!operation.isDone)
			{
				float progress = Mathf.Clamp01(operation.progress / 0.9f);

				if (progress >= 1f)
				{
					yield return new WaitForSeconds(TIME_DEFAULT_ANIM_LOADING);
					_loadingAnim?.ChangeStateAnim(false);
					// yield return new WaitForSeconds(TIME_STOP_ANIM_LOADING);
					operation.allowSceneActivation = true;
				}

				yield return null;
			}
		}
	}
}
