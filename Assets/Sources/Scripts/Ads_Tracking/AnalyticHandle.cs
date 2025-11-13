using System.Collections;
using System.Collections.Generic;
using Firebase.Extensions;
using Firebase;
using Firebase.Analytics;
using UnityEngine;
using Zego;
using System.Threading.Tasks;
using System;



namespace FoodSort
{
    public class AnalyticHandle : Singleton<AnalyticHandle>, IFirebaseTracking
    {
        float[] thresholds = { 0.00f, 0.26f, 0.63f, 1.22f, 2.16f, 3.64f, 6.02f, 10.26f, 18.28f, 37.79f };
        float[] incrementalValues = { 0.04f, 0.36f, 0.47f, 0.74f, 1.17f, 1.84f, 3.09f, 5.65f, 11.52f, 36.22f };
        private const string FirstLoginKey = "FoodSort_FirstLoginDate";
        private const string TotalLTV = "FoodSort_TotalLTV";
        private const string IncrementLTV = "FoodSort_IncrementLTV";
        private const string IncrementLTVInx = "FoodSort_IncrementLTVInx";
        [SerializeField] private RemoteConfig remoteConfig;
        [SerializeField] private MaxMediationController adManager;
        FirebaseApp app;
        bool isReadyToUse = false;
        public static RemoteConfig RemoteConfig => Instance.remoteConfig;

