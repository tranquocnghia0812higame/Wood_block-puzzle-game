using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tidi.Ads
{
    [CreateAssetMenu(menuName = "Tidi/Ad Setting")]
    public class AdSetting : ScriptableObject
    {
        [BoxGroup("Banner Priority")]
        public List<AdProvider> BannerAdPriority;

        [BoxGroup("Interstitial Priority")]
        public List<AdProvider> InterstitialAdPriority;

        [BoxGroup("Rewarded Video Priority")]
        public List<AdProvider> RewardedVideoAdPriority;

        [BoxGroup("Properties")]
        [InfoBox("Limit on the number of watching rewarded videos per day, per network (like Admob, UnityAds, etc)")]
        public int RewardedVideoPerDay;
    }
}
