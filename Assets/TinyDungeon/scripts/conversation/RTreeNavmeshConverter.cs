using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;

#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using UnityEditor;
using Advanced.Algorithms.DataStructures;
using Advanced.Algorithms.Geometry;
#endif

namespace TD
{
    public class ExtendedPolygon : Polygon
    {
        //additional fiels for collision edges
        public ColliderType colliderType;
        public GateColors gateColor;
        public int objectIndex;
        public bool isActive = true;

        public ExtendedPolygon(List<Point> edges, ColliderType cType, GateColors gColor, int index, bool isActive) : base(edges)
        {
            colliderType = cType;
            gateColor = gColor;
            objectIndex = index;
            this.isActive = isActive;
        }
    }

    public class RTreeNavmeshConverter : MonoBehaviour, IConvertGameObjectToEntity
    {
#if UNITY_EDITOR
        //local variables
        //for debuging process
        [Header("Converter settings")]
        public int defaultQueueLenght;
        public int maxChildrensCount;

        [Header("Debug")]
        public Transform debugStartPoint;
        public Transform debugEndPoint;
        public Color lineColor;
        public Color outputColor;
        public float debugOutputPointRadius;
        public Color polygonsColor;
        public Color bbColor;
        public Color searchResultColor;
        public bool normalShiftSearchResult;

        public Vector3 debugOutputPosition;
        List<Polygon> searchResult;

        //structures
        private System.Random random = new System.Random();
        RTree collisionTree;
        List<RTreeNode> nodes;

        RTreeCollisionNode[] treeNodes;
#endif

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            //create rtree
            Convert();
                        
