using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BP
{
    [CreateAssetMenu(menuName = "Tools/Block Configures")]
    public class BlockConfigures : ScriptableObject
    {
        #region Constants
        #endregion
        
        #region Events
        #endregion

        #region Fields
        public int blockSpawnCountPerRow;

        [TableList(ShowIndexLabels = true, DrawScrollView = false)]
        public List<BlockData> blocks = new List<BlockData>();
        #endregion

        #region Properties
        #endregion

        #region Unity Events
        #endregion

        #region Methods
        [PropertyOrder(-1)]
        [Button("Generate Blocks's Data")]
        public void GenerateBlockDatas()
        {
            for (int i = 0; i < blocks.Count; i++)
            {
                blocks[i].id = (blocks[i].prefab != null) ? blocks[i].prefab.name : "";
                Block block = blocks[i].prefab.GetComponent<Block>();
                block.Configure(BlockAngle.ANGLE_0, new List<GridCoordinate>());
                blocks[i].childrenCoordinateByAngleList = new List<ChildrenCoordinateByAngle>();
                BlockAngle[] allAngles = new BlockAngle[] { BlockAngle.ANGLE_0, BlockAngle.ANGLE_90, BlockAngle.ANGLE_180, BlockAngle.ANGLE_270 };
                for (int angleIndex = 0; angleIndex < allAngles.Length; angleIndex++)
                {
                    BlockAngle blockAngle = allAngles[angleIndex];
                    
                    List<GridCoordinate> childrenCoordinates = GetBlockCoordinatesWithAngles(block.relativeGridCoordinates, blockAngle);
                    blocks[i].childrenCoordinateByAngleList.Add(new ChildrenCoordinateByAngle() {
                        angle = blockAngle,
                        childrenCoordinate = childrenCoordinates
                    });
                }
            }
        }

        private List<GridCoordinate> GetBlockCoordinatesWithAngles(List<GridCoordinate> coordinates, BlockAngle angle)
        {
            List<GridCoordinate> output = new List<GridCoordinate>();
            for (int i = 0; i < coordinates.Count; i++)
            {
                GridCoordinate coord = coordinates[i];
                GridCoordinate outputCoord;
                if (angle == BlockAngle.ANGLE_90)
                {
                    outputCoord = new GridCoordinate(coord.y, -1 * coord.x);
                }
                else if (angle == BlockAngle.ANGLE_180)
                {
                    outputCoord = new GridCoordinate(coord.x * -1, coord.y * -1);
                }
                else if (angle == BlockAngle.ANGLE_270)
                {
                    outputCoord = new GridCoordinate(coord.y * -1, coord.x);
                }
                else
                {
                    outputCoord = coord;
                }
                output.Add(outputCoord);
            }

            return output;
        }

        public BlockData GetBlockById(string id)
        {
            for (int i = 0; i < blocks.Count; i++)
            {
                if (string.Compare(blocks[i].id, id) == 0)
                    return blocks[i];
            }

            return blocks[0];
        }
        #endregion
    }

    [System.Serializable]
    public class BlockData
    {
        [VerticalGroup("Properties")]
        [TableColumnWidth(30)]
        public string id;
        
        [VerticalGroup("Properties")]
        public int availableFromSpawnCount = 0;
        
        [VerticalGroup("Properties")]
        public int limitCountOnARow = -1;

        [VerticalGroup("Properties")]
        public List<BlockAngle> availableAngles;

        [VerticalGroup("Properties")]
        public List<ChildrenCoordinateByAngle> childrenCoordinateByAngleList;
        
        [AssetsOnly]
        [PropertyOrder(-1)]
        [TableColumnWidth(50, false)]
        [PreviewField(Alignment = ObjectFieldAlignment.Center, Height = 50)]
        public GameObject prefab;
    }

    [System.Serializable]
    public class ChildrenCoordinateByAngle
    {
        public BlockAngle angle;
        public List<GridCoordinate> childrenCoordinate;
    }

    public enum BlockDifficulty
    {
        EASY, MEDIUM, HARD, INTERMEDIATE
    }
}
