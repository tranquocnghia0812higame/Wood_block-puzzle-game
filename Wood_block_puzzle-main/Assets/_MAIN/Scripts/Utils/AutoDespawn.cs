using System.Collections;
using DarkTonic.PoolBoss;
using UnityEngine;

namespace BP
{
    public class AutoDespawn : MonoBehaviour
    {
        [SerializeField]
        private float m_DelayTime = 4f;

        private void OnEnable()
        {
            StartCoroutine(YieldDespawn());
        }

        private IEnumerator YieldDespawn()
        {
            yield return new WaitForSeconds(m_DelayTime);

            PoolBoss.Despawn(transform);
        }
    }
}
