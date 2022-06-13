using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;
using UnityEngine;

namespace BP
{
    public class Board : BaseBehaviour
    {
        #region Constants
        private const int GRID_SIZE = 10;
        private const int BLOCK_SIZE = 1;
        #endregion

        #region Events
        #endregion

        #region Fields
        [SerializeField]
        private Transform m_Container;
        [SerializeField]
        private GameObject m_GridElementPrefab;
        #endregion

        #region Properties
        private GridElement[,] m_Grid;
        private Rect m_BoardBound;
        #endregion

        #region Unity Events
        #endregion

        #region Methods
        public void Configure(System.Action onComplete)
        {
            StartCoroutine(DoConfigure(onComplete));
        }

        private IEnumerator DoConfigure(System.Action onComplete)
        {
            m_BoardBound = new Rect(new Vector2(-GRID_SIZE / 2f, -GRID_SIZE / 2f), new Vector2(GRID_SIZE, GRID_SIZE));
            
            m_Grid = new GridElement[GRID_SIZE, GRID_SIZE];
            for (int x = 0; x < GRID_SIZE; x++)
            {
                for (int y = 0; y < GRID_SIZE; y++)
                {
                    GridElement gridElement = Instantiate(m_GridElementPrefab).GetComponent<GridElement>();
                    gridElement.VTransform.SetParent(m_Container);
                    gridElement.VTransform.localPosition = GetLocalPositionByGridIndex(x, y);
                    gridElement.Configure(new GridCoordinate(x, y));
                    m_Grid[x, y] = gridElement;
                }
                yield return null;
            }

            if (onComplete != null)
                onComplete();
        }

        public GridElement GetElement(int x, int y)
        {
            if (x < 0 || x >= GRID_SIZE || y < 0 || y >= GRID_SIZE)
                return null;

            return m_Grid[x, y];
        }

        public void ResetAllElements()
        {
            for (int x = 0; x < GRID_SIZE; x++)
            {
                for (int y = 0; y < GRID_SIZE; y++)
                {
                    m_Grid[x, y].ClearData();
                    m_Grid[x, y].ResetRender();
                    m_Grid[x, y].ResetBreakingOrder();
                }
            }
        }

        public void Replay(System.Action onComplete)
        {
            StartCoroutine(DoReplay(onComplete));
        }

        private IEnumerator DoReplay(System.Action onComplete)
        {
            var delay = new WaitForSeconds(0.1f);
            for (int i = 0; i < GRID_SIZE * 2 - 1; i++)
            {
                for (int x = 0, y = i; x <= i && y >= 0; x++, y--)
                {
                    if (x < 0 || x >= GRID_SIZE || y < 0 || y >= GRID_SIZE)
                        continue;

                    m_Grid[x, y].AnimateClean();
                }

                SoundManager.Instance.PlaySpin();
                // yield return delay;
            }

            yield return null;

            if (onComplete != null)
                onComplete();
        }

        public void UnhighlightAllElements()
        {
            for (int x = 0; x < GRID_SIZE; x++)
            {
                for (int y = 0; y < GRID_SIZE; y++)
                {
                    m_Grid[x, y].Unhighlight();
                }
            }
        }

        public void ResetBreakingOrderForAllElements()
        {
            for (int x = 0; x < GRID_SIZE; x++)
            {
                for (int y = 0; y < GRID_SIZE; y++)
                {
                    m_Grid[x, y].ResetBreakingOrder();
                }
            }
        }

        public bool CheckIfBlockFitSlots(Block block, List<GridCoordinate> slots)
        {
            bool shouldBeFit = false;
            List<GridCoordinate> relativeGridCoords = block.relativeGridCoordinates;
            if (relativeGridCoords.Count != slots.Count)
                return false;

            for (int i = 0; i < slots.Count; i++)
            {
                bool validGridCoord = true;
                for (int j = 0; j < block.relativeGridCoordinates.Count; j++)
                {
                    int coordX = slots[i].x + block.relativeGridCoordinates[j].x;
                    int coordY = slots[i].y + block.relativeGridCoordinates[j].y;
                    if (coordX < 0 || coordX >= GRID_SIZE || coordY < 0 || coordY >= GRID_SIZE)
                    {
                        validGridCoord = false;
                        break;
                    }

                    if (m_Grid[coordX, coordY].typeData != null)
                    {
                        validGridCoord = false;
                        break;
                    }
                }

                if (validGridCoord)
                {
                    shouldBeFit = true;
                    return shouldBeFit;
                }
                else
                {
                    continue;
                }
            }

            return shouldBeFit;
        }

