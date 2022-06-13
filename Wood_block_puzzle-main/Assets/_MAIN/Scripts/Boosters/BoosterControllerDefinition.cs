using UnityEngine;

namespace BP
{
    [CreateAssetMenu(menuName = "Tools/Booster Controller Definition")]
    public class BoosterControllerDefinition : ScriptableObject
    {
        public GameObject starPrefab;
        public GameObject rotateIconPrefab;
        public GameObject switchIconPrefab;
        public GameObject bombIconPrefab;
    }
}
