using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace TD
{
    [CustomEditor(typeof(SwitcherIdentifier))]
    public class SwitcherIdentifier_Editor : Editor
    {
        SwitcherIdentifier switcher;

        public override void OnInspectorGUI()
        {
            switcher = target as SwitcherIdentifier;

            DrawDefaultInspector();
            if (GUILayout.Button("Update Links"))
            {
                switcher.UpdateGates();
            }
        }
    }
}