using System;
using AudienceNetwork;
using UnityEngine;

namespace Tidi.Ads
{
    public class FAN : IAdController
    {
        #region Constants
        #endregion

        #region Events
        #endregion

        #region Fields
        #endregion

        #region Properties
        private AdView m_BannerAd;
        private InterstitialAd m_InterstitialAd;
        private RewardedVideoAd m_RewardedVideoAd;

        private bool m_IsBannerLoaded;
        private bool m_IsInterstitialLoaded;
        private bool m_InterstitialDidClose;
        private bool m_IsRewardedVideoLoaded;
        private bool m_RewardedVideoDidClose;
        private bool m_HasRewarded;

        private System.Action<bool> m_OnBannerLoaded;
        private System.Action<bool> m_OnInterstitialLoaded;
        private System.Action<bool> m_OnVideoRewardedLoaded;

        private AdManager m_Manager;
        private BaseAdUnitSetting m_Settings;
        #endregion

        #region Unity Events
        #endregion

        #region Methods
        public void Init(AdManager manager, BaseAdUnitSetting settings)
        {
            m_Manager = manager;
            m_Settings = settings;

            Logger.d(m_Settings.ProviderToString, "Is Initializing!");

            #if !UNITY_EDITOR
            AudienceNetworkAds.Initialize();
            #endif

            m_IsBannerLoaded = false;
            m_IsInterstitialLoaded = false;
            m_InterstitialDidClose = false;
            m_IsRewardedVideoLoaded = false;
            m_RewardedVideoDidClose = false;
            m_HasRewarded = false;
        }

        public void OnApplicationPause(bool isPaused)
        {
            
        }

        public void Dispose()
        {
            m_BannerAd?.Dispose();
            m_InterstitialAd?.Dispose();
            m_RewardedVideoAd?.Dispose();

            m_OnBannerLoaded = null;
            m_OnInterstitialLoaded = null;
            m_OnVideoRewardedLoaded = null;
        }

        #region Banner
        public void RequestBanner(Action<bool> onLoaded)
        {
            Logger.d(m_Settings.ProviderToString, "requesting banner!");
            m_OnBannerLoaded = onLoaded;

            if (IsBannerLoaded())
            {
                if (m_OnBannerLoaded != null)
                    m_OnBannerLoaded(true);
                return;
            }

            m_BannerAd?.Dispose();

            m_BannerAd = new AdView(m_Settings.BannerId, AdSize.BANNER_HEIGHT_50);
            m_BannerAd.Register(m_Manager.gameObject);

            // Set delegates to get notified on changes or when the user interacts with the ad.
            m_BannerAd.AdViewDidLoad = HandleAdViewDidLoad;
            m_BannerAd.AdViewDidFailWithError = HandleAdViewDidFailWithError;
            m_BannerAd.AdViewWillLogImpression = HandleAdViewWillLogImpression;
            m_BannerAd.AdViewDidClick = HandleAdViewDidClick;

            // Initiate a request to load an ad.
            m_BannerAd.LoadAd();
        }

        private void HandleAdViewDidLoad()
        {
            Logger.d(m_Settings.ProviderToString, "Banner loaded and is ", m_BannerAd.IsValid());
            if (m_OnBannerLoaded != null)
                m_OnBannerLoaded(true);

            m_IsBannerLoaded = true;
        }

        private void HandleAdViewDidFailWithError(string error)
        {
            Logger.d(m_Settings.ProviderToString, "Banner failed to load with: ", error);
            if (m_OnBannerLoaded != null)
                m_OnBannerLoaded(false);

            m_IsBannerLoaded = false;
        }

        private void HandleAdViewWillLogImpression()
        {
            Logger.d(m_Settings.ProviderToString, "Banner logged impression");
        }

        private void HandleAdViewDidClick()
        {
            Logger.d(m_Settings.ProviderToString, "Banner clicked");
            m_Manager.HandleOnAdsClicked();
        }

        public void ShowBanner()
        {
            if (IsBannerLoaded())
                m_BannerAd.Show(AdPosition.BOTTOM);
        }

        public void HideBanner()
        {
            m_BannerAd?.Dispose();
            m_IsBannerLoaded = false;
        }

        public bool IsBannerLoaded()
        {
            return m_BannerAd != null && m_IsBannerLoaded && m_BannerAd.IsValid();
        }
        #endregion

