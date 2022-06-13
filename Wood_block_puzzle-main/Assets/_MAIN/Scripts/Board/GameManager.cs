using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using DarkTonic.PoolBoss;
using DG.Tweening;
using Tidi.Ads;
using Tidi.Leaderboard;
using UnityEngine;

namespace BP
{
    public enum GameState
    {
        StartUp,
        Playing,
        Replaying,
        Lose,
    }

    public enum TaskState
    {
        Idle,
        Running
    }

    public enum ScreenType
    {
        NORMAL,
        SHOWING_ADS,
        CLICKED_ADS,
        RATING,
        POLICY,
        LEADERBOARD
    }

    public class GameManager : Singleton<GameManager>
    {
        #region Constants
        #endregion

        #region Events
        #endregion

        #region Fields
        [SerializeField]
        private Board m_Board;
        [SerializeField]
        private BlockSpawner m_Spawner;
        [SerializeField]
        private PlayerInput m_Input;
        [SerializeField]
        private GUILoading m_Loading;
        [SerializeField]
        private TutorialController m_Tutorial;
        [SerializeField]
        private ScoreController m_Score;
        [SerializeField]
        private GUIMenu m_Menu;
        [SerializeField]
        private GUIGameOver m_GameOver;
        [SerializeField]
        private GUIContinue m_Continue;
        [SerializeField]
        private GUIHighscore m_Highscore;
        [SerializeField]
        private GUIPausePopup m_PausePopup;
        [SerializeField]
        private Camera m_Camera;
        [SerializeField]
        private GUITransition m_Transition;

        [Header("Multiple screen handler")]
        [SerializeField]
        private RectTransform m_HeaderRectTransform;
        [SerializeField]
        private Transform m_BoardTransform;
        [SerializeField]
        private Transform m_BonusGroupTransform;
        [SerializeField]
        private Transform m_BlockGroupTransform;

        [Header("Boosters")]
        [SerializeField]
        private BoosterController m_BoosterController;
        public BoosterController pBoosterController => m_BoosterController;
        [SerializeField]
        BombHighlightTargetHandler m_BombHighlightTargetHandler;

        [Header("Configures")]
        [SerializeField]
        private GridElementTypeConfigures m_GridElementTypeConfigures;

        [Header("Breaking Effect Prefabs")]
        [SerializeField]
        private GameObject m_BreakingEffectPrefab;
        public GameObject breakingEffectPrefab => m_BreakingEffectPrefab;
        #endregion

        #region Properties
        public Board board
        {
            get => m_Board;
        }

        public BlockSpawner spawner => m_Spawner;

        public GUIGameOver guiGameOver
        {
            get => m_GameOver;
        }

        public GUIMenu header => m_Menu;

        public GridElementTypeConfigures gridElementTypeConfigures
        {
            get => m_GridElementTypeConfigures;
        }

        public ScoreController scoreController
        {
            get => m_Score;
        }

        private ScreenType m_ScreenType = ScreenType.NORMAL;
        public ScreenType screenType
        {
            get => m_ScreenType;
            set => m_ScreenType = value;
        }

        private List<GridElement> m_CheckingElements;
        private List<GridElement> m_WillBreakElements;
        private int m_BrokenLines;
        private bool m_CanPlaceBlock = false;
        private bool m_PlacingBlock = false;
        private int m_PlacedBlockCount;
        public int placedBlockCount
        {
            get => m_PlacedBlockCount;
        }

        private TutorialData m_CurrentTutorialData;

        private GameState m_State = GameState.StartUp;
        private TaskState m_TaskState = TaskState.Idle;
        #endregion

        #region Unity Events
        protected override void Awake()
        {
            base.Awake();

            m_State = GameState.StartUp;
            m_TaskState = TaskState.Idle;
            Application.targetFrameRate = 60;
            Screen.SetResolution(720, (int)(720f / Camera.main.aspect), true);

            DG.Tweening.DOTween.Init();

            AdManager.Instance.Init();
        }

