using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BP
{
    public class MultipleResolutionCamera : BaseBehaviour
    {
        #region Constants
        #endregion
        
        #region Events
        #endregion

        #region Fields
        [SerializeField]
        private Camera m_Camera;
        #endregion

        #region Properties
        #endregion

        #region Unity Events
        private void OnValidate()
        {
            if (m_Camera == null)
                m_Camera = GetComponent<Camera>();
        }

        /*
            Some screen resolutions
            A51: 1080 x 2400        0.45
            A01 720 x 1560          0.4615
            Note 10: 2280 x 1080    0.4737    
            9 x 16                  0.5625
            10 x 16                 0.625
            3 x 4                   0.75
        */
        private void Awake()
        {
            float aspectRatio = m_Camera.aspect;
            if (aspectRatio >= 0.55f)
                m_Camera.orthographicSize = 9.84f;
            else if (aspectRatio >= 0.47f)  // Note 10
                m_Camera.orthographicSize = 11.08f;
            else if (aspectRatio >= 0.46f)  // A01
                m_Camera.orthographicSize = 11.75f;
            else if (aspectRatio >= 0.44f)  // A51
                m_Camera.orthographicSize = 11.75f;
            else  
                m_Camera.orthographicSize = 9.84f;
            
        }
        #endregion

        #region Methods
        #endregion
    }
}
