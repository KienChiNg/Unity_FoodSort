using AdjustSdk;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using FoodSort;
using UnityEngine;
using UnityEngine.Events;
using Zego;

public class MaxMediationController : MonoBehaviour
{
    public static MaxMediationController instance;

    private bool isBannerShowing;
    private EventBinding<MaxInitializedEvent> _maxInitializedEventBinding;
    private EventBinding<AdImpressionEvent> _adImpressionEventBinding;

    private bool doneAdsInterval = true;//Google policy that everyone has to follow
    private bool displayedBanner = false;
    private WaitForSeconds waitForAdsBreak = new WaitForSeconds(0.5f);
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;

            DontDestroyOnLoad(gameObject);
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnAdPaid;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnAdPaid;
            _maxInitializedEventBinding = new EventBinding<MaxInitializedEvent>(OnMaxInitializedEvent);
            _adImpressionEventBinding = new EventBinding<AdImpressionEvent>(OnAdImpressionEvent);
            EventBus<MaxInitializedEvent>.Register(_maxInitializedEventBinding);
            EventBus<AdImpressionEvent>.Register(_adImpressionEventBinding);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        if (!Config.IS_DEBUGGER_ADS)
        {
            Config.IS_DEBUGGER_ADS = true;
            MaxSdk.ShowMediationDebugger();
        }
        IAdService.Global.Init();
    }
    private void OnAdImpressionEvent(AdImpressionEvent @event)
    {
        if (@event.Format == EAdFormat.Interstitial)
            GameManager.Instance.CountInterAds++;
        if (@event.Format == EAdFormat.RewardedVideo)
            GameManager.Instance.CountRewardAds++;
    }
    private void OnAdPaid(string id, MaxSdkBase.AdInfo info)
    {
        AnalyticHandle.Instance.AdIncremental(info.Revenue);
    }
    private void OnMaxInitializedEvent(MaxInitializedEvent @event)
    {

    }
    #region Inter
    public void ShowInterstitial(string position, System.Action onClose)
    {
        if (doneAdsInterval)
        {
            if (IAdService.Global.IsInterstitialReady)
            {
                //Debug.Log("ad ready, show ad");
                doneAdsInterval = false;
                StartCoroutine(IEShowAdInter(onClose, position));
            }
            else
            {
                onClose.Invoke();
                AdStatic.RaiseAdShowFailNotReady(position, EAdFormat.Interstitial);
            }
        }
        else
        {
            onClose.Invoke();
        }
    }
    private IEnumerator IEShowAdInter(System.Action onClose, string position)
    {
        ShowAdsBreak();
        yield return waitForAdsBreak;
        onClose += SetAdsInterval;
        IAdService.Global.ShowInterstitial(position, onClose);
        HideAdsBreak();
    }
    public void SetAdsInterval()
    {
        doneAdsInterval = false;
        StartCoroutine(WaitEndAdsInterval());
    }
    IEnumerator WaitEndAdsInterval()
    {
        yield return new WaitForSecondsRealtime(AnalyticHandle.RemoteConfig.AdsInterval);
        doneAdsInterval = true;
    }
    #endregion
    #region Rewarded Ad Methods

    public void ShowRewardedAd(string position, System.Action onSuccess)
    {
        if (isRewardAdsReady())
        {
            onSuccess += () => StartCoroutine(WaitEndAdsInterval());
            IAdService.Global.ShowRewardedVideo(position, onSuccess);
        }
        else
        {
            GameManager.Instance.ShowMessage("Failed to load Reward Ad!");
            AdStatic.RaiseAdShowFailNotReady(position, EAdFormat.RewardedVideo);
        }
    }
    public void SetUpBanner()
    {
        if (AnalyticHandle.RemoteConfig.BannerEnable && LevelManager.Instance != null)
        {
            ShowBanner();
        }
        else
        {
            HideBanner();
        }
    }
    public bool isRewardAdsReady()
    => IAdService.Global.IsRewardedVideoReady;
    #endregion

    #region Open App Ad Methods
    #endregion

    #region Banner Ad Methods
    public void ToggleBannerVisibility()
    {
        if (!isBannerShowing)
        {
            HideBanner();
        }
        else
        {
            ShowBanner();
        }

        isBannerShowing = !isBannerShowing;
    }
    public void ShowBanner()
    {
        displayedBanner = true;
        IAdService.Global.ShowBanner();
    }
    public void HideBanner()
    {
        IAdService.Global.HideBanner();
    }
    #endregion

    #region Ads Break

    [SerializeField] private CanvasGroup _adsBreak;

    public bool DisplayedBanner { get => displayedBanner; set => displayedBanner = value; }

    private void ShowAdsBreak()
    {
        if (_adsBreak != null)
        {
            _adsBreak.blocksRaycasts = true;
            _adsBreak.DOFade(1, 0.2f);
        }
    }
    private void HideAdsBreak()
    {
        if (_adsBreak != null)
        {
            _adsBreak.blocksRaycasts = false;
            _adsBreak.DOFade(0, 0.2f);
        }
    }
    private void InstantHideAdsBreak()
    {
        if (_adsBreak != null)
        {
            _adsBreak.blocksRaycasts = false;
            _adsBreak.alpha = 0;
        }
    }

    #endregion Ads Break
}
