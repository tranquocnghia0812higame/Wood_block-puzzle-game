using DG.Tweening;
using UnityEngine;

namespace BP
{
    public class FlyingStar : MonoBehaviour
    {
        #region Constants
        #endregion
        
        #region Events
        #endregion

        #region Fields
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

        private SpriteRenderer m_Renderer;
        #endregion

        #region Unity Events
        #endregion

        #region Methods
        public void Move(Vector2 targetPosition, System.Action onCompleted)
        {
            if (m_Renderer == null)
                m_Renderer = GetComponent<SpriteRenderer>();

            m_Renderer.enabled = true;
            m_Renderer.color = new Color(1f, 1f, 0f, 1f);
            VTransform.rotation = default(Quaternion);

            m_Renderer.DOFade(0f, 1.3f).SetEase(Ease.InQuint);
            VTransform.DORotate(new Vector3(0f, 0f, 180f), 1.3f, RotateMode.FastBeyond360);
            VTransform
            .DOMove(targetPosition, 1.3f)
            .SetEase(Ease.InBack)
            .OnComplete(() => 
            {
                m_Renderer.enabled = false;

                if (onCompleted != null)
                    onCompleted();
            });
        }
        #endregion
    }
}
