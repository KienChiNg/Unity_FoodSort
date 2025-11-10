using System;
using System.Collections.Generic;

#if UNITY_ANDROID
using Kelsey.AntiCheat.Genuine.Android;
#endif
using UnityEngine;
using Zego;

namespace FoodSort
{
	public class GameManager : Singleton<GameManager>, IAntiCheat
	{
		public List<AvatarSO> avatarSOs;
		private Loading _loading;
		private bool isChinaman = false;
		public bool IsCheater => isChinaman;

		public Action<int> OnCoinChange;

		public int inxProgess;
		private int _coin;
		private int _countRewardAds;
		private int _countInterAds;
		private int _countInterPerAds;
		private int _countLevelFeedback = 1;
		private bool _onAds;
		private bool _isPauseGame = false;
		private bool _isSpecialLevel = false;
		private bool _breakSpecialLevel = false;

		public bool IsPauseGame { get => _isPauseGame; set => _isPauseGame = value; }
		public int Coin { get => _coin; set => _coin = value; }
		public bool OnAds { get => _onAds; set => _onAds = value; }
		public int CountInterAds { get => _countInterAds; set => _countInterAds = value; }
		public int CountRewardAds { get => _countRewardAds; set => _countRewardAds = value; }
		public int CountLevelFeedback { get => _countLevelFeedback; set => _countLevelFeedback = value; }
		public int CountInterPerAds { get => _countInterPerAds; set => _countInterPerAds = value; }
		public bool IsSpecialLevel { get => _isSpecialLevel; set => _isSpecialLevel = value; }
		public bool BreakSpecialLevel { get => _breakSpecialLevel; set => _breakSpecialLevel = value; }

		void Awake()
		{
			Application.targetFrameRate = 60;

			_loading = Loading.Instance;

			CheckCheat();
		}
		void Start()
		{
			if (PlayerPrefs.GetInt(Consts.LEVEL_SAVE, 1) == 1)
				_loading.LoadSceneWithLoading(Consts.SCENE_GAMEPLAY);
			else
				_loading.LoadSceneWithLoading(Consts.SCENE_HOME);

			_coin = PlayerPrefs.GetInt(Consts.COIN_FOODSORT, 100);
		}
		public void SetPauseGame(bool state)
		{
			_isPauseGame = state;
		}
		public void StateCoin(bool state, int coin)
		{
			_coin = state ? _coin + coin : _coin - coin;

			PlayerPrefs.SetInt(Consts.COIN_FOODSORT, _coin);
			PlayerPrefs.Save();

			OnCoinChange?.Invoke(_coin);
			AnalyticHandle.Instance.OnCoinChange(_coin, state ? "in" : "out");
		}
		public void CheckCheat()
		{
#if UNITY_ANDROID && !UNITY_EDITOR && !DEVELOPMENT_BUILD
        try
        {
            if (!AppInstallationSourceValidator.IsInstalledFromGooglePlay())
            {
                ShowMessage("Hmm, not a very good boi!!!");
                isChinaman = true;
            }
        }
        catch (Exception e)
        {
            ShowMessage("Hmm!!!");
        }
#endif
			ServiceLocator.Global.Register(typeof(IAntiCheat), this);
		}

		public void LogEventFirebase(string value) { }

		//Show message bang he thong
		public void ShowMessage(string msg)
		{
#if !UNITY_EDITOR && UNITY_ANDROID
        AndroidJavaObject @static = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject androidJavaObject = new AndroidJavaClass("android.widget.Toast");
        androidJavaObject.CallStatic<AndroidJavaObject>("makeText", new object[]
        {
                @static,
                msg,
                androidJavaObject.GetStatic<int>("LENGTH_SHORT")
        }).Call("show", Array.Empty<object>());
#elif UNITY_IOS
        // IOSControl.instance.ShowMessage(msg);
#endif
		}
	}
}
