using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tidi.Ads
{
    public enum WatchRewardedVideoState
    {
        NONE, SKIP, SUCCEED
    }

    public class AdManager : Singleton<AdManager>
    {
        #region Constants
        protected const string PREFS_WATCHED_VIDEO_TODAY = "prefs_watched_video_today";
        protected const string PREFS_LAST_WATCHED_VIDEO_DATE = "prefs_last_watched_video_date";
        #endregion

        #region Events
        public delegate void OnWatchVideoReward(bool succeed);
        public OnWatchVideoReward onWatchVideoReward;

        public delegate void OnClosedInterstitial();
        public OnClosedInterstitial onClosedInterstitial;

        public delegate void OnAdsClicked();
        public OnAdsClicked onAdsClicked;
        #endregion

        #region Fields
        [Header("Settings")]
        [SerializeField]
        private AdSetting m_AdSetting;

        [SerializeField]
        private BaseAdUnitSetting[] m_AdUnitSettings;
        #endregion

        #region Properties
        private bool m_HasInitialized = false;
        public bool HasInitialized => m_HasInitialized;

        private Dictionary<AdProvider, IAdController> m_AdControllers;

        private bool m_ShowingRewardedVideo;
        private WatchRewardedVideoState m_WatchRewardedVideoState;

        private bool m_RequestingBanner;
        private Coroutine m_BannerCoroutine;

        private bool m_RequestingInterstitial;
        private Coroutine m_InterstitialCoroutine;

        private bool m_RequestingRewardedVideo;
        private Coroutine m_RewardedVideoCoroutine;
        #endregion

        #region Unity Events
        private void Update()
        {
            if (m_ShowingRewardedVideo)
            {
                if (m_WatchRewardedVideoState == WatchRewardedVideoState.NONE)
                    return;

                if (m_WatchRewardedVideoState == WatchRewardedVideoState.SUCCEED)
                {
                    if (onWatchVideoReward != null)
                        onWatchVideoReward(true);
                }
                else if (m_WatchRewardedVideoState == WatchRewardedVideoState.SKIP)
                {
                    if (onWatchVideoReward != null)
                        onWatchVideoReward(false);
                }

                m_ShowingRewardedVideo = false;
                m_WatchRewardedVideoState = WatchRewardedVideoState.NONE;

                RequestRewardedVideo();
            }
        }

        protected override void OnDestroy()
        {
            foreach (var pair in m_AdControllers)
            {
                pair.Value.Dispose();
            }

            base.OnDestroy();
        }

        private void OnApplicationPause(bool isPaused)
        {
            if (m_AdControllers == null)
                return;

            foreach (var pair in m_AdControllers)
            {
                pair.Value?.OnApplicationPause(isPaused);
            }
        }
        #endregion

        #region Methods
        public void Init()
        {
            m_AdControllers = new Dictionary<AdProvider, IAdController>();

#if USING_ADMOB
            Admob admob = new Admob();
            admob.Init(this, GetAdUnitSettingByProvider(AdProvider.Admob));
            m_AdControllers[AdProvider.Admob] = admob;
#endif

#if USING_UNITY_ADS
            UnityAds unity = new UnityAds();
            unity.Init(this, GetAdUnitSettingByProvider(AdProvider.UnityAds));
            m_AdControllers[AdProvider.UnityAds] = unity;
#endif

#if !UNITY_EDITOR && USING_FAN
            FAN fan = new FAN();
            fan.Init(this, GetAdUnitSettingByProvider(AdProvider.FacebookAudienceNetwork));
            m_AdControllers[AdProvider.FacebookAudienceNetwork] = fan;
#endif

#if USING_IRONSOURCE
            IronSourceAds ironSource = new IronSourceAds();
            ironSource.Init(this, GetAdUnitSettingByProvider(AdProvider.IronSource));
            m_AdControllers[AdProvider.IronSource] = ironSource;
#endif

            SetupRewardedVideoLimitProperties();

            RequestInterstitial();
            RequestRewardedVideo();

            m_RequestingBanner = false;
            m_RequestingInterstitial = false;
            m_RequestingRewardedVideo = false;

            m_HasInitialized = true;
        }

        private BaseAdUnitSetting GetAdUnitSettingByProvider(AdProvider provider)
        {
            for (int i = 0; i < m_AdUnitSettings.Length; i++)
            {
                if (m_AdUnitSettings[i].Provider == provider)
                    return m_AdUnitSettings[i];
            }

            return null;
        }

        #region Banner
        public void RequestBanner()
        {
            if (m_RequestingBanner)
                return;

            m_BannerCoroutine = StartCoroutine(DoRequestBanner());
        }

        private IEnumerator DoRequestBanner()
        {
            m_RequestingBanner = true;
            for (int i = 0; i < m_AdSetting.BannerAdPriority.Count; i++)
            {
                AdProvider provider = m_AdSetting.BannerAdPriority[i];
                if (m_AdControllers.ContainsKey(provider))
                {
                    bool adLoaded = false;
                    float requestTime = Time.realtimeSinceStartup;
                    m_AdControllers[provider].RequestBanner((_loaded) =>
                    {
                        adLoaded = _loaded;
                    });

                    while (!adLoaded && Time.realtimeSinceStartup - requestTime < 15f)
                        yield return null;

                    if (adLoaded)
                    {
                        ShowBanner();
                        break;
                    }
                }
            }

            m_RequestingBanner = false;
        }

        public void ShowBanner()
        {
            for (int i = 0; i < m_AdSetting.BannerAdPriority.Count; i++)
            {
                AdProvider provider = m_AdSetting.BannerAdPriority[i];
                if (m_AdControllers.ContainsKey(provider) && m_AdControllers[provider].IsBannerLoaded())
                {
                    m_AdControllers[provider].ShowBanner();
                    break;
                }
            }
        }

        public void HideBanner()
        {
            for (int i = 0; i < m_AdSetting.BannerAdPriority.Count; i++)
            {
                AdProvider provider = m_AdSetting.BannerAdPriority[i];
                if (m_AdControllers.ContainsKey(provider))
                {
                    m_AdControllers[provider].HideBanner();
                }
            }
        }

        public bool IsBannerLoaded()
        {
            bool hasLoaded = false;
            for (int i = 0; i < m_AdSetting.BannerAdPriority.Count; i++)
            {
                AdProvider provider = m_AdSetting.BannerAdPriority[i];
                if (m_AdControllers.ContainsKey(provider))
                {
                    hasLoaded = m_AdControllers[provider].IsBannerLoaded();
                }

                if (hasLoaded)
                    break;
            }

            return hasLoaded;
        }
        #endregion

        #region Interstitial
        public void RequestInterstitial()
        {
            if (m_RequestingInterstitial)
                return;

            m_InterstitialCoroutine = StartCoroutine(DoRequestInterstitial());
        }

        private IEnumerator DoRequestInterstitial()
        {
            m_RequestingInterstitial = true;
            for (int i = 0; i < m_AdSetting.InterstitialAdPriority.Count; i++)
            {
                AdProvider provider = m_AdSetting.InterstitialAdPriority[i];
                if (m_AdControllers.ContainsKey(provider))
                {
                    bool adLoaded = false;
                    float requestTime = Time.realtimeSinceStartup;
                    m_AdControllers[provider].RequestInterstitial((_loaded) =>
                    {
                        adLoaded = _loaded;
                    });

                    while (!adLoaded && Time.realtimeSinceStartup - requestTime < 15f)
                        yield return null;

                    if (adLoaded)
                        break;
                }
            }

            m_RequestingInterstitial = false;
        }

        public void ShowInterstitial()
        {
            for (int i = 0; i < m_AdSetting.InterstitialAdPriority.Count; i++)
            {
                AdProvider provider = m_AdSetting.InterstitialAdPriority[i];
                if (m_AdControllers.ContainsKey(provider) && m_AdControllers[provider].IsInterstitialLoaded())
                {
                    m_AdControllers[provider].ShowInterstitial();
                    break;
                }
            }
        }

        public bool IsInterstitialLoaded()
        {
            bool hasLoaded = false;
            for (int i = 0; i < m_AdSetting.InterstitialAdPriority.Count; i++)
            {
                AdProvider provider = m_AdSetting.InterstitialAdPriority[i];
                if (m_AdControllers.ContainsKey(provider))
                {
                    hasLoaded = m_AdControllers[provider].IsInterstitialLoaded();
                }

                if (hasLoaded)
                    break;
            }

            return hasLoaded;
        }
        #endregion

        #region Rewarded Video
        public void RequestRewardedVideo()
        {
            if (m_RequestingRewardedVideo)
                return;

            m_RewardedVideoCoroutine = StartCoroutine(DoRequestRewardedVideo());
        }

        private IEnumerator DoRequestRewardedVideo()
        {
            m_RequestingRewardedVideo = true;
            for (int i = 0; i < m_AdSetting.RewardedVideoAdPriority.Count; i++)
            {
                AdProvider provider = m_AdSetting.RewardedVideoAdPriority[i];
                if (m_AdControllers.ContainsKey(provider))
                {
                    float requestTime = Time.realtimeSinceStartup;
                    bool adLoaded = false;
                    m_AdControllers[provider].RequestRewardedVideo((_loaded) =>
                    {
                        adLoaded = _loaded;
                    });

                    while (!adLoaded && Time.realtimeSinceStartup - requestTime < 15f)
                        yield return null;

                    if (adLoaded)
                        break;
                }
            }

            m_RequestingRewardedVideo = false;
        }

        public void ShowRewardedVideo()
        {
            for (int i = 0; i < m_AdSetting.RewardedVideoAdPriority.Count; i++)
            {
                AdProvider provider = m_AdSetting.RewardedVideoAdPriority[i];
                
                if (DidReachRewardedVideoLimitPerDay(provider))
                    continue;

                if (m_AdControllers.ContainsKey(provider) && m_AdControllers[provider].IsRewardedVideoLoaded())
                {
                    string prefsKey = string.Format("{0}_{1}", PREFS_WATCHED_VIDEO_TODAY, GetPrefsKeyByProvider(provider));
                    int watchedVideoToday = PlayerPrefs.GetInt(prefsKey, 0);
                    PlayerPrefs.SetInt(prefsKey, ++watchedVideoToday);

                    m_ShowingRewardedVideo = true;
                    m_AdControllers[provider].ShowRewardedVideo();
                    break;
                }
            }
        }

        public bool IsRewardedVideoLoaded()
        {
            bool hasLoaded = false;

            for (int i = 0; i < m_AdSetting.RewardedVideoAdPriority.Count; i++)
            {
                AdProvider provider = m_AdSetting.RewardedVideoAdPriority[i];
                if (m_AdControllers.ContainsKey(provider))
                {
                    hasLoaded = m_AdControllers[provider].IsRewardedVideoLoaded();
                }

                if (hasLoaded)
                    break;
            }

            return hasLoaded;
        }

        private void SetupRewardedVideoLimitProperties()
        {
            string lastWatchedVideoDateString = PlayerPrefs.GetString(PREFS_LAST_WATCHED_VIDEO_DATE, "");
            System.DateTime lastWatchedVideoDate;
            if (string.IsNullOrEmpty(lastWatchedVideoDateString))
            {
                lastWatchedVideoDate = System.DateTime.Now;
            }
            else 
            {
                if (System.DateTime.TryParseExact(lastWatchedVideoDateString, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out lastWatchedVideoDate))
                {
                    // Successful parse date
                }
                else 
                {
                    lastWatchedVideoDate = System.DateTime.Now;
                }
            }
                
            bool isDiffDay = System.DateTime.Now.Subtract(lastWatchedVideoDate).TotalDays >= 1;

            if (isDiffDay || string.IsNullOrEmpty(lastWatchedVideoDateString))
            {
                PlayerPrefs.SetInt(string.Format("{0}_{1}", PREFS_WATCHED_VIDEO_TODAY, GetPrefsKeyByProvider(AdProvider.Admob)), 0);
                PlayerPrefs.SetInt(string.Format("{0}_{1}", PREFS_WATCHED_VIDEO_TODAY, GetPrefsKeyByProvider(AdProvider.UnityAds)), 0);
                PlayerPrefs.SetInt(string.Format("{0}_{1}", PREFS_WATCHED_VIDEO_TODAY, GetPrefsKeyByProvider(AdProvider.FacebookAudienceNetwork)), 0);
                PlayerPrefs.SetInt(string.Format("{0}_{1}", PREFS_WATCHED_VIDEO_TODAY, GetPrefsKeyByProvider(AdProvider.IronSource)), 0);
                PlayerPrefs.SetString(PREFS_LAST_WATCHED_VIDEO_DATE, System.DateTime.Now.ToString("dd/MM/yyyy"));
            }
        }

        private bool DidReachRewardedVideoLimitPerDay(AdProvider provider)
        {
            string prefsKey = string.Format("{0}_{1}", PREFS_WATCHED_VIDEO_TODAY, GetPrefsKeyByProvider(provider));
            int watchedVideoToday = PlayerPrefs.GetInt(prefsKey, 0);
            
            int rewardedVideoPerDay = m_AdSetting.RewardedVideoPerDay;

            return watchedVideoToday >= rewardedVideoPerDay;
        }

        private string GetPrefsKeyByProvider(AdProvider provider)
        {
            string prefsKey = "";
            if (provider == AdProvider.Admob)
                prefsKey = "admob";
            else if (provider == AdProvider.FacebookAudienceNetwork)
                prefsKey = "fan";
            else if (provider == AdProvider.UnityAds)
                prefsKey = "unity";
            else if (provider == AdProvider.IronSource)
                prefsKey = "iron";
            
            return prefsKey;
        }
        #endregion

        #region Callback
        public void HandleOnAdsClicked()
        {
            if (onAdsClicked != null)
                onAdsClicked();
        }

        public void HandleOnClosedInterstitial()
        {
            if (onClosedInterstitial != null)
                onClosedInterstitial();

            RequestInterstitial();
        }

        public void HandleOnWatchVideoReward(bool succeed)
        {
            m_WatchRewardedVideoState = succeed ? WatchRewardedVideoState.SUCCEED : WatchRewardedVideoState.SKIP;
        }
        #endregion
        #endregion
    }
}
