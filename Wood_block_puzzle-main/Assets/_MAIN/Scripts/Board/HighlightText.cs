using System.Collections;
using System.Collections.Generic;
using DarkTonic.PoolBoss;
using DG.Tweening;
using UnityEngine;

namespace BP
{
    public class HighlightText : BaseBehaviour
    {
        #region Constants
        #endregion
        
        #region Events
        #endregion

        #region Fields
        [SerializeField]
        private SpriteRenderer m_Renderer;
        #endregion

        #region Properties
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
        #endregion

        #region Unity Events
        private void OnValidate()
        {
            if (m_Renderer == null)
                m_Renderer = GetComponent<SpriteRenderer>();
        }
        #endregion

        #region Methods
        public void Show(Sprite sprite)
        {
            VTransform.localScale = Vector2.zero;
            m_Renderer.sprite = sprite;
            m_Renderer.color = Color.white;
            
            Sequence floatingSequence = DOTween.Sequence();
            floatingSequence
                .Append(VTransform.DOScale(Vector2.one, 0.7f).SetEase(Ease.OutElastic))
                .AppendInterval(0.5f)
                .AppendCallback(() => 
                {
                    VTransform.localEulerAngles = new Vector3(0, 0, 7f);
                })
                .Append(VTransform.DOMoveY(VTransform.position.y + 1.5f, 0.2f))
                .Append(VTransform.DOMoveY(VTransform.position.y - 10f, 0.5f))
                .Insert(1.2f, m_Renderer.DOFade(0, 0.5f));
            
            floatingSequence.OnComplete(() =>
            {
                m_Renderer.color = Color.white;
                VTransform.localScale = Vector2.zero;

                PoolBoss.Despawn(VTransform);
            });
        }
        #endregion
    }
}