        private void Start()
        {
            float aspectRatio = Camera.main.aspect;
            if (aspectRatio <= 0.5f) // 18:9 ratio
            {
                m_HeaderRectTransform.anchoredPosition = new Vector2(0, -80f);

                m_BoardTransform.localPosition = new Vector3(0, 0.9f, 0);
                m_BonusGroupTransform.localPosition = new Vector3(0, -5.9f, 0f);
                m_BlockGroupTransform.localPosition = new Vector3(0, -8.18f, 0f);
            }
            else
            {
                m_HeaderRectTransform.anchoredPosition = new Vector2(0, -15f);

                m_BoardTransform.localPosition = new Vector3(0, 0.9f, 0);
                m_BonusGroupTransform.localPosition = new Vector3(0, -5.59f, 0f);
                m_BlockGroupTransform.localPosition = new Vector3(0, -7.55f, 0f);
            }
        }

        private void Update()
        {
            switch (m_State)
            {
                case GameState.StartUp:
                    StartUp();
                    break;
                case GameState.Playing:
                    Playing();
                    break;
                case GameState.Lose:
                    LoseGame();
                    break;
                default:
                    break;
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && m_State == GameState.Playing)
            {
                m_Board.SaveBoardData();
                m_Spawner.SaveBlocks();
            }

            if (hasFocus)
            {
                // For testing issue:
                // Block did not move back to original position when ads shown
                if (m_State == GameState.Playing)
                {
                    MoveBackAllBlocks();
                    // m_Input.EnableTouch(true);
                }
            }
        }
        #endregion

        #region Methods
        private void StartUp()
        {
            if (m_TaskState == TaskState.Idle)
                StartCoroutine(DoStartUp());
        }

        private IEnumerator DoStartUp()
        {
            m_TaskState = TaskState.Running;

            m_Menu.Configure();
            m_Menu.Hide();

            m_Transition.Configure();
            m_PausePopup.Configure();
            m_GameOver.Configure();
            m_Continue.Configure();

            m_BoosterController.Init();

            m_Loading.Show();
            m_Loading.UpdateProgress(10);

            m_PlacedBlockCount = 0;

            bool boardConfigured = false;
            m_Board.Configure(() =>
            {
                boardConfigured = true;
            });

            bool spawnerConfigured = false;
            m_Spawner.Configure(() =>
            {
                spawnerConfigured = true;
            });

            while (!boardConfigured || !spawnerConfigured)
                yield return null;

            m_Spawner.ShowBlocks();
            m_Loading.UpdateProgress(80);

            m_BombHighlightTargetHandler.Init();

            m_Input.Configure();

            m_CheckingElements = new List<GridElement>();
            m_WillBreakElements = new List<GridElement>();
            m_BrokenLines = 0;

            m_Score.LoadSavedData();

            m_Tutorial.Configure();
            HandleTutorial();
            if (m_CurrentTutorialData == null)
            {
                m_Board.LoadBoardData();
                m_Spawner.LoadSavedBlocks();

                m_Spawner.ShowBlocks();
            }
            else 
            {
                m_BoosterController.DisableAllRotate();
            }

            CheckAvailableSpaceForBlocks();

            yield return null;

            LeaderboardManager.Instance.Initialize();

            bool loading = true;
            m_Loading.UpdateProgress(100, 1f, () =>
            {
                loading = false;
            });

            while (loading)
                yield return null;

            yield return new WaitForSeconds(0.5f);

            bool showing = true;
            m_Transition.Show(() =>
            {
                showing = false;
            });
            while (showing)
                yield return null;

            m_Loading.Hide();
            m_Menu.Show();

            AdManager.Instance.RequestBanner();

            SoundManager.Instance.PlayBackground();
            SoundManager.Instance.PlayStartGame();

            m_Transition.Hide();

            m_State = GameState.Playing;
            m_TaskState = TaskState.Idle;
        }

        private void Playing()
        {

        }

        public void Lose()
        {
            m_State = GameState.Lose;
            m_TaskState = TaskState.Idle;
        }

        private void LoseGame()
        {
            if (m_TaskState == TaskState.Idle)
                StartCoroutine(DoLoseGame());
        }

