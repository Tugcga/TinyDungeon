using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace TD
{
    public class GateIdentifier : ItemIdentifier, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {
        /*
         * This parameters used for collision map conversation, to identify collision adges with each gate object
         */
        public GateColors gateColor;
        public GameObject gatePrefab;

        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            referencedPrefabs.Add(gatePrefab);
        }

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new StartInstantiateComponent()
            {
                prefab = conversionSystem.GetPrimaryEntity(gatePrefab)
            });

            dstManager.AddComponentData(entity, new StartGateIdentifierComponent()
            {
                isActiveGate = isActive,
                gateColor = gateColor,
                gateIndex = gameObject.GetInstanceID()
            });
        }
    }

}