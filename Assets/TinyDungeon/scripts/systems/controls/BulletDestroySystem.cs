using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace TD
{
    [UpdateAfter(typeof(BulletExplosionSystem))]
    [UpdateAfter(typeof(LifetimeControlSystem))]
    public class BulletDestroySystem : SystemBase
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
            Entities.WithAny<DestroyBulletTag>().ForEach((Entity entity) =>
            {
                cmdBuffer.DestroyEntity(entity);
            }).Run();
            cmdBuffer.Playback(manager);
        }
    }
}