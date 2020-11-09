using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using UnityEditor;

namespace TD
{
    public class AmmoIdentifier : ItemIdentifier, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {
        public GameObject ammoPrefab;
        public int ammoCount;

        [Range(0.0f, 10.0f)]
        public float labelHeight;
        public Color handleColor;
        [Range(0.0f, 1.0f)]
        public float handleRadius;

        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            referencedPrefabs.Add(ammoPrefab);
        }

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new StartInstantiateComponent()
            {
                prefab = conversionSystem.GetPrimaryEntity(ammoPrefab)
            });

            dstManager.AddComponentData(entity, new StartAmmoIdentifierComponent()
            {
                ammoIndex = gameObject.GetInstanceID(),
                ammoCount = ammoCount
            });
        }

        public void OnDrawGizmos()
        {
            Handles.Label(transform.position + Vector3.up * labelHeight, ammoCount.ToString());
            Handles.color = handleColor;
            Handles.DrawSolidDisc(transform.position, Vector3.up, handleRadius);
        }
    }

}