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
#if USE_FOREACH_SYSTEM
            EntityCommandBuffer cmdBuffer = new EntityCommandBuffer(Allocator.Temp);
            Entities.ForEach((Entity entity, in LifetimeComponent life) =>
            {
                if (time - life.startTime > life.lifeTime)
                {
                    cmdBuffer.DestroyEntity(entity);
                }
            }).Run();
            cmdBuffer.Playback(manager);
#else
            NativeArray<Entity> entities = lifetimeGroup.ToEntityArray(Allocator.Temp);
            for(int i = 0; i < entities.Length; i++)
            {
                Entity entity = entities[i];
                LifetimeComponent life = manager.GetComponentData<LifetimeComponent>(entity);

                if (time - life.startTime > life.lifeTime)
                {
                    manager.DestroyEntity(entity);
                }
            }
            entities.Dispose();
#endif
        }
    }
}