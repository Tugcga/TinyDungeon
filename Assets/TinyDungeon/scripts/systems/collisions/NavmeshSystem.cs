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
            manager = World.EntityManager;
            movableGroup = manager.CreateEntityQuery(typeof(MovableComponent), typeof(MovableCollisionComponent), ComponentType.Exclude<DeadTag>());
            base.OnCreate();

            _useNavmesh = true;
        }

        protected override void OnUpdate()
        {
            bool useNavmesh = _useNavmesh;

            Entities.WithNone<DeadTag>().ForEach((ref MovableComponent move, ref MovableCollisionComponent collision) =>
            {
                if (move.isMove)
                {
                    if(useNavmesh)
                    {
                        //CollisionInfo info = collision.GetPoint(move.position, move.nextPosition);
                        ref CollisionMapBlobAsset asset = ref collision.collisionMap.Value;
                        CollisionInfo info = asset.GetPoint(move.position, move.nextPosition, true);
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
