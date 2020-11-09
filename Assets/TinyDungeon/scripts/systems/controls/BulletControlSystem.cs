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

        protected override void OnCreate()
        {
            manager = World.EntityManager;

            base.OnCreate();
        }

        protected override void OnUpdate()
        {
            EntityCommandBuffer cmdBuffer = new EntityCommandBuffer(Allocator.Temp);

            float deltaTime = Time.DeltaTime;
            Entities.WithNone<DestroyBulletTag>().ForEach((Entity entity, ref LineMoveComponent move, in DirectionComponent direction, in BulletComponent bullet) =>
            {
                //move.nextPosition = move.position + (new float2(direction.direction.x, direction.direction.y)) * bullet.speed * deltaTime;
                move.currentPoint += (new float2(direction.direction.x, direction.direction.y)) * bullet.speed * deltaTime;

                if(!move.isFreeLife)
                {//check, may be we jump over the end point
                    float2 toEnd = move.endPoint - move.currentPoint;
                    if(math.dot(direction.direction, toEnd) < 0.0)
                    {
                        cmdBuffer.AddComponent<DestroyBulletTag>(entity);
                    }
                }
            }).Run();

            double time = Time.ElapsedTime;

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

            cmdBuffer.Playback(EntityManager);
        }
    }
}
