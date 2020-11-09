using System;
using UnityEngine;
using Unity.Entities;
using UnityEngine.AI;
using Unity.Mathematics;

#if UNITY_EDITOR
using System.Collections.Generic;
#endif

namespace TD
{
    public class NavmeshConverter : MonoBehaviour, IConvertGameObjectToEntity
    {
#if UNITY_EDITOR
        List<NavMeshTriangle> triangles = new List<NavMeshTriangle>();
        BVHNodeClass tree = new BVHNodeClass();
        List<BBox> treeBoxes = new List<BBox>();
#endif

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            //prepare editor data
            Convert();

            List<BVHNodeClass> allNodes = new List<BVHNodeClass>();
            tree.GetNodes(allNodes);

            dstManager.AddComponentData(entity, new NavmeshComponent()
            {
                rootIndex = allNodes.IndexOf(tree)
            });

            DynamicBuffer<NavMeshBVHNode> nodes = dstManager.AddBuffer<NavMeshBVHNode>(entity);

            for(int i = 0; i < allNodes.Count; i++)
            {
                BVHNodeClass node = allNodes[i];
                NavMeshBVHNode dataNode = new NavMeshBVHNode();
                if(node.isLeft)
                {
                    dataNode.isLeft = true;
                    dataNode.leftIndex = allNodes.IndexOf(node.left);
                }
                else
                {
                    dataNode.isLeft = false;
                }
                if(node.isRight)
                {
                    dataNode.isRight = true;
                    dataNode.rightIndex = allNodes.IndexOf(node.right);
                }
                else
                {
                    dataNode.isRight = false;
                }
                dataNode.boundingBox = new BBox
                {
                    min = node.boundingBox.min,
                    max = node.boundingBox.max
                };

                dataNode.index = i;

                if (node.isTriangle)
                {
                    dataNode.isTriangle = true;
                    NavMeshTriangle tr = node.triangle;
                    dataNode.triangle = new NavMeshTriangle
                    {
                        v0 = tr.v0,
                        v1 = tr.v1,
                        v2 = tr.v2,
                        a0 = tr.a0,
                        b0 = tr.b0,
                        c0 = tr.c0,
                        a1 = tr.a1,
                        b1 = tr.b1,
                        c1 = tr.c1,
                        a2 = tr.a2,
                        b2 = tr.b2,
                        c2 = tr.c2,
                        sign0 = tr.sign0,
                        sign1 = tr.sign1,
                        sign2 = tr.sign2
                    };
                }
                else
                {
                    dataNode.isTriangle = false;
                }

                nodes.Add(dataNode);
            }

            //create list of entities for each node
            /*List<Entity> allEntites = new List<Entity>();
            for(int i = 0; i < allNodes.Count; i++)
            {
                BVHNodeClass node = allNodes[i];
                if(node == tree)
                {
                    allEntites.Add(entity);
                }
                else
                {
                    allEntites.Add(dstManager.CreateEntity(nodeArch));
                }
            }

            Debug.Log("navmesh entities: " + allEntites.Count.ToString());

            //next fill data and links for entities
            for(int i = 0; i < allEntites.Count; i++)
            {
                BVHNodeClass node = allNodes[i];
                Entity e = allEntites[i];

                NavMeshBVHNode newNodeData = new NavMeshBVHNode();
                newNodeData.isLeft = node.isLeft;
                if(node.isLeft)
                {
                    newNodeData.left = allEntites[allNodes.IndexOf(node.left)];
                }
                newNodeData.isRight = node.isRight;
                if (node.isLeft)
                {
                    newNodeData.right = allEntites[allNodes.IndexOf(node.right)];
                }
                newNodeData.isTriangle = node.isTriangle;
                if(node.isTriangle)
                {
                    newNodeData.triangle = node.triangle;
                }
                newNodeData.boundingBox = node.boundingBox;

                dstManager.SetComponentData(e, newNodeData);
            }*/
        }

#if UNITY_EDITOR

        public void Convert()
        {
            NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();

            //form the list of triangles
            triangles.Clear();
            Vector3[] vertices = triangulation.vertices;
            int[] indices = triangulation.indices;
            int trianglesCount = indices.Length / 3;

            for (int i = 0; i < trianglesCount; i++)
            {
                float2 v0 = new float2(vertices[indices[3 * i]].x, vertices[indices[3 * i]].z);
                float2 v1 = new float2(vertices[indices[3 * i + 1]].x, vertices[indices[3 * i + 1]].z);
                float2 v2 = new float2(vertices[indices[3 * i + 2]].x, vertices[indices[3 * i + 2]].z);
                
                float a0 = v2.y - v1.y;
                float b0 = -(v2.x - v1.x);
                float c0 = (v2.x - v1.x) * v1.y - (v2.y - v1.y) * v1.x;

                float a1 = v2.y - v0.y;
                float b1 = -(v2.x - v0.x);
                float c1 = (v2.x - v0.x) * v0.y - (v2.y - v0.y) * v0.x;

                float a2 = v1.y - v0.y;
                float b2 = -(v1.x - v0.x);
                float c2 = (v1.x - v0.x) * v0.y - (v1.y - v0.y) * v0.x;

                triangles.Add(new NavMeshTriangle()
                {
                    v0 = v0,
                    v1 = v1,
                    v2 = v2,
                    a0 = a0,
                    b0 = b0,
                    c0 = c0,
                    a1 = a1,
                    b1 = b1,
                    c1 = c1,
                    a2 = a2,
                    b2 = b2,
                    c2 = c2,
                    sign0 = a0 * v0.x + b0 * v0.y + c0 > 0.0f ? true : false,
                    sign1 = a1 * v1.x + b1 * v1.y + c1 > 0.0f ? true : false,
                    sign2 = a2 * v2.x + b2 * v2.y + c2 > 0.0f ? true : false
                }); ;
            }

            // next we should organize all triangles into bvh tree
            tree = new BVHNodeClass(triangles);
            treeBoxes.Clear();

            tree.GetAllBoxes(treeBoxes);
        }

        void OnDrawGizmos()
        {
            if(triangles.Count > 0)
            {
                Gizmos.color = new Color(0.1f, 0.9f, 0.1f);
                for (int i = 0; i < triangles.Count; i++)
                {
                    NavMeshTriangle tr = triangles[i];
                    Gizmos.DrawLine(new Vector3(tr.v0.x, 0.0f, tr.v0.y), new Vector3(tr.v1.x, 0.0f, tr.v1.y));
                    Gizmos.DrawLine(new Vector3(tr.v1.x, 0.0f, tr.v1.y), new Vector3(tr.v2.x, 0.0f, tr.v2.y));
                    Gizmos.DrawLine(new Vector3(tr.v0.x, 0.0f, tr.v0.y), new Vector3(tr.v2.x, 0.0f, tr.v2.y));
                }
            }

            //draw bvh bounding boxes
            Gizmos.color = new Color(0.9f, 0.1f, 0.1f, 0.05f);
            float delta = 0.01f;
            for (int i = 0; i < treeBoxes.Count; i++)
            {
                BBox box = treeBoxes[i];
                float2 center = box.GetCenter();
                Gizmos.DrawCube(new Vector3(center.x, delta, center.y), new Vector3(box.GetWidth() + delta, delta, box.GetHeight() + delta));
            }
        }
#endif
    }
}