        public bool CheckIfAvailableForBlock(List<GridCoordinate> relativeGridCoordinates)
        {
            bool available = false;
            for (int x = 0; x < GRID_SIZE; x++)
            {
                for (int y = 0; y < GRID_SIZE; y++)
                {
                    GridElement element = m_Grid[x, y];
                    if (element.typeData == null)
                    {
                        bool validGridCoord = true;
                        for (int i = 0; i < relativeGridCoordinates.Count; i++)
                        {
                            int coordX = x + relativeGridCoordinates[i].x;
                            int coordY = y + relativeGridCoordinates[i].y;
                            if (coordX < 0 || coordX >= GRID_SIZE || coordY < 0 || coordY >= GRID_SIZE)
                            {
                                validGridCoord = false;
                                break;
                            }

                            if (m_Grid[coordX, coordY].typeData != null)
                            {
                                validGridCoord = false;
                                break;
                            }
                        }

                        if (validGridCoord)
                        {
                            available = true;
                            return available;
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
            }

            return available;
        }

        public void HandleLose(System.Action onComplete)
        {
            StartCoroutine(DoHandleLose(onComplete));
        }

        private IEnumerator DoHandleLose(System.Action onComplete)
        {
            Sprite grayBlockSprite = GameManager.Instance.gridElementTypeConfigures.GetDataByType(GridElementType.Gray).icon;
            Sprite grayBlockWithStarSprite = GameManager.Instance.gridElementTypeConfigures.GetDataByType(GridElementType.Gray).iconWithStar;
            System.Random rnd = new System.Random();
            var arrayX = Enumerable.Range(0, GRID_SIZE).OrderBy(r => rnd.Next()).ToArray();
            var arrayY = Enumerable.Range(0, GRID_SIZE).OrderBy(r => rnd.Next()).ToArray();
            var delay = new WaitForSeconds(0.05f);
            for (int x = 0; x < GRID_SIZE; x++)
            {
                for (int y = 0; y < GRID_SIZE; y++)
                {
                    GridElement element = m_Grid[arrayX[x], arrayY[y]];
                    if (element.typeData != null)
                    {
                        element.HandleLose(grayBlockSprite, grayBlockWithStarSprite);
                        yield return delay;
                    }
                }
            }

            yield return new WaitForSeconds(0.5f);

            // Clear saved data
            PrefsUtils.SetString(Consts.PREFS_BOARD_SAVED_DATA, "");
            yield return null;
            if (onComplete != null)
                onComplete();
        }

        private List<GridElement> m_CheckingBrokenElements = new List<GridElement>(64);
        private List<GridElement> m_CheckingNeighbourElements = new List<GridElement>(64);
        private List<Vector2Int> m_CheckingBrokenLines = new List<Vector2Int>(10);
        public List<GridElement> HandleBreakingElements(List<GridElement> checkingElements, GridElementTypeData data, out int brokenLineCount)
        {
            brokenLineCount = 0;
            m_CheckingBrokenElements.Clear();
            m_CheckingBrokenLines.Clear();

            for (int i = 0; i < checkingElements.Count; i++)
            {
                checkingElements[i].brokenState = BrokenElementState.MAIN_BROKEN_ELEMENT;
            }

            for (int i = 0; i < checkingElements.Count; i++)
            {
                GridElement element = checkingElements[i];
                GridCoordinate coordinate = element.coordinate;
                // Horizontal checking
                bool canBreakHorizontal = true;
                for (int x1 = coordinate.x, x2 = coordinate.x, breakingOrder = 0; x1 >= 0 || x2 < GRID_SIZE; x1--, x2++, breakingOrder++)
                {
                    m_CheckingNeighbourElements.Clear();
                    if (x1 >= 0)
                        m_CheckingNeighbourElements.Add(m_Grid[x1, coordinate.y]);

                    if (x2 < GRID_SIZE)
                        m_CheckingNeighbourElements.Add(m_Grid[x2, coordinate.y]);

                    for (int neighbourIndex = 0; neighbourIndex < m_CheckingNeighbourElements.Count; neighbourIndex++)
                    {
                        GridElement neighbourElement = m_CheckingNeighbourElements[neighbourIndex];
                        if (neighbourElement.typeData != null)
                        {
                            // Do nothing
                        }
                        else if (neighbourElement.brokenState == BrokenElementState.MAIN_BROKEN_ELEMENT)
                        {
                            neighbourElement.breakingOrder = 0;
                        }
                        else
                        {
                            canBreakHorizontal = false;
                        }

                        if (neighbourElement.breakingOrder < 0 || neighbourElement.breakingOrder > breakingOrder)
                            neighbourElement.breakingOrder = breakingOrder;
                    }
                }

                Vector2Int row = new Vector2Int(-1, element.coordinate.y);
                if (canBreakHorizontal && !m_CheckingBrokenLines.Contains(row))
                {
                    m_CheckingBrokenLines.Add(row);

                    for (int x = 0; x < GRID_SIZE; x++)
                    {
                        GridElement neighbourElement = m_Grid[x, element.coordinate.y];
                        neighbourElement.Highlight(data);
                        if (neighbourElement.brokenState == BrokenElementState.NONE)
                            neighbourElement.brokenState = BrokenElementState.RELATIVE_BROKEN_ELEMENT;

                        m_CheckingBrokenElements.Add(neighbourElement);
                    }
                }

                for (int x = 0; x < GRID_SIZE; x++)
                {
                    GridElement resetElement = m_Grid[x, element.coordinate.y];
                    if (!canBreakHorizontal && resetElement.state != GridElementState.EMPTY_HIGHLIGHT)
                        if (resetElement.brokenState == BrokenElementState.NONE)
                            resetElement.breakingOrder = -1;

                    if (resetElement.brokenState == BrokenElementState.RELATIVE_BROKEN_ELEMENT)
                        resetElement.brokenState = BrokenElementState.NONE;
                }

                // Verticle checking
                bool canBreakVerticle = true;
                for (int y1 = coordinate.y, y2 = coordinate.y, breakingOrder = 0; y1 >= 0 || y2 < GRID_SIZE; y1--, y2++, breakingOrder++)
                {
                    m_CheckingNeighbourElements.Clear();
                    if (y1 >= 0)
                        m_CheckingNeighbourElements.Add(m_Grid[coordinate.x, y1]);

                    if (y2 < GRID_SIZE)
                        m_CheckingNeighbourElements.Add(m_Grid[coordinate.x, y2]);

                    for (int neighbourIndex = 0; neighbourIndex < m_CheckingNeighbourElements.Count; neighbourIndex++)
                    {
                        GridElement neighbourElement = m_CheckingNeighbourElements[neighbourIndex];
                        if (neighbourElement.typeData != null)
                        {
                            // Do nothing
                        }
                        else if (neighbourElement.brokenState == BrokenElementState.MAIN_BROKEN_ELEMENT)
                        {
                            neighbourElement.breakingOrder = 0;
                        }
                        else
                        {
                            canBreakVerticle = false;
                        }

                        if (neighbourElement.breakingOrder < 0 || neighbourElement.breakingOrder > breakingOrder)
                            neighbourElement.breakingOrder = breakingOrder;
                    }
                }

                Vector2Int col = new Vector2Int(element.coordinate.x, -1);
                if (canBreakVerticle && !m_CheckingBrokenLines.Contains(col))
                {
                    m_CheckingBrokenLines.Add(col);

                    for (int y = 0; y < GRID_SIZE; y++)
                    {
                        GridElement neighbourElement = m_Grid[element.coordinate.x, y];
                        neighbourElement.Highlight(data);
                        if (neighbourElement.brokenState == BrokenElementState.NONE)
                            neighbourElement.brokenState = BrokenElementState.RELATIVE_BROKEN_ELEMENT;

                        m_CheckingBrokenElements.Add(neighbourElement);
                    }
                }

                for (int y = 0; y < GRID_SIZE; y++)
                {
                    GridElement resetElement = m_Grid[element.coordinate.x, y];
                    if (!canBreakVerticle && resetElement.state != GridElementState.EMPTY_HIGHLIGHT)
                        if (resetElement.brokenState == BrokenElementState.NONE)
                            resetElement.breakingOrder = -1;

                    if (resetElement.brokenState == BrokenElementState.RELATIVE_BROKEN_ELEMENT)
                        resetElement.brokenState = BrokenElementState.NONE;
                }
            }

            // Remove duplicate elements
            m_CheckingBrokenElements = m_CheckingBrokenElements.Distinct().ToList();

            brokenLineCount = m_CheckingBrokenLines.Count;

            for (int i = 0; i < checkingElements.Count; i++)
            {
                checkingElements[i].brokenState = BrokenElementState.NONE;
            }

            return m_CheckingBrokenElements;
        }

        private Vector2 GetLocalPositionByGridIndex(int x, int y)
        {
            float startPosX = -GRID_SIZE / 2f + 0.5f;
            float startPosY = -GRID_SIZE / 2f + 0.5f;

            return new Vector2(startPosX + x * BLOCK_SIZE, startPosY + y * BLOCK_SIZE);
        }

        public Vector3 GetWorldPositionByGridIndex(int x, int y)
        {
            return m_Container.TransformPoint(GetLocalPositionByGridIndex(x, y));
        }

        public GridElement GetElementByWorldPosition(Vector3 worldPosition)
        {
            int x, y = -1;
            Vector3 localPos = m_Container.InverseTransformPoint(worldPosition);
            if (!m_BoardBound.Contains(localPos))
                return null;

            float startPosX = -GRID_SIZE / 2f;
            float startPosY = -GRID_SIZE / 2f;
            x = (int)((localPos.x - startPosX) / 1);
            y = (int)((localPos.y - startPosY) / 1);
            if (x >= GRID_SIZE || y >= GRID_SIZE || x < 0 || y < 0)
                return null;

            return m_Grid[x, y];
        }

        #region Board Archives
        public void LoadBoardData()
        {
            string dataString = PrefsUtils.GetString(Consts.PREFS_BOARD_SAVED_DATA, "");
            if (!string.IsNullOrEmpty(dataString))
            {
                JSONNode rootNode = JSON.Parse(dataString);
                LoadBoardDataByJsonNode(rootNode);
            }
        }

        public void LoadBoardDataByJsonNode(JSONNode rootNode)
        {
            JSONArray gridArray = rootNode["grid"].AsArray;
            for (int i = 0; i < gridArray.Count; i++)
            {
                GridElementType type = (GridElementType)gridArray[i]["type"].AsInt;
                bool containStar = gridArray[i]["containStar"] != null ? gridArray[i]["containStar"].AsBool : false;
                GridElementTypeData data = GameManager.Instance.gridElementTypeConfigures.GetDataByType(type);
                m_Grid[gridArray[i]["coordX"].AsInt, gridArray[i]["coordY"].AsInt].FillWithData(data);
                if (containStar)
                    m_Grid[gridArray[i]["coordX"].AsInt, gridArray[i]["coordY"].AsInt].FillStar();
            }
        }

        public void SaveBoardData()
        {
            PrefsUtils.SetString(Consts.PREFS_BOARD_SAVED_DATA, GetBoardData());
            PrefsUtils.Save();
        }

        public string GetBoardData()
        {
            JSONObject rootNode = new JSONObject();

            for (int x = 0; x < GRID_SIZE; x++)
            {
                for (int y = 0; y < GRID_SIZE; y++)
                {
                    GridElement element = m_Grid[x, y];
                    if (element.typeData != null)
                    {
                        JSONObject gridObject = new JSONObject();
                        gridObject["coordX"] = x;
                        gridObject["coordY"] = y;
                        gridObject["type"] = (int)element.typeData.type;
                        gridObject["containStar"] = element.containStar;

                        rootNode["grid"][-1] = gridObject;
                    }
                }
            }

            return rootNode.ToString();
        }
        #endregion
        #endregion
    }
}