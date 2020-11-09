#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace TD
{
    [CustomEditor(typeof(NavmeshConverter))]
    public class NavmeshConverter_Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            NavmeshConverter converter = (NavmeshConverter)target;
            DrawDefaultInspector();

            if (GUILayout.Button("Convert"))
            {
                converter.Convert();
            }
        }
    }
}
#endif
