using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BP
{
    public class Preload : BaseBehaviour
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
        private void Awake()
        {
            SceneManager.LoadScene("GamePlay");
        }
        #endregion

        #region Methods
        #endregion
    }
}
