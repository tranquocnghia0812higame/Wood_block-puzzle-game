using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BP
{
    [CustomEditor(typeof(Block))]
    public class BlockEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            Block myScript = (Block) target;
            if (GUILayout.Button("Setup Block Children"))
            {
                myScript.Configure(BlockAngle.ANGLE_0, new List<GridCoordinate>());
                myScript.CalculateChildrenPositions();
            }
        }
    }
}