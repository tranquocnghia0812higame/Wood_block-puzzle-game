using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BP
{
    public class GUIMovementItem : BaseBehaviour
    {
        #region Constants
        #endregion
        
        #region Events
        #endregion

        #region Fields
        [SerializeField]
        private RectTransform m_RectTransform;

        [SerializeField]
        private Vector2 m_OriginalAnchorPosition;

        [Button("Set Original anchor from current anchor")]
        public void SetOriginalAnchorFromCurrentAnchor()
        {
            m_OriginalAnchorPosition = m_RectTransform.anchoredPosition;
        }

        [PropertyOrder(3)]
        [Button("Apply Original anchor from current anchor")]
        public void ApplyOriginalAnchorFromCurrentAnchor()
        {
            m_RectTransform.anchoredPosition = m_OriginalAnchorPosition;
        }

        [PropertyOrder(4)]
        [SerializeField]
        private Vector2 m_TargetAnchorPosition;

        [PropertyOrder(5)]
        [Button("Set Target anchor from current anchor")]
        public void SetTargetAnchorFromCurrentAnchor()
        {
            m_TargetAnchorPosition = m_RectTransform.anchoredPosition;
        }

        [PropertyOrder(6)]
        [Button("Apply Target anchor from current anchor")]
        public void ApplyTargetAnchorFromCurrentAnchor()
        {
            m_RectTransform.anchoredPosition = m_TargetAnchorPosition;
        }
        #endregion

        #region Properties
        public RectTransform rectTrans 
        {
            get => m_RectTransform;
        }

        public Vector2 originalAnchorPosition
        {
            get => m_OriginalAnchorPosition;
        }

        public Vector2 targetAnchorPosition
        {
            get => m_TargetAnchorPosition;
        }
        #endregion

        #region Unity Events
        private void OnValidate()
        {
            if (m_RectTransform == null)
                m_RectTransform = GetComponent<RectTransform>();
        }
        #endregion

        #region Methods
        #endregion
    }
}
