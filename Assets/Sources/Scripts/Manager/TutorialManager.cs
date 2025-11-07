using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace FoodSort
{
	public class TutorialManager : MonoBehaviour
	{
		public static TutorialManager Instance;
		private const float TIME_WAIT_HINT = 7;
		private List<string> _textTutorial = new List<string> {
			"<color=#FF9300>Tap</color> the Skewer",
			"Select a new skewer to insert food",
			"Only foods of the same type can be stacked on the same skewer."
			};

		[SerializeField] private GameObject _header;
		[SerializeField] private GameObject _tutorial;
		[SerializeField] private GameObject _tutorialExtra;
		[SerializeField] private TMP_Text _valueTutorial;
		[SerializeField] private Pointer _pointer;

		private LevelManager _levelManager;
		private GameManager _gameManager;

		private bool _isTutorial;
		private int _level;
		private int _step;
		private float _timeCheck;
		private bool _isShowHint;

		public bool IsTutorial { get => _isTutorial; set => _isTutorial = value; }

		void Awake()
		{
			if (Instance == null)
			{
				Instance = this;
			}

			_levelManager = LevelManager.Instance;
			_gameManager = GameManager.Instance;

			_levelManager.OnLevelStart += SetValueTutorial;
			_levelManager.OnLevelStart += ResetTimeCheck;
		}
		void Start()
		{
			// ResetTimeCheck();
		}
		public void ResetTimeCheck(int level)
		{
			this._level = level;
			if (level <= 2) return;
			_timeCheck = 0;
			_isShowHint = false;
			_pointer.gameObject.SetActive(false);
		}
		void Update()
		{
			if (_timeCheck > TIME_WAIT_HINT && !_isShowHint)
			{
				_isShowHint = true;
				_pointer.transform.position = _tutorialExtra.transform.position + new Vector3(0.5f, -0.1f, 0);
				_pointer.gameObject.SetActive(true);
			}
			if (!_isShowHint && !_gameManager.IsPauseGame)
				_timeCheck += Time.deltaTime;

			if (_gameManager.IsPauseGame && _level >= 2)
			{
				_pointer.gameObject.SetActive(false);
				_timeCheck = 0;
				_isShowHint = false;
			}
		}
		private void SetValueTutorial(int level)
		{
			if (level > 2) return;
			_header.SetActive(false);
			_isShowHint = true;
			this._level = level;

			_tutorial.SetActive(true);

			switch (level)
			{
				case 1:
					_isTutorial = true;
					SetValueTutorialText(0);
					break;
				case 2:
					SetValueTutorialText(2);
					break;
			}

			_levelManager.OnLevelStart -= SetValueTutorial;
		}
		public bool SetStoveTutorial(int inx)
		{
			if (inx == 1)
			{
				Vector3 vec3 = _levelManager.Stoves[1].SkewerOnStove.transform.position;
				SetTargetPosPoiter(vec3);
				return true;
			}
			return false;
		}
		public void SetTargetPosPoiter(Vector3 vector3)
		{
			GameObject pointer = _pointer.gameObject;
			pointer.SetActive(true);
			pointer.transform.position = vector3 + new Vector3(0.4f, -0.6f, 0);
			_ = _pointer.OnFade();
		}
		public async void CheckStep()
		{
			switch (_step)
			{
				case 0:
					SetValueTutorialText(1);
					_levelManager.Stoves[0].SkewerOnStove.CanSelect = false;
					_levelManager.Stoves[1].SkewerOnStove.CanSelect = false;
					await _pointer.OffFade();
					Vector3 vec3 = _levelManager.Stoves[0].SkewerOnStove.transform.position;
					SetTargetPosPoiter(vec3);
					await _pointer.OnFade();
					_levelManager.Stoves[0].SkewerOnStove.CanSelect = true;
					_levelManager.Stoves[1].SkewerOnStove.CanSelect = false;
					break;
				case 1:
					_ = _pointer.OffFade();
					// _tutorial.SetActive(false);
					break;
			}
			_step++;
		}
		private void SetValueTutorialText(int inx)
		{
			_valueTutorial.text = _textTutorial[inx];
		}
	}
}
