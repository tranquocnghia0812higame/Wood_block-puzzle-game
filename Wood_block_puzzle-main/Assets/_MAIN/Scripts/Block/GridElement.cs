using System.Collections;
using System.Collections.Generic;
using DarkTonic.PoolBoss;
using DG.Tweening;
using UnityEngine;

namespace BP
{
    [System.Serializable]
    public struct GridCoordinate
    {
        public GridCoordinate(int _x, int _y)
        {
            x = _x;
            y = _y;
        }

        public bool Compare(GridCoordinate other)
        {
            return (x == other.x && y == other.y);
        }

        public int x;
        public int y;
    }

    public enum GridElementState
    {
        NORMAL,
        EMPTY_HIGHLIGHT,
        FILLED_HIGHLIGHT,
        FILLED
    }

    public enum BrokenElementState
    {
        NONE = 0,
        MAIN_BROKEN_ELEMENT = 1,
        RELATIVE_BROKEN_ELEMENT = 2
    }

    public class GridElement : BaseBehaviour
    {
        #region Constants
        private readonly Color m_WhiteColor = new Color(1, 1, 1, 1);
        private readonly Color m_TransparentColor = new Color(1, 1, 1, 0);
        private readonly Color m_HalfTransparentColor = new Color(1, 1, 1, 0.5f);
        #endregion

        #region Events
        #endregion

        #region Fields
        [SerializeField]
        private SpriteRenderer m_ForegroundRenderer;
        [SerializeField]
        private SpriteRenderer m_GrayRenderer;

        [Header("Breaking Effect Prefabs")]
        [SerializeField]
        private GameObject m_BreakingEffectPrefab;
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

        private GridElementTypeData m_Data;
        public GridElementTypeData typeData
        {
            get
            {
                return m_Data;
            }
        }

        private GridCoordinate m_Coordinate;
        public GridCoordinate coordinate
        {
            get
            {
                return m_Coordinate;
            }
        }

        private GridElementState m_State;
        public GridElementState state
        {
            get
            {
                return m_State;
            }
        }

        public int breakingOrder
        {
            get;
            set;
        }

        private BrokenElementState m_BrokenState = BrokenElementState.NONE;
        public BrokenElementState brokenState
        {
            get
            {
                return m_BrokenState;
            }
            set
            {
                m_BrokenState = value;
            }
        }

        private Transform m_ForegroundTransform;

        public bool containStar {get; protected set;}
        #endregion

        #region Unity Events
        #endregion

        #region Methods
        public void Configure(GridCoordinate coordinate)
        {
            m_Coordinate = coordinate;
            breakingOrder = -1;
            m_BrokenState = BrokenElementState.NONE;
            m_ForegroundTransform = m_ForegroundRenderer.transform;

            ResetBreakingOrder();
            ClearData();
            ResetRender();
        }

        public void Highlight(GridElementTypeData data)
        {
            m_ForegroundRenderer.sprite = containStar ? data.iconBrightWithStar : data.iconBright;

            if (m_Data != null)
            {
                m_State = GridElementState.FILLED_HIGHLIGHT;
                m_ForegroundRenderer.color = m_WhiteColor;
            }
            else
            {
                m_State = GridElementState.EMPTY_HIGHLIGHT;
                m_ForegroundRenderer.color = m_HalfTransparentColor;
            }
        }

        public void Unhighlight()
        {
            if (m_State == GridElementState.EMPTY_HIGHLIGHT)
            {
                m_ForegroundRenderer.sprite = null;
            }
            else if (m_State == GridElementState.FILLED_HIGHLIGHT)
            {
                m_ForegroundRenderer.sprite = containStar ? m_Data.iconWithStar : m_Data.icon;
            }

            m_ForegroundRenderer.color = m_WhiteColor;
            m_State = GridElementState.NORMAL;
        }

        public void FillWithData(GridElementTypeData data)
        {
            m_Data = data;
            m_ForegroundRenderer.sprite = m_Data.icon;
            m_ForegroundRenderer.color = m_WhiteColor;
            m_State = GridElementState.FILLED;
        }

        public void FillStar()
        {
            containStar = true;
            m_ForegroundRenderer.sprite = m_Data?.iconWithStar;
        }

        public void Break(GridElementTypeData typeData)
        {
            BreakBlockAnimation breakAnimation = PoolBoss.SpawnInPool(m_BreakingEffectPrefab.transform, default(Vector3), default(Quaternion)).GetComponent<BreakBlockAnimation>();
            breakAnimation.VTransform.position = VTransform.position;
            breakAnimation.Break();

            ClearData();
            ResetRender();
        }

        public void BreakByBomb(Vector3 explodingPoint)
        {
            BreakBlockAnimation breakAnimation = PoolBoss.SpawnInPool(m_BreakingEffectPrefab.transform, default(Vector3), default(Quaternion)).GetComponent<BreakBlockAnimation>();
            breakAnimation.VTransform.position = VTransform.position;
            breakAnimation.BreakByBomb(explodingPoint);

            ClearData();
            ResetRender();
        }

        public void HandleLose(Sprite icon, Sprite iconWithStar)
        {
            m_GrayRenderer.sprite = containStar ? iconWithStar : icon;
            m_GrayRenderer.color = m_TransparentColor;
            m_GrayRenderer.DOFade(Random.Range(0.6f, 0.8f), Random.Range(0.2f, 0.4f));
        }

        public void AnimateClean()
        {
            Sequence cleanSeq = DOTween.Sequence();
            cleanSeq.Append(VTransform.DOScale(0, 0.3f));
            cleanSeq.AppendCallback(() =>
            {
                ClearData();
                ResetRender();
                ResetBreakingOrder();
            });
            cleanSeq.Append(VTransform.DOScale(1, 0.3f));
        }

        public void Clear()
        {
            m_ForegroundRenderer.sprite = null;
            m_GrayRenderer.sprite = null;
            m_Data = null;
            m_State = GridElementState.NORMAL;
            m_BrokenState = BrokenElementState.NONE;
        }

        public void ResetRender()
        {
            m_ForegroundRenderer.sprite = null;
            m_ForegroundRenderer.color = m_WhiteColor;
            m_ForegroundTransform.localScale = Vector3.one;
            m_GrayRenderer.sprite = null;
            m_GrayRenderer.color = m_WhiteColor;
        }

        public void ClearData()
        {
            m_Data = null;
            containStar = false;
            m_State = GridElementState.NORMAL;
            m_BrokenState = BrokenElementState.NONE;
        }

        public void ResetBreakingOrder()
        {
            breakingOrder = -1;
        }
        #endregion
    }
}