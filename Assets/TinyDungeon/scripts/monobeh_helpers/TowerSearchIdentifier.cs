using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.Entities;

namespace TD
{
    public class TowerSearchIdentifier : MonoBehaviour
    {
        public Vector3 searchHandlePosition;
        public Vector3 distanceHandelPosition;

        public float searchAngle;
        public float searchDistance;

        public Color discColor;

        public float visibleAngle;
        public Vector3 visibleHandlePoint;
        public Color visibleColor;
        
        public void SetDistanceHandlePosition(Vector3 point)
        {
            distanceHandelPosition = new Vector3(0f, 0f, point.z);
            searchDistance = point.z;
        }

        public void SetSearchHandlePosition(Vector3 point)
        {
            searchHandlePosition = new Vector3(Mathf.Abs(point.x), 0f, point.z);
            searchAngle = Mathf.Acos(point.normalized.z);
        }

        public void SetVisibleHandlePosition(Vector3 point)
        {
            visibleHandlePoint = new Vector3(Mathf.Abs(point.x), 0f, point.z);
            visibleAngle = Mathf.Acos(point.normalized.z);
        }


        public void OnDrawGizmos()
        {
            Handles.color = discColor;
            Handles.DrawSolidArc(transform.position, Vector3.up, transform.TransformDirection(new Vector3(-Mathf.Sin(searchAngle), 0f, Mathf.Cos(searchAngle))),
                2 * searchAngle * 180 / Mathf.PI, searchDistance);

            Handles.color = visibleColor;
            Handles.DrawSolidArc(transform.position, Vector3.up, transform.TransformDirection(new Vector3(-Mathf.Sin(visibleAngle), 0f, Mathf.Cos(visibleAngle))),
                2 * visibleAngle * 180 / Mathf.PI, searchDistance);
        }
    }

}