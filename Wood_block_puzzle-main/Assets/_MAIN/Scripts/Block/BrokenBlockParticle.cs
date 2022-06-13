using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BP
{
    public class BrokenBlockParticle : BaseBehaviour
    {
        #region Constants
        #endregion
        
        #region Events
        #endregion

        #region Fields
        #endregion

        #region Properties
        #endregion

        #region Unity Events
        private void OnParticleCollision(GameObject other)
        {
            // Random play sound
            float randomFactor = Random.Range(0f, 1f);
            if (randomFactor >= 0.8f)
                SoundManager.Instance.PlayParticleCollision();
        }
        #endregion

        #region Methods
        #endregion
    }
}