        private IEnumerator DoLoseGame()
        {
            m_TaskState = TaskState.Running;

            SoundManager.Instance.PauseBackground();
            SoundManager.Instance.PlayLose();

            int losingScore = m_Score.currentScore;
            int highscore = m_Score.highScore;

            LeaderboardManager.Instance.PostScore(losingScore);

            MoveBackAllBlocks();

            m_Score.HandleLose();
            m_Spawner.HandleLose();

            bool loseAnimating = true;
            m_Board.HandleLose(() =>
            {
                loseAnimating = false;
            });

            while (loseAnimating)
                yield return null;

#if !UNITY_EDITOR
            AdManager.Instance.ShowInterstitial();
#endif
            bool shouldShowHighScore = losingScore == m_Score.highScore;
#if UNITY_EDITOR
            shouldShowHighScore = true;
#endif
            if (shouldShowHighScore)
            {
                bool showingHighscore = true;
                m_Highscore.Show(() =>
                {
                    showingHighscore = false;
                });
                while (showingHighscore)
                    yield return null;
            }

            m_GameOver.Show(losingScore, highscore);
        }

        public void Replay()
        {
            if (m_State == GameState.Replaying)
                return;

            StartCoroutine(DoReplay());
        }

        private IEnumerator DoReplay()
        {
            m_State = GameState.Replaying;
            m_Spawner.Reset();
            m_Score.Reset();

            bool animatingReplay = true;
            m_Board.Replay(() =>
            {
                animatingReplay = false;
            });
            while (animatingReplay)
                yield return null;

            SoundManager.Instance.PlayBackground();
            SoundManager.Instance.PlayStartGame();
            m_Input.EnableTouch(true);
            m_CanPlaceBlock = true;
            m_PlacingBlock = false;
            m_PlacedBlockCount = 0;
            m_Spawner.LoadSavedBlocks();
            m_Spawner.ShowBlocks();

            m_State = GameState.Playing;
            m_TaskState = TaskState.Idle;
        }

        public void ReplayAfterGameOver()
        {
            StartCoroutine(DoReplayAfterGameOver());
        }

        private IEnumerator DoReplayAfterGameOver()
        {
            m_Board.ResetAllElements();
            m_Spawner.Reset();
            m_Score.Reset();
            yield return null;

            SoundManager.Instance.PlayBackground();
            m_Input.EnableTouch(true);
            m_CanPlaceBlock = true;
            m_PlacingBlock = false;
            m_PlacedBlockCount = 0;
            m_Spawner.LoadSavedBlocks();
            m_Spawner.ShowBlocks();

            m_State = GameState.Playing;
            m_TaskState = TaskState.Idle;
        }

        #region Block Action
        public void SelectBlock(int index, Vector3 worldPos)
        {
            if (m_BoosterController.enabledBomb)
            {
                if (index == 1)
                {
                    var blockBomb = m_Spawner.GetBlockBomb();
                    if (blockBomb != null)
                    {
                        SoundManager.Instance.PlayChooseBlock();
                        blockBomb.HandleSelected(worldPos);
                    }
                }

                return;
            }

            Block block = m_Spawner.GetBlock(index);
            if (block != null)
            {
                SoundManager.Instance.PlayChooseBlock();
                block.HandleSelected(worldPos);

                if (m_CurrentTutorialData != null)
                {
                    m_Tutorial.HideHand();
                }
            }
        }

        public void MoveBlock(int index, Vector3 worldPos)
        {
            if (m_BoosterController.enabledBomb)
            {
                if (index == 1)
                {
                    var blockBomb = m_Spawner.GetBlockBomb();
                    if (blockBomb != null)
                    {
                        HandleMovingBomb(blockBomb, worldPos);
                    }
                }

                return;
            }

            Block block = m_Spawner.GetBlock(index);
            if (block != null)
            {
                HandleMovingBlock(block, worldPos);
            }
        }

        List<GridElement> m_AffectedElementsByBomb = new List<GridElement>(25);
        private void HandleMovingBomb(BlockBomb bomb, Vector3 worldPos)
        {
            bomb.HandleMoving(worldPos);

            if (m_PlacingBlock)
                return;

            m_Board.UnhighlightAllElements();
            m_AffectedElementsByBomb.Clear();

            var element = m_Board.GetElementByWorldPosition(bomb.VTransform.position);
            if (element == null)
                return;

            Rect affectedRect = new Rect(Vector2.zero, new Vector2(5, 5));
            affectedRect.center = element.VTransform.position;
            UnityEngine.Debug.DrawLine(new Vector3(affectedRect.xMin, affectedRect.yMin, 0f), new Vector3(affectedRect.xMax, affectedRect.yMin, 0f), Color.green);
            UnityEngine.Debug.DrawLine(new Vector3(affectedRect.xMin, affectedRect.yMax, 0f), new Vector3(affectedRect.xMax, affectedRect.yMax, 0f), Color.red);

            var coordinate = element.coordinate;
            var bottomLeftCoodinate = new GridCoordinate(coordinate.x - 2, coordinate.y - 2);
            for (int x = bottomLeftCoodinate.x; x < bottomLeftCoodinate.x + 5; x++)
            {
                for (int y = bottomLeftCoodinate.y; y < bottomLeftCoodinate.y + 5; y++)
                {
                    var affectedElement = m_Board.GetElement(x, y);
                    if (affectedElement != null && affectedElement.typeData != null)
                        m_AffectedElementsByBomb.Add(affectedElement);
                }
            }

            m_BombHighlightTargetHandler.Highlight(m_AffectedElementsByBomb, affectedRect);
        }

