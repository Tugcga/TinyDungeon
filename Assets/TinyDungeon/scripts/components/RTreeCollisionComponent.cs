using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;

namespace TD
{
    public struct CollisionEdge
    {
        public float2 start;
        public float2 end;

        //parameters of the line
        //x = a1 * t + b1
        //y = a2 * t + b2
        public float a1;
        public float a2;
        public float b1;
        public float b2;

        public float2 normal;

        public override string ToString()
        {
            return start.ToString() + " - " + end.ToString();
        }

        public CollisionEdge(float2 start, float2 end)
        {
            this.start = start;
            this.end = end;

            a1 = end.x - start.x;
            a2 = end.y - start.y;

            b1 = start.x;
            b2 = start.y;

            normal = math.normalize(new float2(end.y - start.y, start.x - end.x));
        }

        public float2 Point(float t)
        {
            return new float2(a1 * t + b1, a2 * t + b2);
        }

        public bool IsOnPositiveSide(float2 point)
        {
            return math.dot(normal, end - point) < 0.0f;
        }

        public float Length()
        {
            return math.distance(start, end);
        }

        public float XMin()
        {
            return math.min(start.x, end.x);
        }

        public float YMin()
        {
            return math.min(start.y, end.y);
        }

        public float XMax()
        {
            return math.max(start.x, end.x);
        }

        public float YMax()
        {
            return math.max(start.y, end.y);
        }

        public float Intersection(CollisionEdge edge)
        {
            var v = edge.a1 * this.a2 - this.a1 * edge.a2;
            if (v != 0)
            {
                var t = (edge.a1 * (edge.b2 - this.b2) - edge.a2 * (edge.b1 - this.b1)) / v;
                if (t < 0 || t > 1)
                {
                    return -1;
                }
                else
                {
                    if (edge.a1 != 0)
                    {
                        var t2 = (this.a1 * t + this.b1 - edge.b1) / edge.a1;
                        if (t2 >= 0 && t2 <= 1)
                        {
                            return t;
                        }
                        else
                        {
                            return -1;
                        }
                    }
                    else
                    {//edge is a vertical line
                        if (edge.a2 != 0)
                        {
                            var t2 = (this.a2 * t + this.b2 - edge.b2) / edge.a2;
                            if (t2 >= 0 && t2 <= 1)
                            {
                                return t;
                            }
                            else
                            {
                                return -1;
                            }
                        }
                        else
                        {
                            return -1;
                        }
                    }
                }
            }
            else
            {
                return -1;
            }
        }
    }

    public struct RTreeBoundingBox
    {
        public float2 leftTop;
        public float2 rightBottom;

        public RTreeBoundingBox(float2 leftTop, float2 rightBottom)
        {
            this.leftTop = leftTop;
            this.rightBottom = rightBottom;
        }

        public bool IsIntersects(RTreeBoundingBox other)
        {
            //check for intersection
            if (this.leftTop.x > other.rightBottom.x // A is right of B   
             || this.rightBottom.x < other.leftTop.x // A is left of B
             || this.rightBottom.y > other.leftTop.y //A is above B
             || this.leftTop.y < other.rightBottom.y)//A is below B
            {
                //no intersection
                return false;
            }

            return true;
        }

        public override string ToString()
        {
            return leftTop.ToString() + " : " + rightBottom.ToString();
        }
    }

    public struct IndexesArray
    {
        public int count;

        public int i0;
        public int i1;
        public int i2;
        public int i3;
        public int i4;
        public int i5;
        public int i6;
        public int i7;
        public int i8;
        public int i9;

        public void Add(int index)
        {
            if (count == 0) { i0 = index; }
            else if (count == 1) { i1 = index; }
            else if (count == 2) { i2 = index; }
            else if (count == 3) { i3 = index; }
            else if (count == 4) { i4 = index; }
            else if (count == 5) { i5 = index; }
            else if (count == 6) { i6 = index; }
            else if (count == 7) { i7 = index; }
            else if (count == 8) { i8 = index; }
            else if (count == 9) { i9 = index; }
            count++;
        }

        public int Get(int index)
        {
            if (index < 0)
            {
                return i0;
            }
            else if (index >= count)
            {
                return i9;
            }
            else
            {
                if (index == 0) { return i0; }
                else if (index == 1) { return i1; }
                else if (index == 2) { return i2; }
                else if (index == 3) { return i3; }
                else if (index == 4) { return i4; }
                else if (index == 5) { return i5; }
                else if (index == 6) { return i6; }
                else if (index == 7) { return i7; }
                else if (index == 8) { return i8; }
                else { return i9; }
            }
        }

        public int Length()
        {
            return count;
        }
    }

    public struct RTCollisionProperty
    {
        public bool isActive;
        public ColliderType colliderType;
        public int colliderHostIndex;  // identifier for objects inside one type (for any barrel, or for gates with the same color)
        public GateColors colliderHostColor;  // identifier for colliderType = COLLIDER_GATE
        public bool isBlockBullet;  // by default true, but for barrel (for example is false)
    }

    public struct RTreeCollisionNode
    {
        public bool isLeaf;  // if true, then all childrens contains only edges, not other nodes
        public int index;  // the index of the current node in the list of all nodes
        public RTreeBoundingBox boundingBox;
        public IndexesArray childrenIndexes;

        public CollisionEdge edge;  // exist if the number of childrens are 0
        public RTCollisionProperty property;
    }

    public struct CollisionInfo
    {
        public bool isCollide;
        public bool isShifted;
        public float2 endPoint;
        public float minT;
        public CollisionEdge collisionEdge;
    }

    public struct CollisionMapBlobAsset
    {
        public int rootIndex;
        public BlobArray<RTreeCollisionNode> nodes;
        public int nodesCount;

