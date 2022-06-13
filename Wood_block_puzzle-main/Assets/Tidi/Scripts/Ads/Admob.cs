using System;
// using GoogleMobileAds.Api;
using UnityEngine;

namespace Tidi.Ads
{
    public class Admob : IAdController
    {
//         #region Constants
//         #endregion
//
//         #region Events
//         #endregion
//
//         #region Fields
//         #endregion
//
//         #region Properties
//         private BannerView m_BannerView;
//
//         private InterstitialAd m_Interstitial;
//         private RewardedAd m_RewardedAd;
//
//         private bool m_BannerLoaded = false;
//         private bool m_HasRewarded = false;
//
//         private System.Action<bool> m_OnBannerLoaded;
//         private System.Action<bool> m_OnInterstitialLoaded;
//         private System.Action<bool> m_OnVideoRewardedLoaded;
//
//         private AdManager m_Manager;
//         private BaseAdUnitSetting m_Settings;
//         #endregion
//
//         #region Unity Events
//         #endregion
//
//         #region Methods
//         public void Init(AdManager manager, BaseAdUnitSetting settings)
//         {
//             m_Manager = manager;
//             m_Settings = settings;
//
//             Logger.d(m_Settings.ProviderToString, "Is Initializing!");
//
//             // Initialize the Google Mobile Ads SDK.
//             MobileAds.Initialize((status) =>
//             {
//
//             });
//         }
//
//         public void OnApplicationPause(bool isPaused)
//         {
//
//         }
//
//         public void Dispose()
//         {
//             m_RewardedAd = null;
//
//             m_OnBannerLoaded = null;
//             m_OnInterstitialLoaded = null;
//             m_OnVideoRewardedLoaded = null;
//         }
//
//         #region Rewarded video
//         public void RequestRewardedVideo(Action<bool> onLoaded)
//         {
//             Logger.d(m_Settings.ProviderToString, "requesting rewarded video!");
//             m_OnVideoRewardedLoaded = onLoaded;
//
//             if (IsRewardedVideoLoaded())
//             {
//                 m_OnVideoRewardedLoaded?.Invoke(true);
//                 return;
//             }
//
//             m_HasRewarded = false;
//
//             m_RewardedAd = new RewardedAd(m_Settings.RewardedVideoId);
//
//             // Called when an ad request has successfully loaded.
//             m_RewardedAd.OnAdLoaded += HandleRewardedAdLoaded;
//             // Called when an ad request failed to load.
//             m_RewardedAd.OnAdFailedToLoad += HandleRewardedAdFailedToLoad;
//             // Called when an ad is shown.
//             m_RewardedAd.OnAdOpening += HandleRewardedAdOpening;
//             // Called when an ad request failed to show.
//             m_RewardedAd.OnAdFailedToShow += HandleRewardedAdFailedToShow;
//             // Called when the user should be rewarded for interacting with the ad.
//             m_RewardedAd.OnUserEarnedReward += HandleUserEarnedReward;
//             // Called when the ad is closed.
//             m_RewardedAd.OnAdClosed += HandleRewardedAdClosed;
//
//             // Create an empty ad request.
//             AdRequest request = new AdRequest.Builder().Build();
//             // Load the rewarded ad with the request.
//             m_RewardedAd.LoadAd(request);
//         }
//
//         public void HandleRewardedAdLoaded(object sender, EventArgs args)
//         {
//             Logger.d(m_Settings.ProviderToString, "HandleRewardedAdLoaded event received");
//
//             m_OnVideoRewardedLoaded?.Invoke(true);
//         }
//
//         public void HandleRewardedAdFailedToLoad(object sender, AdErrorEventArgs args)
//         {
//             Logger.d(m_Settings.ProviderToString, 
//                 "HandleRewardedAdFailedToLoad event received with message: "
//                                  + args.Message);
//
//             m_OnVideoRewardedLoaded?.Invoke(false);
//         }
//
//         public void HandleRewardedAdOpening(object sender, EventArgs args)
//         {
//             Logger.d(m_Settings.ProviderToString, "HandleRewardedAdOpening event received");
//         }
//
//         public void HandleRewardedAdFailedToShow(object sender, AdErrorEventArgs args)
//         {
//             Logger.d(m_Settings.ProviderToString, 
//                 "HandleRewardedAdFailedToShow event received with message: "
//                                  + args.Message);
//         }
//
//         public void HandleRewardedAdClosed(object sender, EventArgs args)
//         {
//             Logger.d(m_Settings.ProviderToString, "HandleRewardedAdClosed event received");
//
//             #if UNITY_IOS
//             m_Manager.HandleOnWatchVideoReward(m_HasRewarded);
//             #endif
//
//             m_RewardedAd = null;
//         }
//
//         public void HandleUserEarnedReward(object sender, Reward args)
//         {
//             string type = args.Type;
//             double amount = args.Amount;
//             Logger.d(m_Settings.ProviderToString, 
//                 "HandleRewardedAdRewarded event received for "
//                             + amount.ToString() + " " + type);
//
//             m_HasRewarded = true;
//
//             #if !UNITY_IOS
//             m_Manager.HandleOnWatchVideoReward(m_HasRewarded);
//             #endif
//         }
//
//         public void ShowRewardedVideo()
//         {
//             Logger.d(m_Settings.ProviderToString, "Show Rewarded Video!");
//             if (m_RewardedAd.IsLoaded())
//                 m_RewardedAd.Show();
//         }
//
//         public bool IsRewardedVideoLoaded()
//         {
//             return (m_RewardedAd != null && m_RewardedAd.IsLoaded());
//         }
//         #endregion
//
//         #region Banner
//         public void RequestBanner(Action<bool> onBannerLoaded)
//         {
//             Logger.d(m_Settings.ProviderToString, "requesting banner!");
//             m_OnBannerLoaded = onBannerLoaded;
//
// #if UNITY_EDITOR
//             if (m_OnBannerLoaded != null)
//                 m_OnBannerLoaded(false);
// #endif
//
//             if (IsBannerLoaded())
//             {
//                 if (m_OnBannerLoaded != null)
//                     m_OnBannerLoaded(true);
//                 return;
//             }
//
//             m_BannerLoaded = false;
//
//             if (m_BannerView != null)
//                 m_BannerView.Destroy();
//
//             string bannerId = m_Settings.BannerId;
//             // bannerId = "ca-app-pub-3940256099942544/6300978111"; // For testing ad
//             AdSize adaptiveSize = AdSize.GetPortraitAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
//             m_BannerView = new BannerView(bannerId, AdSize.Banner, AdPosition.Bottom);
//
//             // Called when an ad request has successfully loaded.
//             m_BannerView.OnAdLoaded += HandleOnBannerAdLoaded;
//             // Called when an ad request failed to load.
//             m_BannerView.OnAdFailedToLoad += HandleOnBannerAdFailedToLoad;
//             // Called when an ad is clicked.
//             m_BannerView.OnAdOpening += HandleOnBannerAdOpened;
//             // Called when the user returned from the app after an ad click.
//             m_BannerView.OnAdClosed += HandleOnBannerAdClosed;
//             // Called when the ad click caused the user to leave the application.
//             m_BannerView.OnAdLeavingApplication += HandleOnBannerAdLeftApplication;
//
//             // Create an empty ad request.
//             AdRequest request = new AdRequest.Builder().Build();
//
//             // Load the banner with the request.
//             m_BannerView.LoadAd(request);
//         }
//
//         public bool IsBannerLoaded()
//         {
//             return (m_BannerLoaded && m_BannerView != null);
//         }
//
//         public void HandleOnBannerAdLoaded(object sender, EventArgs args)
//         {
//             Logger.d(m_Settings.ProviderToString, "Banner HandleAdLoaded event received");
//             m_BannerLoaded = true;
//             if (m_OnBannerLoaded != null)
//                 m_OnBannerLoaded(true);
//         }
//
//         public void HandleOnBannerAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
//         {
//             Logger.d(m_Settings.ProviderToString, "Banner HandleFailedToReceiveAd event received with message: " + args.Message);
//             m_BannerLoaded = false;
//             if (m_OnBannerLoaded != null)
//                 m_OnBannerLoaded(false);
//         }
//
//         public void HandleOnBannerAdOpened(object sender, EventArgs args)
//         {
//             Logger.d(m_Settings.ProviderToString, "HandleAdOpened event received");
//         }
//
//         public void HandleOnBannerAdClosed(object sender, EventArgs args)
//         {
//             Logger.d(m_Settings.ProviderToString, "HandleAdClosed event received");
//         }
//
//         public void HandleOnBannerAdLeftApplication(object sender, EventArgs args)
//         {
//             Logger.d(m_Settings.ProviderToString, "HandleAdLeftApplication event received");
//             m_Manager.HandleOnAdsClicked();
//         }
//
//         public void ShowBanner()
//         {
//             m_BannerView.Show();
//         }
//
//         public void HideBanner()
//         {
//             if (m_BannerView != null)
//                 m_BannerView.Hide();
//         }
//         #endregion
//
//         #region Interstitial Ads
//         public void RequestInterstitial(Action<bool> onInterstitialLoaded)
//         {
//             Logger.d(m_Settings.ProviderToString, "requesting interstitial!");
//             m_OnInterstitialLoaded = onInterstitialLoaded;
//
// #if UNITY_EDITOR
//             if (m_OnInterstitialLoaded != null)
//                 m_OnInterstitialLoaded(false);
// #else
//             if (IsInterstitialLoaded())
//             {
//                 if (m_OnInterstitialLoaded != null)
//                     m_OnInterstitialLoaded(true);
//                 return;
//             }
// #endif
//
//             if (m_Interstitial != null)
//             {
//                 m_Interstitial.Destroy();
//                 m_Interstitial = null;
//             }
//
//             // Initialize an InterstitialAd.
//             string adId = m_Settings.InterstitialId;
//             // adId = "ca-app-pub-3940256099942544/1033173712"; // For testing ad
//             m_Interstitial = new InterstitialAd(m_Settings.InterstitialId);
//
//             // Called when an ad request has successfully loaded.
//             m_Interstitial.OnAdLoaded += HandleOnInterstitialAdLoaded;
//             // Called when an ad request failed to load.
//             m_Interstitial.OnAdFailedToLoad += HandleOnInterstitialAdFailedToLoad;
//             // Called when an ad is shown.
//             m_Interstitial.OnAdOpening += HandleOnInterstitialAdOpened;
//             // Called when the ad is closed.
//             m_Interstitial.OnAdClosed += HandleOnInterstitialAdClosed;
//             // Called when the ad click caused the user to leave the application.
//             m_Interstitial.OnAdLeavingApplication += HandleOnInterstitialAdLeftApplication;
//
//             // Create an empty ad request.
//             // AdRequest request = new AdRequest.Builder().AddTestDevice("60C4C5A97D15C17CD4A3FB729EE88EBC").Build();
//             AdRequest request = new AdRequest.Builder().Build();
//
//             // Load the interstitial with the request.
//             m_Interstitial.LoadAd(request);
//         }
//
//         public bool IsInterstitialLoaded()
//         {
//             return (m_Interstitial != null && m_Interstitial.IsLoaded());
//         }
//
//         public void HandleOnInterstitialAdLoaded(object sender, EventArgs args)
//         {
//             Logger.d(m_Settings.ProviderToString, "Interstitial HandleAdLoaded event received");
//             if (m_OnInterstitialLoaded != null)
//                 m_OnInterstitialLoaded(true);
//         }
//
//         public void HandleOnInterstitialAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
//         {
//             Logger.d(m_Settings.ProviderToString, "Interstitial HandleFailedToReceiveAd event received with message: " + args.Message);
//             if (m_OnInterstitialLoaded != null)
//                 m_OnInterstitialLoaded(false);
//         }
//
//         public void HandleOnInterstitialAdOpened(object sender, EventArgs args)
//         {
//             Logger.d(m_Settings.ProviderToString, "Interstitial HandleAdOpened event received");
//         }
//
//         public void HandleOnInterstitialAdClosed(object sender, EventArgs args)
//         {
//             Logger.d(m_Settings.ProviderToString, "Interstitial HandleAdClosed event received");
//
//             m_Manager.HandleOnClosedInterstitial();
//         }
//
//         public void HandleOnInterstitialAdLeftApplication(object sender, EventArgs args)
//         {
//             Logger.d(m_Settings.ProviderToString, "Interstitial HandleAdLeftApplication event received");
//
//             m_Manager.HandleOnAdsClicked();
//         }
//
//         public void ShowInterstitial()
//         {
//             Logger.d(m_Settings.ProviderToString, "Show Interstitial!");
//             m_Interstitial.Show();
//         }
//         #endregion
//         #endregion
public void Init(AdManager manager, BaseAdUnitSetting settings)
{
    throw new NotImplementedException();
}

public void RequestBanner(Action<bool> onLoaded)
{
    throw new NotImplementedException();
}

public bool IsBannerLoaded()
{
    throw new NotImplementedException();
}

public void ShowBanner()
{
    throw new NotImplementedException();
}

public void HideBanner()
{
    throw new NotImplementedException();
}

public void RequestInterstitial(Action<bool> onLoaded)
{
    throw new NotImplementedException();
}

public bool IsInterstitialLoaded()
{
    throw new NotImplementedException();
}

public void ShowInterstitial()
{
    throw new NotImplementedException();
}

public void RequestRewardedVideo(Action<bool> onLoaded)
{
    throw new NotImplementedException();
}

public bool IsRewardedVideoLoaded()
{
    throw new NotImplementedException();
}

public void ShowRewardedVideo()
{
    throw new NotImplementedException();
}

public void OnApplicationPause(bool isPaused)
{
    throw new NotImplementedException();
}

public void Dispose()
{
    throw new NotImplementedException();
}
    }
}
