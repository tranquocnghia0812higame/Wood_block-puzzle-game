using System.Collections.Generic;
using UnityEngine;

namespace BP
{
    public class BombHighlightTargetHandler : MonoBehaviour
    {
        private readonly Vector3 ORIGINAL_POSITION = new Vector3(-1000, -1000, 0);
        private readonly float MOVING_SPEED = 1000f;

        [SerializeField]
        Transform _topLeftCorner;
        [SerializeField]
        Transform _topRightCorner;
        [SerializeField]
        Transform _bottomLeftCorner;
        [SerializeField]
        Transform _bottomRightCorner;

        GridElementTypeData m_GridElementData;
        Rect m_BoardRect;

        #region Unity functions
        #endregion

        #region Functions
        public void Init()
        {
            Hide();

            m_GridElementData = GameManager.Instance.gridElementTypeConfigures.GetDataByType(GridElementType.Wood);

            var bottomLeftBoardCorner = GameManager.Instance.board.GetWorldPositionByGridIndex(0, 0);
            bottomLeftBoardCorner = new Vector3(bottomLeftBoardCorner.x - 0.5f, bottomLeftBoardCorner.y - 0.5f, 0f);
            var topRightBoardCorner = GameManager.Instance.board.GetWorldPositionByGridIndex(9, 9);
            topRightBoardCorner = new Vector3(topRightBoardCorner.x + 0.5f, topRightBoardCorner.y + 0.5f, 0f);

            m_BoardRect = new Rect(bottomLeftBoardCorner, new Vector2(10.1f, 10.1f));
        }

        public void Highlight(List<GridElement> affectedElements, Rect affectedRect)
        {
            if (affectedElements.Count == 0)
            {
                Hide();
                return;
            }

            var topLeft = new Vector3(affectedRect.xMin, affectedRect.yMax, 0f);
            var topRight = new Vector3(affectedRect.xMax, affectedRect.yMax, 0f);
            var bottomLeft = new Vector3(affectedRect.xMin, affectedRect.yMin, 0f);
            var bottomRight = new Vector3(affectedRect.xMax, affectedRect.yMin, 0f);

            _topLeftCorner.position = m_BoardRect.Contains(topLeft) ? topLeft : ORIGINAL_POSITION;
            _topRightCorner.position = m_BoardRect.Contains(topRight) ? topRight : ORIGINAL_POSITION;
            _bottomLeftCorner.position = m_BoardRect.Contains(bottomLeft) ? bottomLeft : ORIGINAL_POSITION;
            _bottomRightCorner.position = m_BoardRect.Contains(bottomRight) ? bottomRight : ORIGINAL_POSITION;

            for (int i = 0; i < affectedElements.Count; i++)
            {
                affectedElements[i]?.Highlight(m_GridElementData);
            }
        }

        public void Hide()
        {
            _topLeftCorner.position = ORIGINAL_POSITION;
            _topRightCorner.position = ORIGINAL_POSITION;
            _bottomLeftCorner.position = ORIGINAL_POSITION;
            _bottomRightCorner.position = ORIGINAL_POSITION;
        }
        #endregion
    }
}
