using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace BP
{
    public class ChestPopup : MonoBehaviour
    {
        [SerializeField]
        private SpriteRenderer _iconRenderer;
        [SerializeField]
        private TMPro.TextMeshPro _amountText;

        [Header("Booster Icons")]
        [SerializeField]
        private Sprite _iconRotateSprite;
        [SerializeField]
        private Sprite _iconSwitchSprite;
        [SerializeField]
        private Sprite _iconBombSprite;

        private Transform m_Transform;
        public Transform VTransform
        {
            get
            {
                if (m_Transform == null)
                    m_Transform = transform;

                return m_Transform;
            }
        }

        bool _showing = false;
        public bool pIsShowing => _showing;

        private void Update()
        {
            if (!_showing)
                return;

            if (Input.GetMouseButtonDown(0))
            {
                // Hide chest popup
                Hide();
            }
        }

        public void Init()
        {
            VTransform.localScale = Vector3.zero;
        }

        public void UpdateInfo(BoosterType type, int amount)
        {
            _amountText.text = $"x{amount}";

            if (type == BoosterType.ROTATE)
            {
                _iconRenderer.sprite = _iconRotateSprite;
            }
            else if (type == BoosterType.SWITCH)
            {
                _iconRenderer.sprite = _iconSwitchSprite;
            }
            else if (type == BoosterType.BOMB)
            {
                _iconRenderer.sprite = _iconBombSprite;
            }
        }

        public void Show()
        {
            if (_showing)
                return;

            _showing = true;
            VTransform.DOScale(1, 0.5f).SetEase(Ease.OutBack);
        }

        public void ShowThenAutomaticHide()
        {
            StartCoroutine(YieldShowThenAutomaticHide());
        }

        private IEnumerator YieldShowThenAutomaticHide()
        {
            Show();
            yield return new WaitForSeconds(3f);
            Hide();
        }

        public void Hide()
        {
            VTransform.DOScale(0, 0.3f).SetEase(Ease.InBack).OnComplete(() => 
            {
                _showing = false;
            });
        }
    }
}
