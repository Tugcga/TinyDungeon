using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Tiny.Animation;

namespace TD
{
    public class StartInstantiateSystem : SystemBase
    {
        EntityManager manager;
        EntityQuery gateStartGroup;
        EntityQuery barrelsStartGroup;
        EntityQuery towersStartGroup;
        EntityQuery switchersStartGroup;
        EntityQuery ammosStartGroup;
        EntityQuery exitsStartGroup;

        //Random random;

        protected override void OnCreate()
        {
            manager = World.EntityManager;
            RequireSingletonForUpdate<CollisionMap>();
            gateStartGroup = manager.CreateEntityQuery(typeof(StartGateIdentifierComponent), typeof(StartInstantiateComponent), typeof(Translation), typeof(Rotation));
            barrelsStartGroup = manager.CreateEntityQuery(typeof(StartBarrelIdentifierComponent), typeof(StartInstantiateComponent), typeof(Translation), typeof(Rotation));
            towersStartGroup = manager.CreateEntityQuery(typeof(StartTowerIdentifierComponent), typeof(StartInstantiateComponent), typeof(Translation), typeof(Rotation));
            switchersStartGroup = manager.CreateEntityQuery(typeof(StartSwitcherIdentifierComponent), typeof(StartInstantiateComponent), typeof(Translation), typeof(Rotation));
            ammosStartGroup = manager.CreateEntityQuery(typeof(StartAmmoIdentifierComponent), typeof(StartInstantiateComponent), typeof(Translation), typeof(Rotation));
            exitsStartGroup = manager.CreateEntityQuery(typeof(StartLevelExitIdentifierComponent), typeof(StartInstantiateComponent), typeof(Translation), typeof(Rotation));

            //random = new Random(3);  // this random also different for different entities

            base.OnCreate();
        }

