using System;
using UnityEngine;

namespace Tidi.Ads
{
    public class IronSourceAds : IAdController
    {
        #region Constants
        #endregion

        #region Events
        #endregion

        #region Fields
        #endregion

        #region Properties
        private bool m_IsBannerLoaded;
        private bool m_HasRewarded;

        private BaseAdUnitSetting m_Settings;

        private System.Action<bool> m_OnBannerLoaded;
        private System.Action<bool> m_OnInterstitialLoaded;
        private System.Action<bool> m_OnVideoRewardedLoaded;

        private AdManager m_Manager;
        #endregion

        #region Unity Events
        public void OnApplicationPause(bool isPaused)
        {
            Logger.d("OnApplicationPause = ", isPaused);
            IronSource.Agent.onApplicationPause(isPaused);
        }
        #endregion

        #region Methods
        public void Init(AdManager manager, BaseAdUnitSetting settings)
        {
            m_Manager = manager;
            m_Settings = settings;

            Logger.d(m_Settings.ProviderToString, "Is Initializing!");

            IronSource.Agent.validateIntegration();
            #if UNITY_ANDROID
            IronSource.Agent.init(m_Settings.GameId);
            #elif UNITY_IOS
            IronSource.Agent.init(m_Settings.GameIdIOS);
            #endif

            m_IsBannerLoaded = false;
            m_HasRewarded = false;

            //Add Rewarded Video Events
            IronSourceEvents.onRewardedVideoAdOpenedEvent += RewardedVideoAdOpenedEvent;
            IronSourceEvents.onRewardedVideoAdClosedEvent += RewardedVideoAdClosedEvent;
            IronSourceEvents.onRewardedVideoAvailabilityChangedEvent += RewardedVideoAvailabilityChangedEvent;
            IronSourceEvents.onRewardedVideoAdStartedEvent += RewardedVideoAdStartedEvent;
            IronSourceEvents.onRewardedVideoAdEndedEvent += RewardedVideoAdEndedEvent;
            IronSourceEvents.onRewardedVideoAdRewardedEvent += RewardedVideoAdRewardedEvent;
            IronSourceEvents.onRewardedVideoAdShowFailedEvent += RewardedVideoAdShowFailedEvent;
            IronSourceEvents.onRewardedVideoAdClickedEvent += RewardedVideoAdClickedEvent;

            // Add Interstitial Events
            IronSourceEvents.onInterstitialAdReadyEvent += InterstitialAdReadyEvent;
            IronSourceEvents.onInterstitialAdLoadFailedEvent += InterstitialAdLoadFailedEvent;
            IronSourceEvents.onInterstitialAdShowSucceededEvent += InterstitialAdShowSucceededEvent;
            IronSourceEvents.onInterstitialAdShowFailedEvent += InterstitialAdShowFailedEvent;
            IronSourceEvents.onInterstitialAdClickedEvent += InterstitialAdClickedEvent;
            IronSourceEvents.onInterstitialAdOpenedEvent += InterstitialAdOpenedEvent;
            IronSourceEvents.onInterstitialAdClosedEvent += InterstitialAdClosedEvent;

            // Add Banner Events
            IronSourceEvents.onBannerAdLoadedEvent += BannerAdLoadedEvent;
            IronSourceEvents.onBannerAdLoadFailedEvent += BannerAdLoadFailedEvent;
            IronSourceEvents.onBannerAdClickedEvent += BannerAdClickedEvent;
            IronSourceEvents.onBannerAdScreenPresentedEvent += BannerAdScreenPresentedEvent;
            IronSourceEvents.onBannerAdScreenDismissedEvent += BannerAdScreenDismissedEvent;
            IronSourceEvents.onBannerAdLeftApplicationEvent += BannerAdLeftApplicationEvent;
        }

        public void RequestBanner(Action<bool> onLoaded)
        {
            Logger.d(m_Settings.ProviderToString, "requesting banner!");
            m_OnBannerLoaded = onLoaded;

            if (IsBannerLoaded() && m_OnBannerLoaded != null)
            {
                m_OnBannerLoaded(true);
            }
            else 
            {
                IronSource.Agent.loadBanner(IronSourceBannerSize.BANNER, IronSourceBannerPosition.BOTTOM);
            }
        }

        public void ShowBanner()
        {
            IronSource.Agent.displayBanner();
        }

        public void HideBanner()
        {
            IronSource.Agent.destroyBanner();
        }

        public bool IsBannerLoaded()
        {
            return m_IsBannerLoaded;
        }

