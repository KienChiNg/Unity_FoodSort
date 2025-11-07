using Firebase.Analytics;
using Firebase.Extensions;
using Firebase.RemoteConfig;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Zego;

public class RemoteConfig : MonoBehaviour, IRemoteConfig
{
    private int ads_interval = 60;
    private int inter_level = 6;
    private int inter_per_level = 1;
    private int banner_enable = 1;
    private bool isDataFetched = false;
    bool IRemoteConfig.IsDataFetched => isDataFetched;

    public int AdsInterval => ads_interval;
    public int InterLevel => inter_level;
    public int InterPerLevel => inter_per_level;
    public bool BannerEnable => banner_enable != 0;
    public async Task CheckRemoteConfigValues()
    {
        LoadData();
        Debug.Log("Fetching data...");
        var app = FirebaseRemoteConfig.DefaultInstance;
        Dictionary<string, object> defaults = new Dictionary<string, object>
            {
                { "ads_interval", ads_interval },
                { "inter_level", inter_level },
                { "inter_per_level", inter_per_level },
                { "banner_enable", banner_enable },
            };
        await app.SetDefaultsAsync(defaults);
        Task fetchTask = app.FetchAsync(TimeSpan.Zero);
        await fetchTask.ContinueWithOnMainThread(FetchComplete);
    }
    private void FetchComplete(Task fetchTask)
    {
        if (!fetchTask.IsCompleted)
        {
            Debug.LogError("Retrieval hasn't finished.");
            return;
        }

        var remoteConfig = FirebaseRemoteConfig.DefaultInstance;
        var info = remoteConfig.Info;
        if (info.LastFetchStatus != LastFetchStatus.Success)
        {
            Debug.LogError($"{nameof(FetchComplete)} was unsuccessful\n{nameof(info.LastFetchStatus)}: {info.LastFetchStatus}");
            return;
        }

        // Fetch successful. Parameter values must be activated to use.
        remoteConfig.ActivateAsync()
          .ContinueWithOnMainThread(
            task =>
            {
#if UNITY_EDITOR
                // Debug.Log($"Remote data loaded and ready for use. Last fetch time {info.FetchTime}.");

                // string configData = remoteConfig.GetValue("all_Game_data").StringValue;
                // Debug.Log("Total value: " + remoteConfig.AllValues.Count);
                // IDictionary<string, ConfigValue> allValues = FirebaseRemoteConfig.DefaultInstance.AllValues;

                // foreach (var kvp in allValues)
                // {
                //     Debug.Log($"Param Name: {kvp.Key} | Value: {kvp.Value.StringValue}");
                // }
#endif
                RefrectProperties();
            });
    }
    private void RefrectProperties()
    {
        var firebaseRemoteInstance = FirebaseRemoteConfig.DefaultInstance;
        ads_interval = (int)firebaseRemoteInstance.GetValue("ads_interval").DoubleValue;
        inter_level = (int)firebaseRemoteInstance.GetValue("inter_level").DoubleValue;
        inter_per_level = (int)firebaseRemoteInstance.GetValue("inter_per_level").DoubleValue;
        banner_enable = (int)firebaseRemoteInstance.GetValue("banner_enable").DoubleValue;
        SaveData();
        isDataFetched = true;
        MaxMediationController.instance.SetUpBanner();
    }
    // save cache data
    private void SaveData()
    {
        PlayerPrefs.SetInt("ads_interval", ads_interval);
        PlayerPrefs.SetInt("inter_level", inter_level);
        PlayerPrefs.SetInt("inter_per_level", inter_per_level);
        PlayerPrefs.SetInt("banner_enable", banner_enable);
    }

    //load cache data
    private void LoadData()
    {
        ads_interval = PlayerPrefs.GetInt("ads_interval", 60);
        inter_level = PlayerPrefs.GetInt("inter_level", 6);
        banner_enable = PlayerPrefs.GetInt("banner_enable", 1);
    }
    string IRemoteConfig.GetRemoteConfigData(string key, string defaultValue)
    {
        try
        {
            return FirebaseRemoteConfig.DefaultInstance.GetValue(key).StringValue;
        }
        catch
        {
            return defaultValue;
        }
    }
}