        private void HandleMovingBlock(Block block, Vector3 worldPos)
        {
            block.HandleMoving(worldPos);

            if (m_PlacingBlock)
                return;

            m_Board.UnhighlightAllElements();
            m_CheckingElements.Clear();
            m_WillBreakElements.Clear();
            for (int i = 0; i < block.elements.Count; i++)
            {
                GridElement element = m_Board.GetElementByWorldPosition(block.elements[i].position);
                if (element == null)
                    continue;

                bool containsElement = false;
                for (int j = 0; j < m_CheckingElements.Count; j++)
                {
                    if (m_CheckingElements[j].coordinate.Compare(element.coordinate))
                    {
                        containsElement = true;
                        break;
                    }
                }

                if (!containsElement && element.typeData == null)
                {
                    m_CheckingElements.Add(element);
                }
            }

            if (m_CheckingElements.Count != block.elements.Count)
            {
                m_CanPlaceBlock = false;
                return;
            }

            HighlightCheckingElements(block.typeData);

            if (m_CurrentTutorialData != null)
            {
                if (m_CheckingElements.Count != m_CurrentTutorialData.targetPositions.Count)
                {
                    m_CanPlaceBlock = false;
                    return;
                }

                bool validPositionForTutorial = true;
                for (int i = 0; i < m_CheckingElements.Count; i++)
                {
                    bool contains = false;
                    for (int j = 0; j < m_CurrentTutorialData.targetPositions.Count; j++)
                    {
                        contains = m_CheckingElements[i].coordinate.Compare(m_CurrentTutorialData.targetPositions[j]);
                        if (contains)
                            break;
                    }

                    if (!contains)
                    {
                        validPositionForTutorial = false;
                        break;
                    }
                }

                if (!validPositionForTutorial)
                {
                    m_CanPlaceBlock = false;
                    return;
                }
            }

            m_WillBreakElements.AddRange(m_Board.HandleBreakingElements(m_CheckingElements, block.typeData, out m_BrokenLines));
            m_CanPlaceBlock = true;
        }

        public void UnselectBlock(int index, Vector3 worldPos, bool hasCanceled)
        {
            if (m_BoosterController.enabledBomb)
            {
                if (index == 1)
                {
                    HandlePlacedBomb();
                    return;
                }
                
                return;
            }

            Block block = m_Spawner.GetBlock(index);
            if (block != null)
            {
                block.HandleRelease();
                if (m_CanPlaceBlock && !m_PlacingBlock && !hasCanceled && m_CheckingElements.Count > 0)
                {
                    m_PlacingBlock = true;
                    m_PlacedBlockCount++;
                    m_CanPlaceBlock = false;

                    StartCoroutine(DoPlaceBlock(index));

                    if (m_CurrentTutorialData != null)
                    {
                        m_Tutorial.Hide();
                    }
                }
                else
                {
                    SoundManager.Instance.PlayPlacedFailed();

                    UnhighlightCheckingElements();
                    block.MoveBack();
                    m_CheckingElements.Clear();
                    m_BrokenLines = 0;

                    if (m_CurrentTutorialData != null)
                    {
                        ShowTutorialAnimation(block);
                    }
                }
            }
        }