        void Awake()
        {
            ServiceLocator.Global.Register(typeof(IRemoteConfig), remoteConfig);
            ServiceLocator.Global.Register(typeof(IFirebaseTracking), this);
        }
        public async Task InitializeFireBase()
        {
            await FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                var status = task.Result;
                if (status == DependencyStatus.Available)
                {
                    // Create and hold a reference to your FirebaseApp,
                    // where app is a Firebase.FirebaseApp property of your application class.
                    app = FirebaseApp.DefaultInstance;

                    // Set a flag here to indicate whether Firebase is ready to use by your app.
                    isReadyToUse = true;

                    //We should only fetch remote config values after the default Firebase App had been created
                    _ = remoteConfig.CheckRemoteConfigValues();
                }
                else
                {
                    Debug.LogError($"[FIREBASE CORE] Could not resolve all Firebase dependencies: {status}");
                    return;
                }
            });
        }
        public void OnCoinChange(int coin, string status)
        {
            // Debug.Log(coin + " " + status);
            FirebaseAnalytics.LogEvent(
                    "soft_currency",
                    status,
                    coin.ToString()
                );
            SetUpUserProperty(true, true, true, true, true, true, true, true, true);

        }
        public void OnLevelFinish(LevelResult levelResult)
        {
            if (GameManager.Instance.IsCheater || !isReadyToUse) return;
            // Debug.Log(levelResult.level.ToString().GetType());
            var levelFinishParameters = new[] {
            new Parameter("level", levelResult.level.ToString()),
            new Parameter("step", levelResult.step.ToString()),
            new Parameter("result", levelResult.result),
            new Parameter("duration", levelResult.duration.ToString()),
            new Parameter("action_type", "end"),
            new Parameter("action_name", levelResult.action_name),
            new Parameter("is_use_booster", levelResult.is_use_booster.ToString()),
            new Parameter("is_view_ads", levelResult.is_view_ads.ToString()),
        };

            if (LevelManager.Instance.IsLevelSpecial)
            {
                Debug.Log("level special: " + levelResult.level.ToString() + " ,duration: " + levelResult.duration.ToString() + " ,step: " + levelResult.step + " ,result: " + levelResult.result + " ,actionName: " + levelResult.action_name);

                FirebaseAnalytics.LogEvent("special_level", levelFinishParameters);
            }
            else
            {
                Debug.Log("level normal: " + levelResult.level.ToString() + " ,duration: " + levelResult.duration.ToString() + " ,step: " + levelResult.step + " ,result: " + levelResult.result + " ,actionName: " + levelResult.action_name + " ,is_use_booster" + levelResult.is_use_booster.ToString());

                FirebaseAnalytics.LogEvent("level_track", levelFinishParameters);
            }
            SetUpUserProperty(true, true, true, true, true, true, true, true, true);

        }
        public void OnLevelStart(int level)
        {
            if (GameManager.Instance.IsCheater || !isReadyToUse) return;
            // Debug.Log(SystemInfo.systemMemorySize);
            var levelStart = new[] {
            new Parameter("level", level.ToString()),
            new Parameter("action_type", "start"),
            new Parameter("action_name", "start"),
        };

            if (LevelManager.Instance.IsLevelSpecial)
            {
                Debug.Log("startLevelSpecial " + level);
                FirebaseAnalytics.LogEvent("special_level", levelStart);
            }
            else
            {
                Debug.Log("startLevelNormal " + level);

                FirebaseAnalytics.LogEvent("level_track", levelStart);
            }

            SetUpUserProperty(true, true, true, true, true, true, true, true, true);

        }
        public void OnUseBooster(string boosterName, string status, int value, int level)
        {
            if (GameManager.Instance.IsCheater || !isReadyToUse) return;
            // Debug.Log(boosterName + " " + status + " " + value + " " + level);
            var boosterParameters = new[] {
            new Parameter("booster_name", boosterName),
            new Parameter("action_name", status),
            new Parameter("placement", $"level_{level}"),
            new Parameter("value", value.ToString()),
        };
            FirebaseAnalytics.LogEvent("booster_track", boosterParameters);
            SetUpUserProperty(true, true, true, true, true, true, true, true, true);

        }

        public void OnAdsRevenueImpression(double rev, string networkName, string adUnit, string adFormat, string adPlacement)
        {
            if (GameManager.Instance.IsCheater || !isReadyToUse) return;
            // Debug.Log(rev + networkName + adUnit + adFormat + adPlacement);
            if (isReadyToUse)
            {
                var impressionParameters = new[] {
                        new Parameter("ad_platform", "AppLovin"),
                        new Parameter("ad_source", networkName),
                        new Parameter("action_name", "show"),
                        new Parameter("ad_unit_name", adUnit),
                        new Parameter("ad_format", adFormat),
                        new Parameter("value", rev),
                        new Parameter("currency", "USD"), // All AppLovin revenue is sent in USD
                        new Parameter("placement", adPlacement),
                        };

                FirebaseAnalytics.LogEvent("ad_track", impressionParameters);
            }

            SetUpUserProperty(true, true, true, true, true, true, true, true, true);
        }
        public bool CheckUnder7Day()
        {
            if (!PlayerPrefs.HasKey(FirstLoginKey))
            {
                PlayerPrefs.SetString(FirstLoginKey, DateTime.UtcNow.ToString());
                PlayerPrefs.Save();
            }

            DateTime firstLogin = DateTime.Parse(PlayerPrefs.GetString(FirstLoginKey));
            double daysActive = (DateTime.UtcNow - firstLogin).TotalDays;

            return daysActive < 7;
        }
        public int GetRevenueBin(float value)
        {
            int inx = thresholds.Length - 1;
            for (int i = 0; i < thresholds.Length; i++)
            {
                if (value < thresholds[i])
                {
                    inx = i - 1;
                    break;
                }
            }
            return inx;
        }
        public void AdIncremental(double value)
        {
            if (CheckUnder7Day())
            {
                float valueMoney = (float)value;
                float valueTotal = PlayerPrefs.GetFloat(TotalLTV, 0) + valueMoney;
                float incrementTotal = PlayerPrefs.GetFloat(IncrementLTV, 0) + valueMoney;

                float valueIncrementalValues = incrementalValues[PlayerPrefs.GetInt(IncrementLTVInx, 0)];
                // Debug.Log(value + " " + valueIncrementalValues + " " + incrementTotal + " " + valueTotal);
                if (incrementTotal >= valueIncrementalValues)
                {
                    FirebaseAnalytics.LogEvent("ad_incremental", new Parameter("value", valueIncrementalValues.ToString()));

                    // Debug.Log("Check");
                    PlayerPrefs.SetFloat(IncrementLTV, 0);
                    PlayerPrefs.SetInt(IncrementLTVInx, GetRevenueBin(valueTotal));
                }
                else
                    PlayerPrefs.SetFloat(IncrementLTV, incrementTotal);

                PlayerPrefs.SetFloat(TotalLTV, valueTotal);
                PlayerPrefs.Save();
            }
        }
        public void SetUpUserProperty(bool unlock_level, bool soft_currency_current, bool monet_reward_impression, bool monet_inter_impression, bool device_model, bool device_ram, bool device_capacity, bool device_chip, bool device_dpi)
        {
            if (GameManager.Instance.IsCheater || !isReadyToUse) return;
            // Debug.Log(Config.COUNT_REWARD_ADS + " " + Config.COUNT_INTER_ADS + " " + SystemInfo.systemMemorySize + " " + SystemInfo.deviceModel + " " + Screen.dpi);
            if (unlock_level)
                FirebaseAnalytics.SetUserProperty("unlock_level", $"{LevelManager.Instance.LevelDisplay}");
            if (soft_currency_current)
                FirebaseAnalytics.SetUserProperty("soft_currency_current", $"{GameManager.Instance.Coin}");
            if (monet_reward_impression)
                FirebaseAnalytics.SetUserProperty("monet_reward_impression", $"{GameManager.Instance.CountRewardAds}");
            if (monet_inter_impression)
                FirebaseAnalytics.SetUserProperty("monet_inter_impression", $"{GameManager.Instance.CountInterAds}");
            if (device_model)
                FirebaseAnalytics.SetUserProperty("device_model", $"{SystemInfo.deviceModel}");
            if (device_ram)
                FirebaseAnalytics.SetUserProperty("device_ram", $"{SystemInfo.systemMemorySize}");
            if (device_chip)
                FirebaseAnalytics.SetUserProperty("device_chip", $"{SystemInfo.processorType}");
            if (device_capacity)
                FirebaseAnalytics.SetUserProperty("device_capacity", $"{GetAndroidStorageCapacity()}");
            if (device_dpi)
                FirebaseAnalytics.SetUserProperty("device_dpi", $"{Screen.dpi}");

            // Debug.Log(GetAndroidStorageCapacity());
        }

        public void LogEvent(string eventName, Parameter[] parameters)
        {
            if (!isReadyToUse) return;
            FirebaseAnalytics.LogEvent(eventName, parameters);
        }

        public void SetUserProperty(string propertyName, string propertyValue)
        {
            if (!isReadyToUse) return;
            FirebaseAnalytics.SetUserProperty(propertyName, propertyValue);
        }
        public static long GetAndroidStorageCapacity()
        {
#if UNITY_ANDROID
            using AndroidJavaClass statFsClass = new AndroidJavaClass("android.os.StatFs");
            using AndroidJavaObject statFs =
                new AndroidJavaObject("android.os.StatFs", Application.persistentDataPath);
            long blockSize = statFs.Call<long>("getBlockSizeLong");
            long blockCount = statFs.Call<long>("getBlockCountLong");
            long totalBytes = blockSize * blockCount;
            var totalGb = totalBytes / (1024 * 1024 * 1024);
            // Debug.Log("Total Storage: " + totalGb + " GB");
            return totalGb;
#endif
            return 0;
        }
    }
}
