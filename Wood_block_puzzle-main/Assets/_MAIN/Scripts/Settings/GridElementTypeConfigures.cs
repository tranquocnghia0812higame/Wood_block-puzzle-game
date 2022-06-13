using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BP
{
    [CreateAssetMenu(menuName = "Tools/Grid Element Type Configures")]
    public class GridElementTypeConfigures : ScriptableObject
    {
        #region Constants
        #endregion
        
        #region Events
        #endregion

        #region Fields
        [SerializeField]
        private List<GridElementTypeData> m_Configures;
        #endregion

        #region Properties
        #endregion

        #region Unity Events
        #endregion

        #region Methods
        public GridElementTypeData GetDataByType(GridElementType type)
        {
            for (int i = 0; i < m_Configures.Count; i++)
            {
                if (m_Configures[i].type == type)
                    return m_Configures[i];
            }

            return m_Configures[1];
        }

        public GridElementTypeData GetRandomData()
        {
            int randomIndex = Random.Range(1, 7);
            return GetDataByType((GridElementType)randomIndex);
        }

        public int GetBlockTypeCount()
        {
            return m_Configures.Count - 1; // Cause of we have a gray block here
        }
        #endregion
    }

    [System.Serializable]
    public class GridElementTypeData
    {
        public GridElementType type;
        public Sprite icon;
        public Sprite iconWithStar;
        public Sprite iconBright;
        public Sprite iconBrightWithStar;
    }

    public enum GridElementType
    {
        Gray = 0,
        Wood = 1
    }
}
