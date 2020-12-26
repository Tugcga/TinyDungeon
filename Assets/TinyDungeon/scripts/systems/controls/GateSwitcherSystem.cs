using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Tiny.Audio;

namespace TD
{
    [UpdateAfter(typeof(PlayerPositionSystem))]
    public class GateSwitcherSystem : SystemBase
    {
        EntityQuery switchersGroup;
        EntityQuery gatesGroup;
        EntityQuery bulletGroup;

        EntityManager manager;

        protected override void OnCreate()
        {
            base.OnCreate();
            manager = World.EntityManager;
            switchersGroup = manager.CreateEntityQuery(typeof(GateSwitcherComponent), ComponentType.ReadOnly<PlayerPositionComponent>(), ComponentType.ReadOnly<Translation>());
            gatesGroup = manager.CreateEntityQuery(typeof(GateComponent), typeof(CollisionEdgesSetComponent));
            bulletGroup = manager.CreateEntityQuery(typeof(BulletComponent), typeof(LineMoveComponent), ComponentType.Exclude<DestroyBulletTag>(), ComponentType.Exclude<LineMoveInitTag>());

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
#if USE_FOREACH_SYSTEM
                Entities.ForEach((Entity entity, in GateComponent gate, in CollisionEdgesSetComponent edges) =>
                {
#else
                NativeArray<Entity> gates = gatesGroup.ToEntityArray(Allocator.Temp);
                for (int i = 0; i < gates.Length; i++)
                {
                    Entity entity = gates[i];
                    GateComponent gate = manager.GetComponentData<GateComponent>(entity);
                    CollisionEdgesSetComponent edges = manager.GetComponentData<CollisionEdgesSetComponent>(entity);
#endif
                    gatesMap.Add((int)gate.gateColor, new GateData()
                    {
                        entity = entity,
                        gate = gate,
                        indexes = edges
                    });
#if USE_FOREACH_SYSTEM
                }).Run();
#else
                }
                gates.Dispose();
#endif

                //next iterate throw all switchers and find the close to the player
                double time = Time.ElapsedTime;
                CollisionMap map = GetSingleton<CollisionMap>();
                bool updateBullets = false;
#if USE_FOREACH_SYSTEM
                EntityCommandBuffer cmdBuffer = new EntityCommandBuffer(Allocator.Temp);
                Entities.ForEach((Entity entity, ref GateSwitcherComponent switcher, in PlayerPositionComponent playerPos, in Translation swCenter) =>
                {
#else
                NativeArray<Entity> switchers = switchersGroup.ToEntityArray(Allocator.Temp);
                for (int i = 0; i < switchers.Length; i++)
                {
                    Entity entity = switchers[i];
                    GateSwitcherComponent switcher = manager.GetComponentData<GateSwitcherComponent>(entity);
                    PlayerPositionComponent playerPos = manager.GetComponentData<PlayerPositionComponent>(entity);
                    Translation swCenter = manager.GetComponentData<Translation>(entity);
#endif
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
#if USE_FOREACH_SYSTEM
                                cmdBuffer.AddComponent(currentGate.entity, new StartAnimationTag() { animationIndex = currentGate.gate.isActive ? 0 : 1});  // 0 - open
                                cmdBuffer.AddComponent<AudioSourceStart>(switcher.soundAction);
                                cmdBuffer.AddComponent(entity, new StartAnimationTag() { animationIndex = 0 });
                                cmdBuffer.AddComponent<AudioSourceStart>(currentGate.gate.soundOpenClose);
#else
                                manager.AddComponentData(currentGate.entity, new StartAnimationTag() { animationIndex = currentGate.gate.isActive ? 0 : 1 });  // 0 - open, 1 - close
                                manager.AddComponent<AudioSourceStart>(switcher.soundAction);  // play action sound
                                //also start switcher animation
                                manager.AddComponentData(entity, new StartAnimationTag() { animationIndex = 0 });
                                //gate sound
                                manager.AddComponent<AudioSourceStart>(currentGate.gate.soundOpenClose);
#endif

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
#if USE_FOREACH_SYSTEM
                                cmdBuffer.SetComponent(currentGate.entity, currentGate.gate);
#else
                                manager.SetComponentData(currentGate.entity, currentGate.gate);
#endif

                                updateBullets = true;
                            }
                            while (gatesMap.TryGetNextValue(out currentGate, ref iterator));
                        }
                    }
#if USE_FOREACH_SYSTEM
                }).Run();
#else
                    manager.SetComponentData(entity, switcher);
                }
                switchers.Dispose();
#endif

                gatesMap.Dispose();

                if (updateBullets)
                {
#if USE_FOREACH_SYSTEM
                    Entities.WithAll<BulletComponent>().WithNone<DestroyBulletTag, LineMoveInitTag>().ForEach((Entity entity, in LineMoveComponent move) =>
                    {
                        cmdBuffer.AddComponent(entity, new LineMoveInitTag()
                        {
                            hostPosition = move.currentPoint
                        });
                    }).Run();
#else
                    NativeArray<Entity> bullets = bulletGroup.ToEntityArray(Allocator.Temp);
                    for(int i = 0; i < bullets.Length; i++)
                    {
                        Entity entity = bullets[i];
                        LineMoveComponent move = manager.GetComponentData<LineMoveComponent>(entity);

                        manager.AddComponentData(entity, new LineMoveInitTag()
                        {
                            hostPosition = move.currentPoint
                        });
                    }
                    bullets.Dispose();
#endif
                }
#if USE_FOREACH_SYSTEM
                cmdBuffer.Playback(manager);
#endif
            }
        }
    }
}