using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace TD
{
    [UpdateBefore(typeof(BulletCollisionSystem))]
    public class LineMoveInitSystem : SystemBase
    {
        EntityManager manager;
        float collisionThickness;

        protected override void OnCreate()
        {
            manager = World.EntityManager;
            RequireSingletonForUpdate<CollisionMap>();
            
            collisionThickness = 0.5f;  // by default the player has radius = 0.5f
            base.OnCreate();
        }

        protected override void OnUpdate()
        {
            CollisionMap mapData = GetSingleton<CollisionMap>();  // here we copy struct data to the variable

            EntityCommandBuffer cmdBuffer = new EntityCommandBuffer(Allocator.Temp);
            double time = Time.ElapsedTime;
            float shift = collisionThickness;
            Entities.WithNone<DestroyBulletTag>().WithAll<LineMoveInitTag>().ForEach((Entity entity, ref LineMoveComponent move, in BulletComponent bullet, in LineMoveInitTag init, in DirectionComponent direction, in LifetimeComponent lifetime) =>
            {
                //check, is there are any collisions between host position and emited position
                float2 eStart = init.hostPosition;
                float2 eEnd = move.currentPoint;

                //check collision with the edge between host center and bullet-emiter start point
                CollisionInfo eInfo = mapData.collisionMap.Value.GetPoint(eStart, eEnd, false);
                if (eInfo.isCollide)
                {//bullet starts inside the collider, so, we should only try to find the point near the geometry of the collider
                    float cosAlpha = math.dot(direction.direction, -eInfo.collisionEdge.normal);
                    float2 ea = new float2(eInfo.collisionEdge.a1, eInfo.collisionEdge.a2);
                    float2 pointOnEdge = eStart + (eEnd - eStart) * math.min(eInfo.minT, 1.0f);
                    float2 finishPoint = pointOnEdge -
                                        shift * eInfo.collisionEdge.normal +
                                        (math.dot(ea, direction.direction) > 0f ? 1f : -1f) * math.normalize(ea) * shift * math.tan(math.acos(cosAlpha));
                    //if the finishPoint is close, than eEnd to the host center, then bullet start is inside the geomtry, delete it
                    if (math.distancesq(eStart, finishPoint) < math.distancesq(eStart, eEnd))
                    {
                        cmdBuffer.DestroyEntity(entity);
                    }
                    else
                    {
                        float2 end = eEnd + direction.direction * bullet.speed * (float)(lifetime.lifeTime - (time - lifetime.startTime));  // free end point
                        //if end is closer, than finishPoint, then use free fly
                        if (math.distancesq(eStart, finishPoint) > math.distancesq(eStart, end))
                        {
                            move.endPoint = end;
                            move.isFreeLife = true;
                            cmdBuffer.RemoveComponent<LineMoveInitTag>(entity);
                        }
                        else
                        {
                            move.endPoint = finishPoint;
                            move.isFreeLife = false;
                            cmdBuffer.RemoveComponent<LineMoveInitTag>(entity);
                        }
                    }
                }
                else
                {
                    float2 start = move.currentPoint;
                    float2 end = start + direction.direction * bullet.speed * (float)(lifetime.lifeTime - (time - lifetime.startTime));

                    //find the result of intersection of the ray from start to end by collision map
                    CollisionInfo info = mapData.collisionMap.Value.GetPoint(start, end, false);
                    if (info.isCollide)
                    {
                        float cosAlpha = math.dot(direction.direction, -info.collisionEdge.normal);
                        float2 pointOnEdge = start + (end - start) * math.min(info.minT, 1.0f);
                        float2 a = new float2(info.collisionEdge.a1, info.collisionEdge.a2);
                        
                        float2 finishPoint = pointOnEdge -
                                            shift * info.collisionEdge.normal +
                                            (math.dot(a, direction.direction) > 0f ? 1.0f : -1.0f) * math.normalize(new float2(info.collisionEdge.a1, info.collisionEdge.a2)) * shift * math.tan(math.acos(cosAlpha));
                        
                        if(math.distancesq(start, end) < math.distancesq(start, finishPoint))
                        {
                            move.endPoint = info.endPoint;
                        }
                        else
                        {
                            move.endPoint = finishPoint;
                            move.isFreeLife = false;
                        }
                    }
                    else
                    {//no collisions in the trajectory of the bullet
                        move.endPoint = info.endPoint;
                    }

                    cmdBuffer.RemoveComponent<LineMoveInitTag>(entity);
                }
            }).Run();

            cmdBuffer.Playback(EntityManager);
        }
    }
}
