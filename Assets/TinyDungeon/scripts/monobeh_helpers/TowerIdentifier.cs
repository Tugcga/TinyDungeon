using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace TD
{
    public class TowerIdentifier : ItemIdentifier, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {
        public GameObject towerPrefab;

        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            referencedPrefabs.Add(towerPrefab);
        }

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            TowerSearchIdentifier search = GetComponent<TowerSearchIdentifier>();

            dstManager.AddComponentData(entity, new StartInstantiateComponent()
            {
                prefab = conversionSystem.GetPrimaryEntity(towerPrefab)
            });

            dstManager.AddComponentData(entity, new StartTowerIdentifierComponent()
            {
                isActive = isActive,
                towerIndex = gameObject.GetInstanceID(),
                visibleDistance = search.searchDistance,
                searchAngle = search.searchAngle,
                visibleAngle = search.visibleAngle
            });
        }
    }

}