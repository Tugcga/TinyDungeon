using Unity.Entities;
using Unity.Mathematics;

#if UNITY_EDITOR
using System.Collections.Generic;
#endif

namespace TD
{
    public struct TriangleSampleInfo
    {
        public float2 result;
        public bool isInside;
    }

    public struct NavMeshTriangle : IBufferElementData
    {
        public float2 v0;
        public float2 v1;
        public float2 v2;

        public float a0;
        public float b0;
        public float c0;
        public bool sign0;  // true if v0 in l0 > 0, false if v0 in l0 < 0

        public float a1;
        public float b1;
        public float c1;
        public bool sign1;

        public float a2;
        public float b2;
        public float c2;
        public bool sign2;

        public bool IsInside(float2 point)
        {
            bool s0 = a0 * point.x + b0 * point.y + c0 > 0.0f ? true : false;
            bool s1 = a1 * point.x + b1 * point.y + c1 > 0.0f ? true : false;
            bool s2 = a2 * point.x + b2 * point.y + c2 > 0.0f ? true : false;
            return s0 == sign0 && s1 == sign1 && s2 == sign2;
        }

        public float2 ClosestToLine(float2 position, float a, float b, float c)
        {
            float t = (c + a * position.x + b * position.y) / (a * a + b * b);
            return new float2(position.x - a * t, position.y - b * t);
        }

        public TriangleSampleInfo Sample(float2 position)
        {
            bool s0 = a0 * position.x + b0 * position.y + c0 > 0.0f ? true : false;
            bool s1 = a1 * position.x + b1 * position.y + c1 > 0.0f ? true : false;
            bool s2 = a2 * position.x + b2 * position.y + c2 > 0.0f ? true : false;
            if (s0 == sign0 && s1 == sign1 && s2 == sign2)
            {//inside triangle
                return new TriangleSampleInfo
                {
                    result = position,
                    isInside = true
                };
            }
            else if (s0 != sign0 && s1 == sign1 && s2 == sign2)
            {//near l0 and may be v1 or v2
                float2 newPosition = ClosestToLine(position, a0, b0, c0);
                //check the side for l1 and l2
                bool sl1 = a1 * newPosition.x + b1 * newPosition.y + c1 > 0.0f ? true : false;
                bool sl2 = a2 * newPosition.x + b2 * newPosition.y + c2 > 0.0f ? true : false;
                if (sl1 == sign1 && sl2 == sign2)
                {
                    return new TriangleSampleInfo
                    {
                        result = newPosition,
                        isInside = false
                    };
                }
                else if (sl1 == sign1 && sl2 != sign2)
                {
                    return new TriangleSampleInfo
                    {
                        result = v1,
                        isInside = false
                    };
                }
                else
                {
                    return new TriangleSampleInfo
                    {
                        result = v2,
                        isInside = false
                    };
                }
            }
            else if (s0 == sign0 && s1 != sign1 && s2 == sign2)
            {//near l1 and may be v0 or v2
                float2 newPosition = ClosestToLine(position, a1, b1, c1);
                bool sl0 = a0 * newPosition.x + b0 * newPosition.y + c0 > 0.0f ? true : false;
                bool sl2 = a2 * newPosition.x + b2 * newPosition.y + c2 > 0.0f ? true : false;
                if (sl0 == sign0 && sl2 == sign2)
                {
                    return new TriangleSampleInfo
                    {
                        result = newPosition,
                        isInside = false
                    };
                }
                else if (sl0 == sign0 && sl2 != sign2)
                {
                    return new TriangleSampleInfo
                    {
                        result = v0,
                        isInside = false
                    };
                }
                else
                {
                    return new TriangleSampleInfo
                    {
                        result = v2,
                        isInside = false
                    };
                }
            }
            else if (s0 == sign0 && s1 == sign1 && s2 != sign2)
            {//near l2 and may be v0 or v1
                float2 newPosition = ClosestToLine(position, a2, b2, c2);
                bool sl0 = a0 * newPosition.x + b0 * newPosition.y + c0 > 0.0f ? true : false;
                bool sl1 = a1 * newPosition.x + b1 * newPosition.y + c1 > 0.0f ? true : false;
                if (sl0 == sign0 && sl1 == sign1)
                {
                    return new TriangleSampleInfo
                    {
                        result = newPosition,
                        isInside = false
                    };
                }
                else if (sl0 == sign0 && sl1 != sign1)
                {
                    return new TriangleSampleInfo
                    {
                        result = v0,
                        isInside = false
                    };
                }
                else
                {
                    return new TriangleSampleInfo
                    {
                        result = v1,
                        isInside = false
                    };
                }
            }
            else if (s0 == sign0 && s1 != sign1 && s2 != sign2)
            {
                return new TriangleSampleInfo
                {
                    result = v0,
                    isInside = false
                };
            }
            else if (s0 != sign0 && s1 == sign1 && s2 != sign2)
            {
                return new TriangleSampleInfo
                {
                    result = v1,
                    isInside = false
                };
            }
            else if (s0 != sign0 && s1 != sign1 && s2 == sign2)
            {
                return new TriangleSampleInfo
                {
                    result = v2,
                    isInside = false
                };
            }
            else
            {//impossible case
                //return new float2(-3, -3);
                return new TriangleSampleInfo
                {
                    result = position,
                    isInside = false
                };
            }

        }
    }

