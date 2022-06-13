using System.Collections;
using System.Collections.Generic;
using DarkTonic.PoolBoss;
using DG.Tweening;
using UnityEngine;

namespace BP
{
    public class FloatingScore : BaseBehaviour
    {
        #region Constants
        #endregion

        #region Events
        #endregion

        #region Fields
        [SerializeField]
        private TMPro.TextMeshPro m_ScoreText;
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
        #endregion

        #region Methods
        public void Show(int score, GridElementType type)
        {
            VTransform.localScale = Vector2.zero;

            m_ScoreText.text = string.Format("+{0}", score);
            m_ScoreText.color = Color.white;
            
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
                .Insert(1.2f, m_ScoreText.DOFade(0, 0.5f));
            
            floatingSequence.OnComplete(() =>
            {
                m_ScoreText.text = "";
                m_ScoreText.alpha = 1;

                VTransform.localScale = Vector2.zero;

                PoolBoss.Despawn(VTransform);
            });
        }
        #endregion
    }
}