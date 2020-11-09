using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace TD
{
    [CustomEditor(typeof(TowerSearchIdentifier))]
    public class TowerSearchIdentifier_Editor : Editor
    {
        TowerSearchIdentifier tower;

        public void OnSceneGUI()
        {
            tower = target as TowerSearchIdentifier;

            Transform handleTransform = tower.transform;
            Vector3 distancePoint= handleTransform.TransformPoint(tower.distanceHandelPosition);
            Vector3 searchPoint = handleTransform.TransformPoint(tower.searchHandlePosition);
            Vector3 visiblePoint = handleTransform.TransformPoint(tower.visibleHandlePoint);

            EditorGUI.BeginChangeCheck();
            distancePoint = Handles.FreeMoveHandle(distancePoint, handleTransform.rotation, 0.25f, Vector3.one * 0.5f, Handles.SphereHandleCap);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(tower, "Move Distance Point");
                EditorUtility.SetDirty(tower);
                tower.SetDistanceHandlePosition(handleTransform.InverseTransformPoint(distancePoint));
            }

            EditorGUI.BeginChangeCheck();
            searchPoint = Handles.FreeMoveHandle(searchPoint, handleTransform.rotation, 0.25f, Vector3.one * 0.5f, Handles.SphereHandleCap);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(tower, "Move Search Point");
                EditorUtility.SetDirty(tower);
                tower.SetSearchHandlePosition(handleTransform.InverseTransformPoint(searchPoint));
            }

            EditorGUI.BeginChangeCheck();
            visiblePoint = Handles.FreeMoveHandle(visiblePoint, handleTransform.rotation, 0.25f, Vector3.one * 0.5f, Handles.SphereHandleCap);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(tower, "Move Visible Point");
                EditorUtility.SetDirty(tower);
                tower.SetVisibleHandlePosition(handleTransform.InverseTransformPoint(visiblePoint));
            }
        }
    }
}