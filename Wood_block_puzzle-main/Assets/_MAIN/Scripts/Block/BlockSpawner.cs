using System.Collections;
using System.Collections.Generic;
using DarkTonic.PoolBoss;
using DG.Tweening;
using SimpleJSON;
using Tidi.Ads;
using UnityEngine;

namespace BP
{
    [System.Serializable]
    public class BlockSpawnRule
    {
        public string id;
        public BlockAngle blockAngle;
        public List<BlockAngle> availableAngles;
        public List<ChildrenCoordinateByAngle> childrenCoordinatesByAngleList;
        public int availableFromSpawnCount;
        public int limitCountOnARow;
        public int spawnedCountOnARow;
        public float probability;
        public GameObject prefab;
        public GridElementType type;

        public BlockAngle GetRandomAngle()
        {
            int randomAngleIndex = Random.Range(0, availableAngles.Count);
            return availableAngles[randomAngleIndex];
        }

        public List<GridCoordinate> GetChildrenCoordinatesByAngle(BlockAngle angle)
        {
            for (int i = 0; i < childrenCoordinatesByAngleList.Count; i++)
            {
                if (childrenCoordinatesByAngleList[i].angle == angle)
                    return childrenCoordinatesByAngleList[i].childrenCoordinate;
            }

            return childrenCoordinatesByAngleList[0].childrenCoordinate;
        }
    }

    public class BlockSpawner : BaseBehaviour
    {
        #region Constants
        #endregion

        #region Events
        #endregion

        #region Fields
        [SerializeField]
        private Transform m_BlockContainer;
        [SerializeField]
        private Transform[] m_SpawningPositions;

        [Header("Boosters")]
        [SerializeField]
        private GameObject m_RotatePrefab;
        [SerializeField]
        private GameObject m_BombPrefab;

        [Header("Configures")]
        [SerializeField]
        private BlockConfigures m_Configures;
        #endregion

        #region Properties
        private Transform m_Transform;

        public Transform[] spawningPositions => m_SpawningPositions;

        [SerializeField]
        private List<BlockSpawnRule> m_BlockRules;

        private Block[] m_Blocks;

        [SerializeField]
        private int m_SpawnedCount;

        private List<Transform> m_RotateAnimations;
        #endregion

        #region Unity Events
        private void OnValidate()
        {
            if (m_SpawningPositions.Length < 3)
                Logger.e("Block Spawner: need 3 spawning positions");
        }
        #endregion

        #region Methods
        public void Configure(System.Action onComplete)
        {
            StartCoroutine(DoConfigure(onComplete));
        }

        private IEnumerator DoConfigure(System.Action onComplete)
        {
            m_Transform = transform;
            m_Blocks = new Block[3];
            m_SpawnedCount = 0;
            ConfigureRules();

            m_RotateAnimations = new List<Transform>();
            yield return null;

            if (onComplete != null)
                onComplete();
        }

        private void ConfigureRules()
        {
            m_BlockRules = new List<BlockSpawnRule>();

            for (int i = 0; i < m_Configures.blocks.Count; i++)
            {
                BlockData data = m_Configures.blocks[i];
                BlockSpawnRule rule = new BlockSpawnRule()
                {
                    id = data.id,
                    blockAngle = BlockAngle.ANGLE_0,
                    availableAngles = data.availableAngles,
                    childrenCoordinatesByAngleList = data.childrenCoordinateByAngleList,
                    availableFromSpawnCount = data.availableFromSpawnCount,
                    limitCountOnARow = data.limitCountOnARow,
                    prefab = data.prefab
                };

                if (m_SpawnedCount >= rule.availableFromSpawnCount)
                    rule.probability = 1f;
                else
                    rule.probability = 0f;

                m_BlockRules.Add(rule);
            }
        }

        public BlockSpawnRule GetSpawnRuleById(string id)
        {
            for (int i = 0; i < m_BlockRules.Count; i++)
                if (string.Compare(m_BlockRules[i].id, id) == 0)
                    return m_BlockRules[i];

            return null;
        }

        public void Show(float duration)
        {
            m_Transform.DOLocalMoveY(-6.13f, duration).SetEase(Ease.OutCubic);
        }

        public void Hide(float duration)
        {
            m_Transform.DOLocalMoveY(-15f, duration).SetEase(Ease.OutCubic);
        }

