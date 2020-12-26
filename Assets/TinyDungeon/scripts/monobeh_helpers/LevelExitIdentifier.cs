using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using UnityEditor;

namespace TD
{
    public class LevelExitIdentifier : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {
        public GameObject levelExitPrefab;
        public int levelIndex;

        [Range(0.0f, 10.0f)]
        public float labelHeight;
        public Color handleColor;
        [Range(0.0f, 10.0f)]
        public float handleRadius;

        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            referencedPrefabs.Add(levelExitPrefab);
        }

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new StartInstantiateComponent()
            {
                prefab = conversionSystem.GetPrimaryEntity(levelExitPrefab)
            });

            dstManager.AddComponentData(entity, new StartLevelExitIdentifierComponent()
            {
                levelIndex = levelIndex,
                activeRadius = handleRadius
            });
        }

        public void OnDrawGizmos()
        {
            Handles.Label(transform.position + Vector3.up * labelHeight, levelIndex.ToString());
            Handles.color = handleColor;
            Handles.DrawSolidDisc(transform.position, Vector3.up, handleRadius);
        }
    }

}