        public void RequestInterstitial(Action<bool> onLoaded)
        {
            Logger.d(m_Settings.ProviderToString, "requesting interstitial!");
            m_OnInterstitialLoaded = onLoaded;

            if (IsInterstitialLoaded() && m_OnInterstitialLoaded != null)
            {
                m_OnInterstitialLoaded(true);
            }
            else 
            {
                IronSource.Agent.loadInterstitial();
            }
        }

        public void ShowInterstitial()
        {
            Logger.d(m_Settings.ProviderToString, "Show Interstitial!");
            IronSource.Agent.showInterstitial();
        }

        public bool IsInterstitialLoaded()
        {
            return IronSource.Agent.isInterstitialReady();
        }

        public void RequestRewardedVideo(Action<bool> onLoaded)
        {
            Logger.d(m_Settings.ProviderToString, "requesting rewarded video!");
            m_OnVideoRewardedLoaded = onLoaded;

            m_HasRewarded = false;

            if (m_OnVideoRewardedLoaded != null)
            {
                m_OnVideoRewardedLoaded(IsRewardedVideoLoaded());
            }
        }

        public bool IsRewardedVideoLoaded()
        {
            return IronSource.Agent.isRewardedVideoAvailable();
        }

        public void ShowRewardedVideo()
        {
            Logger.d(m_Settings.ProviderToString, "Show Rewarded Video!");
            IronSource.Agent.showRewardedVideo();
        }

        public void Dispose()
        {
            m_OnBannerLoaded = null;
            m_OnInterstitialLoaded = null;
            m_OnVideoRewardedLoaded = null;

            //Add Rewarded Video Events
            IronSourceEvents.onRewardedVideoAdOpenedEvent -= RewardedVideoAdOpenedEvent;
            IronSourceEvents.onRewardedVideoAdClosedEvent -= RewardedVideoAdClosedEvent;
            IronSourceEvents.onRewardedVideoAvailabilityChangedEvent -= RewardedVideoAvailabilityChangedEvent;
            IronSourceEvents.onRewardedVideoAdStartedEvent -= RewardedVideoAdStartedEvent;
            IronSourceEvents.onRewardedVideoAdEndedEvent -= RewardedVideoAdEndedEvent;
            IronSourceEvents.onRewardedVideoAdRewardedEvent -= RewardedVideoAdRewardedEvent;
            IronSourceEvents.onRewardedVideoAdShowFailedEvent -= RewardedVideoAdShowFailedEvent;
            IronSourceEvents.onRewardedVideoAdClickedEvent -= RewardedVideoAdClickedEvent;

            // Add Interstitial Events
            IronSourceEvents.onInterstitialAdReadyEvent -= InterstitialAdReadyEvent;
            IronSourceEvents.onInterstitialAdLoadFailedEvent -= InterstitialAdLoadFailedEvent;
            IronSourceEvents.onInterstitialAdShowSucceededEvent -= InterstitialAdShowSucceededEvent;
            IronSourceEvents.onInterstitialAdShowFailedEvent -= InterstitialAdShowFailedEvent;
            IronSourceEvents.onInterstitialAdClickedEvent -= InterstitialAdClickedEvent;
            IronSourceEvents.onInterstitialAdOpenedEvent -= InterstitialAdOpenedEvent;
            IronSourceEvents.onInterstitialAdClosedEvent -= InterstitialAdClosedEvent;

            // Add Banner Events
            IronSourceEvents.onBannerAdLoadedEvent -= BannerAdLoadedEvent;
            IronSourceEvents.onBannerAdLoadFailedEvent -= BannerAdLoadFailedEvent;
            IronSourceEvents.onBannerAdClickedEvent -= BannerAdClickedEvent;
            IronSourceEvents.onBannerAdScreenPresentedEvent -= BannerAdScreenPresentedEvent;
            IronSourceEvents.onBannerAdScreenDismissedEvent -= BannerAdScreenDismissedEvent;
            IronSourceEvents.onBannerAdLeftApplicationEvent -= BannerAdLeftApplicationEvent;
        }

        #region RewardedAd callback handlers
        private void RewardedVideoAvailabilityChangedEvent(bool canShowAd)
        {
            Logger.d(m_Settings.ProviderToString, "I got RewardedVideoAvailabilityChangedEvent, value = " + canShowAd);
            if (m_OnVideoRewardedLoaded != null)
                m_OnVideoRewardedLoaded(canShowAd);
        }

        private void RewardedVideoAdOpenedEvent()
        {
            Logger.d(m_Settings.ProviderToString, "I got RewardedVideoAdOpenedEvent");
        }

