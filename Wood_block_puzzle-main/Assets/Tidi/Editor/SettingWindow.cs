using System;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using Tidi.Ads;
using UnityEditor;
using UnityEngine;

namespace Tidi
{
    public class SettingWindow : OdinMenuEditorWindow
    {
        [MenuItem("Tools/Tidi/Settings")]
        private static void Open()
        {
            var window = GetWindow<SettingWindow>();
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 500);
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree(true);
            tree.DefaultMenuStyle.IconSize = 28.00f;
            tree.Config.DrawSearchToolbar = true;

            tree.AddAssetAtPath("Ads", "Assets/Tidi/Settings/Ads/AdSetting.asset");

            // Adds all ads.
            tree.AddAllAssetsAtPath("Ads", "Assets/Tidi/Settings/Ads", typeof(BaseAdUnitSetting), true, true);

            tree.AddAssetAtPath("Leaderboard", "Assets/Tidi/Settings/Leaderboard/LeaderboardSetting.asset");

            return tree;
        }
    }
}
