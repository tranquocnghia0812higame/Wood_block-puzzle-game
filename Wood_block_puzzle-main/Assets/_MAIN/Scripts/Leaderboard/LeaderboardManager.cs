using System.Collections;
using System.Collections.Generic;
// using GooglePlayGames;
// using GooglePlayGames.BasicApi;
using Tidi.Leaderboard;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace Tidi.Leaderboard
{
    public class LeaderboardManager : Singleton<LeaderboardManager>
    {
        #region Constants
        #endregion

        #region Events
        #endregion

        #region Fields
        [SerializeField]
        private LeaderboardSetting m_Setting;
        #endregion

        #region Properties
        private bool m_HasInitialized = false;
        public bool hasInitialized
        {
            get
            {
                return m_HasInitialized;
            }
        }

        private ILeaderboard m_Leaderboard;
        private string m_LeaderboardId = "CgkIrYLmidADEAIQAQ";
        #endregion

        #region Unity Events
        #endregion

        #region Methods
        public void Initialize()
        {
#if UNITY_ANDROID
            // PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder().Build();
            //
            // PlayGamesPlatform.InitializeInstance(config);
            // // recommended for debugging:
            // PlayGamesPlatform.DebugLogEnabled = true;
            // // Activate the Google Play Games platform
            // PlayGamesPlatform.Activate();
#endif      
        }

        public void Authenticate(System.Action<bool> onCompleted)
        {
// #if UNITY_IOS
//             StartCoroutine(DoAuthenticate(onCompleted));
// #elif UNITY_ANDROID
//             StartCoroutine(DoAuthenticate(onCompleted));
// #else
//             if (onCompleted != null)
//                 onCompleted(false);
// #endif
        }

        // private IEnumerator DoAuthenticate(System.Action<bool> onCompleted)
        // {
        //     bool authenticating = true;
        //     bool authSuccessful = false;
        //     Social.localUser.Authenticate(success =>
        //     {
        //         if (success)
        //             Logger.d("Authentication successful");
        //         else
        //             Logger.d("Authentication failed");
        //
        //         authSuccessful = success;
        //         authenticating = false;
        //     });
        //
        //     while (authenticating)
        //         yield return null;
        //
        //     if (onCompleted != null)
        //         onCompleted(authSuccessful);
        //
        //     m_HasInitialized = authSuccessful;
        // }

        public void ShowLeaderboard(int localHighscore)
        {
            // StartCoroutine(DoShowLeaderboard(localHighscore));
        }

        // private IEnumerator DoShowLeaderboard(int localHighscore)
        // {
        //     if (!m_HasInitialized)
        //     {
        //         bool authenticating = true;
        //         bool authSuccessful = false;
        //         Authenticate(succeed =>
        //         {
        //             authSuccessful = succeed;
        //             authenticating = false;
        //         });
        //         while (authenticating)
        //             yield return null;
        //     }
        //
        //     if (m_HasInitialized)
        //     {
        //         if (localHighscore > 0)
        //         {
        //             bool processing = true;
        //             PostScore(localHighscore, (succeed) =>
        //             {
        //                 processing = false;
        //             });
        //             while (processing)
        //                 yield return null;
        //
        //             Social.ShowLeaderboardUI();
        //         }
        //     }
        // }

        public void PostScore(long score, System.Action<bool> onCompleted = null)
        {
//             if (!m_HasInitialized)
//             {
//                 if (onCompleted != null)
//                     onCompleted(false);
//
//                 return;
//             }
//
// #if UNITY_IOS
//             Social.ReportScore(score, m_Setting.LeaderboardId, success =>
//             {
//                 Logger.d(success ? "Reported score successfully" : "Failed to report score");
//                 if (onCompleted != null)
//                     onCompleted(success);
//             });
// #elif UNITY_ANDROID
//             Social.ReportScore(score, m_Setting.LeaderboardId, success =>
//             {
//                 Logger.d(success ? "Reported score successfully" : "Failed to report score");
//                 if (onCompleted != null)
//                     onCompleted(success);
//             });
// #else
// #endif
        }
        #endregion
    }
}