        private void RewardedVideoAdRewardedEvent(IronSourcePlacement ssp)
        {
            Logger.d(m_Settings.ProviderToString, "I got RewardedVideoAdRewardedEvent, amount = " + ssp.getRewardAmount() + " name = " + ssp.getRewardName());
            m_HasRewarded = true;
        }

        private void RewardedVideoAdClosedEvent()
        {
            Logger.d(m_Settings.ProviderToString, "I got RewardedVideoAdClosedEvent");
            m_Manager.HandleOnWatchVideoReward(m_HasRewarded);
        }

        private void RewardedVideoAdStartedEvent()
        {
            Logger.d(m_Settings.ProviderToString, "I got RewardedVideoAdStartedEvent");
            m_HasRewarded = false;
        }

        private void RewardedVideoAdEndedEvent()
        {
            Logger.d(m_Settings.ProviderToString, "I got RewardedVideoAdEndedEvent");
        }

        private void RewardedVideoAdShowFailedEvent(IronSourceError error)
        {
            Logger.d(m_Settings.ProviderToString, "I got RewardedVideoAdShowFailedEvent, code :  " + error.getCode() + ", description : " + error.getDescription());
            m_HasRewarded = false;
        }

        private void RewardedVideoAdClickedEvent(IronSourcePlacement ssp)
        {
            Logger.d(m_Settings.ProviderToString, "I got RewardedVideoAdClickedEvent, name = " + ssp.getRewardName());
            m_Manager.HandleOnAdsClicked();
        }
        #endregion

        #region Interstitial callback handlers
        private void InterstitialAdReadyEvent()
        {
            Logger.d(m_Settings.ProviderToString, "I got InterstitialAdReadyEvent");
            if (m_OnInterstitialLoaded != null)
                m_OnInterstitialLoaded(true);
        }

        private void InterstitialAdLoadFailedEvent(IronSourceError error)
        {
            Logger.d(m_Settings.ProviderToString, "I got InterstitialAdLoadFailedEvent, code: " + error.getCode() + ", description : " + error.getDescription());
            if (m_OnInterstitialLoaded != null)
                m_OnInterstitialLoaded(false);
        }

        private void InterstitialAdShowSucceededEvent()
        {
            Logger.d(m_Settings.ProviderToString, "I got InterstitialAdShowSucceededEvent");
        }

        private void InterstitialAdShowFailedEvent(IronSourceError error)
        {
            Logger.d(m_Settings.ProviderToString, "I got InterstitialAdShowFailedEvent, code :  " + error.getCode() + ", description : " + error.getDescription());
        }

        private void InterstitialAdClickedEvent()
        {
            Logger.d(m_Settings.ProviderToString, "I got InterstitialAdClickedEvent");
            m_Manager.HandleOnAdsClicked();
        }

        private void InterstitialAdOpenedEvent()
        {
            Logger.d(m_Settings.ProviderToString, "I got InterstitialAdOpenedEvent");
        }

        private void InterstitialAdClosedEvent()
        {
            Logger.d(m_Settings.ProviderToString, "I got InterstitialAdClosedEvent");
        }
        #endregion

        #region Banner callback handlers
        private void BannerAdLoadedEvent()
        {
            Logger.d(m_Settings.ProviderToString, "Banner loaded!");
            if (m_OnBannerLoaded != null)
                m_OnBannerLoaded(true);

            m_IsBannerLoaded = true;
        }

        private void BannerAdLoadFailedEvent(IronSourceError error)
        {
            Logger.d(m_Settings.ProviderToString, "Banner failed to load with code: ", error.getCode(), ", description: ", error.getDescription());
            if (m_OnBannerLoaded != null)
                m_OnBannerLoaded(false);

            m_IsBannerLoaded = false;
        }

        private void BannerAdClickedEvent()
        {
            Logger.d(m_Settings.ProviderToString, "Banner clicked");
            m_Manager.HandleOnAdsClicked();
        }

        private void BannerAdScreenPresentedEvent()
        {
            Logger.d(m_Settings.ProviderToString, "I got BannerAdScreenPresentedEvent");
        }

        private void BannerAdScreenDismissedEvent()
        {
            Logger.d(m_Settings.ProviderToString, "I got BannerAdScreenDismissedEvent");
        }

        private void BannerAdLeftApplicationEvent()
        {
            Logger.d(m_Settings.ProviderToString, "I got BannerAdLeftApplicationEvent");
        }
        #endregion

        #endregion
    }
}