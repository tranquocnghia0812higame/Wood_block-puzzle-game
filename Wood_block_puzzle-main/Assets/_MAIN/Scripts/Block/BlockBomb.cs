using DarkTonic.PoolBoss;
using DG.Tweening;
using UnityEngine;

namespace BP
{
    public class BlockBomb : BaseBehaviour
    {
        [SerializeField]
        private GameObject m_ExplodePrefab;

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

        private GameObject m_GameObject;
        public GameObject VGameObject
        {
            get
            {
                if (m_GameObject == null)
                    m_GameObject = gameObject;

                return m_GameObject;
            }
        }

        private Vector3 m_OriginalPosition;

        private bool m_HandMoving = false;
        private bool m_ShouldMove = false;
        private bool m_HandReleased = false;
        private bool m_ShouldMoveBack = false;
        private bool m_ShouldBePlaced = false;
        private Vector3 m_LastTargetPosition;
        private Vector3 m_TargetPosition;

        #region Unity Functions
        private void LateUpdate()
        {
            if (!m_ShouldMove)
                return;

            float speed = 150f;
            if (m_HandReleased)
            {
                if (m_ShouldBePlaced)
                    speed = 4f;
                else if (m_ShouldMoveBack)
                    speed = 60f;
                else
                    speed = 3f;
            }
            else if (!m_HandMoving)
                speed = 60f;

            var delta = speed * Time.deltaTime;
            float sqrDistance = Vector3.SqrMagnitude(m_TargetPosition - VTransform.position);

            VTransform.position = Vector3.MoveTowards(VTransform.position, m_TargetPosition, delta);

            if (sqrDistance <= 0.4f && m_ShouldMoveBack)
            {
                VTransform.position = m_TargetPosition;
                m_ShouldMove = false;
                m_ShouldMoveBack = false;
            }

            if (sqrDistance <= 0.05f && m_ShouldBePlaced)
            {
                m_ShouldBePlaced = false;
            }
        }
        #endregion

        #region Methods
        public void Configure()
        {
            
        }

        public void ConfigureOriginalPosition(Vector3 localPos)
        {
            VTransform.localPosition = localPos;
            m_OriginalPosition = VTransform.position;
        }

        public void HandleSelected(Vector3 worldPos)
        {
            m_ShouldMoveBack = false;
            m_HandMoving = false;
            m_HandReleased = false;
            m_ShouldBePlaced = false;

            m_TargetPosition = worldPos;
            m_TargetPosition.y += 3.5f;
            m_LastTargetPosition = m_TargetPosition;

            m_ShouldMove = true;

            Scale(1, 0.2f);
        }

        public void HandleMoving(Vector3 worldPos)
        {
            m_TargetPosition = worldPos;
            m_TargetPosition.y += 3.5f;

            if (Vector3.Distance(m_TargetPosition, m_LastTargetPosition) < 0.2f && !m_HandMoving)
            {
                m_ShouldMove = true;
            }
            else
            {
                if (!m_HandMoving)
                    Scale(1, 0.05f);

                m_HandMoving = true;

                m_ShouldMove = true;
            }
        }

        public void HandleRelease()
        {
            m_HandReleased = true;
        }

        public void MoveBack()
        {
            m_ShouldMoveBack = true;
            m_TargetPosition = m_OriginalPosition;

            Scale(1f, 0.2f);
        }

        public void Explode()
        {
            var explodeEffect = PoolBoss.SpawnInPool(m_ExplodePrefab.transform, VTransform.position, default(Quaternion));
            explodeEffect.transform.localScale = m_ExplodePrefab.transform.localScale;

            Reset();
            PoolBoss.Despawn(VTransform);
        }

        public void Scale(float blockScale, float duration, System.Action onComplete = null)
        {
            VTransform.DOScale(blockScale, duration).OnComplete(() =>
            {
                if (onComplete != null)
                    onComplete();
            });
        }

        public void Reset()
        {
            m_ShouldMove = false;
            m_ShouldMoveBack = false;
        }
        #endregion
    }
}
