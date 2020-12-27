using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace TD
{
    [UpdateBefore(typeof(BulletDestroySystem))]
    public class LifetimeControlSystem : SystemBase
    {
        EntityManager manager;
        EntityQuery lifetimeGroup;

        protected override void OnCreate()
        {
            manager = World.EntityManager;
            lifetimeGroup = manager.CreateEntityQuery(typeof(LifetimeComponent));

            base.OnCreate();
        }

        protected override void OnUpdate()
        {
            double time = Time.ElapsedTime;
            EntityCommandBuffer cmdBuffer = new EntityCommandBuffer(Allocator.Temp);

            //for bullets add destroy tag
            Entities.WithAny<BulletComponent>().ForEach((Entity entity, in LifetimeComponent life) =>
            {
                if (time - life.startTime > life.lifeTime)
                {
                    cmdBuffer.AddComponent<DestroyBulletTag>(entity);
                }
            }).Run();

            //for all other life-time objects (explosions, etc) - simply destroy it
            Entities.WithNone<BulletComponent, DestroyBulletTag>().ForEach((Entity entity, in LifetimeComponent life) =>
            {
                if (time - life.startTime > life.lifeTime)
                {
                    cmdBuffer.DestroyEntity(entity);
                }
            }).Run();

            cmdBuffer.Playback(manager);
        }
    }
}