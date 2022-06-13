using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace BP
{
    public class GUIQuit : BaseBehaviour
    {
        public enum State
        {
            SHOWING, HIDING
        }

        #region Constants
        #endregion

        #region Events
        #endregion

        #region Fields
        [SerializeField]
        private RectTransform m_ContainerRectTrans;
        [SerializeField]
        private Image m_ShadowImage;
        [SerializeField]
        private RectTransform[] m_ButtonTransArray;

        [Header("Animations")]
        [SerializeField]
        private AnimationCurve m_OutBackEase;
        [SerializeField]
        private AnimationCurve m_InBackEase;
        #endregion

        #region Properties
        private State m_State = State.HIDING;
        public State state 
        {
            get 
            {
                return m_State;
            }
        }
        #endregion

        #region Unity Events
        #endregion

        #region Methods
        public void Show()
        {
            m_ContainerRectTrans.gameObject.SetActive(true);
            m_ContainerRectTrans.anchoredPosition = new Vector2(-1000f, 0);
            m_ShadowImage.enabled = true;
            m_ShadowImage.color = new Color(0, 0, 0, 0);

            for (int i = 0; i < m_ButtonTransArray.Length; i++)
                m_ButtonTransArray[i].localScale = Vector3.zero;

            Sequence movingSeq = DOTween.Sequence();
            movingSeq.Append(m_ContainerRectTrans.DOAnchorPosX(0, 0.5f).SetEase(m_OutBackEase));
            movingSeq.Insert(0f, m_ShadowImage.DOFade(0.7f, 0.5f));

            for (int i = 0; i < m_ButtonTransArray.Length; i++)
            {
                movingSeq.Insert(0.5f + i * 0.2f, m_ButtonTransArray[i].DOScale(new Vector2(0.6f, 1.2f), 0.1f));
                movingSeq.Insert(0.6f + i * 0.2f, m_ButtonTransArray[i].DOScale(new Vector2(1.0f, 0.8f), 0.1f));
                movingSeq.Insert(0.7f + i * 0.2f, m_ButtonTransArray[i].DOScale(0.75f, 0.1f));
            }
            
            movingSeq.OnComplete(() => 
            {
                m_State = State.SHOWING;
            });
        }

        public void Hide()
        {
            m_ContainerRectTrans.anchoredPosition = new Vector2(0, 0);

            for (int i = 0; i < m_ButtonTransArray.Length; i++)
                m_ButtonTransArray[i].localScale = Vector3.one * 0.75f;

            Sequence movingSeq = DOTween.Sequence();
            for (int i = m_ButtonTransArray.Length - 1; i >= 0 ; i--)
            {
                movingSeq.Insert(0f + (m_ButtonTransArray.Length - 1 - i) * 0.2f, m_ButtonTransArray[i].DOScale(new Vector2(0.6f, 1.2f), 0.1f));
                movingSeq.Insert(0.1f + (m_ButtonTransArray.Length - 1 - i) * 0.2f, m_ButtonTransArray[i].DOScale(new Vector2(1.0f, 0.8f), 0.1f));
                movingSeq.Insert(0.2f + (m_ButtonTransArray.Length - 1 - i) * 0.2f, m_ButtonTransArray[i].DOScale(0f, 0.1f));
            }

            movingSeq.Insert(0.5f, m_ContainerRectTrans.DOAnchorPosX(-1000f, 0.5f).SetEase(m_InBackEase));
            movingSeq.Insert(0.5f, m_ShadowImage.DOFade(0.0f, 0.5f));
            movingSeq.OnComplete(() =>
            {
                m_ShadowImage.enabled = false;
                m_ContainerRectTrans.gameObject.SetActive(false);
                m_State = State.HIDING;
            });
        }

        public void Resume()
        {
            Hide();
        }

        public void Quit()
        {
#if !UNITY_EDITOR
            Application.Quit();
#endif
        }
        #endregion
    }
}