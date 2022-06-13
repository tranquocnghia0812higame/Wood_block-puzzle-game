using UnityEngine;

namespace BP
{
    public class GUIGameOverEvent : MonoBehaviour
    {
        [SerializeField]
        private GUIGameOver m_Manager;

        public void OnHided()
        {
            m_Manager.OnHided();
        }
    }
}
