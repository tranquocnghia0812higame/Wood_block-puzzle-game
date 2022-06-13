using System.Text;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Tidi.Ads
{
    public enum AdProvider
    {
        Admob,
        UnityAds,
        FacebookAudienceNetwork,
        IronSource
    }

    [CreateAssetMenu(menuName = "Tidi/Base Ad Unit Setting")]
    public class BaseAdUnitSetting : ScriptableObject
    {
        protected const string ADMOB_DOCUMENT_URL = "https://developers.google.com/admob/unity/quick-start";
        protected const string UNITY_ADS_DOCUMENT_URL = "https://unityads.unity3d.com/help/monetization/getting-started";
        protected const string FAN_DOCUMENT_URL = "https://developers.facebook.com/docs/audience-network/overview";
        protected const string IRONSOURCE_DOCUMENT_URL = "https://developers.ironsrc.com/ironsource-mobile/unity/unity-plugin/#step-1";

        protected const string ADMOB_DEFINE_SYMBOL = "USING_ADMOB";
        protected const string UNITY_ADS_DEFINE_SYMBOL = "USING_UNITY_ADS";
        protected const string FAN_DEFINE_SYMBOL = "USING_FAN";
        protected const string IRONSOURCE_DEFINE_SYMBOL = "USING_IRONSOURCE";

        [PropertyOrder(1000)]
        [PropertySpace(SpaceBefore = 10, SpaceAfter = 10)]
        [InfoBox("Please check out official document before setting ads")]
        [Button]
        public void OpenOfficialDocument()
        {
            if (Provider == AdProvider.Admob)
                Application.OpenURL(ADMOB_DOCUMENT_URL);
            else if (Provider == AdProvider.UnityAds)
                Application.OpenURL(UNITY_ADS_DOCUMENT_URL);
            else if (Provider == AdProvider.FacebookAudienceNetwork)
                Application.OpenURL(FAN_DOCUMENT_URL);
            else if (Provider == AdProvider.IronSource)
                Application.OpenURL(IRONSOURCE_DOCUMENT_URL);
        }

        [HideInInspector]
        public bool UsingThisNetwork;

        [HideIf("UsingThisNetwork")]
        [PropertySpace(SpaceBefore = 5, SpaceAfter = 10)]
        [PropertyOrder(-1)]
        [Button(ButtonSizes.Large), GUIColor(0, 1, 0)]
        public void EnableNetwork()
        {
            UsingThisNetwork = true;

#if UNITY_EDITOR
#if UNITY_ANDROID
            BuildTargetGroup targetGroup = BuildTargetGroup.Android;
#elif UNITY_IOS
            BuildTargetGroup targetGroup = BuildTargetGroup.iOS;
#endif
            string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
            var symbolList = symbols.Split(';').ToList();
            string providerSymbol = GetDefineSymbolByProvider();
            if (!string.IsNullOrEmpty(providerSymbol))
            {
                bool containSymbol = false;
                for (int i = 0; i < symbolList.Count; i++)
                {
                    if (string.Compare(symbolList[i], providerSymbol) == 0)
                    {
                        containSymbol = true;
                    }
                }

                if (!containSymbol)
                {
                    symbolList.Add(providerSymbol);
                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < symbolList.Count; i++)
                    {
                        builder.Append(symbolList[i]).Append(";");

                        if (i == symbolList.Count - 1)
                            builder.Length--;
                    }

                    PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, builder.ToString());
                }
            }
#endif
        }

        [ShowIf("UsingThisNetwork")]
        [PropertySpace(SpaceBefore = 5, SpaceAfter = 10)]
        [PropertyOrder(-1)]
        [Button(ButtonSizes.Large), GUIColor(1, 1, 0)]
        public void DisableNetwork()
        {
            UsingThisNetwork = false;

#if UNITY_EDITOR
#if UNITY_ANDROID
            BuildTargetGroup targetGroup = BuildTargetGroup.Android;
#elif UNITY_IOS
            BuildTargetGroup targetGroup = BuildTargetGroup.iOS;
#endif
            string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
            var symbolList = symbols.Split(';').ToList();
            string providerSymbol = GetDefineSymbolByProvider();
            if (!string.IsNullOrEmpty(providerSymbol))
            {
                for (int i = symbolList.Count - 1; i >= 0; i--)
                {
                    if (string.Compare(symbolList[i], providerSymbol) == 0)
                    {
                        symbolList.RemoveAt(i);
                    }
                }

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < symbolList.Count; i++)
                {
                    builder.Append(symbolList[i]).Append(";");

                    if (i == symbolList.Count - 1)
                        builder.Length--;
                }

                PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, builder.ToString());
            }
#endif
        }

        [EnableIf("UsingThisNetwork")]
        [BoxGroup("Properties")]
        public AdProvider Provider;
        public string ProviderToString
        {
            get
            {
                if (Provider == AdProvider.Admob)
                    return "Admob";
                else if (Provider == AdProvider.FacebookAudienceNetwork)
                    return "Fan";
                else if (Provider == AdProvider.IronSource)
                    return "IronSource";
                else if (Provider == AdProvider.UnityAds)
                    return "UnityAds";
                else
                    return "Unknown";
            }
        }

        [EnableIf("UsingThisNetwork")]
        [BoxGroup("Properties")]
        public string GameId;
        [EnableIf("UsingThisNetwork")]
        [BoxGroup("Properties")]
        public string GameIdIOS;
        [EnableIf("UsingThisNetwork")]
        [BoxGroup("Properties")]
        public string BannerId;
        [EnableIf("UsingThisNetwork")]
        [BoxGroup("Properties")]
        public string InterstitialId;
        [EnableIf("UsingThisNetwork")]
        [BoxGroup("Properties")]
        public string RewardedVideoId;

        private string GetDefineSymbolByProvider()
        {
            if (Provider == AdProvider.Admob)
                return ADMOB_DEFINE_SYMBOL;
            else if (Provider == AdProvider.FacebookAudienceNetwork)
                return FAN_DEFINE_SYMBOL;
            else if (Provider == AdProvider.IronSource)
                return IRONSOURCE_DEFINE_SYMBOL;
            else if (Provider == AdProvider.UnityAds)
                return UNITY_ADS_DEFINE_SYMBOL;
            else
                return "";
        }
    }
}
