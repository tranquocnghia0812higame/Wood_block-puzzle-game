using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace BP
{
    [RequireComponent(typeof(Image))]
    public class BasePopup : BaseBehaviour
    {
        #region Constants
        protected const float ANIMATION_DURATION = 0.2f;
        #endregion
        
        #region Events
        #endregion

        #region Fields
        #endregion

        #region Properties
        private Canvas m_Canvas;
        private RectTransform m_ContainerRectTrans;
        private CanvasGroup m_ContainerCanvasGroup;
        private Image m_ShadowImage;

        private float m_ScreenWidth;
        private bool m_HasConfigured = false;
        #endregion

        #region Unity Events
        private void OnValidate()
        {
            #if UNITY_EDITOR
            Canvas canvas = GetComponent<Canvas>();
            if (canvas == null)
                Logger.e("Popup misses component Canvas!");

            GameObject contentObject = GameObject.Find("Contents");
            if (contentObject == null)
                Logger.e("Popup misses Contents as child!");

            Image shadowImage = GetComponent<Image>();
            if (shadowImage == null)
                Logger.e("Popup misses component Shadow Image!");
            #endif
        }
        #endregion

        #region Methods
        public virtual void Configure()
        {
            if (m_ContainerRectTrans == null || m_ContainerCanvasGroup == null || m_ShadowImage == null)
            {
                Transform contentTransform = transform.Find("Contents");
                if (contentTransform != null)
                {
                    m_ContainerRectTrans = contentTransform.GetComponent<RectTransform>();
                    m_ContainerCanvasGroup = contentTransform.GetComponent<CanvasGroup>();
                }
                else 
                {
                    Logger.e("Can not find Contents or Canvas Group!!!");
                }

                m_ShadowImage = GetComponent<Image>();
            }

            if (m_Canvas == null)
                m_Canvas = GetComponent<Canvas>();

            if (m_ContainerRectTrans == null || m_ContainerCanvasGroup == null)
            {
                m_HasConfigured = false;
                return;
            }
            else if (m_ShadowImage == null)
            {
                Logger.e("Can not find Shadow Image!!!");
                m_HasConfigured = false;
                return;
            }
            else if (m_Canvas == null)
            {
                Logger.e("Can not find Canvas component!!!");
                m_HasConfigured = false;
                return;
            }
            else 
            {
                m_HasConfigured = true;
            }

            m_ScreenWidth = Screen.width;

            m_ContainerCanvasGroup.alpha = 0;
            m_ContainerCanvasGroup.blocksRaycasts = false;
            m_ContainerRectTrans.anchoredPosition = new Vector2(-m_ScreenWidth, 0f);

            m_ShadowImage.enabled = false;
        }

        public virtual void Show()
        {
            Show(null);
        }

        public virtual void Show(System.Action onCompleted)
        {
            m_Canvas.enabled = true;
            m_ContainerCanvasGroup.alpha = 0;
            m_ContainerCanvasGroup.blocksRaycasts = true;
            m_ContainerRectTrans.anchoredPosition = new Vector2(0f, 0f);

            m_ShadowImage.color = new Color(0,0,0,0);
            m_ShadowImage.enabled = true;

            m_ShadowImage.DOFade(0.78f, ANIMATION_DURATION);
            m_ContainerCanvasGroup.DOFade(1, ANIMATION_DURATION).OnComplete(() => 
            {
                OnShown();

                if (onCompleted != null)
                    onCompleted();
            });
        }

        protected virtual void OnShown() 
        {

        }

        public virtual void Hide()
        {
            Hide(null);
        }

        public virtual void Hide(System.Action onCompleted)
        {
            m_ContainerCanvasGroup.DOFade(0, ANIMATION_DURATION);
            m_ShadowImage.DOFade(0, ANIMATION_DURATION).OnComplete(() => 
            {
                OnHided();
                
                if (onCompleted != null)
                    onCompleted();
            });
        }

        protected virtual void OnHided()
        {
            m_Canvas.enabled = false;
            m_ContainerCanvasGroup.blocksRaycasts = false;
            m_ContainerRectTrans.anchoredPosition = new Vector2(-m_ScreenWidth, 0f);

            m_ShadowImage.enabled = false;
        }
        #endregion
    }
}