        public int queueLength;

        public CollisionInfo GetPoint(float2 start, float2 end, bool movingTask)
        {
            NativeArray<int> queueIndexes = new NativeArray<int>(queueLength, Allocator.TempJob);  // in Burst enabled, dispose this array is crash the game
            int queuePointer = 0;
            queueIndexes[0] = rootIndex;

            RTreeBoundingBox searchBB = new RTreeBoundingBox(new float2(math.min(start.x, end.x), math.max(start.y, end.y)), new float2(math.max(start.x, end.x), math.min(start.y, end.y)));
            CollisionEdge testEdge = new CollisionEdge(start, end);
            float minT = 2.0f;
            CollisionEdge closestEdge = new CollisionEdge();
            closestEdge.normal = new float2(0, 0);
            //UnityEngine.Debug.Log("start");
            bool normalShiftSearchResult = movingTask;

            for (int step = 0; step < (normalShiftSearchResult ? 2 : 1); step++)  // for normal shift we use two iterations
            {
                while (queuePointer >= 0)
                {
                    //get the task
                    int currentIndex = queueIndexes[queuePointer];
                    queuePointer--;

                    RTreeCollisionNode node = nodes[currentIndex];
                    
                    if(node.property.isActive)
                    {//consider only active nodes
                        if (node.isLeaf)
                        {//test intersection of the searchBB with child bbs
                            for (int i = 0; i < node.childrenIndexes.Length(); i++)
                            {
                                int v = node.childrenIndexes.Get(i);
                                //skip nonactive nodes
                                if (nodes[v].property.isActive && searchBB.IsIntersects(nodes[v].boundingBox) && (movingTask || (!movingTask && nodes[v].property.isBlockBullet)))
                                {
                                    //there is intersection with the bbox, this is collision edge
                                    CollisionEdge edge = nodes[v].edge;
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
                                if (nodes[v].property.isActive && searchBB.IsIntersects(nodes[v].boundingBox))
                                {
                                    queuePointer++;
                                    if (queueIndexes.Length <= queuePointer)
                                    {//the queue is too small, we should reinit it with larger size
                                        NativeArray<int> copy = new NativeArray<int>(queueIndexes, Allocator.TempJob);
                                        queueIndexes.Dispose();
                                        queueLength = queueLength + 10;

                                        queueIndexes = new NativeArray<int>(queueLength, Allocator.TempJob);
                                        for (int s = 0; s < copy.Length; s++)
                                        {
                                            queueIndexes[s] = copy[s];
                                        }
                                        copy.Dispose();
                                    }
                                    queueIndexes[queuePointer] = nodes[v].index;
                                }
                            }
                        }
                    }
                    //if the node is nonActive, simply skip it
                }
                if (step == 0)
                {//we make the first step
                    if (normalShiftSearchResult == false || minT >= 1.0f)
                    {
                        queueIndexes.Dispose();
                        return new CollisionInfo() {isCollide = minT < 1.0f, isShifted = false, minT = minT, endPoint = testEdge.Point(math.min(minT, 1.0f)), collisionEdge = closestEdge };
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
                    if (minT < 1.0f)
                    {
                        queueIndexes.Dispose();
                        return new CollisionInfo() { endPoint = start, isCollide = true, isShifted = true, minT = minT, collisionEdge = closestEdge };
                    }
                    else
                    {
                        queueIndexes.Dispose();
                        return new CollisionInfo() { endPoint = end, isCollide = true, isShifted = true, minT = minT, collisionEdge = closestEdge };
                    }
                }
            }

            queueIndexes.Dispose();
            return new CollisionInfo() { endPoint = end, isCollide = false, minT = minT, isShifted = false, collisionEdge = closestEdge };
        }

        public void Deactivate(CollisionEdgesSetComponent indexes)
        {
            if (indexes.index01 != 0) { nodes[indexes.index01].property.isActive = false; }
            if (indexes.index02 != 0) { nodes[indexes.index02].property.isActive = false; }
            if (indexes.index03 != 0) { nodes[indexes.index03].property.isActive = false; }
            if (indexes.index04 != 0) { nodes[indexes.index04].property.isActive = false; }
            if (indexes.index05 != 0) { nodes[indexes.index05].property.isActive = false; }
            if (indexes.index06 != 0) { nodes[indexes.index06].property.isActive = false; }
            if (indexes.index07 != 0) { nodes[indexes.index07].property.isActive = false; }
            if (indexes.index08 != 0) { nodes[indexes.index08].property.isActive = false; }
        }

        public void Activate(CollisionEdgesSetComponent indexes)
        {
            if (indexes.index01 != 0) { nodes[indexes.index01].property.isActive = true; }
            if (indexes.index02 != 0) { nodes[indexes.index02].property.isActive = true; }
            if (indexes.index03 != 0) { nodes[indexes.index03].property.isActive = true; }
            if (indexes.index04 != 0) { nodes[indexes.index04].property.isActive = true; }
            if (indexes.index05 != 0) { nodes[indexes.index05].property.isActive = true; }
            if (indexes.index06 != 0) { nodes[indexes.index06].property.isActive = true; }
            if (indexes.index07 != 0) { nodes[indexes.index07].property.isActive = true; }
            if (indexes.index08 != 0) { nodes[indexes.index08].property.isActive = true; }
        }
    }

    public struct CollisionMap : IComponentData
    {
        public BlobAssetReference<CollisionMapBlobAsset> collisionMap;
    }

    public struct MovableCollisionComponent : IComponentData
    {
        public BlobAssetReference<CollisionMapBlobAsset> collisionMap;
        
        public CollisionInfo GetPoint(float2 start, float2 end)
        {
            return collisionMap.Value.GetPoint(start, end, true);
        }
    }
}