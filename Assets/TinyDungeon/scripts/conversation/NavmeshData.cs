using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TD
{
    public class NavmeshData : ScriptableObject
    {
        public Vector3[] originalVertices;
        public int[] originalIndexes;
    }
}
