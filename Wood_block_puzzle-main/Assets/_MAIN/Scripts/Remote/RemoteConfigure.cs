using System.Collections;
using System.Collections.Generic;
using Firebase.Extensions;
using Firebase.RemoteConfig;
using UnityEngine;
using UnityEngine.Networking;

namespace BP
{
    public class RemoteConfigure : Singleton<RemoteConfigure>
    {
        #region Constants
        #endregion

        #region Events
        #endregion

        #region Fields
        #endregion

        #region Properties
        public bool hasFetched;
        #endregion

        #region Unity Events
        #endregion

        #region Methods
        public void Initialize()
        {
            hasFetched = false;

            FetchData();
        }

        public void FetchData()
        {
            Logger.d("Remote is fetching data...");
            // FetchAsync only fetches new data if the current data is older than the provided
            // timespan.  Otherwise it assumes the data is "recent enough", and does nothing.
            // By default the timespan is 12 hours, and for production apps, this is a good
            // number.  For this example though, it's set to a timespan of zero, so that
            // changes in the console will always show up immediately.
            System.Threading.Tasks.Task fetchTask = FirebaseRemoteConfig.FetchAsync(System.TimeSpan.Zero);
            fetchTask.ContinueWithOnMainThread(FetchComplete);
        }

        void FetchComplete(System.Threading.Tasks.Task fetchTask)
        {
            if (fetchTask.IsCanceled)
            {
                Logger.d("Fetch canceled.");
            }
            else if (fetchTask.IsFaulted)
            {
                Logger.d("Fetch encountered an error.");
            }
            else if (fetchTask.IsCompleted)
            {
                Logger.d("Fetch completed successfully!");
            }

            var info = FirebaseRemoteConfig.Info;

            switch (info.LastFetchStatus)
            {
                case LastFetchStatus.Success:
                    FirebaseRemoteConfig.ActivateFetched();

                    Logger.d(string.Format("Remote data loaded and ready (last fetch time {0}).", info.FetchTime));
                    break;
                case LastFetchStatus.Failure:
                    switch (info.LastFetchFailureReason)
                    {
                        case FetchFailureReason.Error:
                            Logger.d("Fetch failed for unknown reason");
                            break;
                        case FetchFailureReason.Throttled:
                            Logger.d("Fetch throttled until " + info.ThrottledEndTime);
                            break;
                    }
                    break;
                case LastFetchStatus.Pending:
                    Logger.d("Latest Fetch call still pending.");
                    break;
            }

            bool succeed = info.LastFetchStatus == LastFetchStatus.Success;
            if (succeed)
            {
                SaveRemoteData();
            }

            hasFetched = true;
        }

        private void SaveRemoteData()
        {
            
        }

        private string GetInterstitialKey()
        {
            string key = "";
            key = "ads_admob_full_id";
            return key;
        }

        public string GetLocalInterstitialId()
        {
            string prefsKey = "";
            string defaultId = "";

            prefsKey = Consts.PREFS_ADS_ADMOB_FULL_ID;
            defaultId = "ca-app-pub-1574931327419595/5550398593";

            return PrefsUtils.GetString(prefsKey, defaultId);
        }

        private string GetRemoteInterstitialId()
        {
            string key = GetInterstitialKey();
            if (string.IsNullOrEmpty(key))
                return "";
            else
                return FirebaseRemoteConfig.GetValue(key).StringValue;
        }

        private string GetBannerKey()
        {
            string key = "";
#if UNITY_ANDROID
            key = "ads_banner_android";
#elif UNITY_IPHONE
            key = "ads_banner_ios";
#else
            key = "";
#endif
            return key;
        }

        private string GetRemoteBannerId()
        {
            string key = GetBannerKey();
            if (string.IsNullOrEmpty(key))
                return "";
            else
                return FirebaseRemoteConfig.GetValue(key).StringValue;
        }

        public string GetLocalBannerId()
        {
            string key = Consts.PREFS_ADS_BANNER_ID;
            string defaultId = "";
#if UNITY_ANDROID
            defaultId = "ca-app-pub-5165473781248790/7595078861";
#elif UNITY_IPHONE
            defaultId = "ca-app-pub-4216775262535111/4701382595";
#endif
            return PrefsUtils.GetString(key, defaultId);
        }
        #endregion
    }
}