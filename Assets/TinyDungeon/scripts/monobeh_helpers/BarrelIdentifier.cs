using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using UnityEditor;

namespace TD
{
    public class BarrelIdentifier : ItemIdentifier, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {
        public GameObject barrelPrefab;
        public Vector3 damageHandlePosition;
        public float damageRadius;
        public Color damageRadiusSolidColor;
        public Color damageRadiusBorderColor;

        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            referencedPrefabs.Add(barrelPrefab);
        }

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new StartInstantiateComponent()
            {
                prefab = conversionSystem.GetPrimaryEntity(barrelPrefab)
            });

            dstManager.AddComponentData(entity, new StartBarrelIdentifierComponent()
            {
                isActive = isActive,
                barrelIndex = gameObject.GetInstanceID(),
                damageRadius = damageRadius
            });
        }

        public void SetDamageHandle(Vector3 point)
        {
            damageHandlePosition = point;
            damageRadius = Vector3.Magnitude(point);
        }

        public void OnDrawGizmos()
        {
            Handles.color = damageRadiusSolidColor;
            Handles.DrawSolidDisc(transform.position, Vector3.up, damageRadius);

            Handles.color = damageRadiusBorderColor;
            Handles.DrawWireDisc(transform.position, Vector3.up, damageRadius);
        }
    }

}