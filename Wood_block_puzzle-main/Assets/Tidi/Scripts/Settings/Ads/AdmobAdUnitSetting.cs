using Sirenix.OdinInspector;
using UnityEngine;
#if USING_ADMOB && UNITY_EDITOR
using GoogleMobileAds.Editor;
#endif

namespace Tidi.Ads
{
    [CreateAssetMenu(menuName = "Tidi/Admob Ad Unit Setting")]
    public class AdmobAdUnitSetting : BaseAdUnitSetting
    {
        [PropertySpace(SpaceBefore = 10, SpaceAfter = 10)]
        [InfoBox("Google use a special setting for Admob. You should complete filling App Id from there!", InfoMessageType.Warning)]
        [Button]
        public void OpenAdmobSettings()
        {
            #if USING_ADMOB && UNITY_EDITOR
            GoogleMobileAdsSettingsEditor.OpenInspector();
            #endif
        }
    }
}
