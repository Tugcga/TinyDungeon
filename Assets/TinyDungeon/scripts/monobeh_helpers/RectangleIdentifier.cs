using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace TD
{
    public class RectangleIdentifier : MonoBehaviour
    {
        public float width;
        public float height;
        public Color color;

        public float thickness;
        public Color borderColor;

        void Start()
        {

        }


        void OnDrawGizmos()
        {
            Vector3 pos = transform.position;
            Vector3[] points = new Vector3[] {
                new Vector3(- width / 2, 0.0f, - height / 2),
                new Vector3(- width / 2, 0.0f, height / 2),
                new Vector3(width / 2, 0.0f, height / 2),
                new Vector3(width / 2, 0.0f, - height / 2)
            };

            Vector3[] borders = new Vector3[] { 
                new Vector3(-width / 2 - thickness, 0.0f, -height / 2),
                new Vector3(-width / 2, 0.0f, -height / 2 - thickness),
                new Vector3(width / 2, 0.0f, -height / 2 - thickness),
                new Vector3(width / 2 + thickness, 0.0f, -height / 2),
                new Vector3(width / 2 + thickness, 0.0f, height / 2),
                new Vector3(width / 2, 0.0f, height / 2 + thickness),
                new Vector3(-width / 2, 0.0f, height / 2 + thickness),
                new Vector3(-width / 2 - thickness, 0.0f, height / 2),
                new Vector3(-width / 2 - thickness, 0.0f, -height / 2) // the double of the first point
            };

            for(int i = 0; i < points.Length; i++)
            {
                points[i] = transform.TransformPoint(points[i]);
            }

            for(int i = 0; i < borders.Length; i++)
            {
                borders[i] = transform.TransformPoint(borders[i]);
            }

            Handles.DrawSolidRectangleWithOutline(points, color, color);
            Handles.color = borderColor;
            Handles.DrawPolyLine(borders);

        }
    }

}