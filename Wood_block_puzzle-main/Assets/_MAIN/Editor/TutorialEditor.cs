using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BP
{
    [CustomEditor(typeof(BlockSpawner))]
    public class TutorialEditor : Editor
    {
        private const string BLOCK_CONFIGURES_PATH = "Assets/_MAIN/Configures/Block Configures.asset";
        private BlockConfigures m_BlockConfigures;
        private List<GridElementType> m_ElementTypeArray;
        private GridElementType m_SelectedElementType = GridElementType.Wood;
        private BlockAngle m_SelectedBlockAngle = BlockAngle.ANGLE_0;
        private string m_SelectedElementId = "Block_1";

        private string m_SavedFileName = "Board_Tutorial_1";

        private void OnEnable()
        {
            m_BlockConfigures = AssetDatabase.LoadAssetAtPath(BLOCK_CONFIGURES_PATH,
                typeof(BlockConfigures)) as BlockConfigures;

            m_ElementTypeArray = new List<GridElementType>()
            {
                GridElementType.Wood
            };
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (!EditorApplication.isPlaying)
                return;

            GUILayout.Space(20);

            if (GUILayout.Button("Clean Board"))
            {
                GameManager.Instance.CleanBoard();
            }
            GUILayout.Space(5);

            GUILayout.Label("Tutorial File Name: ");
            m_SavedFileName = GUILayout.TextField(m_SavedFileName);
            if (GUILayout.Button("Save Tutorial"))
            {
                string boardData = GameManager.Instance.board.GetBoardData();
                SaveTutorial(boardData);
            }

            GUILayout.Label("Pick Element Color: ");
            m_SelectedElementType = (GridElementType) EditorGUILayout.EnumPopup(m_SelectedElementType);

            GUILayout.Label("Pick Block Angle: ");
            m_SelectedBlockAngle = (BlockAngle) EditorGUILayout.EnumPopup(m_SelectedBlockAngle);

            GUILayout.Label("Pick Block: ");
            GUILayout.BeginVertical();
            for (int i = 0; i < m_BlockConfigures.blocks.Count; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Box(AssetPreview.GetAssetPreview(m_BlockConfigures.blocks[i].prefab), GUILayout.Width(50), GUILayout.Height(50));
                GUILayout.Space(10);
                GUILayout.Label(string.Format("Id: {0}", m_BlockConfigures.blocks[i].id));
                GUILayout.Space(10);
                if (GUILayout.Button("Change All"))
                {
                    m_SelectedElementId = m_BlockConfigures.blocks[i].id;
                    ChangeAllBlocks();
                }
                GUILayout.Space(10);
                if (GUILayout.Button("Change One"))
                {
                    m_SelectedElementId = m_BlockConfigures.blocks[i].id;
                    ChangeBlock();
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }

        private void ChangeAllBlocks()
        {
            BlockSpawner myScript = (BlockSpawner) target;
            myScript.ChangeAllBlocks(m_SelectedElementId, m_SelectedBlockAngle, m_SelectedElementType);
        }

        private void ChangeBlock()
        {
            BlockSpawner myScript = (BlockSpawner) target;
            myScript.ChangeBlock(0, m_SelectedElementId, m_SelectedBlockAngle, m_SelectedElementType);
        }

        private void SaveTutorial(string content)
        {
            Logger.d("Tutorial Content: ", content);
            using(FileStream fs = new FileStream(string.Format("Assets/Resources/{0}.json", m_SavedFileName), FileMode.Create))
            {
                using(StreamWriter writer = new StreamWriter(fs))
                {
                    writer.Write(content);
                }
            }

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
        }
    }
}