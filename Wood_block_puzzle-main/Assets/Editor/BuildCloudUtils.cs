using UnityEditor;
using UnityEngine;

namespace BP
{
    public class BuildCloudUtils : MonoBehaviour
    {
        #if UNITY_CLOUD_BUILD
        public static void PreExport(UnityEngine.CloudBuild.BuildManifestObject manifest)
        {
            EnableAndroidArchitecturesSplit();
            AutoIncrementVersionCode(manifest);
        }

        private static void EnableAndroidArchitecturesSplit()
        {
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.All;
            PlayerSettings.Android.buildApkPerCpuArchitecture = true;
        }

        private static void AutoIncrementVersionCode(UnityEngine.CloudBuild.BuildManifestObject manifest)
        {
            string buildNumber = manifest.GetValue("buildNumber", "0");
            Debug.LogWarning("Setting build number to " + buildNumber);
            PlayerSettings.Android.bundleVersionCode = int.Parse(buildNumber);
            PlayerSettings.iOS.buildNumber = buildNumber;
        }
        #endif
    }
}