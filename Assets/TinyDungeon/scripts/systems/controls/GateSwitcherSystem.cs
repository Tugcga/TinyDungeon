using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Tiny.Animation;

namespace TD
{
    [UpdateAfter(typeof(PlayerPositionSystem))]
    public class GateSwitcherSystem : SystemBase
    {
        EntityQuery switchersGroup;
        EntityQuery gatesGroup;
        //EntityQuery lineMovesGroup;
        EntityManager manager;

        protected override void OnCreate()
        {
            base.OnCreate();
            manager = World.EntityManager;
            switchersGroup = manager.CreateEntityQuery(typeof(GateSwitcherComponent), ComponentType.ReadOnly<PlayerPositionComponent>(), ComponentType.ReadOnly<Translation>());
            gatesGroup = manager.CreateEntityQuery(typeof(GateComponent), typeof(CollisionEdgesSetComponent));
            //lineMovesGroup = manager.CreateEntityQuery(typeof(LineMoveComponent), typeof(BulletComponent), ComponentType.Exclude<DestroyBulletTag>(), ComponentType.Exclude<LineMoveInitTag>());
            RequireSingletonForUpdate<CollisionMap>();
            RequireSingletonForUpdate<KeyboardInputComponent>();
        }

        struct GateData
        {
            public Entity entity;
            public GateComponent gate;
            public CollisionEdgesSetComponent indexes;
        }
        