        Entity ItemProcess(Entity e, ref CollisionMapBlobAsset map)
        {
            StartInstantiateComponent start = manager.GetComponentData<StartInstantiateComponent>(e);
            Translation translation = manager.GetComponentData<Translation>(e);
            Rotation rotation = manager.GetComponentData<Rotation>(e);

            bool hasGate = manager.HasComponent<StartGateIdentifierComponent>(e);
            StartGateIdentifierComponent startGate = new StartGateIdentifierComponent();
            if (hasGate)
            {
                startGate = manager.GetComponentData<StartGateIdentifierComponent>(e);
            }
            bool hasbarrel = manager.HasComponent<StartBarrelIdentifierComponent>(e);
            StartBarrelIdentifierComponent startBarrel = new StartBarrelIdentifierComponent();
            if(hasbarrel)
            {
                startBarrel = manager.GetComponentData<StartBarrelIdentifierComponent>(e);
            }
            bool hasTower = manager.HasComponent<StartTowerIdentifierComponent>(e);
            StartTowerIdentifierComponent startTower = new StartTowerIdentifierComponent();
            if(hasTower)
            {
                startTower = manager.GetComponentData<StartTowerIdentifierComponent>(e);
            }
            bool hasSwitcher = manager.HasComponent<StartSwitcherIdentifierComponent>(e);
            StartSwitcherIdentifierComponent startSwitcher = new StartSwitcherIdentifierComponent();
            if (hasSwitcher)
            {
                startSwitcher = manager.GetComponentData<StartSwitcherIdentifierComponent>(e);
            }
            bool hasAmmo = manager.HasComponent<StartAmmoIdentifierComponent>(e);
            StartAmmoIdentifierComponent startAmmo = new StartAmmoIdentifierComponent();
            if (hasAmmo)
            {
                startAmmo = manager.GetComponentData<StartAmmoIdentifierComponent>(e);
            }
            bool hasExit = manager.HasComponent<StartLevelExitIdentifierComponent>(e);
            StartLevelExitIdentifierComponent startExit = new StartLevelExitIdentifierComponent();
            if (hasExit)
            {
                startExit = manager.GetComponentData<StartLevelExitIdentifierComponent>(e);
            }

            //as in simple case, instntiate geometry
            Entity newEntity = manager.Instantiate(start.prefab);
            manager.SetComponentData<Translation>(newEntity, new Translation()
            {
                Value = translation.Value
            });
            manager.SetComponentData<Rotation>(newEntity, new Rotation()
            {
                Value = rotation.Value
            });

            //but next we should setup collision edges indexes
            NativeArray<int> indexes = new NativeArray<int>(8, Allocator.TempJob);
            int position = 0;
            for (int i = 0; i < map.nodes.Length; i++)
            {
                RTreeCollisionNode node = map.nodes[i];
                if (node.childrenIndexes.Length() == 0)
                {//this node contains collision edge
                    if (hasGate && node.property.colliderType == ColliderType.COLLIDER_GATE)
                    {

                        if (node.property.colliderHostColor == startGate.gateColor && node.property.colliderHostIndex == startGate.gateIndex)
                        {
                            map.nodes[i].property.isActive = startGate.isActiveGate;
                            indexes[position] = node.index;
                            position++;
                        }
                    }
                    else if (hasbarrel && node.property.colliderType == ColliderType.COLLIDER_BARELL)
                    {
                        if (node.property.colliderHostIndex == startBarrel.barrelIndex)
                        {
                            map.nodes[i].property.isActive = startBarrel.isActive;
                            indexes[position] = node.index;
                            position++;
                        }
                    }
                    else if (hasTower && node.property.colliderType == ColliderType.COLLIDER_TOWER)
                    {
                        if (node.property.colliderHostIndex == startTower.towerIndex)
                        {
                            map.nodes[i].property.isActive = startTower.isActive;
                            indexes[position] = node.index;
                            position++;
                        }
                    }
                }
            }
            for (int i = position; i < indexes.Length; i++)
            {
                indexes[i] = 0;
            }

            //we should set indexes only for collideble items
            if (hasGate || hasbarrel || hasTower)
            {
                manager.SetComponentData(newEntity, new CollisionEdgesSetComponent(indexes[0], indexes[1], indexes[2], indexes[3], indexes[4], indexes[5], indexes[6], indexes[7]));
            }

            //set type component to the new entity
            if(hasGate)
            {
                GateComponent gate = manager.GetComponentData<GateComponent>(newEntity);
                gate.gateColor = startGate.gateColor;
                gate.isActive = startGate.isActiveGate;
                manager.SetComponentData(newEntity, gate);

                //play animation to open or close the gate
                if(startGate.isActiveGate)
                {
                    manager.AddComponentData(newEntity, new StartAnimationTag() { animationIndex = 1});
                }
                else
                {
                    manager.AddComponentData(newEntity, new StartAnimationTag() { animationIndex = 0 });
                }
            }
            else if(hasbarrel)
            {
                BarrelComponent barrel = manager.GetComponentData<BarrelComponent>(newEntity);
                barrel.isActive = startBarrel.isActive;
                manager.SetComponentData(newEntity, barrel);

                ExplodedItemComponent exp = manager.GetComponentData<ExplodedItemComponent>(newEntity);
                exp.damageRadius = startBarrel.damageRadius;
                manager.SetComponentData(newEntity, exp);
            }
            else if (hasTower)
            {
                TowerComponent tower = manager.GetComponentData<TowerComponent>(newEntity);
                tower.isActive = startTower.isActive;
                manager.SetComponentData(newEntity, tower);

                //set tower properties
                SearchPlayerComponent sp = manager.GetComponentData<SearchPlayerComponent>(newEntity);
                StartTowerIdentifierComponent towerData = manager.GetComponentData<StartTowerIdentifierComponent>(e);

                Random towerRandom = new Random((uint)towerData.towerIndex);
                manager.SetComponentData(newEntity, new RandomComponent()
                {
                    random = towerRandom
                });

                sp.visibleDistance = towerData.visibleDistance;
                sp.searchAngle = towerData.searchAngle;
                sp.visibleAngle = towerData.visibleAngle;
                float a = towerRandom.NextFloat(-sp.searchAngle, sp.searchAngle);  // something strange here, because values are not too random
                sp.angle = a;

                manager.SetComponentData(newEntity, sp);

            }
            else if(hasSwitcher)
            {
                GateSwitcherComponent switcher = manager.GetComponentData<GateSwitcherComponent>(newEntity);
                switcher.isActive = startSwitcher.isActive;
                switcher.radius = startSwitcher.radius;
                switcher.gateColor = startSwitcher.color;

                manager.SetComponentData(newEntity, switcher);
            }
            else if(hasAmmo)
            {
                AmmoComponent ammo = manager.GetComponentData<AmmoComponent>(newEntity);
                ammo.count = startAmmo.ammoCount;

                manager.SetComponentData(newEntity, ammo);
                manager.AddComponentData(newEntity, new Scale() { Value = 1.0f});  // we need scale for animator
            }
            else if(hasExit)
            {
                LevelExitComponent exit = manager.GetComponentData<LevelExitComponent>(newEntity);
                exit.levelIndex = startExit.levelIndex;
                exit.activeRadius = startExit.activeRadius;

                manager.SetComponentData(newEntity, exit);
            }
            indexes.Dispose();

            return newEntity;
        }

