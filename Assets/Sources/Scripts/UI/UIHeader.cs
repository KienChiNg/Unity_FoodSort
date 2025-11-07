using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FoodSort
{
	public class UIHeader : UICanvas
	{
		[SerializeField] private UIClick _uIClickSetting;
		[SerializeField] private UIClick _uIClickReplay;
		[SerializeField] private UISettings _uISettings;
		[SerializeField] private TMP_Text _levelDisplay;

		void Awake()
		{
			LevelManager.Instance.OnLevelStart += SetLevelDisplay;
			_uIClickSetting.ActionAfterClick += OpenSetting;
			_uIClickReplay.ActionAfterClick += Replay;
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
				levelResult.duration = Time.time - levelResult.startTime;
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
			if (LevelManager.Instance.IsLevelSpecial)
				_levelDisplay.text = $"Special Level ";
			else
				_levelDisplay.text = $"Level {level}";
		}
	}
}
