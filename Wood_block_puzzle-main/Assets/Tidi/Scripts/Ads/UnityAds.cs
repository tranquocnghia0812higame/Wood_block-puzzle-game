using System;
using UnityEngine.Advertisements;

namespace Tidi.Ads
{
    public class UnityAds : IAdController, IUnityAdsListener
    {
        #region Constants
        #endregion

        #region Events
        #endregion

        #region Fields
        #endregion

        #region Properties
        private BaseAdUnitSetting m_Settings;

        private System.Action<bool> m_OnBannerLoaded;
        private System.Action<bool> m_OnInterstitialLoaded;
        private System.Action<bool> m_OnVideoRewardedLoaded;

        private AdManager m_Manager;
        #endregion

        #region Unity Events
        #endregion

        #region Methods
        public void Init(AdManager manager, BaseAdUnitSetting settings)
        {
            m_Manager = manager;
            m_Settings = settings;

            Logger.d(m_Settings.ProviderToString, "Is Initializing!");

            // Advertisement.AddListener(this);
            //
            // Advertisement.Initialize(m_Settings.GameId, true);
            // Advertisement.Banner.SetPosition(BannerPosition.BOTTOM_CENTER);
        }

        public void RequestBanner(Action<bool> onLoaded)
        {
            Logger.d(m_Settings.ProviderToString, "requesting banner!");
            m_OnBannerLoaded = onLoaded;

            if (m_OnBannerLoaded != null)
            {
                m_OnBannerLoaded(IsBannerLoaded());
            }
        }

        public bool IsBannerLoaded()
        {
            throw new NotImplementedException();
        }

        public void ShowBanner()
        {
            // if (!Advertisement.isInitialized)
            //     return;
            //
            // Advertisement.Banner.Show(m_Settings.BannerId);
        }

        public void HideBanner()
        {
            // Advertisement.Banner.Hide();
        }

        // public bool IsBannerLoaded()
        // {
        //     return Advertisement.IsReady(m_Settings.BannerId);
        // }

        public void RequestInterstitial(Action<bool> onLoaded)
        {
            Logger.d(m_Settings.ProviderToString, "requesting interstitial!");
            m_OnInterstitialLoaded = onLoaded;

            if (m_OnInterstitialLoaded != null)
            {
                m_OnInterstitialLoaded(IsInterstitialLoaded());
            }
        }

        public bool IsInterstitialLoaded()
        {
            throw new NotImplementedException();
        }

        public void ShowInterstitial()
        {
            Logger.d(m_Settings.ProviderToString, "Show Interstitial!");
            // Advertisement.Show(m_Settings.InterstitialId);
        }

        // public bool IsInterstitialLoaded()
        // {
        //     return Advertisement.IsReady(m_Settings.InterstitialId);
        // }

        public void RequestRewardedVideo(Action<bool> onLoaded)
        {
            Logger.d(m_Settings.ProviderToString, "requesting rewarded video!");
            m_OnVideoRewardedLoaded = onLoaded;

            if (m_OnVideoRewardedLoaded != null)
            {
                m_OnVideoRewardedLoaded(IsRewardedVideoLoaded());
            }
        }

        public bool IsRewardedVideoLoaded()
        {
            throw new NotImplementedException();
        }

        // public bool IsRewardedVideoLoaded()
        // {
        //     return Advertisement.IsReady(m_Settings.RewardedVideoId);
        // }

        public void ShowRewardedVideo()
        {
            Logger.d(m_Settings.ProviderToString, "Show Rewarded Video!");
            // Advertisement.Show(m_Settings.RewardedVideoId);
        }

        public void OnApplicationPause(bool isPaused)
        {
            
        }

        public void Dispose()
        {
            // Advertisement.RemoveListener(this);

            m_OnBannerLoaded = null;
            m_OnInterstitialLoaded = null;
            m_OnVideoRewardedLoaded = null;
        }
        #endregion

        #region Implement IUnityAdsListener interface methods
        // public void OnUnityAdsDidFinish(string placementId, ShowResult showResult)
        // {
        //     // Define conditional logic for each ad completion status:
        //     if (showResult == ShowResult.Finished)
        //     {
        //         // Reward the user for watching the ad to completion.
        //         if (string.Compare(placementId, m_Settings.RewardedVideoId) == 0)
        //             m_Manager.HandleOnWatchVideoReward(true);
        //     }
        //     else if (showResult == ShowResult.Skipped)
        //     {
        //         // Do not reward the user for skipping the ad.
        //         if (string.Compare(placementId, m_Settings.RewardedVideoId) == 0)
        //             m_Manager.HandleOnWatchVideoReward(false);
        //     }
        //     else if (showResult == ShowResult.Failed)
        //     {
        //         Logger.w(m_Settings.ProviderToString, "The ad did not finish due to an error.");
        //         if (string.Compare(placementId, m_Settings.RewardedVideoId) == 0)
        //             m_Manager.HandleOnWatchVideoReward(false);
        //     }
        // }

        public void OnUnityAdsReady(string placementId)
        {
            // If the ready Placement is rewarded, show the ad:
            if (string.Compare(placementId, m_Settings.InterstitialId) == 0) // Optional actions to take when the placement becomes ready(For example, enable the rewarded ads button)
            {
                if (m_OnInterstitialLoaded != null)
                    m_OnInterstitialLoaded(true);
            }
            else if (string.Compare(placementId, m_Settings.RewardedVideoId) == 0)
            {
                if (m_OnVideoRewardedLoaded != null)
                    m_OnVideoRewardedLoaded(true);
            }
            else if (string.Compare(placementId, m_Settings.BannerId) == 0)
            {
                if (m_OnBannerLoaded != null)
                    m_OnBannerLoaded(true);
            }
        }

        public void OnUnityAdsDidError(string message)
        {
            // Log the error.
        }

        public void OnUnityAdsDidStart(string placementId)
        {
            // Optional actions to take when the end-users triggers an ad.
        }
        #endregion
    }

    public interface IUnityAdsListener
    {
    }
}