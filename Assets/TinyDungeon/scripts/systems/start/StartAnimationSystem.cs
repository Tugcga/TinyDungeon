using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Tiny.Animation;

namespace TD
{
    /*
     * Run this system for each object, which should change the animation
     * We add to the object StartAnimationTag component, and delete it here, after starting animation
     * 
     * We can not use in this system Bursted function, because it should use World class
     */
    public class StartAnimationSystem : SystemBase
    {
        EntityQuery animationsGroup;
        EntityManager manager;

        protected override void OnCreate()
        {
            base.OnCreate();
            manager = World.EntityManager;
            animationsGroup = manager.CreateEntityQuery(typeof(StartAnimationTag));
        }

        protected override void OnUpdate()
        {
            NativeArray<Entity> entities = animationsGroup.ToEntityArray(Allocator.TempJob);
            for(int i = 0; i < entities.Length; i++)
            {
                Entity e = entities[i];
                StartAnimationTag anim = manager.GetComponentData<StartAnimationTag>(e);

                Helper.SelectClipAtIndex(World, e, anim.animationIndex);
                TinyAnimation.Play(World, e);

                manager.RemoveComponent<StartAnimationTag>(e);
            }
            entities.Dispose();
        }
    }
}