        #region Interstitial
        public void RequestInterstitial(Action<bool> onLoaded)
        {
            Logger.d(m_Settings.ProviderToString, "requesting interstitial!");
            m_OnInterstitialLoaded = onLoaded;

#if UNITY_EDITOR
            if (m_OnInterstitialLoaded != null)
                m_OnInterstitialLoaded(false);
#else
            if (IsInterstitialLoaded())
            {
                if (m_OnInterstitialLoaded != null)
                    m_OnInterstitialLoaded(true);
                return;
            }
#endif

            m_InterstitialAd?.Dispose();

            m_InterstitialAd = new InterstitialAd(m_Settings.InterstitialId);
            m_InterstitialAd.Register(m_Manager.gameObject);

            // Set delegates to get notified on changes or when the user interacts with the ad.
            m_InterstitialAd.InterstitialAdDidLoad = HandleInterstitialAdDidLoad;
            m_InterstitialAd.InterstitialAdDidFailWithError = HandleInterstitialAdDidFailWithError;
            m_InterstitialAd.InterstitialAdWillLogImpression = HandleInterstitialAdWillLogImpression;
            m_InterstitialAd.InterstitialAdDidClick = HandleInterstitialAdDidClick;
            m_InterstitialAd.InterstitialAdDidClose = HandleInterstitialAdDidClose;

#if UNITY_ANDROID
            /*
             * Only relevant to Android.
             * This callback will only be triggered if the Interstitial activity has
             * been destroyed without being properly closed. This can happen if an
             * app with launchMode:singleTask (such as a Unity game) goes to
             * background and is then relaunched by tapping the icon.
             */
            m_InterstitialAd.interstitialAdActivityDestroyed = HandleinterstitialAdActivityDestroyed;
#endif

            // Initiate the request to load the ad.
            m_InterstitialAd.LoadAd();
        }

        private void HandleInterstitialAdDidLoad()
        {
            m_IsInterstitialLoaded = true;
            m_InterstitialDidClose = false;

            Logger.d(m_Settings.ProviderToString, "Interstitial loaded and is ", m_InterstitialAd.IsValid());
            if (m_OnInterstitialLoaded != null)
                m_OnInterstitialLoaded(true);
        }

        private void HandleInterstitialAdDidFailWithError(string error)
        {
            Logger.d(m_Settings.ProviderToString, "Interstitial ad failed to load with error: ", error);
            if (m_OnInterstitialLoaded != null)
                m_OnInterstitialLoaded(false);

            m_InterstitialDidClose = false;
            m_IsInterstitialLoaded = false;
        }

        private void HandleInterstitialAdWillLogImpression()
        {
            Logger.d(m_Settings.ProviderToString, "Interstitial ad logged impression.");
        }

        private void HandleInterstitialAdDidClick()
        {
            Logger.d(m_Settings.ProviderToString, "Interstitial ad clicked.");
            m_Manager.HandleOnAdsClicked();
        }

        private void HandleInterstitialAdDidClose()
        {
            Logger.d(m_Settings.ProviderToString, "Interstitial ad did close.");
            m_InterstitialDidClose = true;
            m_InterstitialAd?.Dispose();
            m_IsInterstitialLoaded = false;
        }

        private void HandleinterstitialAdActivityDestroyed()
        {
            if (!m_InterstitialDidClose)
            {
                Logger.d(m_Settings.ProviderToString, "Interstitial activity destroyed without being closed first.");
                Logger.d("Game should resume.");
            }
        }

        public bool IsInterstitialLoaded()
        {
            return m_InterstitialAd != null && m_IsInterstitialLoaded && m_InterstitialAd.IsValid();
        }

        public void ShowInterstitial()
        {
            if (IsInterstitialLoaded())
                m_InterstitialAd.Show();
        }
        #endregion

