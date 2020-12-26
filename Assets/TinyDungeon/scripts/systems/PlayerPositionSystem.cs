using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace TD
{
    /*
     * This system transfer the player position to each PlayerComponent, which can be exist in any entity
     * This component needs for tracking player position (for camera center, for example)
     */
    [UpdateAfter(typeof(NavmeshSystem))]
    [UpdateBefore(typeof(CameraTransformSystem))]
    [UpdateBefore(typeof(SearchControlSystem))]
    [UpdateBefore(typeof(GateSwitcherSystem))]
    [UpdateBefore(typeof(AmmoCollectionSystem))]
    public class PlayerPositionSystem : SystemBase
    {
        EntityManager manager;
        EntityQuery playerGroup;

        protected override void OnCreate()
        {
            manager = World.EntityManager;
            playerGroup = manager.CreateEntityQuery(typeof(PlayerPositionComponent));
            RequireSingletonForUpdate<PlayerComponent>();
            
            base.OnCreate();
        }

        protected override void OnUpdate()
        {
            //use PlayerComponent as singleton
            Entity playerEntity = GetSingletonEntity<PlayerComponent>();
            MovableComponent move = manager.GetComponentData<MovableComponent>(playerEntity);
            RadiusComponent radius = manager.GetComponentData<RadiusComponent>(playerEntity);
            bool isDead = manager.HasComponent<DeadTag>(playerEntity);
#if USE_FOREACH_SYSTEM
            Entities.ForEach((ref PlayerPositionComponent playerPosition) =>
            {
                playerPosition.position = move.position;
                playerPosition.isActive = !isDead;
            }).Run();
#else
            NativeArray<Entity> entities = playerGroup.ToEntityArray(Allocator.Temp);
            for(int i = 0; i < entities.Length; i++)
            {
                PlayerPositionComponent playerPosition = manager.GetComponentData<PlayerPositionComponent>(entities[i]);

                //actions over entities data
                playerPosition.position = move.position;
                playerPosition.isActive = !isDead;

                manager.SetComponentData(entities[i], playerPosition);
            }
            entities.Dispose();
#endif
        }
    }
}