using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace BP
{
    public class ButtonBooster : MonoBehaviour
    {
        [SerializeField]
        private Collider2D m_Collider;

        [SerializeField]
        private Transform _boosterIconTransform;
        public Transform pBoosterIconTransform => _boosterIconTransform;

        [SerializeField]
        private GameObject m_PlusObject;

        [SerializeField]
        private SpriteRenderer m_DisableRenderer;
        [SerializeField]
        private SpriteRenderer m_HighlightRenderer;

        [SerializeField]
        private GameObject m_InfoGroupObject;
        [SerializeField]
        private SpriteRenderer m_CancelButtonRenderer;

        [SerializeField]
        private TMPro.TextMeshPro _rotateCountText;

        [Header("Effects")]
        [SerializeField]
        private GameObject _receivedEffect;

        public void Init()
        {
            _receivedEffect.SetActive(false);
        }

        public void AnimateReceivingBoosterIcon()
        {
            StartCoroutine(YieldAnimateReceivingBoosterIcon());
        }

        private IEnumerator YieldAnimateReceivingBoosterIcon()
        {
            SoundManager.Instance.PlayGotSpin();
            
            _receivedEffect.SetActive(true);
            yield return new WaitForSeconds(1.5f);
            _receivedEffect.SetActive(false);
        }

        public void UpdateBoosterRemainingCount(int count)
        {
            _rotateCountText.text = $"{count}";
        }

        public void Disable()
        {
            m_DisableRenderer.enabled = true;
            m_Collider.enabled = false;
            
            if (m_PlusObject != null)
                m_PlusObject.SetActive(false);
        }

        public void Enable()
        {
            m_DisableRenderer.enabled = false;
            m_Collider.enabled = true;

            if (m_PlusObject != null)
                m_PlusObject?.SetActive(true);
        }

        public void TurnOnHighlight()
        {
            m_HighlightRenderer.color = new Color(1, 1, 1, 0);
            m_HighlightRenderer.enabled = true;

            DOTween.Kill("button_booster_highlight");
            Sequence highlightSeq = DOTween.Sequence();
            highlightSeq.SetId("button_booster_highlight");
            highlightSeq.Append(m_HighlightRenderer.DOFade(1f, 1f));
            highlightSeq.Append(m_HighlightRenderer.DOFade(0f, 1f));
            highlightSeq.SetLoops(-1);
        }

        public void TurnOffHighlight()
        {
            DOTween.Kill("button_booster_highlight");
            m_HighlightRenderer.enabled = false;
        }

        public void UseBomb()
        {
            m_InfoGroupObject.SetActive(false);
            m_CancelButtonRenderer.enabled = true;

            TurnOnHighlight();
        }

        public void CancelBomb()
        {
            m_InfoGroupObject.SetActive(true);
            m_CancelButtonRenderer.enabled = false;

            TurnOffHighlight();
        }

        public void UseRotate()
        {
            TurnOnHighlight();
        }

        public void CancelRotate()
        {
            TurnOffHighlight();
        }
    }
}