        public void HandleLose()
        {
            PrefsUtils.SetString(Consts.PREFS_BLOCK_SAVED_DATA, "");
            PrefsUtils.Save();
        }

        public void Reset()
        {
            m_SpawnedCount = 0;

            for (int i = 0; i < m_Blocks.Length; i++)
            {
                if (m_Blocks[i] != null)
                {
                    m_Blocks[i].Reset();
                    PoolBoss.Despawn(m_Blocks[i].VTransform);
                    m_Blocks[i] = null;
                }
            }

            m_EnabledRotate = false;
            for (int i = 0; i < m_RotateAnimations.Count; i++)
            {
                PoolBoss.Despawn(m_RotateAnimations[i]);
                m_RotateAnimations[i] = null;
            }
            m_RotateAnimations.Clear();

            PrefsUtils.SetString(Consts.PREFS_BLOCK_SAVED_DATA, "");
            PrefsUtils.Save();
        }

        public void ShowBlocks()
        {
            for (int i = 0; i < m_Blocks.Length; i++)
            {
                if (m_Blocks[i] != null)
                    m_Blocks[i].ScaleWithChildren(0.5f, 1, 0.2f);
            }
        }

        public Block GetBlock(int index)
        {
            return m_Blocks[index];
        }

        public void RemoveBlock(int index)
        {
            if (m_Blocks[index] != null)
            {
                m_Blocks[index] = null;
            }
        }

        // Check if all blocks has despawned, spawn new blocks
        public bool CheckIfShouldBeSpawnMore()
        {
            bool shouldBeSpawnNew = true;
            for (int i = 0; i < m_Blocks.Length; i++)
            {
                if (m_Blocks[i] != null)
                {
                    shouldBeSpawnNew = false;
                    break;
                }
            }

            return shouldBeSpawnNew;
        }

        public void DespawnBlock(int index)
        {
            if (m_Blocks[index] != null)
            {
                m_Blocks[index].Reset();
                PoolBoss.Despawn(m_Blocks[index].VTransform);
                m_Blocks[index] = null;
            }

            // Check if all blocks has despawned, spawn new blocks
            bool shouldBeSpawnNew = true;
            for (int i = 0; i < m_Blocks.Length; i++)
            {
                if (m_Blocks[i] != null)
                {
                    shouldBeSpawnNew = false;
                    break;
                }
            }

            if (shouldBeSpawnNew)
            {
                StartCoroutine(DoSpawnAndShowBlocks());
            }
        }

        public void SpawnAndShowBlocks()
        {
            StartCoroutine(DoSpawnAndShowBlocks());
        }

        private IEnumerator DoSpawnAndShowBlocks()
        {
            int totalStar = GetRandomTotalStarsForOneBlockSpawning();

            for (int i = 0; i < m_Blocks.Length; i++)
            {
                int starInOneBlock = Random.Range(0, totalStar + 1);
                totalStar -= starInOneBlock;
                m_Blocks[i] = SpawnRandomBlock(starInOneBlock);
                m_Blocks[i].ConfigureOriginalPosition(m_SpawningPositions[i].localPosition);
            }

            yield return null;
            ShowBlocks();

            if (m_EnabledRotate)
                EnableRotate();
        }

        private int[] _starProbabilities = new int[] {0, 0, 0, 1, 1, 2};
        private int GetRandomTotalStarsForOneBlockSpawning()
        {
            return _starProbabilities[Random.Range(0, _starProbabilities.Length)];
        }

        public void SpawnAndShow3SingleBlocksWithStar()
        {
            for (int i = 0; i < m_Blocks.Length; i++)
            {
                if (m_Blocks[i] != null)
                {
                    m_Blocks[i].Reset();
                    PoolBoss.Despawn(m_Blocks[i].VTransform);
                    m_Blocks[i] = null;
                }

                GridElementType blockType = GridElementType.Wood;
                List<GridCoordinate> starCoordinates = new List<GridCoordinate>() { new GridCoordinate(0, 0) };
                BlockSpawnRule rule = m_BlockRules[0];
                Block block = PoolBoss
                        .Spawn(
                            rule.prefab.transform,
                            default(Vector3),
                            default(Quaternion),
                            m_BlockContainer)
                        .GetComponent<Block>();

                block.VTransform.localPosition = Vector3.zero;
                block.VTransform.localScale = Vector3.zero;
                block.Configure(BlockAngle.ANGLE_0, starCoordinates);
                block.Id = rule.id;
                block.ConfigureType(GameManager.Instance.gridElementTypeConfigures.GetDataByType(blockType));
                block.ConfigureOriginalPosition(m_SpawningPositions[i].localPosition);
                m_Blocks[i] = block;

                rule.spawnedCountOnARow++;
                m_SpawnedCount++;
            }

            ShowBlocks();
        }

