using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace BP
{
    public class GUILoading : BaseBehaviour
    {
        #region Constants
        #endregion
        
        #region Events
        #endregion

        #region Fields
        [SerializeField]
        private Image m_ProgressImage;
        #endregion

        #region Properties
        private Canvas m_Canvas;
        #endregion

        #region Unity Events
        #endregion

        #region Methods
        public void Show()
        {
            if (m_Canvas == null)
                m_Canvas = GetComponent<Canvas>();

            m_Canvas.enabled = true;
            m_ProgressImage.fillAmount = 0f;
        }

        public void Hide()
        {
            if (m_Canvas == null)
                m_Canvas = GetComponent<Canvas>();

            m_Canvas.enabled = false;
        }
        
        public void UpdateProgress(float progressPercentage, float duration = 0.5f, System.Action onComplete = null)
        {
            float fillAmount = (float)progressPercentage / 100f;
            m_ProgressImage.DOFillAmount(fillAmount, duration).OnComplete(() => 
            {
                if (onComplete != null)
                    onComplete();
            });
        }
        #endregion
    }
}
