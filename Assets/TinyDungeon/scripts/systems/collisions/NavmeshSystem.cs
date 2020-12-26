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
        EntityQuery movableGroup;
        EntityManager manager;
        bool _useNavmesh;

        protected override void OnCreate()
        {
            //RequireSingletonForUpdate<CollisionMap>();
            //isInit = false;
            manager = World.EntityManager;
            movableGroup = manager.CreateEntityQuery(typeof(MovableComponent), typeof(MovableCollisionComponent), ComponentType.Exclude<DeadTag>());
            base.OnCreate();

            _useNavmesh = true;
        }

        protected override void OnUpdate()
        {
            bool useNavmesh = _useNavmesh;

            Entities.WithNone<DeadTag>().ForEach((ref MovableComponent move, in MovableCollisionComponent collision) =>
            {
                if (move.isMove)
                {
                    if(useNavmesh)
                    {
                        CollisionInfo info = collision.GetPoint(move.position, move.nextPosition);
                        move.position = info.endPoint;
                    }
                    else
                    {
                        move.position = move.nextPosition;
                    }
                }
            }).Run();
        }
    }

}