        private Block SpawnRandomBlock(int starCount = 0)
        {
            // Update rules before getting block
            bool shouldBeResetSpawnCount = m_SpawnedCount % m_Configures.blockSpawnCountPerRow == 0;
            for (int i = 0; i < m_BlockRules.Count; i++)
            {
                BlockSpawnRule rule = m_BlockRules[i];

                if (shouldBeResetSpawnCount)
                    rule.spawnedCountOnARow = 0;

                if (m_SpawnedCount >= rule.availableFromSpawnCount && (rule.limitCountOnARow < 0 || rule.spawnedCountOnARow < rule.limitCountOnARow))
                {
                    rule.probability = 1f;
                }
                else
                {
                    rule.probability = 0f;
                }
            }

            GridElementType blockType = GridElementType.Wood;
            float totalProbability = 0f;
            for (int i = 0; i < m_BlockRules.Count; i++)
            {
                totalProbability += m_BlockRules[i].probability;
            }

            float randomProbabilityPoint = Random.value * totalProbability;

            for (int i = 0; i < m_BlockRules.Count; i++)
            {
                float probability = m_BlockRules[i].probability;
                if (randomProbabilityPoint <= probability)
                {
                    BlockSpawnRule rule = m_BlockRules[i];
                    BlockAngle blockAngle = rule.GetRandomAngle();
                    List<GridCoordinate> coordinates = new List<GridCoordinate>(rule.GetChildrenCoordinatesByAngle(blockAngle));
                    coordinates.Shuffle();
                    List<GridCoordinate> starCoordinates = new List<GridCoordinate>();
                    int coordinateCount = Mathf.Min(starCount, coordinates.Count);
                    for (int j = 0; j < coordinateCount; j++)
                        starCoordinates.Add(coordinates[j]);

                    Block block = PoolBoss
                        .Spawn(
                            rule.prefab.transform,
                            default(Vector3),
                            default(Quaternion),
                            m_BlockContainer)
                        .GetComponent<Block>();

                    block.VTransform.localPosition = Vector3.zero;
                    block.VTransform.localScale = Vector3.zero;
                    block.Configure(blockAngle, starCoordinates);
                    block.Id = rule.id;
                    block.ConfigureType(GameManager.Instance.gridElementTypeConfigures.GetDataByType(blockType));

                    rule.spawnedCountOnARow++;
                    m_SpawnedCount++;

                    return block;
                }
                else
                {
                    randomProbabilityPoint -= probability;
                }
            }

            return null;
        }

        public Vector2 GetTutorialBlockWorldPosition()
        {
            return m_SpawningPositions[1].position;
        }

        #region Booster Rotate
        private bool m_EnabledRotate = false;
        public List<BlockAngle> m_OriginalAnglesBeforeRotating = new List<BlockAngle>(3);
        public void EnableRotate()
        {
            m_EnabledRotate = true;

            for (int i = 0; i < m_Blocks.Length; i++)
            {
                m_Blocks[i]?.TurnRotateMode(true);

                if (m_RotateAnimations.Count <= i)
                {
                    Transform rotateAnimation = PoolBoss.SpawnInPool(m_RotatePrefab.transform, Vector3.zero, default(Quaternion));
                    m_RotateAnimations.Add(rotateAnimation);
                }

                m_RotateAnimations[i].position = m_SpawningPositions[i].position;
            }

            UpdateRotateStateOfAllBlocks();
        }

        public void DisableRotate()
        {
            m_EnabledRotate = false;
            for (int i = 0; i < m_RotateAnimations.Count; i++)
            {
                if (m_RotateAnimations[i] != null)
                    PoolBoss.Despawn(m_RotateAnimations[i]);

                m_RotateAnimations[i] = null;
            }
            m_RotateAnimations.Clear();

            for (int i = 0; i < m_Blocks.Length; i++)
            {
                m_Blocks[i]?.TurnRotateMode(false);
            }
        }

