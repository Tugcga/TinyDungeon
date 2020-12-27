using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace TD
{
    [UpdateAfter(typeof(BulletControlSystem))]
    [UpdateBefore(typeof(BulletDestroySystem))]
    public class BulletExplosionSystem : SystemBase
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

            double time = Time.ElapsedTime;

            Entities.WithAll<BulletExplosionTag>().WithNone<DestroyBulletTag>().ForEach((Entity entity, in BulletComponent bullet, in HeightComponent height, in LineMoveComponent move) =>
            {
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

                cmdBuffer.AddComponent<DestroyBulletTag>(entity);
            }).Run();

            cmdBuffer.Playback(manager);
        }
    }
}
