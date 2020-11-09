using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using UnityEditor;

namespace TD
{
    [ExecuteInEditMode]
    public class SwitcherIdentifier : ItemIdentifier, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {
        public GameObject switcherPrefab;
        public GateColors color;
        public Color handleColor;
        public float visualHandleTangentHeight;
        [Range(0.0f, 5.0f)]
        public float visualHandleWidth;

        GateColors lastUpdateColor;
        GameObject[] gates;

        public void Start()
        {
            
        }

        public void Update()
        {
            if(color != lastUpdateColor)
            {
                UpdateGates();

                lastUpdateColor = color;
            }
        }

        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            referencedPrefabs.Add(switcherPrefab);
        }

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new StartInstantiateComponent()
            {
                prefab = conversionSystem.GetPrimaryEntity(switcherPrefab)
            });

            dstManager.AddComponentData(entity, new StartSwitcherIdentifierComponent()
            {
                isActive = isActive,
                color = color,
                radius = gameObject.GetComponent<RadiusIdentifier>().radius,
                switcherIndex = gameObject.GetInstanceID()
            });
        }

        public void UpdateGates()
        {
            GateIdentifier[] gs = FindObjectsOfType<GateIdentifier>();
            List<GameObject> objs = new List<GameObject>();
            for(int i = 0; i < gs.Length; i++)
            {
                if(gs[i].gateColor == color)
                {
                    objs.Add(gs[i].gameObject);
                }
            }

            gates = objs.ToArray();
        }

        public void OnDrawGizmos()
        {
            Handles.color = handleColor;
            if(gates != null)
            {
                for (int i = 0; i < gates.Length; i++)
                {
                    //Handles.DrawLine(transform.position, gates[i].transform.position);
                    Handles.DrawBezier(transform.position, gates[i].transform.position,
                        transform.position + Vector3.up * visualHandleTangentHeight, gates[i].transform.position + Vector3.up * visualHandleTangentHeight,
                        handleColor, null, visualHandleWidth);
                }
            }
        }

    }

}