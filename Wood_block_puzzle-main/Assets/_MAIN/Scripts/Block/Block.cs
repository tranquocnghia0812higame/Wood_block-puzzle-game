using System.Collections;
using System.Collections.Generic;
using DarkTonic.PoolBoss;
using DG.Tweening;
using SimpleJSON;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BP
{
    public enum BlockAngle
    {
        ANGLE_0 = 0,
        ANGLE_90 = 90,
        ANGLE_180 = 180,
        ANGLE_270 = 270
    }

    public class Block : BaseBehaviour
    {
        #region Constants
        #endregion

        #region Events
        private System.Action m_OnPlacedBlock;
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

        private List<SpriteRenderer> m_ElementRenderers;
        private List<Transform> m_ElementTransforms;

        public List<Transform> elements
        {
            get
            {
                return m_ElementTransforms;
            }
        }

        private string m_Id;
        public string Id
        {
            get
            {
                return m_Id;
            }
            set
            {
                m_Id = value;
            }
        }

        private BlockAngle m_Angle;
        public BlockAngle blockAngle => m_Angle;
        private BlockAngle m_OriginalAngle;

        [SerializeField]
        private List<GridCoordinate> m_RelativeGridCoordinates;
        public List<GridCoordinate> relativeGridCoordinates
        {
            get
            {
                return m_RelativeGridCoordinates;
            }
        }

        private GridElementTypeData m_TypeData;
        public GridElementTypeData typeData
        {
            get
            {
                return m_TypeData;
            }
        }

        private Rect m_BlockRect;

        private Vector3 m_OriginalPosition;

        private bool m_HandMoving = false;
        private bool m_ShouldMove = false;
        private bool m_HandReleased = false;
        private bool m_ShouldMoveBack = false;
        private bool m_ShouldBePlaced = false;
        private Vector3 m_LastTargetPosition;
        private Vector3 m_TargetPosition;

        private bool m_IsRotateMode;

        public List<Vector3> starWorldPositions
        {
            get
            {
                List<Vector3> positions = new List<Vector3>();
                for (int i = 0; i < m_StarCoordinates.Count; i++)
                {
                    GridCoordinate coordinate = m_StarCoordinates[i];
                    Vector3 localPosition = m_ElementTransforms[0].localPosition + new Vector3(coordinate.x, coordinate.y, 0);
                    positions.Add(VTransform.TransformPoint(localPosition));
                }

                return positions;
            }
        }
        private List<GridCoordinate> m_StarCoordinates;
        #endregion

        #region Unity Events
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
                if (m_OnPlacedBlock != null)
                    m_OnPlacedBlock();
            }
        }
        #endregion

        #region Methods
        public void Configure(BlockAngle targetAngle, List<GridCoordinate> starCoords)
        {
            m_ElementRenderers = new List<SpriteRenderer>();
            m_ElementTransforms = new List<Transform>();
            m_RelativeGridCoordinates = new List<GridCoordinate>();
            m_StarCoordinates = new List<GridCoordinate>(starCoords);

            Transform cacheTrans = transform;
            for (int i = 0; i < cacheTrans.childCount; i++)
            {
                Transform child = cacheTrans.GetChild(i);
                SpriteRenderer childRenderer = child.GetComponent<SpriteRenderer>();
                if (childRenderer == null)
                    continue;

                child.localScale = Vector3.one;
                child.localPosition = GetChildLocalPositionByAngle(m_Angle, targetAngle, child.localPosition);

                m_ElementTransforms.Add(child);
                m_ElementRenderers.Add(childRenderer);

                childRenderer.sortingOrder = 15;

                Vector2 offset = child.localPosition - m_ElementTransforms[0].localPosition;
                GridCoordinate coordinate = new GridCoordinate((int)offset.x, (int)offset.y);
                m_RelativeGridCoordinates.Add(coordinate);
            }

            m_Angle = targetAngle;

            CalculateBlockRect();
        }

        public void ConfigureOriginalPosition(Vector3 localPos)
        {
            VTransform.localPosition = localPos;
            m_OriginalPosition = VTransform.position;
        }

        public void ConfigureType(GridElementTypeData typeData)
        {
            m_TypeData = typeData;
            for (int i = 0; i < m_ElementRenderers.Count; i++)
            {
                bool containStar = false;
                for (int j = 0; j < m_StarCoordinates.Count; j++)
                {
                    if (m_StarCoordinates[j].Compare(m_RelativeGridCoordinates[i]))
                    {
                        containStar = true;
                    }
                }

                m_ElementRenderers[i].sprite = containStar ? m_TypeData.iconWithStar : m_TypeData.icon;
                m_ElementRenderers[i].color = Color.white;
            }
        }

        private void CalculateBlockRect()
        {
            if (m_ElementTransforms.Count == 0)
                return;

            Vector2 bottomLeftPoint = m_ElementTransforms[0].localPosition;
            Vector2 topRightPoint = m_ElementTransforms[0].localPosition;
            for (int i = 0; i < m_ElementTransforms.Count; i++)
            {
                Vector2 localPos = m_ElementTransforms[i].localPosition;
                if (localPos.x < bottomLeftPoint.x)
                    bottomLeftPoint.x = localPos.x;

                if (localPos.y < bottomLeftPoint.y)
                    bottomLeftPoint.y = localPos.y;

                if (localPos.x > topRightPoint.x)
                    topRightPoint.x = localPos.x;

                if (localPos.y > topRightPoint.y)
                    topRightPoint.y = localPos.y;
            }

            Vector2 centerPos = (topRightPoint + bottomLeftPoint) / 2f;
            Vector2 size = new Vector2(topRightPoint.x - bottomLeftPoint.x, topRightPoint.y - bottomLeftPoint.y);
            m_BlockRect = new Rect(centerPos, size);
        }

        public void CalculateChildrenPositions()
        {
            for (int i = 0; i < m_ElementTransforms.Count; i++)
            {
                m_ElementTransforms[i].localPosition -= (Vector3)m_BlockRect.position;
                m_ElementTransforms[i].name = "Element";
            }
        }

        [Button("Rotate 90 degree")]
        public void Rotate90Degree()
        {
            BlockAngle newAngle = (BlockAngle)(((int)m_Angle + 90) % 360);
            Rotate(newAngle, true);
        }

        public void Rotate(BlockAngle angle, bool withAnimation)
        {
            bool[] containStarArray = new bool[m_RelativeGridCoordinates.Count];
            for (int i = 0; i < m_RelativeGridCoordinates.Count; i++)
            {
                bool containStar = false;
                for (int j = 0; j < m_StarCoordinates.Count; j++)
                {
                    if (m_RelativeGridCoordinates[i].Compare(m_StarCoordinates[j]))
                    {
                        containStar = true;
                        break;
                    }
                }
                containStarArray[i] = containStar;
            }


            m_RelativeGridCoordinates.Clear();
            m_StarCoordinates.Clear();
            for (int i = 0; i < m_ElementTransforms.Count; i++)
            {
                Transform child = m_ElementTransforms[i];

                child.localPosition = GetChildLocalPositionByAngle(m_Angle, angle, child.localPosition);

                Vector2 offset = child.localPosition - m_ElementTransforms[0].localPosition;
                GridCoordinate coordinate = new GridCoordinate((int)offset.x, (int)offset.y);
                m_RelativeGridCoordinates.Add(coordinate);

                if (containStarArray[i])
                    m_StarCoordinates.Add(coordinate);
            }

            m_Angle = angle;

            CalculateBlockRect();

            if (withAnimation)
            {
                m_Transform.eulerAngles = new Vector3(0, 0, 90);
                m_Transform.DORotate(Vector3.zero, 0.2f, RotateMode.FastBeyond360);
            }

            GameManager.Instance.CheckAvailableSpaceForBlocks();
        }

        public void TurnRotateMode(bool enable)
        {
            m_IsRotateMode = enable;

            if (enable)
            {
                m_OriginalAngle = m_Angle;
            }
        }

        public void RevertOriginalRotation()
        {
            Rotate(m_OriginalAngle, false);
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

            if (!m_IsRotateMode)
            {
                m_ShouldMove = true;

                ScaleWithChildren(1, 0.9f, 0.2f);

                for (int i = 0; i < m_ElementRenderers.Count; i++)
                {
                    bool containStar = false;
                    for (int j = 0; j < m_StarCoordinates.Count; j++)
                    {
                        if (m_StarCoordinates[j].Compare(m_RelativeGridCoordinates[i]))
                        {
                            containStar = true;
                        }
                    }

                    m_ElementRenderers[i].sprite = containStar ? m_TypeData.iconBrightWithStar : m_TypeData.iconBright;
                    m_ElementRenderers[i].sortingOrder = 36;
                }
            }
        }

        public void HandleMoving(Vector3 worldPos)
        {
            m_TargetPosition = worldPos;
            m_TargetPosition.y += 3.5f;

            if (Vector3.Distance(m_TargetPosition, m_LastTargetPosition) < 0.2f && !m_HandMoving)
            {
                if (!m_IsRotateMode)
                {
                    m_ShouldMove = true;
                }
            }
            else
            {
                if (!m_HandMoving)
                    ScaleWithChildren(1, 0.85f, 0.05f);

                m_HandMoving = true;

                m_ShouldMove = true;
            }
        }

        public void HandleRelease()
        {
            m_HandReleased = true;

            if (!m_IsRotateMode || m_HandMoving)
            {
                for (int i = 0; i < m_ElementRenderers.Count; i++)
                {
                    m_ElementRenderers[i].sortingOrder = 35;
                }
            }

            if (!m_HandMoving && m_IsRotateMode)
            {
                Rotate90Degree();
            }
        }

        public void MoveBack()
        {
            m_ShouldMoveBack = true;
            m_TargetPosition = m_OriginalPosition;

            UnhighlightAllElements();

            ScaleWithChildren(0.5f, 1f, 0.2f);
        }

        public void UnhighlightAllElements()
        {
            for (int i = 0; i < m_ElementRenderers.Count; i++)
            {
                bool containStar = false;
                for (int j = 0; j < m_StarCoordinates.Count; j++)
                {
                    if (m_StarCoordinates[j].Compare(m_RelativeGridCoordinates[i]))
                    {
                        containStar = true;
                    }
                }

                m_ElementRenderers[i].sprite = containStar ? m_TypeData.iconWithStar : m_TypeData.icon;
            }
        }

        public void PlaceBlock(Vector3 targetPos, System.Action onPlacedBlock)
        {
            m_TargetPosition = targetPos;
            m_ShouldBePlaced = true;
            m_OnPlacedBlock = onPlacedBlock;
        }

        public void ScaleWithChildren(float blockScale, float childScale, float duration, System.Action onComplete = null)
        {
            for (int i = 0; i < m_ElementTransforms.Count; i++)
            {
                m_ElementTransforms[i].DOScale(childScale, duration);
            }

            VTransform.DOScale(blockScale, duration).OnComplete(() =>
            {
                if (onComplete != null)
                    onComplete();
            });
        }

        public void Activate()
        {
            for (int i = 0; i < m_ElementTransforms.Count; i++)
            {
                m_ElementRenderers[i].DOColor(Color.white, 0.5f);
            }
        }

        public void Deactivate()
        {
            for (int i = 0; i < m_ElementTransforms.Count; i++)
            {
                m_ElementRenderers[i].DOColor(new Color(0.7f, 0.7f, 0.7f, 1f), 0.5f);
            }
        }

        public void Break()
        {
            var breakingPrefab = GameManager.Instance.breakingEffectPrefab.transform;
            for (int i = 0; i < m_ElementTransforms.Count; i++)
            {
                BreakBlockAnimation breakAnimation = PoolBoss.SpawnInPool(breakingPrefab, default(Vector3), default(Quaternion)).GetComponent<BreakBlockAnimation>();
                breakAnimation.VTransform.localScale = new Vector3(0.5f, 0.5f, 1f);
                breakAnimation.VTransform.position = m_ElementTransforms[i].position;
                breakAnimation.Break();
            }
        }

        private Vector2 GetChildLocalPositionByAngle(BlockAngle originalAngle, BlockAngle newAngle, Vector2 originalLocalPos)
        {
            int changeAngle = (int)newAngle - (int)originalAngle;
            changeAngle = (changeAngle < 0) ? 360 + changeAngle : changeAngle;

            if (changeAngle == 90)
            {
                return new Vector2(originalLocalPos.y, -1 * originalLocalPos.x);
            }
            else if (changeAngle == 180)
            {
                return new Vector2(originalLocalPos.x * -1, originalLocalPos.y * -1);
            }
            else if (changeAngle == 270)
            {
                return new Vector2(originalLocalPos.y * -1, originalLocalPos.x);
            }
            else
            {
                return originalLocalPos;
            }
        }

        public void Reset()
        {
            m_ShouldMove = false;
            m_ShouldMoveBack = false;
            m_IsRotateMode = false;
        }

        public JSONObject ToJSON()
        {
            JSONObject blockNode = new JSONObject();
            blockNode["id"] = m_Id;
            blockNode["type"] = (int)m_TypeData.type;
            blockNode["blockAngle"] = (int)m_Angle;

            for (int i = 0; i < m_StarCoordinates.Count; i++)
            {
                // Vector2 offset = m_StarTransforms[i].parent.localPosition - m_ElementTransforms[0].localPosition;
                Vector2 offset = new Vector2(m_StarCoordinates[i].x, m_StarCoordinates[i].y);
                JSONObject starPosNode = new JSONObject();
                starPosNode["x"] = (int)offset.x;
                starPosNode["y"] = (int)offset.y;
                blockNode["starCoords"][-1] = starPosNode;
            }

            return blockNode;
        }
        #endregion
    }
}