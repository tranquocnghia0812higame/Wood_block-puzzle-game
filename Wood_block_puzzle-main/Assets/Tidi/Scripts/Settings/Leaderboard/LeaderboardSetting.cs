using Sirenix.OdinInspector;
using UnityEngine;

namespace Tidi.Leaderboard
{
    [CreateAssetMenu(menuName = "Tidi/Leaderboard Setting")]
    public class LeaderboardSetting : ScriptableObject
    {
        protected const string DOCUMENT_URL = "https://github.com/playgameservices/play-games-plugin-for-unity";

        [PropertyOrder(1000)]
        [PropertySpace(SpaceBefore = 10, SpaceAfter = 10)]
        [InfoBox("Please check out official document for setting leaderboard!")]
        [Button]
        public void OpenDocument()
        {
            Application.OpenURL(DOCUMENT_URL);
        }
        
        [PropertySpace(SpaceBefore = 10, SpaceAfter = 5)]
        public string LeaderboardId;
    }
}
