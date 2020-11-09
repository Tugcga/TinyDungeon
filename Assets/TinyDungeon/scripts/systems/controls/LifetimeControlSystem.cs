using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace TD
{
    public class LifetimeControlSystem : SystemBase
    {
        EntityManager manager;

        protected override void OnCreate()
        {
            manager = World.EntityManager;

            base.OnCreate();
        }

        protected override void OnUpdate()
        {
            double time = Time.ElapsedTime;
            EntityCommandBuffer cmdBuffer = new EntityCommandBuffer(Allocator.Temp);
            Entities.ForEach((Entity entity, in LifetimeComponent life) =>
            {
                if (time - life.startTime > life.lifeTime)
                {
                    cmdBuffer.DestroyEntity(entity);
                }
            }).Run();
            cmdBuffer.Playback(EntityManager);
        }
    }
}