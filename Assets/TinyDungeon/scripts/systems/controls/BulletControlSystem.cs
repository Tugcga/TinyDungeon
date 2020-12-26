using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace TD
{
    [UpdateBefore(typeof(BulletCollisionSystem))]
    [UpdateAfter(typeof(LineMoveInitSystem))]
    public class BulletControlSystem : SystemBase
    {
        EntityManager manager;
        EntityQuery bulletDirectionGroup;
        EntityQuery bulletHeightGroup;

        protected override void OnCreate()
        {
            manager = World.EntityManager;
            bulletDirectionGroup = manager.CreateEntityQuery(typeof(LineMoveComponent), typeof(BulletComponent), typeof(DirectionComponent), ComponentType.Exclude<DestroyBulletTag>());
            bulletHeightGroup = manager.CreateEntityQuery(typeof(LineMoveComponent), typeof(BulletComponent), typeof(HeightComponent), typeof(DestroyBulletTag));

            base.OnCreate();
        }

        protected override void OnUpdate()
        {
#if USE_FOREACH_SYSTEM
            EntityCommandBuffer cmdBuffer = new EntityCommandBuffer(Allocator.Temp);
#endif

            float deltaTime = Time.DeltaTime;
#if USE_FOREACH_SYSTEM
            Entities.WithNone<DestroyBulletTag>().ForEach((Entity entity, ref LineMoveComponent move, in DirectionComponent direction, in BulletComponent bullet) =>
            {
#else
            NativeArray<Entity> entities = bulletDirectionGroup.ToEntityArray(Allocator.Temp);
            for (int i = 0; i < entities.Length; i++)
            {
                Entity entity = entities[i];
                LineMoveComponent move = manager.GetComponentData<LineMoveComponent>(entity);
                DirectionComponent direction = manager.GetComponentData<DirectionComponent>(entity);
                BulletComponent bullet = manager.GetComponentData<BulletComponent>(entity);
#endif
                move.currentPoint += (new float2(direction.direction.x, direction.direction.y)) * bullet.speed * deltaTime;

                if (!move.isFreeLife)
                {//check, may be we jump over the end point
                    float2 toEnd = move.endPoint - move.currentPoint;
                    if (math.dot(direction.direction, toEnd) < 0.0)
                    {
#if USE_FOREACH_SYSTEM
                        cmdBuffer.AddComponent<DestroyBulletTag>(entity);
#else
                        manager.AddComponentData(entity, new DestroyBulletTag());
#endif
                    }
                }
#if USE_FOREACH_SYSTEM
            }).Run();
#else
                manager.SetComponentData(entity, move);
            }
            entities.Dispose();
#endif

            //next two parts are too different, to union it into ine code
            double time = Time.ElapsedTime;

#if USE_FOREACH_SYSTEM
            Entities.WithAll<DestroyBulletTag>().ForEach((Entity entity, in BulletComponent bullet, in HeightComponent height, in LineMoveComponent move) =>
            {
                cmdBuffer.DestroyEntity(entity);
                Entity explosion = cmdBuffer.Instantiate(bullet.explosionPrefab);
                cmdBuffer.SetComponent<LifetimeComponent>(explosion, new LifetimeComponent()
                {
                    startTime = time,
                    lifeTime = bullet.explosionLifetime
                });
                cmdBuffer.SetComponent<Translation>(explosion, new Translation()
                {
                    Value = new float3(move.currentPoint.x, height.Value, move.currentPoint.y)
                });
            }).Run();

            cmdBuffer.Playback(manager);
#else
            NativeArray<Entity> bullets = bulletHeightGroup.ToEntityArray(Allocator.Temp);
            for (int i = 0; i < bullets.Length; i++)
            {
                Entity entity = bullets[i];
                LineMoveComponent move = manager.GetComponentData<LineMoveComponent>(entity);
                HeightComponent height = manager.GetComponentData<HeightComponent>(entity);
                BulletComponent bullet = manager.GetComponentData<BulletComponent>(entity);
                manager.DestroyEntity(entity);
                Entity explosion = manager.Instantiate(bullet.explosionPrefab);
                manager.SetComponentData<LifetimeComponent>(explosion, new LifetimeComponent()
                {
                    startTime = time,
                    lifeTime = bullet.explosionLifetime
                });
                manager.SetComponentData<Translation>(explosion, new Translation()
                {
                    Value = new float3(move.currentPoint.x, height.Value, move.currentPoint.y)
                });
            }
            bullets.Dispose();
#endif
        }
    }
}
