using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.UI;


namespace FoodSort
{
	public class UIHeader : UICanvas
	{
		[SerializeField] private UIClick _uIClickSetting;
		[SerializeField] private UIClick _uIClickReplay;
		[SerializeField] private UIClick _uIClickReplaySpecial;
		[SerializeField] private UISettings _uISettings;
		[SerializeField] private TMP_Text _levelDisplay;
		[SerializeField] private GameObject _objLevel;
		[SerializeField] private GameObject _objLevelSpecial;
		[SerializeField] private TMP_Text _timerDisplay;
		[SerializeField] private Image _timerProgress;

		private float _levelMaxTime;

		void Awake()
		{
			LevelManager.Instance.OnLevelStart += SetLevelDisplay;
			LevelManager.Instance.OnTimerUpdate += SetTimerDisplay;
			LevelManager.Instance.OnTimerWarning += DoTimerWarning;
			_uIClickSetting.ActionAfterClick += OpenSetting;
			_uIClickReplay.ActionAfterClick += Replay;
			_uIClickReplaySpecial.ActionAfterClick += Replay;
		}
		void OpenSetting()
		{
			if (LevelManager.Instance.CheckTimeReplay())
				_uISettings.gameObject.SetActive(true);
		}
		void Replay()
		{
			LevelResult levelResult = LevelManager.Instance.levelResult;
			if (levelResult != null && LevelManager.Instance.CheckTimeReplay())
			{
				levelResult.duration = Time.time - levelResult.start_time;
				levelResult.action_name = "surrender";
				levelResult.result = "lose";

				AnalyticHandle.Instance.OnLevelFinish(levelResult);
			}
			;
			CheckAds();
		}
		public void CheckAds()
		{
			if (GameManager.Instance.OnAds && (PlayerPrefs.GetInt(Consts.LEVEL_SAVE, 1) >= AnalyticHandle.RemoteConfig.InterLevel))
			{
				MaxMediationController.instance.ShowInterstitial("_start_level", () => { LevelManager.Instance.Replay(); });
			}
			else
				LevelManager.Instance.Replay();
		}

		public void SetLevelDisplay(int level)
		{
			_objLevel.SetActive(!LevelManager.Instance.IsSpecialLevel);
			_objLevelSpecial.SetActive(LevelManager.Instance.IsSpecialLevel);
			if (LevelManager.Instance.IsSpecialLevel)
            {
				//_levelDisplay.text = $"Special Level ";
				_timerDisplay.color = Color.white;
				_timerProgress.fillAmount = 1;
				_levelMaxTime = LevelManager.Instance.LevelMaxTime;
            }
			else
			{
				_levelDisplay.text = $"Level {level}";
            }
		}

		public void SetTimerDisplay(int sec)
		{
			_timerDisplay.text = Utilities.GetStringMMSSFromInt(sec);
			_timerProgress.DOKill();
			_timerProgress.DOFillAmount(Mathf.Max(0f, (sec - 1) / _levelMaxTime), 1f).SetEase(Ease.Linear);
		}

		public void DoTimerWarning()
        {
			_timerDisplay.DOKill();
			_timerDisplay.DOColor(Color.red, 0.2f).From(Color.white).SetEase(Ease.OutCubic).SetLoops(2, LoopType.Yoyo);
        }
	}
}
