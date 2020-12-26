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
#if USE_FOREACH_SYSTEM
            EntityCommandBuffer cmdBuffer = new EntityCommandBuffer(Allocator.Temp);
            Entities.
                WithNone<BlockTag>().
                ForEach((Entity entity, in Translation translate, in PlayerPositionComponent playerPosition, in LevelExitComponent exit) =>
                {
#else
            NativeArray<Entity> exists = exitsGroup.ToEntityArray(Allocator.Temp);
            for (int i = 0; i < exists.Length; i++)
            {
                Entity entity = exists[i];
                Translation translate = manager.GetComponentData<Translation>(entity);
                PlayerPositionComponent playerPosition = manager.GetComponentData<PlayerPositionComponent>(entity);
                LevelExitComponent exit = manager.GetComponentData<LevelExitComponent>(entity);
#endif
                if (playerPosition.isActive && math.distance(playerPosition.position, new float2(translate.Value.x, translate.Value.z)) < exit.activeRadius)
                {
                    //start command to load next level
                    fade.isActive = true;
                    fade.direction = true;
                    isChange = true;

                    controller.targetSceneIndex = exit.levelIndex;
#if USE_FOREACH_SYSTEM
                    cmdBuffer.AddComponent<BlockTag>(entity);
#else
                    manager.AddComponent<BlockTag>(entity);
#endif
                }
#if USE_FOREACH_SYSTEM
            }).Run();
            cmdBuffer.Playback(manager);
#else
            }
            exists.Dispose();
#endif

            if(isChange)
            {
                SetSingleton<UIFadeScreenComponent>(fade);
                SetSingleton<SceneControllerComponent>(controller);
            }
        }

    }
}