        private void HandlePlacedBomb()
        {
            m_BombHighlightTargetHandler.Hide();

            var blockBomb = m_Spawner.GetBlockBomb();
            if (blockBomb == null)
                throw new System.Exception("Error when placing bomb. Can't find bomb in blockSpawner");

            blockBomb.HandleRelease();
            m_Board.UnhighlightAllElements();
            m_AffectedElementsByBomb.Clear();

            var element = m_Board.GetElementByWorldPosition(blockBomb.VTransform.position);
            if (element == null)
            {
                SoundManager.Instance.PlayPlacedFailed();
                blockBomb.MoveBack();
            }
            else
            {
                SoundManager.Instance.PlayBomb();
                ShakeCamera(2);

                Rect affectedRect = new Rect(Vector2.zero, new Vector2(5, 5));
                affectedRect.center = element.VTransform.position;
                UnityEngine.Debug.DrawLine(new Vector3(affectedRect.xMin, affectedRect.yMin, 0f), new Vector3(affectedRect.xMax, affectedRect.yMin, 0f), Color.green);
                UnityEngine.Debug.DrawLine(new Vector3(affectedRect.xMin, affectedRect.yMax, 0f), new Vector3(affectedRect.xMax, affectedRect.yMax, 0f), Color.red);

                var coordinate = element.coordinate;
                var bottomLeftCoodinate = new GridCoordinate(coordinate.x - 2, coordinate.y - 2);
                for (int x = bottomLeftCoodinate.x; x < bottomLeftCoodinate.x + 5; x++)
                {
                    for (int y = bottomLeftCoodinate.y; y < bottomLeftCoodinate.y + 5; y++)
                    {
                        var affectedElement = m_Board.GetElement(x, y);
                        if (affectedElement != null && affectedElement.typeData != null)
                            m_AffectedElementsByBomb.Add(affectedElement);
                    }
                }

                if (m_AffectedElementsByBomb.Count > 0)
                {
                    m_Score.AddScore(400);
                    ShakeCamera(4);

                    List<Vector3> starElementPositions = new List<Vector3>(3);
                    for (int i = 0; i < m_AffectedElementsByBomb.Count; i++)
                    {
                        if (m_AffectedElementsByBomb[i].containStar)
                            starElementPositions.Add(m_AffectedElementsByBomb[i].VTransform.position);

                        m_AffectedElementsByBomb[i].BreakByBomb(blockBomb.VTransform.position);
                    }

                    if (starElementPositions.Count > 0)
                        m_BoosterController.CollectStars(starElementPositions);
                }

                blockBomb.Explode();
                if (m_BoosterController.enabledBomb)
                {
                    m_BoosterController.ConfirmUsedBomb();
                    m_Spawner.CancelBoosterBomb();
                }

                CheckAvailableSpaceForBlocks();
            }
        }

        private void MoveBackAllBlocks()
        {
            for (int i = 0; i < 3; i++)
                MoveBackBlock(i);
        }

        private void MoveBackBlock(int index)
        {
            Block block = m_Spawner.GetBlock(index);
            if (block != null)
            {
                UnhighlightCheckingElements();
                block.MoveBack();
                m_CheckingElements.Clear();
                m_BrokenLines = 0;
            }
        }