        #region Rewarded Video
        public void RequestRewardedVideo(Action<bool> onLoaded)
        {
            Logger.d(m_Settings.ProviderToString, "requesting rewarded video!");
            m_OnVideoRewardedLoaded = onLoaded;

            m_HasRewarded = false;

            if (IsRewardedVideoLoaded())
            {
                if (m_OnVideoRewardedLoaded != null)
                    m_OnVideoRewardedLoaded(true);
                return;
            }

            m_RewardedVideoAd = new RewardedVideoAd(m_Settings.RewardedVideoId);
            m_RewardedVideoAd.Register(m_Manager.gameObject);

            // Set delegates to get notified on changes or when the user interacts with the ad.
            m_RewardedVideoAd.RewardedVideoAdDidLoad = HandleRewardedVideoAdDidLoad;
            m_RewardedVideoAd.RewardedVideoAdDidFailWithError = HandleRewardedVideoAdDidFailWithError;
            m_RewardedVideoAd.RewardedVideoAdWillLogImpression = HandleRewardedVideoAdWillLogImpression;
            m_RewardedVideoAd.RewardedVideoAdDidClick = HandleRewardedVideoAdDidClick;

            // For S2S validation you need to register the following two callback
            // Refer to documentation here:
            // https://developers.facebook.com/docs/audience-network/android/rewarded-video#server-side-reward-validation
            // https://developers.facebook.com/docs/audience-network/ios/rewarded-video#server-side-reward-validation
            m_RewardedVideoAd.RewardedVideoAdDidSucceed = HandleRewardedVideoAdDidSucceed;

            m_RewardedVideoAd.RewardedVideoAdDidFail = HandleRewardedVideoAdDidFail;

            m_RewardedVideoAd.RewardedVideoAdDidClose = HandleRewardedVideoAdDidClose;

#if UNITY_ANDROID
            /*
             * Only relevant to Android.
             * This callback will only be triggered if the Rewarded Video activity
             * has been destroyed without being properly closed. This can happen if
             * an app with launchMode:singleTask (such as a Unity game) goes to
             * background and is then relaunched by tapping the icon.
             */
            m_RewardedVideoAd.RewardedVideoAdActivityDestroyed = HandleRewardedVideoAdActivityDestroyed;
#endif

            // Initiate the request to load the ad.
            m_RewardedVideoAd.LoadAd();
        }

        private void HandleRewardedVideoAdDidLoad()
        {
            Logger.d(m_Settings.ProviderToString, "RewardedVideo ad loaded.");
            m_IsRewardedVideoLoaded = true;
            m_RewardedVideoDidClose = false;
            if (m_OnVideoRewardedLoaded != null)
                m_OnVideoRewardedLoaded(true);
        }

        private void HandleRewardedVideoAdDidFailWithError(string error)
        {
            Logger.d(m_Settings.ProviderToString, "RewardedVideo ad failed to load with error: ", error);
            if (m_OnVideoRewardedLoaded != null)
                m_OnVideoRewardedLoaded(false);

            m_IsRewardedVideoLoaded = false;
            m_RewardedVideoDidClose = false;
        }

        private void HandleRewardedVideoAdWillLogImpression()
        {
            Logger.d(m_Settings.ProviderToString, "RewardedVideo ad logged impression.");
        }

        private void HandleRewardedVideoAdDidClick()
        {
            Logger.d(m_Settings.ProviderToString, "RewardedVideo ad clicked.");
            m_Manager.HandleOnAdsClicked();
        }

        private void HandleRewardedVideoAdDidSucceed()
        {
            Logger.d(m_Settings.ProviderToString, "Rewarded video ad validated by server");
            m_HasRewarded = true;
        }

        private void HandleRewardedVideoAdDidFail()
        {
            Logger.d(m_Settings.ProviderToString, "Rewarded video ad not validated, or no response from server");
            m_HasRewarded = false;
        }

        private void HandleRewardedVideoAdDidClose()
        {
            Logger.d(m_Settings.ProviderToString, "Rewarded video ad did close.");
            m_RewardedVideoDidClose = true;
            m_IsRewardedVideoLoaded = false;
            m_RewardedVideoAd?.Dispose();

            m_Manager.HandleOnWatchVideoReward(m_HasRewarded);
        }

        private void HandleRewardedVideoAdActivityDestroyed()
        {
            if (!m_RewardedVideoDidClose)
            {
                Logger.d(m_Settings.ProviderToString, "Rewarded video activity destroyed without being closed first.");
                Logger.d("Game should resume. User should not get a reward.");
                m_Manager.HandleOnWatchVideoReward(m_HasRewarded);
            }
        }

        public void ShowRewardedVideo()
        {
            if (IsRewardedVideoLoaded())
                m_RewardedVideoAd.Show();
        }

        public bool IsRewardedVideoLoaded()
        {
            return m_RewardedVideoAd != null && m_IsRewardedVideoLoaded && m_RewardedVideoAd.IsValid();
        }
        #endregion
        #endregion
    }
}