using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace TD
{
    [UpdateBefore(typeof(BulletCollisionSystem))]
    [UpdateAfter(typeof(LineMoveInitSystem))]
    public class BulletControlSystem : SystemBase
    {
        EntityManager manager;
        EntityQuery bulletDirectionGroup;
        EntityQuery bulletHeightGroup;

        protected override void OnCreate()
        {
            manager = World.EntityManager;
            bulletDirectionGroup = manager.CreateEntityQuery(typeof(LineMoveComponent), typeof(BulletComponent), typeof(DirectionComponent), ComponentType.Exclude<DestroyBulletTag>());
            bulletHeightGroup = manager.CreateEntityQuery(typeof(LineMoveComponent), typeof(BulletComponent), typeof(HeightComponent), typeof(DestroyBulletTag));

            base.OnCreate();
        }

        protected override void OnUpdate()
        {
            EntityCommandBuffer cmdBuffer = new EntityCommandBuffer(Allocator.Temp);

            float deltaTime = Time.DeltaTime;

            Entities.WithNone<BulletExplosionTag, DestroyBulletTag>().ForEach((Entity entity, ref LineMoveComponent move, in DirectionComponent direction, in BulletComponent bullet) =>
            {
                move.currentPoint += (new float2(direction.direction.x, direction.direction.y)) * bullet.speed * deltaTime;

                if (!move.isFreeLife)
                {//check, may be we jump over the end point
                    float2 toEnd = move.endPoint - move.currentPoint;
                    if (math.dot(direction.direction, toEnd) < 0.0)
                    {
                        cmdBuffer.AddComponent<BulletExplosionTag>(entity);
                    }
                }
            }).Run();

            cmdBuffer.Playback(manager);
        }
    }
}