            using (BlobBuilder blobBuilder = new BlobBuilder(Allocator.Temp))
            {
                ref CollisionMapBlobAsset collisionMapAsset = ref blobBuilder.ConstructRoot<CollisionMapBlobAsset>();
                collisionMapAsset.rootIndex = 0;
                collisionMapAsset.nodesCount = treeNodes.Length;
                collisionMapAsset.queueLength = defaultQueueLenght;
                BlobBuilderArray<RTreeCollisionNode> nodesArray = blobBuilder.Allocate(ref collisionMapAsset.nodes, treeNodes.Length);
                for (int t = 0; t < treeNodes.Length; t++)
                {
                    nodesArray[t] = new RTreeCollisionNode();
                    nodesArray[t].isLeaf = treeNodes[t].isLeaf;
                    nodesArray[t].index = treeNodes[t].index;
                    nodesArray[t].boundingBox = new RTreeBoundingBox(treeNodes[t].boundingBox.leftTop, treeNodes[t].boundingBox.rightBottom);
                    nodesArray[t].childrenIndexes = treeNodes[t].childrenIndexes;
                    nodesArray[t].edge = new CollisionEdge(treeNodes[t].edge.start, treeNodes[t].edge.end);
                    nodesArray[t].property = new RTCollisionProperty()
                    {
                        isActive = treeNodes[t].property.isActive,
                        colliderHostColor = treeNodes[t].property.colliderHostColor,
                        colliderHostIndex = treeNodes[t].property.colliderHostIndex,
                        colliderType = treeNodes[t].property.colliderType,
                        isBlockBullet = treeNodes[t].property.isBlockBullet
                    };
                }
                BlobAssetReference<CollisionMapBlobAsset> assetReference = blobBuilder.CreateBlobAssetReference<CollisionMapBlobAsset>(Allocator.Persistent);
                dstManager.AddComponentData(entity, new CollisionMap { collisionMap = assetReference });
            }
        }

#if UNITY_EDITOR
        public void Convert()
        {
            if(maxChildrensCount > 10)
            {
                UnityEngine.Debug.Log("The maximmum number of RTree children is greater than 10, clamp to 10");
                maxChildrensCount = 10;
            }
            collisionTree = new RTree(maxChildrensCount);

            List<BoundaryEdge> navmeshEdges = GetNavmeshBoundary();

            //next add boundary edges of all gate objects
            //we convert to collision edges all object in two steps
            //the first - obejcts with ReactangleIdentifiers
            //the second - all other objects with radius identifier
            RectangleIdentifier[] rectangles = FindObjectsOfType<RectangleIdentifier>();
            for(int i = 0; i < rectangles.Length; i++)
            {
                RectangleIdentifier rect = rectangles[i];
                Transform recTfm = rect.gameObject.transform;
                float w = rect.width;
                float h = rect.height;
                float t = rect.thickness;

                Vector3[] points = new Vector3[]
                {
                    new Vector3(-w / 2 - t, 0.0f, -h / 2),
                    new Vector3(-w / 2, 0.0f, -h / 2 - t),
                    new Vector3(w / 2, 0.0f, -h / 2 - t),
                    new Vector3(w / 2 + t, 0.0f, -h / 2),
                    new Vector3(w / 2 + t, 0.0f, h / 2),
                    new Vector3(w / 2, 0.0f, h / 2 + t),
                    new Vector3(-w / 2, 0.0f, h / 2 + t),
                    new Vector3(-w / 2 - t, 0.0f, h / 2),
                };

                for(int s = 0; s < points.Length; s++)
                {
                    points[s] = recTfm.TransformPoint(points[s]);
                }

                GateIdentifier gateData = rect.gameObject.GetComponent<GateIdentifier>();
                ColliderType cType = ColliderType.COLLIDER_UNDEFINED;
                GateColors gColor = GateColors.GATE_INDEFINED;
                bool oIsActive = true;
                int oIndex = 0;
                if(gateData != null)
                {
                    cType = ColliderType.COLLIDER_GATE;
                    gColor = gateData.gateColor;
                    oIsActive = gateData.isActive;
                    oIndex = gateData.gameObject.GetInstanceID();
                }

                //add edges to the tree
                for(int s = 0; s < points.Length; s++)
                {
                    List<Point> edgePoints = new List<Point>();
                    edgePoints.Add(new Point(points[s].x, points[s].z));
                    edgePoints.Add(new Point(points[(s + 1) % points.Length].x, points[(s + 1) % points.Length].z));
                    collisionTree.Insert(new ExtendedPolygon(edgePoints, cType, gColor, oIndex, oIsActive));
                }
            }

            //boundary for barrels
            RadiusIdentifier[] radiuses = FindObjectsOfType<RadiusIdentifier>();
            for (int i = 0; i < radiuses.Length; i++) 
            {
                RadiusIdentifier radiusId = radiuses[i];
                ItemIdentifier item = radiusId.gameObject.GetComponent<ItemIdentifier>();
                if(item != null && item.type != ColliderType.COLLIDER_UNDEFINED)
                {
                    float radius = radiusId.radius;
                    float t = radiusId.thickness;
                    Transform tfm = item.transform;

                    Vector3[] points = new Vector3[8];
                    for (int p = 0; p < points.Length; p++)
                    {
                        float a = 2 * Mathf.PI * p / points.Length;
                        points[p] = new Vector3(Mathf.Cos(a) * (radius + t), 0.0f, Mathf.Sin(a) * (radius + t));
                    }

                    //apply transform
                    for (int s = 0; s < points.Length; s++)
                    {
                        points[s] = tfm.TransformPoint(points[s]);
                    }

                    ColliderType cType = ColliderType.COLLIDER_UNDEFINED;
                    GateColors gColor = GateColors.GATE_INDEFINED;
                    bool oIsActive = true;
                    int oIndex = 0;
                    if (item != null)
                    {
                        cType = item.type;
                        oIndex = item.gameObject.GetInstanceID();
                        oIsActive = item.isActive;
                    }

                    //add edges to the tree
                    for (int s = 0; s < points.Length; s++)
                    {
                        List<Point> edgePoints = new List<Point>();
                        edgePoints.Add(new Point(points[s].x, points[s].z));
                        edgePoints.Add(new Point(points[(s + 1) % points.Length].x, points[(s + 1) % points.Length].z));
                        collisionTree.Insert(new ExtendedPolygon(edgePoints, cType, gColor, oIndex, oIsActive));
                    }
                }
            }

            foreach (BoundaryEdge edge in navmeshEdges)
            {
                //convert edge to polygon
                List<Point> polyPoints = new List<Point>();
                polyPoints.Add(new Point(edge.start.x, edge.start.z));
                polyPoints.Add(new Point(edge.end.x, edge.end.z));
                collisionTree.Insert(new ExtendedPolygon(polyPoints, ColliderType.COLLIDER_UNDEFINED, GateColors.GATE_INDEFINED, 0, true));
            }

            nodes = new List<RTreeNode>();
            collisionTree.Root.GeatherNodes(nodes);

            //convert to struct
            treeNodes = new RTreeCollisionNode[nodes.Count];
            for(int i = 0; i < nodes.Count; i++)
            {
                RTreeNode node = nodes[i];
                IndexesArray chIndexes = new IndexesArray() { count = 0 };
                for (int kIndex = 0; kIndex < node.KeyCount; kIndex++)
                {
                    chIndexes.Add(nodes.IndexOf(node.Children[kIndex]));
                }
                CollisionEdge edge = new CollisionEdge();
                ColliderType colliderType = ColliderType.COLLIDER_UNDEFINED;
                int objectIndex = 0;
                GateColors gateColor = GateColors.GATE_INDEFINED;
                bool isActive = true;
                if (node.MBRectangle.Polygon != null)
                {
                    ExtendedPolygon p = (ExtendedPolygon)node.MBRectangle.Polygon;
                    edge = new CollisionEdge(PointToFloat2(p.Edges[0].Left), PointToFloat2(p.Edges[0].Right));
                    colliderType = p.colliderType;
                    objectIndex = p.objectIndex;
                    gateColor = p.gateColor;
                    isActive = p.isActive;
                }
                treeNodes[i] = new RTreeCollisionNode()
                {
                    index = i,
                    boundingBox = new RTreeBoundingBox(PointToFloat2(node.MBRectangle.LeftTop), PointToFloat2(node.MBRectangle.RightBottom)),
                    childrenIndexes = chIndexes,
                    isLeaf = node.IsLeaf,
                    edge = edge,
                    property = new RTCollisionProperty()
                    {
                        colliderHostColor = gateColor,
                        colliderHostIndex = objectIndex,
                        colliderType = colliderType,
                        isActive = isActive,
                        isBlockBullet = (colliderType == ColliderType.COLLIDER_GATE || colliderType == ColliderType.COLLIDER_UNDEFINED) ? true : false
                    }
                };
            }
        }