        private IEnumerator DoPlaceBlock(int index)
        {
            Block block = m_Spawner.GetBlock(index);
            m_Spawner.RemoveBlock(index);
            GridElementTypeData typeData = block.typeData;

            List<GridElement> localCheckingElements = new List<GridElement>(m_CheckingElements);
            m_CheckingElements.Clear();
            for (int i = 0; i < localCheckingElements.Count; i++)
                localCheckingElements[i].Unhighlight();

            List<GridElement> gotStarElements = GetGridCellGotStarByBlock(block);

            m_Input.EnableTouch(false);

            if (m_WillBreakElements.Count == 0)
                SoundManager.Instance.PlayPlacedBlock();
            else
                SoundManager.Instance.PlayBreakSingleBlock();

            m_Score.AddScore(block.elements.Count);

            Vector3 offset = localCheckingElements[0].VTransform.position - block.elements[0].position;
            Vector3 targetPos = block.VTransform.position + offset;
            bool blockMoving = true;
            block.PlaceBlock(targetPos, () =>
            {
                blockMoving = false;
            });
            while (blockMoving)
                yield return null;

            // block.UnhighlightAllElements();

            bool animating = true;
            block.ScaleWithChildren(1, 1, 0.1f, () =>
            {
                animating = false;
            });
            while (animating)
                yield return null;

            for (int i = 0; i < localCheckingElements.Count; i++)
                localCheckingElements[i].FillWithData(block.typeData);

            for (int i = 0; i < gotStarElements.Count; i++)
                gotStarElements[i].FillStar();

            m_Input.EnableTouch(true);

            ConfirmPlacedRotatedBlock(index, block.blockAngle);

            // Despawn block
            block.Reset();
            PoolBoss.Despawn(block.VTransform);
            if (m_Spawner.CheckIfShouldBeSpawnMore())
                m_Spawner.SpawnAndShowBlocks();

            if (m_WillBreakElements.Count > 0)
            {
                m_Score.AddScoreText(m_BrokenLines, targetPos, block.typeData.type);
                m_Score.AddHighlightText(m_BrokenLines, targetPos);
                ShakeCamera(m_BrokenLines);

                List<Vector3> starElementPositions = new List<Vector3>(3);
                for (int i = 0; i < m_WillBreakElements.Count; i++)
                {
                    if (m_WillBreakElements[i].containStar)
                        starElementPositions.Add(m_WillBreakElements[i].VTransform.position);

                    m_WillBreakElements[i].Break(typeData);
                }

                if (starElementPositions.Count > 0)
                    m_BoosterController.CollectStars(starElementPositions);

                yield return new WaitForSeconds(0.2f);
            }

            yield return new WaitForSeconds(0.1f);

            m_WillBreakElements.Clear();
            localCheckingElements.Clear();
            gotStarElements.Clear();
            m_BrokenLines = 0;
            m_Board.ResetBreakingOrderForAllElements();

            bool availableSpaceForBlocks = CheckAvailableSpaceForBlocks();
#if UNITY_EDITOR
            // availableSpaceForBlocks = false;
#endif
            if (availableSpaceForBlocks)
            {
                m_PlacingBlock = false;
            }
            else if (AdManager.Instance.IsRewardedVideoLoaded())
            {
                m_Input.EnableTouch(false);

                bool showing = true;
                bool shouldBeRewarded = false;
                m_Continue.Show(m_Score.currentScore, (rewarded) =>
                {
                    showing = false;
                    shouldBeRewarded = rewarded;
                });
                while (showing) yield return null;

                if (shouldBeRewarded)
                {
                    // Spawn 3 single blocks with star
                    m_Spawner.SpawnAndShow3SingleBlocksWithStar();

                    m_PlacingBlock = false;
                    m_Input.EnableTouch(true);
                }
                else
                {
                    Lose();
                    m_Input.EnableEventSystem(true);
                    m_Input.EnableTouch(false);
                }
            }
            else
            {
                Lose();
                m_Input.EnableEventSystem(true);
                m_Input.EnableTouch(false);
            }

            HandleTutorial();
        }

