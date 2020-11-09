using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace TD
{
    [CustomEditor(typeof(BarrelIdentifier))]
    public class BarrelIdentifier_Editor : Editor
    {
        BarrelIdentifier barrel;

        public void OnSceneGUI()
        {
            barrel = target as BarrelIdentifier;

            Transform handleTransform = barrel.transform;
            Vector3 damagePoint = handleTransform.TransformPoint(barrel.damageHandlePosition);

            EditorGUI.BeginChangeCheck();
            damagePoint = Handles.FreeMoveHandle(damagePoint, handleTransform.rotation, 0.25f, Vector3.one * 0.5f, Handles.SphereHandleCap);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(barrel, "Move Damage Point");
                EditorUtility.SetDirty(barrel);
                barrel.SetDamageHandle(handleTransform.InverseTransformPoint(damagePoint));
            }
        }
    }
}
