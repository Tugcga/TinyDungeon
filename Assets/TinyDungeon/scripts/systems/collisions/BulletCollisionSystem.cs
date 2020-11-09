using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace TD
{
    [UpdateAfter(typeof(BulletControlSystem))]
    [UpdateBefore(typeof(LifeControlSystem))]
    public class BulletCollisionSystem : SystemBase
    {
        EntityManager manager;
        EntityQuery targetsGroup;
        //EntityQuery bulletsGroup;
        float squareSize;

        protected override void OnCreate()
        {
            manager = World.EntityManager;
            targetsGroup = manager.CreateEntityQuery(typeof(LifeComponent), typeof(RadiusComponent), typeof(Translation), ComponentType.Exclude<DeadTag>());
            
            squareSize = 2.0f;

            base.OnCreate();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        struct MapData
        {
            public Entity entity;
            public float2 position;
            public ItemType type;
            public float radius;
            public int maxLife;
            public int life;
        }

        protected override void OnUpdate()
        {
            EntityCommandBuffer cmdBuffer = new EntityCommandBuffer(Allocator.Temp);
            NativeMultiHashMap<int, MapData> targetsMap = new NativeMultiHashMap<int, MapData>(targetsGroup.CalculateEntityCount(), Allocator.Persistent);
            float localSize = squareSize;
            Entities.WithNone<DeadTag>().ForEach((Entity entity, in Translation trn, in RadiusComponent radius, in LifeComponent life, in ItemTypeComponent itemType) =>
            {
                int index = Helper.GetSegmentIndexFromPosition(trn.Value, localSize);
                targetsMap.Add(index, new MapData()
                {
                    entity = entity,
                    position = new float2(trn.Value.x, trn.Value.z),
                    radius = radius.Value,
                    maxLife = life.maxLife,
                    life = life.life,
                    type = itemType.type
                });
            }).Run();

            Entities.ForEach((Entity entity, in LineMoveComponent move, in HostTypeComponent host, in BulletComponent bullet, in DirectionComponent direction) =>
            {
                int2 uv = Helper.GetSegmentUVFromPosition(move.currentPoint, localSize);
                NativeArray<int> segmentIndexes = new NativeArray<int>(9, Allocator.TempJob);
                //fill segment indexes
                segmentIndexes[0] = Helper.GetSegmentIndex(uv);
                segmentIndexes[1] = Helper.GetSegmentIndex(uv + new int2(0, 1));
                segmentIndexes[2] = Helper.GetSegmentIndex(uv + new int2(1, 0));
                segmentIndexes[3] = Helper.GetSegmentIndex(uv + new int2(1, 1));
                segmentIndexes[4] = Helper.GetSegmentIndex(uv + new int2(0, -1));
                segmentIndexes[5] = Helper.GetSegmentIndex(uv + new int2(-1, 0));
                segmentIndexes[6] = Helper.GetSegmentIndex(uv + new int2(-1, -1));
                segmentIndexes[7] = Helper.GetSegmentIndex(uv + new int2(-1, 1));
                segmentIndexes[8] = Helper.GetSegmentIndex(uv + new int2(1, -1));

                //get all entities from the hashmap
                NativeMultiHashMapIterator<int> iterator;
                MapData currentTarget;
                for (int i = 0; i < segmentIndexes.Length; i++)
                {
                    int bulletIndex = segmentIndexes[i];
                    if (targetsMap.TryGetFirstValue(bulletIndex, out currentTarget, out iterator))
                    {
                        do
                        {
                            if (!((host.host == HostTypes.HOST_PLAYER && currentTarget.type == ItemType.ITEM_PLAYER) || (host.host == HostTypes.HOST_ENEMY && currentTarget.type == ItemType.ITEM_TOWER)))  //if bullet hosted by player and entity is player, ignore collision
                            {
                                if (math.distancesq(move.currentPoint, currentTarget.position) < currentTarget.radius * currentTarget.radius)
                                {//apply damage to entity
                                    cmdBuffer.SetComponent<LifeComponent>(currentTarget.entity, new LifeComponent()
                                    {
                                        life = currentTarget.life - bullet.damage,
                                        maxLife = currentTarget.maxLife
                                    });
                                    //destory the bullet
                                    cmdBuffer.AddComponent(entity, new DestroyBulletTag());

                                    if(host.host == HostTypes.HOST_PLAYER && currentTarget.type == ItemType.ITEM_TOWER)
                                    {//player hit the tower
                                        //switch tower state to atack
                                        cmdBuffer.AddComponent(currentTarget.entity, new StartAtackPlayerTag() { atackDirection = -direction .direction});
                                    }
                                }
                            }
                        }
                        while (targetsMap.TryGetNextValue(out currentTarget, ref iterator));
                    }
                }
                segmentIndexes.Dispose();
            }
            ).Run();

            targetsMap.Dispose();
            cmdBuffer.Playback(manager);
        }
    }
}