        public float2 PointToFloat2(Point point)
        {
            return new float2((float)point.X, (float)point.Y);
        }

        public void TestStruct()
        {
            float2 start = new float2(debugStartPoint.position.x, debugStartPoint.position.z);
            float2 end = new float2(debugEndPoint.position.x, debugEndPoint.position.z);

            CollisionEdge testEdge = new CollisionEdge(start, end);
            float minT = 2.0f;  // we should find parameter t from 0 to 1 (because it in the interval), 2 means that there are no intersections
            CollisionEdge closestEdge;
            closestEdge.normal = new float2(0, 0);

            RTreeBoundingBox searchBB = new RTreeBoundingBox(new float2(math.min(start.x, end.x), math.max(start.y, end.y)), new float2(math.max(start.x, end.x), math.min(start.y, end.y)));
            int queueSize = 1000;
            int[] queueIndexes = new int[queueSize];
            queueIndexes[0] = 0;
            int queuePointer = 0;  // pointer to the last index in the stack
            for(int step = 0; step < (normalShiftSearchResult ? 2 : 1); step++)  // for normal shift we use two iterations
            {
                while (queuePointer >= 0)
                {
                    //get the task
                    int currentIndex = queueIndexes[queuePointer];
                    queuePointer--;

                    RTreeCollisionNode node = treeNodes[currentIndex];
                    if (node.isLeaf)
                    {//test intersection of the searchBB with child bbs
                        for (int i = 0; i < node.childrenIndexes.Length(); i++)
                        {
                            int v = node.childrenIndexes.Get(i);
                            //if (searchBB.IsIntersects(treeNodes[node.childrenIndexes[i]].boundingBox))
                            if (searchBB.IsIntersects(treeNodes[v].boundingBox))
                            {
                                //there is intersection with the bbox, this is collision edge
                                CollisionEdge edge = treeNodes[v].edge;
                                float t = testEdge.Intersection(edge);
                                if (t > -0.5f && t < minT)
                                {
                                    minT = t;
                                    closestEdge = edge;
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < node.childrenIndexes.Length(); i++)
                        {
                            int v = node.childrenIndexes.Get(i);
                            if (searchBB.IsIntersects(treeNodes[v].boundingBox))
                            {
                                queuePointer++;
                                queueIndexes[queuePointer] = treeNodes[v].index;
                            }
                        }
                    }
                }
                if(step == 0)
                {//we make the first step
                    if (normalShiftSearchResult == false || minT >= 1.0f)
                    {
                        float2 point = testEdge.Point(math.min(minT, 1.0f));
                        debugOutputPosition = new Vector3(point.x, 0.0f, point.y);
                    }
                    else
                    {//we should shift and minT < 1.0f
                     //we should apply normal shift for founded closes edge
                        float2 shift = end - start;
                        float d = math.dot(closestEdge.normal, shift);

                        //reinit end position
                        end = start + shift - d * closestEdge.normal;

                        //reinit testEdge and searchBB
                        queueIndexes[0] = 0;
                        queuePointer = 0;

                        testEdge = new CollisionEdge(start, end);
                        searchBB = new RTreeBoundingBox(new float2(math.min(start.x, end.x), math.max(start.y, end.y)), new float2(math.max(start.x, end.x), math.min(start.y, end.y)));
                        minT = 2.0f;
                    }
                }
                else
                {//here we make the second step (if we need to shift the intersection position)
                    //return start, if the second edge intersect the wlls, otherwise return end position
                    if(minT < 1.0f)
                    {
                        debugOutputPosition = new Vector3(start.x, 0.0f, start.y);
                    }
                    else
                    {
                        debugOutputPosition = new Vector3(end.x, 0.0f, end.y);
                    }
                }
            }
        }
        
        public void Test()
        {
            if(collisionTree != null)
            {
                //crate rectangle for start and end point
                Point leftTop = new Point(Math.Min(debugStartPoint.transform.position.x, debugEndPoint.transform.position.x), Math.Max(debugStartPoint.transform.position.z, debugEndPoint.transform.position.z));
                Point rightBottom = new Point(Math.Max(debugStartPoint.transform.position.x, debugEndPoint.transform.position.x), Math.Min(debugStartPoint.transform.position.z, debugEndPoint.transform.position.z));
                Rectangle searchRect = new Rectangle(leftTop, rightBottom);

                searchResult = collisionTree.RangeSearch(searchRect);


                //next we should iterate throw search result and fine the closest intersection point in the interval
                float2 start = new float2(debugStartPoint.position.x, debugStartPoint.position.z);
                float2 end = new float2(debugEndPoint.position.x, debugEndPoint.position.z);
                bool shiftAlongNormal = true;

                CollisionEdge testEdge = new CollisionEdge(start, end);
                float minT = 2.0f;  // we should find parameter t from 0 to 1 (because it in the interval), 2 means that there are no intersections
                int edgeIndex = -1;
                for(int i = 0; i < searchResult.Count; i++)
                {
                    Polygon p = searchResult[i];
                    float2 s = new float2((float)p.Edges[0].Left.X, (float)p.Edges[0].Left.Y);
                    float2 e = new float2((float)p.Edges[0].Right.X, (float)p.Edges[0].Right.Y);
                    CollisionEdge edge = new CollisionEdge(s, e);
                    float t = testEdge.Intersection(edge);
                    if (t > -0.5f && t < minT)
                    {
                        minT = t;
                        edgeIndex = i;
                    }
                }

                if (minT <= 1 && edgeIndex >= 0)
                {
                    Polygon wallPolygon = searchResult[edgeIndex];
                    float2 ws = new float2((float)wallPolygon.Edges[0].Left.X, (float)wallPolygon.Edges[0].Left.Y);
                    float2 we = new float2((float)wallPolygon.Edges[0].Right.X, (float)wallPolygon.Edges[0].Right.Y);
                    CollisionEdge wallEdge = new CollisionEdge(ws, we);
                    if (wallEdge.IsOnPositiveSide(start))
                    {
                        //shift along the normal
                        if(shiftAlongNormal)
                        {
                            float shiftX = end.x - start.x;
                            float shiftY = end.y - start.y;
                            //float d = shiftX * wallEdge.normal.x + shiftY * wallEdge.normal.y;
                            float d = math.dot(wallEdge.normal, end - start);
                            float2 secondEnd = new float2(start.x + shiftX - d * wallEdge.normal.x, start.y + shiftY - d * wallEdge.normal.y);
                            //and next we should check that new interval is not intersected with wall edges
                            //if there is any intersection, then the object in the corner and we should stop here
                            Point secondLeftTop = new Point(math.min(start.x, secondEnd.x), math.max(start.y, secondEnd.y));
                            Point secondRightBottom = new Point(math.max(start.x, secondEnd.x), math.min(start.y, secondEnd.y));
                            Rectangle secondSearchRect = new Rectangle(secondLeftTop, secondRightBottom);
                            List<Polygon> secondSearchResult = collisionTree.RangeSearch(secondSearchRect);
                            CollisionEdge secondTestEdge = new CollisionEdge(start, secondEnd);
                            bool isSecondIntersect = false;
                            for(int j = 0; j < secondSearchResult.Count; j++)
                            {
                                Polygon secondP = secondSearchResult[j];
                                float2 s = new float2((float)secondP.Edges[0].Left.X, (float)secondP.Edges[0].Left.Y);
                                float2 e = new float2((float)secondP.Edges[0].Right.X, (float)secondP.Edges[0].Right.Y);
                                CollisionEdge edge = new CollisionEdge(s, e);

                                float t = secondTestEdge.Intersection(edge);
                                if(t > -0.5f)
                                {
                                    isSecondIntersect = true;
                                    j = secondSearchResult.Count;
                                }
                            }

                            if(isSecondIntersect)
                            {
                                debugOutputPosition = new Vector3(start.x, 0.0f, start.y);
                            }
                            else
                            {
                                debugOutputPosition = new Vector3(secondEnd.x, 0.0f, secondEnd.y);
                            }
                        }
                        else
                        {
                            float2 point = testEdge.Point(minT);
                            debugOutputPosition = new Vector3(point.x, 0.0f, point.y);
                        }
                    }
                    else
                    {//go from back side of the edge, ignore collisions
                        debugOutputPosition = new Vector3(end.x, 0.0f, end.y);
                    }
                }
                else
                {//no intersection
                    debugOutputPosition = new Vector3(end.x, 0.0f, end.y);
                }
            }
        }

        void OnDrawGizmos()
        {
            if(debugStartPoint != null && debugEndPoint != null)
            {
                Gizmos.color = lineColor;
                Gizmos.DrawLine(new Vector3(debugStartPoint.transform.position.x, 0.0f, debugStartPoint.transform.position.z), new Vector3(debugEndPoint.transform.position.x, 0.0f, debugEndPoint.transform.position.z));
                
                Handles.color = outputColor;
                Handles.DrawSolidDisc(debugOutputPosition, Vector3.up, debugOutputPointRadius);
            }

            if(collisionTree != null)
            {
                Gizmos.color = polygonsColor;
                /*foreach (Polygon p in collisionTree)
                {//get only the first edge, because we store only edges, not complite polygons
                    Vector3 s = new Vector3((float)p.Edges[0].Left.X, 0.0f, (float)p.Edges[0].Left.Y);
                    Vector3 e = new Vector3((float)p.Edges[0].Right.X, 0.0f, (float)p.Edges[0].Right.Y);
                    Gizmos.DrawLine(s, e);
                }*/
                //draw collision edges and bounding boxes
                for(int i = 0; i < treeNodes.Length; i++)
                {
                    RTreeCollisionNode node = treeNodes[i];
                    //if(node.childrenIndexes.Length == 0)
                    if (node.childrenIndexes.Length() == 0)
                    {//draw edge
                        Gizmos.DrawLine(new Vector3(node.edge.start.x, 0.0f, node.edge.start.y), new Vector3(node.edge.end.x, 0.0f, node.edge.end.y));
                    }
                }

                Gizmos.color = bbColor;
                for(int i = 0; i < treeNodes.Length; i++)
                {
                    RTreeCollisionNode node = treeNodes[i];
                    Vector3 leftTop = new Vector3(node.boundingBox.leftTop.x, 0.0f, node.boundingBox.leftTop.y);
                    Vector3 rightBottom = new Vector3(node.boundingBox.rightBottom.x, 0.0f, node.boundingBox.rightBottom.y);
                    Vector3 rightTop = new Vector3(rightBottom.x, 0.0f, leftTop.z);
                    Vector3 leftBottom = new Vector3(leftTop.x, 0.0f, rightBottom.z);

                    Gizmos.DrawLine(leftTop, rightTop);
                    Gizmos.DrawLine(rightTop, rightBottom);
                    Gizmos.DrawLine(rightBottom, leftBottom);
                    Gizmos.DrawLine(leftBottom, leftTop);
                }
                /*foreach(RTreeNode node in nodes)
                {
                    if(true)
                    {
                        MBRectangle rect = node.MBRectangle;
                        Vector3 leftTop = new Vector3((float)rect.LeftTop.X, 0.0f, (float)rect.LeftTop.Y);
                        Vector3 rightBottom = new Vector3((float)rect.RightBottom.X, 0.0f, (float)rect.RightBottom.Y);
                        Vector3 rightTop = new Vector3(rightBottom.x, 0.0f, leftTop.z);
                        Vector3 leftBottom = new Vector3(leftTop.x, 0.0f, rightBottom.z);

                        Gizmos.DrawLine(leftTop, rightTop);
                        Gizmos.DrawLine(rightTop, rightBottom);
                        Gizmos.DrawLine(rightBottom, leftBottom);
                        Gizmos.DrawLine(leftBottom, leftTop);
                    }
                }*/

                if(searchResult != null)
                {
                    Gizmos.color = searchResultColor;
                    foreach(Polygon p in searchResult)
                    {
                        foreach (Line edge in p.Edges)
                        {
                            Vector3 s = new Vector3((float)edge.Left.X, 0.0f, (float)edge.Left.Y);
                            Vector3 e = new Vector3((float)edge.Right.X, 0.0f, (float)edge.Right.Y);
                            Gizmos.DrawLine(s, e);
                        }
                    }
                }
            }
        }

        //Navmesh to boundary edges
        public struct BoundaryEdge
        {
            public int index;
            public Vector3 start;
            public Vector3 end;

            public int startVertex;
            public int endVertes;
        }

        public class VertexData
        {
            public Vector3 position;
            public List<int> indexes = new List<int>();
        }

        public class IntIntClass
        {
            public int value1;
            public int value2;
            public IntIntClass(int v1, int v2)
            {
                value1 = v1;
                value2 = v2;
            }
        }

        public struct IntInt
        {
            public int value01;
            public int value02;

            public override string ToString()
            {
                return "(" + value01.ToString() + ", " + value02.ToString() + ")";
            }
        }

        public class EdgeData
        {
            public int v1;
            public int v2;
            public bool isForward;
            public bool isBackfard;

            public EdgeData(int s, int e)
            {
                if (s < e)
                {
                    v1 = s;
                    v2 = e;
                    isForward = true;
                    isBackfard = false;
                }
                else
                {
                    v1 = e;
                    v2 = s;
                    isForward = false;
                    isBackfard = true;
                }
            }

            public bool IsContainsVertices(int i1, int i2)
            {
                if ((v1 == i1 && v2 == i2) || (v1 == i2 && v2 == i1))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public bool TryToAdd(int s, int e)
            {
                if (s == v1 && e == v2)
                {
                    isForward = true;
                    return true;
                }
                else if (s == v2 && e == v1)
                {
                    isBackfard = true;
                    return true;
                }
                return false;
            }

            public bool IsBoundary()
            {
                return !(isForward && isBackfard);
            }

            public IntIntClass GetEdgeVertices()
            {
                if (isForward)
                {
                    return new IntIntClass(v1, v2);
                }
                else
                {
                    return new IntIntClass(v2, v1);
                }
            }
        }

        bool IsArrayContains(int value, List<int> array)
        {
            for (int i = 0; i < array.Count; i++)
            {
                if (array[i] == value)
                {
                    return true;
                }
            }
            return false;
        }

        int GetVertexIndex(int originalIndex, List<VertexData> vertices)
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                if (IsArrayContains(originalIndex, vertices[i].indexes))
                {
                    return i;
                }
            }
            return -1;
        }

        public bool IsEdgesCollinear(Vector3 pStart, Vector3 pEnd, Vector3 qStart, Vector3 qEnd)
        {
            Vector3 pDirection = (pEnd - pStart).normalized;
            Vector3 qDirection = (qEnd - qStart).normalized;
            if (Math.Abs(Math.Abs(Vector3.Dot(pDirection, qDirection)) - 1.0f) < 0.0001f)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsPointsClose(Vector3 a, Vector3 b)
        {
            return Vector3.Distance(a, b) < 0.0001f;
        }

        public bool IsPointOnTheEdge(Vector3 point, Vector3 start, Vector3 end)
        {
            //check the start and end positions
            if (Vector3.Distance(point, start) < 0.01f || Vector3.Distance(point, end) < 0.01f)
            {
                return false;
            }
            else
            {
                //calculate two dot products: (sp, se) and (ep, es), both of them should be close to 1
                float d1 = Vector3.Dot((end - start).normalized, (point - start).normalized);
                float d2 = Vector3.Dot((start - end).normalized, (point - end).normalized);
                if (Math.Abs(d1 - 1.0f) < 0.00001f && Math.Abs(d2 - 1.0f) < 0.00001f)
                {
                    return true;
                }
                return false;
            }
        }

        public List<BoundaryEdge> GetNavmeshBoundary()
        {
            //copy algorithm 
            NavMeshTriangulation triangulatedNavMesh = NavMesh.CalculateTriangulation();
            Vector3[] originalVertices = triangulatedNavMesh.vertices;
            int[] originalIndexes = triangulatedNavMesh.indices;
            float weldValue = 0.01f;
            List<VertexData> vertexList = new List<VertexData>();
            for (int i = 0; i < originalVertices.Length; i++)
            {
                Vector3 vPos = originalVertices[i];
                bool isNew = true;
                //Try to find this vertex on vertexList
                for (int vIndex = 0; vIndex < vertexList.Count; vIndex++)
                {
                    VertexData v = vertexList[vIndex];
                    if (Vector3.Distance(vPos, v.position) < weldValue)
                    {//i-th vertex placed in the same position as vIndex in the list. Add it index
                        v.indexes.Add(i);
                        vIndex = vertexList.Count;
                        isNew = false;
                    }
                }
                if (isNew)
                {
                    VertexData newVertex = new VertexData();
                    newVertex.position = vPos;
                    newVertex.indexes = new List<int>();
                    newVertex.indexes.Add(i);
                    vertexList.Add(newVertex);
                }
            }
            List<EdgeData> edgesList = new List<EdgeData>();
            int originalTrianglesCount = originalIndexes.Length / 3;
            for (int i = 0; i < originalTrianglesCount; i++)
            {
                int i1 = GetVertexIndex(originalIndexes[3 * i], vertexList);
                int i2 = GetVertexIndex(originalIndexes[3 * i + 1], vertexList);
                int i3 = GetVertexIndex(originalIndexes[3 * i + 2], vertexList);
                bool isFindI12 = false;
                bool isFindI23 = false;
                bool isFindI31 = false;
                for (int eIndex = 0; eIndex < edgesList.Count; eIndex++)
                {
                    isFindI12 = edgesList[eIndex].TryToAdd(i1, i2) || isFindI12;
                    isFindI23 = edgesList[eIndex].TryToAdd(i2, i3) || isFindI23;
                    isFindI31 = edgesList[eIndex].TryToAdd(i3, i1) || isFindI31;
                }
                if (!isFindI12)
                {
                    EdgeData newEdge = new EdgeData(i1, i2);
                    edgesList.Add(newEdge);
                }
                if (!isFindI23)
                {
                    EdgeData newEdge = new EdgeData(i2, i3);
                    edgesList.Add(newEdge);
                }
                if (!isFindI31)
                {
                    EdgeData newEdge = new EdgeData(i3, i1);
                    edgesList.Add(newEdge);
                }
            }

            //filter boundary edges, if it contains vertex of the other boundary edge inside it
            List<int> boundaryIndexes = new List<int>();
            for (int i = 0; i < edgesList.Count; i++)
            {
                if (edgesList[i].IsBoundary())
                {
                    boundaryIndexes.Add(i);
                }
            }

            bool isUpdate = true;
            while (isUpdate)
            {
                isUpdate = false;
                int i = 0;
                while (i < boundaryIndexes.Count)
                {
                    IntInt currentEdge;
                    currentEdge.value01 = edgesList[boundaryIndexes[i]].GetEdgeVertices().value1;
                    currentEdge.value02 = edgesList[boundaryIndexes[i]].GetEdgeVertices().value2;
                    Vector3 currentStart = vertexList[currentEdge.value01].position;
                    Vector3 currentEnd = vertexList[currentEdge.value02].position;
                    if (Vector3.Distance(currentStart, currentEnd) < 0.01f)
                    {
                        boundaryIndexes.RemoveAt(i);
                        i = boundaryIndexes.Count + 1;
                        isUpdate = true;
                    }
                    else
                    {
                        int j = 0;
                        while (j < boundaryIndexes.Count)
                        {
                            IntInt testEdge;
                            testEdge.value01 = edgesList[boundaryIndexes[j]].GetEdgeVertices().value1;
                            testEdge.value02 = edgesList[boundaryIndexes[j]].GetEdgeVertices().value2;
                            Vector3 testStart = vertexList[testEdge.value01].position;
                            Vector3 testEnd = vertexList[testEdge.value02].position;
                            if (Vector3.Distance(testStart, testEnd) < 0.01f)
                            {
                                boundaryIndexes.RemoveAt(j);
                                j = boundaryIndexes.Count + 1;
                                i = boundaryIndexes.Count + 1;
                                isUpdate = true;
                            }
                            else if (i != j && boundaryIndexes[i] < boundaryIndexes[j])
                            {
                                if (IsEdgesCollinear(currentStart, currentEnd, testStart, testEnd))
                                {
                                    if (IsPointsClose(currentStart, testEnd) && IsPointOnTheEdge(testStart, currentStart, currentEnd))
                                    {
                                        EdgeData ith = new EdgeData(testEdge.value01, currentEdge.value02);
                                        EdgeData jth = new EdgeData(currentEdge.value01, testEdge.value02);
                                        edgesList[boundaryIndexes[i]] = ith;
                                        edgesList[boundaryIndexes[j]] = jth;
                                        i = boundaryIndexes.Count + 1;
                                        j = boundaryIndexes.Count + 1;
                                        isUpdate = true;
                                    }
                                    else if (IsPointsClose(currentEnd, testStart) && IsPointOnTheEdge(testEnd, currentStart, currentEnd))
                                    {
                                        EdgeData ith = new EdgeData(currentEdge.value01, testEdge.value02);
                                        EdgeData jth = new EdgeData(testEdge.value01, currentEdge.value02);
                                        edgesList[boundaryIndexes[i]] = ith;
                                        edgesList[boundaryIndexes[j]] = jth;
                                        i = boundaryIndexes.Count + 1;
                                        j = boundaryIndexes.Count + 1;
                                        isUpdate = true;
                                    }
                                    else if (IsPointsClose(currentStart, testEnd) && IsPointOnTheEdge(currentEnd, testStart, testEnd))
                                    {
                                        EdgeData ith = new EdgeData(currentEdge.value01, testEdge.value02);
                                        EdgeData jth = new EdgeData(testEdge.value01, currentEdge.value02);
                                        edgesList[boundaryIndexes[i]] = ith;
                                        edgesList[boundaryIndexes[j]] = jth;
                                        i = boundaryIndexes.Count + 1;
                                        j = boundaryIndexes.Count + 1;
                                        isUpdate = true;
                                    }
                                    else if (IsPointsClose(currentEnd, testStart) && IsPointOnTheEdge(currentStart, testStart, testEnd))
                                    {
                                        EdgeData ith = new EdgeData(testEdge.value01, currentEdge.value02);
                                        EdgeData jth = new EdgeData(currentEdge.value01, testEdge.value02);
                                        edgesList[boundaryIndexes[i]] = ith;
                                        edgesList[boundaryIndexes[j]] = jth;
                                        i = boundaryIndexes.Count + 1;
                                        j = boundaryIndexes.Count + 1;
                                        isUpdate = true;
                                    }
                                    else if (IsPointsClose(currentEnd, testStart) && IsPointsClose(currentStart, testEnd))
                                    {
                                        EdgeData ith = new EdgeData(currentEdge.value01, currentEdge.value01);
                                        EdgeData jth = new EdgeData(testEdge.value01, testEdge.value01);
                                        edgesList[boundaryIndexes[i]] = ith;
                                        edgesList[boundaryIndexes[j]] = jth;
                                        i = boundaryIndexes.Count + 1;
                                        j = boundaryIndexes.Count + 1;
                                        isUpdate = true;
                                    }
                                }
                            }

                            j++;
                        }
                    }
                    i++;
                }
            }

            //copy to the output
            List<BoundaryEdge> toReturn = new List<BoundaryEdge>();
            for (int i = 0; i < boundaryIndexes.Count; i++)
            {
                int index = boundaryIndexes[i];
                IntIntClass edge = edgesList[index].GetEdgeVertices();
                BoundaryEdge newEdge = new BoundaryEdge();
                newEdge.index = index;
                newEdge.start = vertexList[edge.value1].position;
                newEdge.end = vertexList[edge.value2].position;
                newEdge.startVertex = edge.value1;
                newEdge.endVertes = edge.value2;
                toReturn.Add(newEdge);
            }
            return toReturn;
        }
#endif
    }

}