    public class Raycastinfo
    {
        public int steps;
        public int nodeIndex;
        public float2 targetPosition;
        public float2 minimalPosition;
        public float minDistance;
    }
    
    public struct NavmeshComponent : IComponentData
    {
        public int rootIndex;
    }

    public struct BBox : IComponentData
    {
        public float2 min;
        public float2 max;
        
        public void Init()
        {
            min = new float2(0.0f, 0.0f);
            max = new float2(0.0f, 0.0f);
        }

        public void Extend()
        {//slightly extend bounding box, by adding some margin
            float margin = 0.5f;
            min = new float2(min.x - margin, min.y - margin);
            max = new float2(max.x + margin, max.y + margin);
        }

        public void SetFromTriangle(NavMeshTriangle triangle)
        {
            min = new float2(
                math.min(math.min(triangle.v0.x, triangle.v1.x), triangle.v2.x),
                math.min(math.min(triangle.v0.y, triangle.v1.y), triangle.v2.y));

            max = new float2(
                math.max(math.max(triangle.v0.x, triangle.v1.x), triangle.v2.x),
                math.max(math.max(triangle.v0.y, triangle.v1.y), triangle.v2.y));
        }

        public void ExtendByPoint(float2 p)
        {
            min = new float2(
                math.min(min.x, p.x),
                math.min(min.y, p.y));

            max = new float2(
                math.max(max.x, p.x),
                math.max(max.y, p.y));
        }

        public float GetWidth()
        {
            return max.x - min.x;
        }

        public float GetHeight()
        {
            return max.y - min.y;
        }

        public float2 GetCenter()
        {
            return new float2((min.x + max.x) / 2, (min.y + max.y) / 2);
        }

        public bool IsContains(float2 position)
        {
            return position.x >= min.x && position.x <= max.x && position.y >= min.y && position.y <= max.y;
        }
    }

    public struct NavmeshSampleInfo
    {
        public bool isCorrect;
        public int triangleIndex;
        public bool isInside;  // true, if the sampled point inside any triangle, false if returnd point computed as boundary triangle point

        public float2 result;
    }

    public struct NavMeshBVHNode : IBufferElementData
    {
        public int index;

        public int leftIndex;
        public bool isLeft;

        public int rightIndex;
        public bool isRight;

        public NavMeshTriangle triangle;
        public bool isTriangle;

        public BBox boundingBox;

        //return closest position inside the given node
        public NavmeshSampleInfo Sample(DynamicBuffer<NavMeshBVHNode> nodes, float2 position)
        {
            if(isTriangle)
            {//this node contains triangle, sample position on it
                TriangleSampleInfo trSample = triangle.Sample(position);
                return new NavmeshSampleInfo
                {
                    isCorrect = true,
                    triangleIndex = index,
                    result = trSample.result,
                    isInside = trSample.isInside
                };
            }
            else
            {
                if(isLeft && isRight)
                {
                    NavMeshBVHNode left = nodes[leftIndex];
                    NavMeshBVHNode right = nodes[rightIndex];

                    //check is position inside the left and right bounding boxes
                    bool isInLeft = left.boundingBox.IsContains(position);
                    bool isInRight = right.boundingBox.IsContains(position);

                    if(isInLeft && !isInRight)
                    {//only on the left part
                        return left.Sample(nodes, position);
                    }
                    else if(!isInLeft && isInRight)
                    {//only on the right
                        return right.Sample(nodes, position);
                    }
                    else if(isInLeft && isInRight)
                    {//on both sides
                        NavmeshSampleInfo leftSample = left.Sample(nodes, position);
                        NavmeshSampleInfo rightSample = right.Sample(nodes, position);

                        if(leftSample.isCorrect && rightSample.isCorrect)
                        {
                            if(math.distancesq(position, leftSample.result) < math.distancesq(position, rightSample.result))
                            {
                                return leftSample;
                            }
                            else
                            {
                                return rightSample;
                            }
                        }
                        else if(leftSample.isCorrect && !rightSample.isCorrect)
                        {
                            return leftSample;
                        }
                        else if(!leftSample.isCorrect && rightSample.isCorrect)
                        {
                            return rightSample;
                        }
                        else
                        {//both samples incorrect, return any of them
                            return leftSample;
                        }
                    }
                    else
                    {//not in left or right, ignore this bounding box
                        return new NavmeshSampleInfo
                        {
                            isCorrect = false,
                            result = new float2(),
                            triangleIndex = -1,
                            isInside = false
                        };
                    }
                }
                else
                {
                    //this is impossible
                    return new NavmeshSampleInfo
                    {
                        isCorrect = false,
                        result = new float2(),
                        triangleIndex = -1,
                        isInside = false
                    };
                }
            }
        }
    }

#if UNITY_EDITOR
    public class BVHNodeClass
    {
        public BVHNodeClass left;
        public bool isLeft;  // true if left entity is an entity for left part of the tree, false if this entity is null
        public BVHNodeClass right;
        public bool isRight;

