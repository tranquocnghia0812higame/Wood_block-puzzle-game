using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace BP
{
    public class GUIToggle : MonoBehaviour
    {
        #region Constants
        private const float ANIMATION_DURATION = 0.2f;
        #endregion
        
        #region Events
        #endregion

        #region Fields
        [SerializeField]
        private RectTransform m_KnotRectTrans;
        [SerializeField]
        private Image m_OnKnotImage;
        [SerializeField]
        private Image m_OffKnotImage;

        [SerializeField]
        private TMPro.TextMeshProUGUI m_InfoText;

        [Header("Colors")]
        [SerializeField]
        private Color32 m_OnColor;
        [SerializeField]
        private Color32 m_OffColor;
        #endregion

        #region Properties
        private RectTransform m_RectTrans;
        #endregion

        #region Unity Events
        #endregion

        #region Methods
        public void Configure(bool isOn)
        {
            if (m_RectTrans == null)
                m_RectTrans = GetComponent<RectTransform>();

            if (isOn)
            {
                m_InfoText.text = "On";
                m_InfoText.color = m_OnColor;

                m_OnKnotImage.color = new Color(1, 1, 1, 1);
                m_OffKnotImage.color = new Color(1, 1, 1, 0);
                
                m_KnotRectTrans.anchoredPosition = new Vector2(m_RectTrans.sizeDelta.x / 2f - 5f, 0f);
            }
            else 
            {
                m_InfoText.text = "OFF";
                m_InfoText.color = m_OffColor;

                m_OnKnotImage.color = new Color(1, 1, 1, 0);
                m_OffKnotImage.color = new Color(1, 1, 1, 1);

                m_KnotRectTrans.anchoredPosition = new Vector2(-m_RectTrans.sizeDelta.x / 2f + 5f, 0f);
            }
        }
        
        public void AnimateOn()
        {
            if (m_RectTrans == null)
                m_RectTrans = GetComponent<RectTransform>();

            m_OnKnotImage.color = new Color(1, 1, 1, 0);
            m_OffKnotImage.color = new Color(1, 1, 1, 1);

            m_OnKnotImage.DOFade(1, ANIMATION_DURATION);
            m_OffKnotImage.DOFade(0, ANIMATION_DURATION);

            m_InfoText.DOColor(m_OnColor, ANIMATION_DURATION);
            m_InfoText.text = "ON";

            m_KnotRectTrans.DOAnchorPosX(m_RectTrans.sizeDelta.x / 2f - 5f, ANIMATION_DURATION);
        }

        public void AnimateOff()
        {
            if (m_RectTrans == null)
                m_RectTrans = GetComponent<RectTransform>();

            m_OnKnotImage.color = new Color(1, 1, 1, 1);
            m_OffKnotImage.color = new Color(1, 1, 1, 0);

            m_OnKnotImage.DOFade(0, ANIMATION_DURATION);
            m_OffKnotImage.DOFade(1, ANIMATION_DURATION);

            m_InfoText.DOColor(m_OffColor, ANIMATION_DURATION);
            m_InfoText.text = "OFF";

            m_KnotRectTrans.DOAnchorPosX(-m_RectTrans.sizeDelta.x / 2f + 5f, ANIMATION_DURATION);
        }
        #endregion
    }
}