        protected override void OnUpdate()
        {
            KeyboardInputComponent keyboard = GetSingleton<KeyboardInputComponent>();
            if (keyboard.isPressAction)
            {
                //geather all gates by it color
                //key - color
                NativeMultiHashMap<int, GateData> gatesMap = new NativeMultiHashMap<int, GateData>(gatesGroup.CalculateEntityCount(), Allocator.TempJob);
                Entities.ForEach((Entity entity, in GateComponent gate, in CollisionEdgesSetComponent edges) =>
                {
                    gatesMap.Add((int)gate.gateColor, new GateData() 
                    { 
                        entity = entity,
                        gate = gate,
                        indexes = edges
                    });
                }).Run();

                //next iterate throw all switchers and find the close to the player
                double time = Time.ElapsedTime;
                EntityCommandBuffer cmdBuffer = new EntityCommandBuffer(Allocator.Temp);
                CollisionMap map = GetSingleton<CollisionMap>();
                bool updateBullets = false;
                Entities.ForEach((Entity entity, ref GateSwitcherComponent switcher, in PlayerPositionComponent playerPos, in Translation swCenter) =>
                {
                    if (switcher.isActive && time - switcher.lastActionTime > switcher.actionCooldawn && playerPos.isActive && math.distancesq(playerPos.position, new float2(swCenter.Value.x, swCenter.Value.z)) < switcher.radius * switcher.radius)
                    {
                        switcher.lastActionTime = time;

                        //next iterate by gates with the given color
                        NativeMultiHashMapIterator<int> iterator;
                        GateData currentGate;
                        if (gatesMap.TryGetFirstValue((int)switcher.gateColor, out currentGate, out iterator))
                        {
                            do
                            {
                                //change the state of the gate
                                //play animation, for example
                                if (currentGate.gate.isActive)
                                {
                                    cmdBuffer.AddComponent(currentGate.entity, new StartAnimationTag() { animationIndex = 0 });
                                    //Helper.SelectClipAtIndex(World, currentGate.entity, 0);  // 0 - open
                                }
                                else
                                {
                                    cmdBuffer.AddComponent(currentGate.entity, new StartAnimationTag() { animationIndex = 1 });
                                    //Helper.SelectClipAtIndex(World, currentGate.entity, 1);  // 1 - close
                                }
                                //TinyAnimation.Play(World, currentGate.entity);

                                //and disable the collision edges
                                if (currentGate.gate.isActive)
                                {
                                    map.collisionMap.Value.Deactivate(currentGate.indexes);
                                }
                                else
                                {
                                    map.collisionMap.Value.Activate(currentGate.indexes);
                                }
                                currentGate.gate.isActive = !currentGate.gate.isActive;
                                cmdBuffer.SetComponent(currentGate.entity, currentGate.gate);

                                updateBullets = true;
                            }
                            while (gatesMap.TryGetNextValue(out currentGate, ref iterator));
                        }
                    }
                }).Run();

                gatesMap.Dispose();

                if (updateBullets)
                {
                    Entities.WithAll<BulletComponent>().WithNone<DestroyBulletTag, LineMoveInitTag>().ForEach((Entity entity, in LineMoveComponent move) =>
                    {
                        cmdBuffer.AddComponent(entity, new LineMoveInitTag()
                        {
                            hostPosition = move.currentPoint
                        });
                    }).Run();
                }

                cmdBuffer.Playback(manager);
            }

            /*ref CollisionMapBlobAsset map = ref GetSingleton<CollisionMap>().collisionMap.Value;
            KeyboardInputComponent keyboard = GetSingleton<KeyboardInputComponent>();

            bool updateBullets = false;
            double time = Time.ElapsedTime;

            if(keyboard.isPressAction)
            {
                NativeArray<Entity> gates = gatesGroup.ToEntityArray(Allocator.TempJob);
                NativeArray<Entity> switchers = switchersGroup.ToEntityArray(Allocator.TempJob);
                for (int i = 0; i < switchers.Length; i++)
                {
                    Entity e = switchers[i];
                    PlayerPositionComponent playerPos = manager.GetComponentData<PlayerPositionComponent>(e);
                    GateSwitcherComponent switcher = manager.GetComponentData<GateSwitcherComponent>(e);
                    Translation swCenter = manager.GetComponentData<Translation>(e);

                    if (switcher.isActive && time - switcher.lastActionTime > switcher.actionCooldawn && playerPos.isActive && math.distancesq(playerPos.position, new float2(swCenter.Value.x, swCenter.Value.z)) < switcher.radius * switcher.radius)
                    {//active player press action near the switcher
                        switcher.lastActionTime = time;
                        //we should change the state of all gates with a switcher color
                        for (int g = 0; g < gates.Length; g++)
                        {
                            Entity gate = gates[g];
                            GateComponent gData = manager.GetComponentData<GateComponent>(gate);
                            if(gData.gateColor == switcher.gateColor)
                            {
                                //change the state of the gate
                                //play animation, for example
                                if(gData.isActive)
                                {
                                    Helper.SelectClipAtIndex(World, gate, 0);  // 0 - open
                                }
                                else
                                {
                                    Helper.SelectClipAtIndex(World, gate, 1);  // 1 - close
                                }
                                TinyAnimation.Play(World, gate);

                                //and disable the collision edges
                                CollisionEdgesSetComponent indexes = manager.GetComponentData<CollisionEdgesSetComponent>(gate);
                                if (gData.isActive)
                                {
                                    map.Deactivate(indexes);
                                }
                                else
                                {
                                    map.Activate(indexes);
                                }
                                gData.isActive = !gData.isActive;
                                manager.SetComponentData(gate, gData);
                                updateBullets = true;
                            }
                        }

                        manager.SetComponentData(e, switcher);
                    }
                }
                switchers.Dispose();
                gates.Dispose();
            }

            if(updateBullets)
            {
                EntityCommandBuffer cmdBuffer = new EntityCommandBuffer(Allocator.Temp);
                Entities.WithAll<BulletComponent>().WithNone<DestroyBulletTag, LineMoveInitTag>().ForEach((Entity entity, in LineMoveComponent move) =>
                {
                    cmdBuffer.AddComponent(entity, new LineMoveInitTag()
                    {
                        hostPosition = move.currentPoint
                    });
                }).Run();

                cmdBuffer.Playback(manager);
            }*/
        }
    }
}