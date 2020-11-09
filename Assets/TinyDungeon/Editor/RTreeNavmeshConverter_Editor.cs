#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace TD
{
    [CustomEditor(typeof(RTreeNavmeshConverter))]
    public class RTreeNavmeshConverter_Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            RTreeNavmeshConverter converter = (RTreeNavmeshConverter)target;
            DrawDefaultInspector();

            if (GUILayout.Button("Convert"))
            {
                converter.Convert();
            }

            if (GUILayout.Button("Test"))
            {
                converter.Test();
            }

            if (GUILayout.Button("Test with struct"))
            {
                converter.TestStruct();
            }
        }
    }
}
#endif