        protected override void OnUpdate()
        {
            EntityCommandBuffer cmdBuffer = new EntityCommandBuffer(Allocator.Temp);
            ref CollisionMapBlobAsset map = ref GetSingleton<CollisionMap>().collisionMap.Value;

            //for simply dynamic instantiated objects
            Entities.
#if USE_FOREACH_SYSTEM
#else
                WithoutBurst().
#endif
                WithNone<StartGateIdentifierComponent, StartBarrelIdentifierComponent, StartTowerIdentifierComponent>().
                WithNone<StartSwitcherIdentifierComponent, StartAmmoIdentifierComponent, StartLevelExitIdentifierComponent>()
                .ForEach((Entity entity, in StartInstantiateComponent start, in Translation translation, in Rotation rotation) =>
            {
                Entity newEntity = cmdBuffer.Instantiate(start.prefab);
                cmdBuffer.SetComponent<Translation>(newEntity, new Translation()
                {
                    Value = translation.Value
                });
                cmdBuffer.SetComponent<Rotation>(newEntity, new Rotation()
                {
                    Value = rotation.Value
                });
                cmdBuffer.DestroyEntity(entity);
            }).Run();

            //for objects, which requres collision edges identifiers
            //Entities.ForEach((Entity entity, in StartGateIdentifierComponent startGate, in StartInstantiateComponent start, in Translation translation, in Rotation rotation) =>
            //collision map reference is not compatible with ForEach lambdas, so, use the cycle
            NativeArray<Entity> gates = gateStartGroup.ToEntityArray(Allocator.TempJob);
            for(int g = 0; g < gates.Length; g++)
            {
                Entity e = gates[g];
                ItemProcess(e, ref map);

                cmdBuffer.DestroyEntity(e);
            }
            //}).Run();
            gates.Dispose();

            NativeArray<Entity> barrels = barrelsStartGroup.ToEntityArray(Allocator.TempJob);
            for (int b = 0; b < barrels.Length; b++)
            {
                Entity e = barrels[b];
                ItemProcess(e, ref map);

                cmdBuffer.DestroyEntity(e);
            }
            barrels.Dispose();

            NativeArray<Entity> towers = towersStartGroup.ToEntityArray(Allocator.TempJob);
            for (int t = 0; t < towers.Length; t++)
            {
                Entity e = towers[t];
                Entity newEntity = ItemProcess(e, ref map);

                cmdBuffer.DestroyEntity(e);
            }
            towers.Dispose();

            NativeArray<Entity> switchers = switchersStartGroup.ToEntityArray(Allocator.TempJob);
            for (int s = 0; s < switchers.Length; s++)
            {
                Entity e = switchers[s];
                Entity newEntity = ItemProcess(e, ref map);

                cmdBuffer.DestroyEntity(e);
            }
            switchers.Dispose();

            NativeArray<Entity> ammoms = ammosStartGroup.ToEntityArray(Allocator.TempJob);
            for (int a = 0; a < ammoms.Length; a++)
            {
                Entity e = ammoms[a];
                Entity newEntity = ItemProcess(e, ref map);

                cmdBuffer.DestroyEntity(e);
            }
            ammoms.Dispose();

            NativeArray<Entity> exits = exitsStartGroup.ToEntityArray(Allocator.TempJob);
            for (int e = 0; e < exits.Length; e++)
            {
                Entity exit = exits[e];
                Entity newEntity = ItemProcess(exit, ref map);

                cmdBuffer.DestroyEntity(exit);
            }
            exits.Dispose();

            cmdBuffer.Playback(manager);
        }
    }
}