        public NavMeshTriangle triangle;
        public bool isTriangle;  // true, if the node store the object, false if this node is inner node of the tree

        public BBox boundingBox;

        public BVHNodeClass()
        {

        }

        public BVHNodeClass(List<NavMeshTriangle> objects)
        {
            if(objects.Count == 1)
            {//only one object, place it to the triangle struct
                isTriangle = true;
                triangle = objects[0];
                isLeft = false;
                isRight = false;

                boundingBox = new BBox();
                boundingBox.SetFromTriangle(triangle);
                boundingBox.Extend();
            }
            else
            {
                //build common bounding box
                boundingBox = new BBox();
                boundingBox.Init();
                float medianX = 0.0f;
                float medianY = 0.0f;
                for(int i = 0; i < objects.Count; i++)
                {
                    NavMeshTriangle tr = objects[i];
                    boundingBox.ExtendByPoint(tr.v0);
                    boundingBox.ExtendByPoint(tr.v1);
                    boundingBox.ExtendByPoint(tr.v2);

                    medianX += tr.v0.x;
                    medianX += tr.v1.x;
                    medianX += tr.v2.x;

                    medianY += tr.v0.y;
                    medianY += tr.v1.y;
                    medianY += tr.v2.y;
                }
                boundingBox.Extend();

                medianX = medianX / (objects.Count * 3);
                medianY = medianY / (objects.Count * 3);

                //choose axis with bigest length of the commonBB
                float width = boundingBox.GetWidth();
                float height = boundingBox.GetHeight();

                int axis = width > height ? 0 : 1;

                //devide all objects into two lists
                List<NavMeshTriangle> leftTriangles = new List<NavMeshTriangle>();
                List<NavMeshTriangle> rightTriangles = new List<NavMeshTriangle>();

                for(int i = 0; i < objects.Count; i++)
                {
                    NavMeshTriangle tr = objects[i];
                    float2 center = new float2(
                        (tr.v0.x + tr.v1.x + tr.v2.x) / 3.0f,
                        (tr.v0.y + tr.v1.y + tr.v2.y) / 3.0f);

                    if(axis == 0)
                    {
                        if(center.x < medianX)
                        {
                            leftTriangles.Add(tr);
                        }
                        else
                        {
                            rightTriangles.Add(tr);
                        }
                    }
                    else
                    {
                        if (center.y < medianY)
                        {
                            leftTriangles.Add(tr);
                        }
                        else
                        {
                            rightTriangles.Add(tr);
                        }
                    }
                }

                //check non-infinite recursion
                if(leftTriangles.Count > 0 && rightTriangles.Count == 0)
                {
                    //move last triangle from left to right
                    rightTriangles.Add(leftTriangles[leftTriangles.Count - 1]);
                    leftTriangles.RemoveAt(leftTriangles.Count - 1);
                }
                else if(leftTriangles.Count == 0 && rightTriangles.Count > 0)
                {
                    //move last triangle from right to left
                    leftTriangles.Add(rightTriangles[rightTriangles.Count - 1]);
                    rightTriangles.RemoveAt(rightTriangles.Count - 1);
                }

                //create two new nodes
                isTriangle = false;
                isLeft = true;
                left = new BVHNodeClass(leftTriangles);
                isRight = true;
                right = new BVHNodeClass(rightTriangles);
            }
        }

        public void GetAllBoxes(List<BBox> boxes)
        {
            boxes.Add(boundingBox);
            if(isLeft)
            {
                left.GetAllBoxes(boxes);
            }
            if(isRight)
            {
                right.GetAllBoxes(boxes);
            }
        }

        public void GetNodes(List<BVHNodeClass> nodes)
        {
            nodes.Add(this);
            if(isLeft)
            {
                left.GetNodes(nodes);
            }
            if(isRight)
            {
                right.GetNodes(nodes);
            }
        }
    }
#endif
}

