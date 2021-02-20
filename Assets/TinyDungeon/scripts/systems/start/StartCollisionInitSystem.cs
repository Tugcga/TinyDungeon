using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace TD
{
    public class StartCollisionInitSystem : SystemBase
    {
        EntityQuery movableGroup;
        EntityManager manager;

        protected override void OnCreate()
        {
            base.OnCreate();
            manager = World.EntityManager;
            movableGroup = manager.CreateEntityQuery(typeof(MovableComponent), ComponentType.Exclude<MovableCollisionComponent>()/*, ComponentType.Exclude<BulletComponent>()*/);
            RequireSingletonForUpdate<CollisionMap>();
        }

        protected override void OnUpdate()
        {
            CollisionMap map = GetSingleton<CollisionMap>();

            EntityCommandBuffer cmdBuffer = new EntityCommandBuffer(Allocator.Temp);
            //for any movable entity we should add component, which contains collision data
            //add to player (contains movablecomponent) or to the bullet (contains linemoveinittag)
            Entities.WithoutBurst().
                WithNone<MovableCollisionComponent>().
                WithAny<MovableComponent, LineMoveInitTag>().
                ForEach((Entity entity) =>
            {//use WithoutBurst because with burst in some cases the component is not added to the entity
                cmdBuffer.AddComponent(entity, new MovableCollisionComponent() { collisionMap = map.collisionMap});
            }).Run();

            cmdBuffer.Playback(manager);
        }
    }
}