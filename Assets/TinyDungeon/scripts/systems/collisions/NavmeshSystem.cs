using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace TD
{
    [UpdateAfter(typeof(PlayerControlSystem))]
    [UpdateBefore(typeof(PlayerPositionSystem))]
    [UpdateBefore(typeof(PlayerTransformSystem))]
    [UpdateBefore(typeof(BulletTransformSystem))]
    public class NavmeshSystem : SystemBase
    {
        //EntityQuery movableGroup;
        //EntityManager manager;

        protected override void OnCreate()
        {
            //RequireSingletonForUpdate<CollisionMap>();
            //isInit = false;
            //manager = World.EntityManager;
            //movableGroup = manager.CreateEntityQuery(typeof(MovableComponent), typeof(MovableCollisionComponent), ComponentType.Exclude<DeadTag>());
            base.OnCreate();
        }

        protected override void OnUpdate()
        {
            Entities.WithNone<DeadTag>().ForEach((ref MovableComponent move, in MovableCollisionComponent collision) =>
            {
                if (move.isMove)
                {
                    CollisionInfo info = collision.GetPoint(move.position, move.nextPosition);

                    move.position = info.endPoint;
                }
            }).Run();
        }
            
    }

}
