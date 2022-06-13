using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace BP
{
    public class GUIHighscore : MonoBehaviour
    {
        #region Constants
        #endregion
        
        #region Events
        #endregion

        #region Fields
        [SerializeField]
        private RectTransform m_CupContainerRectTrans; 
        [SerializeField]
        private Image m_ShadowImage;
        [SerializeField]
        private RectTransform m_HighlightRectTrans;
        [SerializeField]
        private RectTransform m_CupRectTrans;
        [SerializeField]
        private RectTransform[] m_TitleArrayRectTrans;

        [Header("Effects")]
        [SerializeField]
        private GameObject m_FireworkObject;
        [SerializeField]
        private GameObject m_PaperObject;
        #endregion

        #region Properties
        private Canvas m_Canvas;
        #endregion

        #region Unity Events
        #endregion

        #region Methods
        public void Show(System.Action onCompleted)
        {
            if (m_Canvas == null)
                m_Canvas = GetComponent<Canvas>();

            m_Canvas.enabled = true;
            m_ShadowImage.color = new Color(0, 0, 0, 0);
            m_ShadowImage.enabled = true;
            m_CupContainerRectTrans.localScale = new Vector3(0.6f, 0.6f, 1f);
            m_FireworkObject.SetActive(true);
            m_PaperObject.SetActive(false);

            SoundManager.Instance.PlayFireWork();

            for (int i = 0; i < m_TitleArrayRectTrans.Length; i++)
            {
                m_TitleArrayRectTrans[i].localScale = Vector3.zero;
            }

            m_HighlightRectTrans
                .DORotate(new Vector3(0, 0, -360), 10f, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1)
                .SetId("highscore_highlight_rotate");

            Sequence showSeq = DOTween.Sequence();
            showSeq.Append(m_ShadowImage.DOFade(0.9f, 0.2f));
            showSeq.Insert(0f, m_CupContainerRectTrans.DOScale(1f, 0.4f).SetEase(Ease.OutBack));
            showSeq.InsertCallback(0.3f, () => 
            {
                m_PaperObject.SetActive(true);
            });

            for (int i = 0; i < m_TitleArrayRectTrans.Length; i++)
            {
                showSeq.Insert(0.2f + i * 0.02f, m_TitleArrayRectTrans[i].DOScale(1f, 0.5f).SetEase(Ease.OutElastic));
                showSeq.Insert(3f + (m_TitleArrayRectTrans.Length - 1 - i) * 0.02f, m_TitleArrayRectTrans[i].DOScale(0f, 0.2f).SetEase(Ease.InBack));
            }
            
            showSeq.Insert(3.5f, m_CupContainerRectTrans.DOScale(0f, 0.3f).SetEase(Ease.InBack));
            showSeq.Insert(3.5f, m_ShadowImage.DOFade(0f, 0.2f));

            showSeq.OnComplete(() => 
            {
                m_ShadowImage.enabled = false;
                m_Canvas.enabled = false;

                m_FireworkObject.SetActive(false);
                m_PaperObject.SetActive(false);

                DOTween.Kill("highscore_highlight_rotate");

                if (onCompleted != null)
                    onCompleted();
            });
        }
        #endregion
    }
}
