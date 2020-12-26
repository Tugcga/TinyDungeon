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
#if USE_FOREACH_SYSTEM
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
#else
            NativeArray<Entity> entities = movableGroup.ToEntityArray(Allocator.Temp);
            //UnityEngine.Debug.Log("movable entities: " + entities.Length.ToString());
            for(int i = 0; i < entities.Length; i++)
            {
                MovableComponent move = manager.GetComponentData<MovableComponent>(entities[i]);
                MovableCollisionComponent collision = manager.GetComponentData<MovableCollisionComponent>(entities[i]);

                if (useNavmesh)
                {
                    CollisionInfo info = collision.GetPoint(move.position, move.nextPosition);

                    //UnityEngine.Debug.Log("i=" + i.ToString() + " set position " + move.position.ToString() + " to " + info.endPoint.ToString() + " from " + move.nextPosition.ToString());
                    move.position = info.endPoint;
                }
                else
                {
                    move.position = move.nextPosition;
                }

                manager.SetComponentData(entities[i], move);
            }
            entities.Dispose();
#endif
        }
            
    }

}
