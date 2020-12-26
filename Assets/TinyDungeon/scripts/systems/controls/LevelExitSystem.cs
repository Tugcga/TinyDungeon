using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace TD
{
    [UpdateAfter(typeof(PlayerPositionSystem))]
    public class LevelExitSystem : SystemBase
    {
        EntityManager manager;
        EntityQuery exitsGroup;

        protected override void OnCreate()
        {
            manager = EntityManager;
            exitsGroup = manager.CreateEntityQuery(ComponentType.ReadOnly<Translation>(),
                ComponentType.ReadOnly<PlayerPositionComponent>(),
                ComponentType.ReadOnly<LevelExitComponent>(),
                ComponentType.Exclude<BlockTag>());

            base.OnCreate();
            RequireSingletonForUpdate<SceneControllerComponent>();
            RequireSingletonForUpdate<UIFadeScreenComponent>();
        }

        protected override void OnUpdate()
        {
            SceneControllerComponent controller = GetSingleton<SceneControllerComponent>();
            UIFadeScreenComponent fade = GetSingleton<UIFadeScreenComponent>();

            bool isChange = false;
            double time = Time.ElapsedTime;

            EntityCommandBuffer cmdBuffer = new EntityCommandBuffer(Allocator.Temp);
            Entities.
                WithNone<BlockTag>().
                ForEach((Entity entity, in Translation translate, in PlayerPositionComponent playerPosition, in LevelExitComponent exit) =>
                {
                if (playerPosition.isActive && math.distance(playerPosition.position, new float2(translate.Value.x, translate.Value.z)) < exit.activeRadius)
                {
                    //start command to load next level
                    fade.isActive = true;
                    fade.direction = true;
                    isChange = true;

                    controller.targetSceneIndex = exit.levelIndex;

                    cmdBuffer.AddComponent<BlockTag>(entity);
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