        public bool CheckIfUserRotatedBlocks()
        {
            if (m_OriginalAnglesBeforeRotating.Count == 0)
                return false;

            for (int i = 0; i < m_Blocks.Length; i++)
            {
                BlockAngle angle = m_Blocks[i] != null ? m_Blocks[i].blockAngle : BlockAngle.ANGLE_0;
                if (m_OriginalAnglesBeforeRotating[i] != angle)
                    return true;
            }

            return false;
        }

        public bool CheckIfUserRotatedBlock(int index, BlockAngle currentAngle)
        {
            if (index < 0 || index >= m_OriginalAnglesBeforeRotating.Count)
                return false;

            return m_OriginalAnglesBeforeRotating[index] != currentAngle;
        }

        public void UpdateRotateStateOfAllBlocks()
        {
            m_OriginalAnglesBeforeRotating.Clear();

            for (int i = 0; i < m_Blocks.Length; i++)
            {
                m_OriginalAnglesBeforeRotating.Add(m_Blocks[i] != null ? m_Blocks[i].blockAngle : BlockAngle.ANGLE_0);
            }
        }
        #endregion

        #region Booster Switch
        public void SwitchBlocks()
        {
            StartCoroutine(YieldSwitchBlocks());
        }

        private IEnumerator YieldSwitchBlocks()
        {
            for (int i = 0; i < m_Blocks.Length; i++)
            {
                if (m_Blocks[i] != null)
                {
                    m_Blocks[i].Break();
                    m_Blocks[i].Reset();
                    PoolBoss.Despawn(m_Blocks[i].VTransform);
                    m_Blocks[i] = null;
                }
            }

            yield return null;
            int totalStar = GetRandomTotalStarsForOneBlockSpawning();

            for (int i = 0; i < m_Blocks.Length; i++)
            {
                int starInOneBlock = Random.Range(0, totalStar + 1);
                totalStar -= starInOneBlock;
                m_Blocks[i] = SpawnRandomBlock(starInOneBlock);
                m_Blocks[i].ConfigureOriginalPosition(m_SpawningPositions[i].localPosition);
            }

            yield return null;
            ShowBlocks();

            if (m_EnabledRotate)
                EnableRotate();
        }
        #endregion

        #region Booster Bomb
        BlockBomb _blockBomb;

        public BlockBomb GetBlockBomb()
        {
            return _blockBomb;
        }

        public void UseBoosterBomb()
        {
            _blockBomb = PoolBoss
                        .Spawn(
                            m_BombPrefab.transform,
                            default(Vector3),
                            default(Quaternion),
                            m_BlockContainer)
                        .GetComponent<BlockBomb>();

            _blockBomb.Configure();
            _blockBomb.VTransform.localScale = Vector3.zero;
            _blockBomb.ConfigureOriginalPosition(m_SpawningPositions[1].localPosition);
            _blockBomb.Scale(1f, 0.3f);

            for (int i = 0; i < m_Blocks.Length; i++)
                m_Blocks[i]?.ScaleWithChildren(0, 1, 0.1f);
        }

        public void CancelBoosterBomb()
        {
            if (_blockBomb != null)
            {
                PoolBoss.Despawn(_blockBomb.VTransform);
                _blockBomb = null;
            }

            for (int i = 0; i < m_Blocks.Length; i++)
                m_Blocks[i]?.ScaleWithChildren(0.5f, 1, 0.2f);
        }
        #endregion

        #region Block Spawn Archives
        public void LoadSavedBlocks()
        {
            string dataString = PrefsUtils.GetString(Consts.PREFS_BLOCK_SAVED_DATA, "");
            if (string.IsNullOrEmpty(dataString))
            {
                for (int i = 0; i < m_Blocks.Length; i++)
                {
                    m_Blocks[i] = SpawnRandomBlock();
                    m_Blocks[i].ConfigureOriginalPosition(m_SpawningPositions[i].localPosition);
                }
            }
            else
            {
                JSONNode rootNode = JSON.Parse(dataString);
                m_SpawnedCount = rootNode["spawnedCount"].AsInt;
                LoadBlocksByJsonNode(rootNode);
            }
        }

