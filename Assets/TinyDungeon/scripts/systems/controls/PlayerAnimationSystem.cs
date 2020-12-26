using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Tiny.Audio;

namespace TD
{
    public class PlayerAnimationSystem : SystemBase
    {
        EntityManager manager;

        protected override void OnCreate()
        {
            manager = World.EntityManager;
        }

        protected override void OnUpdate()
        {
            EntityCommandBuffer cmdBuffer = new EntityCommandBuffer(Allocator.Temp);
            Entities.
#if USE_FOREACH_SYSTEM
#else
                WithoutBurst().
#endif
                ForEach((Entity entity, ref PlayerAnimationsComponent playerAnim) =>
                {
                    if(playerAnim.newAimation != playerAnim.currentAnimation)
                    {
                        cmdBuffer.AddComponent<StartAnimationTag>(entity, new StartAnimationTag() { animationIndex = playerAnim.newAimation});

                        playerAnim.currentAnimation = playerAnim.newAimation;
                    }
                }).Run();
            cmdBuffer.Playback(manager);
        }
    }
}