using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace TD
{
    public class DeadCooldawnSystem : SystemBase
    {
        EntityQuery playerGroup;
        EntityManager manager;

        protected override void OnCreate()
        {
            manager = World.EntityManager;
            playerGroup = manager.CreateEntityQuery(typeof(PlayerComponent), typeof(LifeComponent), typeof(DeadCooldawnComponent), typeof(DeadTag), ComponentType.Exclude<BlockTag>());

            RequireSingletonForUpdate<SceneControllerComponent>();
            RequireSingletonForUpdate<UIFadeScreenComponent>();
            base.OnCreate();
        }

        protected override void OnUpdate()
        {
            double time = Time.ElapsedTime;
            UIFadeScreenComponent fade = GetSingleton<UIFadeScreenComponent>();
            SceneControllerComponent controller = GetSingleton<SceneControllerComponent>();

            bool isChange = false;

            EntityCommandBuffer cmdBuffer = new EntityCommandBuffer(Allocator.Temp);
            //PlayerComponent is singleton, but use Bursted code for speed up
            Entities.WithAll<DeadTag>().WithNone<BlockTag>().ForEach((Entity entity, in PlayerComponent player, in LifeComponent life, in DeadCooldawnComponent cooldawn) =>
            {
                if (time - cooldawn.startTime > cooldawn.delayTime)
                {
                    //restart the level
                    cmdBuffer.AddComponent<BlockTag>(entity);
                    fade.isActive = true;
                    fade.direction = true;
                    controller.targetSceneIndex = controller.loadedSceneIndex;
                    isChange = true;        
                }
            }).Run();

            cmdBuffer.Playback(manager);

            if(isChange)
            {
                SetSingleton<UIFadeScreenComponent>(fade);
                SetSingleton<SceneControllerComponent>(controller);
            }
        }
    }
}