        public void LoadBlocksByJsonNode(JSONNode dataNode)
        {
            JSONArray blockNodes = dataNode["blocks"].AsArray;
            for (int i = 0; i < m_Blocks.Length; i++)
            {
                if (m_Blocks[i] != null)
                    PoolBoss.Despawn(m_Blocks[i].VTransform);

                if (i >= blockNodes.Count)
                {
                    m_Blocks[i] = SpawnRandomBlock();
                    m_Blocks[i].ConfigureOriginalPosition(m_SpawningPositions[i].localPosition);
                }
                else
                {
                    string blockId = blockNodes[i]["id"].Value;
                    GridElementType blockType = (GridElementType)blockNodes[i]["type"].AsInt;
                    BlockAngle blockAngle = (blockNodes[i]["blockAngle"] != null) ? (BlockAngle)blockNodes[i]["blockAngle"].AsInt : BlockAngle.ANGLE_0;

                    JSONArray starCoordinateNodes = blockNodes[i]["starCoords"].AsArray;
                    List<GridCoordinate> starCoordinates = new List<GridCoordinate>();
                    for (int j = 0; j < starCoordinateNodes.Count; j++)
                    {
                        GridCoordinate coordinate = new GridCoordinate(starCoordinateNodes[j]["x"].AsInt, starCoordinateNodes[j]["y"].AsInt);
                        starCoordinates.Add(coordinate);
                    }

                    if (!string.IsNullOrEmpty(blockId))
                    {
                        Block block = PoolBoss
                            .Spawn(
                                m_Configures.GetBlockById(blockId).prefab.transform,
                                default(Vector3),
                                default(Quaternion),
                                m_BlockContainer)
                            .GetComponent<Block>();

                        block.VTransform.localPosition = Vector3.zero;
                        block.VTransform.localScale = Vector3.zero;
                        block.Configure(blockAngle, starCoordinates);
                        block.ConfigureType(GameManager.Instance.gridElementTypeConfigures.GetDataByType(blockType));
                        block.Id = blockId;
                        block.ConfigureOriginalPosition(m_SpawningPositions[i].localPosition);

                        m_Blocks[i] = block;
                    }
                    else
                    {
                        m_Blocks[i] = null;
                    }
                }
            }
        }

        public void SaveBlocks()
        {
            JSONObject rootNode = new JSONObject();
            bool validBlockData = false;

            for (int i = 0; i < m_Blocks.Length; i++)
            {
                if (m_Blocks[i] != null)
                {
                    rootNode["blocks"][-1] = m_Blocks[i].ToJSON();

                    validBlockData = true;
                }
                else
                {
                    JSONObject blockNode = new JSONObject();
                    blockNode["id"] = "";
                    blockNode["type"] = -1;
                    blockNode["blockAngle"] = (int)BlockAngle.ANGLE_0;
                    rootNode["blocks"][-1] = blockNode;
                }
            }

            rootNode["spawnedCount"] = m_SpawnedCount;
            string dataString = (validBlockData) ? rootNode.ToString() : "";
            PrefsUtils.SetString(Consts.PREFS_BLOCK_SAVED_DATA, dataString);
            PrefsUtils.Save();
        }
        #endregion

        #region Editor Functions
        public void ChangeBlock(int blockIndex, string blockId, BlockAngle angle, GridElementType blockType)
        {
            if (m_Blocks[blockIndex] != null)
                PoolBoss.Despawn(m_Blocks[blockIndex].VTransform);

            Block block = PoolBoss
                .Spawn(
                    m_Configures.GetBlockById(blockId).prefab.transform,
                    default(Vector3),
                    default(Quaternion),
                    m_BlockContainer)
                .GetComponent<Block>();

            block.VTransform.localPosition = Vector3.zero;
            block.VTransform.localScale = Vector3.zero;
            block.Configure(angle, new List<GridCoordinate>());
            block.ConfigureType(GameManager.Instance.gridElementTypeConfigures.GetDataByType(blockType));
            block.Id = blockId;
            block.ConfigureOriginalPosition(m_SpawningPositions[blockIndex].localPosition);

            block.ScaleWithChildren(0.5f, 1, 0.2f);

            m_Blocks[blockIndex] = block;
        }

        public void ChangeAllBlocks(string blockId, BlockAngle angle, GridElementType blockType)
        {
            for (int i = 0; i < m_Blocks.Length; i++)
            {
                ChangeBlock(i, blockId, angle, blockType);
            }
        }
        #endregion
        #endregion
    }
}