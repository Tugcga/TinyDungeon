using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace TD
{
    /*
     * This system runs only for entities, which contains StartAtackPlayer component
     * We add this component when the player hit the tower, and tower should rotate to atack him
     * In fact we should only change the tower search state
     */
    public class StartAtackPlayerSystem : SystemBase
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
            Entities.
#if USE_FOREACH_SYSTEM
#else
                WithoutBurst().
#endif
                ForEach((Entity entity, ref SearchPlayerComponent search, in TowerComponent tower, in StartAtackPlayerTag atack) =>
            {
                if(tower.isActive)
                {
                    search.state = TowerState.STATE_CHECK_DIRECTION;
                    search.checkDirection = atack.atackDirection;
                }
                cmdBuffer.RemoveComponent<StartAtackPlayerTag>(entity);

            }).Run();
            cmdBuffer.Playback(manager);
        }
    }
}