using UnityEngine;

namespace BP
{
    public class FlyingBoosterIcon : MonoBehaviour
    {
        [SerializeField]
        private GameObject _trail;

        private Transform m_Transform;
        public Transform VTransform
        {
            get
            {
                if (m_Transform == null)
                    m_Transform = transform;

                return m_Transform;
            }
        }

        public void EnableTrail(bool enable)
        {
            _trail.SetActive(enable);
        }
    }
}