        public bool CheckAvailableSpaceForBlocks()
        {
            BlockAngle[] angles = new BlockAngle[] { BlockAngle.ANGLE_0, BlockAngle.ANGLE_90, BlockAngle.ANGLE_180, BlockAngle.ANGLE_270 };
            bool available = false;
            for (int i = 0; i < 3; i++)
            {
                Block block = m_Spawner.GetBlock(i);
                if (block != null)
                {
                    if (m_Board.CheckIfAvailableForBlock(block.relativeGridCoordinates))
                    {
                        available = true;
                        block.Activate();
                    }
                    else
                    {
                        block.Deactivate();
                    }

                    if (!available && m_BoosterController.IsAvailableRotateTurn())
                    {
                        BlockSpawnRule data = m_Spawner.GetSpawnRuleById(block.Id);
                        if (data != null)
                        {
                            for (int j = 0; j < angles.Length; j++)
                            {
                                if (angles[j] == block.blockAngle)
                                    continue;

                                if (m_Board.CheckIfAvailableForBlock(data.GetChildrenCoordinatesByAngle(angles[j])))
                                {
                                    available = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return available;
        }

        private void HighlightCheckingElements(GridElementTypeData typeData)
        {
            for (int i = 0; i < m_CheckingElements.Count; i++)
                m_CheckingElements[i].Highlight(typeData);
        }

        private void UnhighlightCheckingElements()
        {
            for (int i = 0; i < m_CheckingElements.Count; i++)
                m_CheckingElements[i].Unhighlight();
        }

        private List<GridElement> GetGridCellGotStarByBlock(Block block)
        {
            List<GridElement> cells = new List<GridElement>();
            List<Vector3> starWorldPositions = block.starWorldPositions;
            for (int i = 0; i < starWorldPositions.Count; i++)
            {
                GridElement cell = m_Board.GetElementByWorldPosition(starWorldPositions[i]);
                if (cell != null)
                    cells.Add(cell);
            }

            return cells;
        }
        #endregion

        #region Boosters
        public void SelectBoosterRotate()
        {
            SoundManager.Instance.PlayClick();

            if (m_BoosterController.enabledRotate && m_Spawner.CheckIfUserRotatedBlocks())
            {
                m_BoosterController.HandleJustUsedRotatedBlock();
                m_Spawner.UpdateRotateStateOfAllBlocks();
            }

            m_BoosterController.SelectBoosterRotate();

            if (m_BoosterController.enabledRotate)
            {
                m_Spawner.EnableRotate();
            }
            else
            {
                m_Spawner.DisableRotate();
            }

            if (m_CurrentTutorialData != null)
            {
                Block block = m_Spawner.GetBlock(1);
                if (block != null)
                {
                    ShowTutorialAnimation(block);
                }
            }
        }

        public void SelectBoosterSwitch()
        {
            SoundManager.Instance.PlayClick();

            if (m_BoosterController.IsAvailableSwitchTurn())
            {
                m_BoosterController.UseBoosterSwitch();

                m_Spawner.SwitchBlocks();
            }
        }

        public void SelectBoosterBomb()
        {
            SoundManager.Instance.PlayClick();

            if (m_BoosterController.enabledBomb)
            {
                m_BoosterController.CancelBoosterBomb();
                m_Spawner.CancelBoosterBomb();
            }
            else if (m_BoosterController.IsAvailableBombTurn())
            {
                m_BoosterController.UseBoosterBomb();

                m_Spawner.UseBoosterBomb();
            }
        }

        public void ShowRotateRewardedVideoPopup()
        {
            SoundManager.Instance.PlayClick();

            m_BoosterController.ShowRewardedVideoPopup(BoosterType.ROTATE);
        }

        public void ShowSwitchRewardedVideoPopup()
        {
            SoundManager.Instance.PlayClick();

            m_BoosterController.ShowRewardedVideoPopup(BoosterType.SWITCH);
        }

        public void ShowBombRewardedVideoPopup()
        {
            SoundManager.Instance.PlayClick();

            m_BoosterController.ShowRewardedVideoPopup(BoosterType.BOMB);
        }

        private void ConfirmPlacedRotatedBlock(int blockIndex, BlockAngle angle)
        {
            if (m_BoosterController.enabledRotate)
            {
                // if (m_Spawner.CheckIfUserRotatedBlock(blockIndex, angle))
                // {
                //     m_BoosterController.HandleJustUsedRotatedBlock();

                //     if (!m_BoosterController.enabledRotate)
                //         m_Spawner.DisableRotate();
                // }

                m_BoosterController.HandleJustUsedRotatedBlock();

                if (!m_BoosterController.enabledRotate)
                    m_Spawner.DisableRotate();

                m_Spawner.UpdateRotateStateOfAllBlocks();
            }
            else
            {
                m_Spawner.DisableRotate();
            }
        }

        public void RewardRotateByWatchingVideo()
        {
            m_BoosterController.RewardRotateByWatchingVideo();
        }

        public void RewardSwitchByWatchingVideo()
        {
            m_BoosterController.RewardSwitchByWatchingVideo();
        }

        public void RewardBombByWatchingVideo()
        {
            m_BoosterController.RewardBombByWatchingVideo();
        }

        public void OnChestPressed()
        {
            m_BoosterController.OnChestPressed();
        }
        #endregion

        #region Tutorial
        private void HandleTutorial()
        {
            if (PrefsUtils.GetBool(Consts.PREFS_SHOWN_TUTORIAL, false))
                return;

            m_CurrentTutorialData = m_Tutorial.GetTutorial();
            if (m_CurrentTutorialData != null)
            {
                m_Board.LoadBoardDataByJsonNode(m_CurrentTutorialData.boardData);
                m_Spawner.LoadBlocksByJsonNode(m_CurrentTutorialData.blockData);

                m_Spawner.ShowBlocks();

                m_CheckingElements.Clear();
                m_WillBreakElements.Clear();

                // Show Tutorial
                Rect targetUnitRect = GetTutorialTargetUnitRect();
                // Vector2 worldPos = m_Board.transform.TransformPoint(targetUnitRect.position);
                Vector2 worldPos = targetUnitRect.position;
                m_Tutorial.Show(worldPos, UnitToPixel(targetUnitRect.width), UnitToPixel(targetUnitRect.height));

                Block block = m_Spawner.GetBlock(1);
                if (block != null)
                {
                    ShowTutorialAnimation(block);
                }
            }
            else
            {
                m_Tutorial.Hide();
                PrefsUtils.SetBool(Consts.PREFS_SHOWN_TUTORIAL, true);
                PrefsUtils.Save();

                m_BoosterController.EnableAllRotate();
            }
        }

        private void ShowTutorialAnimation(Block block)
        {
            bool blockFit = m_Board.CheckIfBlockFitSlots(block, m_CurrentTutorialData.targetPositions);
            if (blockFit)
                block.Activate();
            else
                block.Deactivate();

            if (!m_BoosterController.enabledRotate && !blockFit)
            {
                m_Tutorial.AnimatePressingHand(m_BoosterController.GetButtonRotatePosition());
            }
            else if (m_BoosterController.enabledRotate && !blockFit)
            {
                m_Tutorial.AnimatePressingHand(m_Spawner.spawningPositions[1].position);
            }
            else
            {
                m_Tutorial.AnimateMovingHand(m_Spawner.GetTutorialBlockWorldPosition());
            }
        }

        private Rect GetTutorialTargetUnitRect()
        {
            Vector2 offset = new Vector2(0.5f, 0.5f);
            GridCoordinate firstCoordinate = m_CurrentTutorialData.targetPositions[0];
            GridElement firstElement = m_Board.GetElement(firstCoordinate.x, firstCoordinate.y);
            Vector2 bottomLeftPoint = (Vector2)firstElement.VTransform.position - offset;
            Vector2 topRightPoint = (Vector2)firstElement.VTransform.position + offset;
            for (int i = 0; i < m_CurrentTutorialData.targetPositions.Count; i++)
            {
                GridCoordinate coord = m_CurrentTutorialData.targetPositions[i];
                Vector2 localPos = m_Board.GetElement(coord.x, coord.y).VTransform.position;
                if (localPos.x - 0.5f < bottomLeftPoint.x)
                    bottomLeftPoint.x = localPos.x - 0.5f;

                if (localPos.y - 0.5f < bottomLeftPoint.y)
                    bottomLeftPoint.y = localPos.y - 0.5f;

                if (localPos.x + 0.5f > topRightPoint.x)
                    topRightPoint.x = localPos.x + 0.5f;

                if (localPos.y + 0.5f > topRightPoint.y)
                    topRightPoint.y = localPos.y + 0.5f;
            }

            Vector2 centerPos = (topRightPoint + bottomLeftPoint) / 2f;
            Vector2 size = new Vector2(topRightPoint.x - bottomLeftPoint.x, topRightPoint.y - bottomLeftPoint.y);
            return new Rect(centerPos, size);
        }

        private float UnitToPixel(float unit)
        {
            return unit * (720f / Camera.main.aspect * 0.5f) / (float)Camera.main.orthographicSize;
        }
        #endregion

        #region GUI Functions
        public void ShowPausePopup()
        {
            m_PausePopup.Show();
        }

        public void ShowRating()
        {
            SoundManager.Instance.PlayClick();
            GameManager.Instance.screenType = ScreenType.RATING;

            Application.OpenURL("market://details?id=" + Application.identifier);
        }

        public void ShakeCamera(int brokenLines)
        {
            if (brokenLines <= 1)
                return;

            if (brokenLines == 2)
                m_Camera.DOShakePosition(0.3f, new Vector3(0.1f, 0.1f, 0), 20, 0);
            else if (brokenLines == 3)
                m_Camera.DOShakePosition(0.3f, new Vector3(0.115f, 0.115f, 0), 25, 0);
            else
                m_Camera.DOShakePosition(0.3f, new Vector3(0.13f, 0.13f, 0), 30, 0);
        }
        #endregion

        #region Plugins
        #endregion

        #region Tutorial Editor 
        public void CleanBoard()
        {
            m_Board.ResetAllElements();
        }
        #endregion
        #endregion
    }
}