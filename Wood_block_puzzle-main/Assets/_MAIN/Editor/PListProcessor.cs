using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

public class PListProcessor : MonoBehaviour
{
    const string TrackingDescription =
        "This identifier will be used to deliver personalized ads to you. ";
    
#if UNITY_IOS
    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget buildTarget,
        string path)
    {
        string appId = "ca-app-pub-1839004489502882~8320499376"; // Replace with your Admob APP id

        string plistPath = Path.Combine(path, "Info.plist");
        PlistDocument plist = new PlistDocument();

        plist.ReadFromFile(plistPath);
        plist.root.SetString("GADApplicationIdentifier", appId);
        plist.root.SetString("NSUserTrackingUsageDescription", TrackingDescription);
        plist.root.SetBoolean("ITSAppUsesNonExemptEncryption", false);
        PlistElementArray array = plist.root.CreateArray("SKAdNetworkItems");

        string[] ls = new[]
        {
            // admob
            "cstr6suwn9",
            "4fzdc2evr5",
            "2fnua5tdw4",
            "ydx93a7ass",
            "5a6flpkh64",
            "p78axxw29g",
            "v72qych5uu",
            "c6k4g5qg8m",
            "s39g8k73mm",
            "3qy4746246",
            "3sh42y64q3",
            "f38h382jlk",
            "hs6bdukanm",
            "prcb7njmu6",
            "wzmmz9fp6w",
            "yclnxrl5pm",
            "4468km3ulz",
            "t38b2kh725",
            "7ug5zh24hu",
            "9rd848q2bz",
            "n6fk4nfna4",
            "kbd757ywx3",
            "9t245vhmpl",
            "2u9pt9hc89",
            "8s468mfl3y",
            "av6w8kgt66",
            "klf5c3l5u5",
            "ppxm28t8ap",
            "424m5254lk",
            "uw77j35x4d",
            "e5fvkxwrpn",
            "zq492l623r",
            "3qcr597p9d",
            // unity ads
            "4dzt52r2t5",
            "bvpn9ufa9b",
            // applovin
            "24t9a8vw3c",
            "2fnua5tdw4",
            "32z4fx6l9h",
            "3qcr597p9d",
            "3rd42ekr43",
            "3sh42y64q3",
            "424m5254lk",
            "4468km3ulz",
            "4fzdc2evr5",
            "4pfyvq9l8r",
            "523jb4fst2",
            "54nzkqm89y",
            "578prtvx9j",
            "5a6flpkh64",
            "5l3tpt7t6e",
            "5lm9lj6jb7",
            "6xzpu9s2p8",
            "79pbpufp6p",
            "7rz58n8ntl",
            "7ug5zh24hu",
            "8s468mfl3y",
            "9b89h5y424",
            "9nlqeag3gk",
            "9rd848q2bz",
            "9t245vhmpl",
            "av6w8kgt66",
            "c6k4g5qg8m",
            "cg4yq2srnc",
            "cj5566h2ga",
            "cstr6suwn9",
            "cstr6suwn9",
            "ejvt5qm6ak",
            "f38h382jlk",
            "feyaarzu9v",
            "g28c52eehv",
            "ggvn48r87g",
            "glqzh8vgby",
            "gta9lk7p23",
            "hs6bdukanm",
            "k674qkevps",
            "kbd757ywx3",
            "klf5c3l5u5",
            "ludvb6z3bs",
            "m8dbw4sv7c",
            "mlmmfzh3r3",
            "mtkv5xtk9e",
            "n9x2a789qt",
            "p78axxw29g",
            "ppxm28t8ap",
            "prcb7njmu6",
            "pwa73g5rt2",
            "t38b2kh725",
            "tl55sbb4fm",
            "uw77j35x4d",
            "v72qych5uu",
            "wg4vff78zm",
            "wzmmz9fp6w",
            "xy9t38ct57",
            "yclnxrl5pm",
            "ydx93a7ass",
            "zmvfpc5aq8",
            // ironsource
            "su67r6k2v3"
        };

        foreach (var item in ls)
        {
            var dict = array.AddDict();
            dict.SetString("SKAdNetworkIdentifier", $"{item}.skadnetwork");
        }

        File.WriteAllText(plistPath, plist.WriteToString());
    }
#endif
}