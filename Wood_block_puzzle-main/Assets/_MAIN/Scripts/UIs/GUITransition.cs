using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace BP
{
    public class GUITransition : MonoBehaviour
    {
        #region Constants
        #endregion
        
        #region Events
        #endregion

        #region Fields
        [SerializeField]
        private Image m_ShadowImage;
        #endregion

        #region Properties
        #endregion

        #region Unity Events
        #endregion

        #region Methods
        public void Configure()
        {
            m_ShadowImage.enabled = false;
            m_ShadowImage.color = new Color(0,0,0,0);
        }

        public void Show(System.Action onCompleted = null)
        {
            m_ShadowImage.enabled = true;
            m_ShadowImage.DOFade(1f, 0.2f).OnComplete(() => 
            {
                if (onCompleted != null)
                    onCompleted();
            });
        }

        public void Hide(System.Action onCompleted = null)
        {
            m_ShadowImage.DOFade(0f, 0.2f).OnComplete(() => 
            {
                if (onCompleted != null)
                    onCompleted();

                m_ShadowImage.enabled = false;
            });
        }
        #endregion
    }
}
