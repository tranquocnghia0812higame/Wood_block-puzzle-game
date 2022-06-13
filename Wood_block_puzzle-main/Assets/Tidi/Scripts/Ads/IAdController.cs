namespace Tidi.Ads
{
    public interface IAdController
    {
        void Init(AdManager manager, BaseAdUnitSetting settings);
        void RequestBanner(System.Action<bool> onLoaded);
        bool IsBannerLoaded();
        void ShowBanner();
        void HideBanner();

        void RequestInterstitial(System.Action<bool> onLoaded);
        bool IsInterstitialLoaded();
        void ShowInterstitial();

        void RequestRewardedVideo(System.Action<bool> onLoaded);
        bool IsRewardedVideoLoaded();
        void ShowRewardedVideo();
        
        void OnApplicationPause(bool isPaused);
        void Dispose();
    }
}
