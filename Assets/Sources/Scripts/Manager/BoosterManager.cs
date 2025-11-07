using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace FoodSort
{
	public class BoosterManager : MonoBehaviour
	{
		public static BoosterManager Instance; 
		[SerializeField] private UIClick _uIClickUndo;
		[SerializeField] private UIClick _uIClickExtra;
		[SerializeField] private TMP_Text _textNotice;
		[SerializeField] private TMP_Text _textUndo;
		[SerializeField] private TMP_Text _textExtra;
		[SerializeField] private Transform _textNoticeTF;
		[SerializeField] private GameObject _booster;
		[SerializeField] private GameObject _boosterBackstep;
		[SerializeField] private GameObject _boosterExtra;
		private Plate _plate;

		private LevelManager _levelManager;

		private Tween _tweenText;

		private int _numOfUndo;
		private int _numOfExtra;

		private Stack<KeyValuePair<int, int>> _pairUndo = new Stack<KeyValuePair<int, int>>();
		private Stack<int> _foodNumUndo = new Stack<int>();

		public int NumOfUndo { get => _numOfUndo; set => _numOfUndo = value; }
		public int NumOfExtra { get => _numOfExtra; set => _numOfExtra = value; }

		void Awake()
		{
			if (Instance == null)
			{
				Instance = this;
			}

			_levelManager = LevelManager.Instance;

			_uIClickUndo.ActionAfterClick += UndoBooster;
			_uIClickExtra.ActionAfterClick += ExtraStoveBooster;

			_levelManager.OnLevelStart += CheckBooster;
		}
		void Start()
		{
			_plate = _levelManager.Header.Plate;
		}
		void CheckBooster(int level)
		{
			if (level > 2)
			{
				_booster.SetActive(true);

				_numOfUndo = PlayerPrefs.GetInt(Consts.UNDO_FOODSORT, 3);
				_numOfExtra = PlayerPrefs.GetInt(Consts.EXTRA_FOODSORT, 1);

				SetUndoText(_numOfUndo);
				SetExtraText(_numOfExtra);
			}
		}
		public void StateUndoBooster(bool state)
		{
			if (!state && LevelManager.Instance.levelResult != null)
				LevelManager.Instance.levelResult.is_use_booster = 1;

			_numOfUndo = !state ? _numOfUndo - 1 : _numOfUndo + 3;

			PlayerPrefs.SetInt(Consts.UNDO_FOODSORT, _numOfUndo);
			PlayerPrefs.Save();

			SetUndoText(_numOfUndo);

			AnalyticHandle.Instance.OnUseBooster("undo_booster", state ? "in" : "out", _numOfUndo, LevelManager.Instance.LevelDisplay);
		}
		public void StateExtraBooster(bool state)
		{
			if (!state && LevelManager.Instance.levelResult != null)
				LevelManager.Instance.levelResult.is_use_booster = 1;

			_numOfExtra = !state ? _numOfExtra - 1 : _numOfExtra + 1;

			PlayerPrefs.SetInt(Consts.EXTRA_FOODSORT, _numOfExtra);
			PlayerPrefs.Save();

			SetExtraText(_numOfExtra);
			AnalyticHandle.Instance.OnUseBooster("extra_skewer_booster", state ? "in" : "out", _numOfExtra, LevelManager.Instance.LevelDisplay);
		}
		public void AddStep(int inxOut, int inxIn, int numFood)
		{
			KeyValuePair<int, int> pair = new KeyValuePair<int, int>(inxOut, inxIn);

			_pairUndo.Push(pair);
			_foodNumUndo.Push(numFood);
		}
		public void UndoBooster()
		{
			if (!CheckUndo()) return;

			if (_numOfUndo <= 0)
			{
				_boosterBackstep.SetActive(true);
				return;
			}

			if (_pairUndo.Count <= 0)
			{
				SetAnimTextNotice("No moves to use Backstep yet");
				return;
			}

			KeyValuePair<int, int> pair = _pairUndo.Pop();

			List<Stove> stoves = _levelManager.Stoves;

			Skewer skewerOut = stoves[pair.Key].SkewerOnStove;
			Skewer skewerIn = stoves[pair.Value].SkewerOnStove;

			int status = skewerIn.CheckFullSameFood() ? _levelManager.IsLevelSpecial ? Consts.STATUS_UNDO : Consts.STATUS_UNDO_FLY_PLATE : Consts.STATUS_UNDO;


			skewerIn.gameObject.SetActive(true);
			if (skewerIn.OnTop) _levelManager.ResetSkewerSelect();
			skewerIn.NumUndo = _foodNumUndo.Pop();
			skewerIn.SelectFood(status, skewerOut);

			StateUndoBooster(false);
		}
		public void ExtraStoveBooster()
		{
			if (_levelManager.CheckMaxStove())
			{
				SetAnimTextNotice("Maximum Extra Stoves reached for this level");

				return;
			}

			if (_numOfExtra <= 0)
			{
				_boosterExtra.SetActive(true);

				return;
			}

			TutorialManager.Instance.ResetTimeCheck(LevelManager.Instance.LevelDisplay); 

			_levelManager.AddStove();

			StateExtraBooster(false);
		}
		public void SetAnimTextNotice(string value)
		{
			_tweenText?.Kill();
			_textNotice.text = value;
			_textNoticeTF.gameObject.SetActive(true);
			_textNoticeTF.localScale = Vector3.zero;
			_tweenText = _textNoticeTF.DOScale(Vector3.one, 0.5f)
				.SetEase(Ease.OutBack)
				.OnComplete(() =>
				{
					_tweenText = DOVirtual.DelayedCall(0.5f, () =>
					{
						_tweenText = _textNoticeTF.DOScale(Vector3.zero, 0.5f)
							.SetEase(Ease.InBack)
							.OnComplete(() =>
							{
								_textNoticeTF.localScale = Vector3.one;
								_textNoticeTF.gameObject.SetActive(false);
							});

					});
				});
		}
		bool CheckUndo()
		{
			for (int i = 0; i < _levelManager.Stoves.Count; i++)
			{
				Skewer skewer = _levelManager.Stoves[i].SkewerOnStove;
				if (!_plate.CheckUndo()) return false;
				// if (skewer.IsOnPlate()) continue;
				if (!skewer.CanSelect) return false;
			}
			return true;
		}
		public void SetUndoText(int num)
		{
			_textUndo.text = num != 0 ? num.ToString() : "+";
		}
		public void SetExtraText(int num)
		{
			_textExtra.text = num != 0 ? num.ToString() : "+";
		}
	}
}
