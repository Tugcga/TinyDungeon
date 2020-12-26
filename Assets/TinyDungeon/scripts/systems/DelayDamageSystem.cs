using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace TD
{
    public class DelaySystem : SystemBase
    {
        EntityQuery delayGroup;
        EntityManager manager;

        protected override void OnCreate()
        {
            base.OnCreate();
            manager = World.EntityManager;
            delayGroup = manager.CreateEntityQuery(typeof(LifeComponent), typeof(DelayDamageComponent));
        }

        protected override void OnUpdate()
        {
            double time = Time.ElapsedTime;

            EntityCommandBuffer cmdBuffer = new EntityCommandBuffer(Allocator.Temp);
            ComponentType t = typeof(DelayDamageComponent);
            Entities.ForEach((Entity entity, ref LifeComponent life, in DelayDamageComponent damage) =>
            {
                if(time - damage.startTime > damage.delay)
                {
                    life.life -= damage.damage;
                    cmdBuffer.RemoveComponent(entity, t);
                }
            }).Run();

            cmdBuffer.Playback(manager);
        }
    }
}