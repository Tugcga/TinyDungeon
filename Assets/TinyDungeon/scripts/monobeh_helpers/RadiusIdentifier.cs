using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace TD
{
    public class RadiusIdentifier : MonoBehaviour
    {
        public float radius;
        public Color color;
        public Color borderColor;

        public float thickness;

        void Start()
        {

        }


        void OnDrawGizmos()
        {
            Handles.color = color;

            Handles.DrawSolidDisc(transform.position, Vector3.up, radius);

            Transform tfm = transform;

            Vector3[] points = new Vector3[9];
            for (int p = 0; p < points.Length; p++)
            {
                float a = 2 * Mathf.PI * p / (points.Length - 1);
                points[p] = new Vector3(Mathf.Cos(a) * (radius + thickness), 0.0f, Mathf.Sin(a) * (radius + thickness));
            }

            //apply transform
            for (int s = 0; s < points.Length; s++)
            {
                points[s] = tfm.TransformPoint(points[s]);
            }

            //draw collision zone
            Handles.color = borderColor;
            Handles.DrawPolyLine(points);
        }
    }

}