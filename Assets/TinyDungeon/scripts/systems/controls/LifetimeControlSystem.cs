using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace TD
{
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
            Entities.ForEach((Entity entity, in LifetimeComponent life) =>
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