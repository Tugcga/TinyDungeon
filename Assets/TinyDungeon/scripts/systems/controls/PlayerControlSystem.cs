using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace TD
{
    [UpdateAfter(typeof(MouseInputSystem))]
    [UpdateAfter(typeof(KeyboardInputSystem))]
    [UpdateAfter(typeof(CameraRotateSystem))]
    [UpdateBefore(typeof(NavmeshSystem))]
    public class PlayerControlSystem : SystemBase
    {
        EntityManager manager;

        protected override void OnCreate()
        {
            manager = World.EntityManager;

            RequireSingletonForUpdate<KeyboardInputComponent>();
            RequireSingletonForUpdate<MouseInputComponent>();
            RequireSingletonForUpdate<CameraDirectionComponent>();
            RequireSingletonForUpdate<CollisionMap>();

            base.OnCreate();
        }

        protected override void OnUpdate()
        {
            EntityCommandBuffer cmdBuffer = new EntityCommandBuffer(Allocator.Temp);

            KeyboardInputComponent input = GetSingleton<KeyboardInputComponent>();
            MouseInputComponent mouseInput = GetSingleton<MouseInputComponent>();
            CameraDirectionComponent cameraDirection = GetSingleton<CameraDirectionComponent>();

            float dt = Time.DeltaTime;
            double time = Time.ElapsedTime;
            //PlayerComponent is isngleton, but we use Bursten function for speed up
            Entities.WithNone<DeadTag>().ForEach((ref PlayerComponent player, ref ShoterComponent shoter, ref MovableComponent move, ref AmmunitionComponent ammo) =>
            {
                int horizontal = 0;
                int vertical = 0;
                if (input.isPressFront)
                {
                    vertical++;
                }
                if (input.isPressBack)
                {
                    vertical--;
                }
                if (input.isPressLeft)
                {
                    horizontal--;
                }
                if (input.isPressRight)
                {
                    horizontal++;
                }

                if (vertical != 0 || horizontal != 0)
                {
                    float2 vector = new float2(horizontal, vertical);
                    vector = math.normalize(vector);

                    move.nextPosition = move.position + (cameraDirection.forward * vector.y + cameraDirection.right * vector.x) * player.speed * dt;
                    move.isMove = true;
                }
                else
                {
                    move.isMove = false;
                }

                player.direction = mouseInput.mouseGroundPosition - move.position;
                player.direction = math.normalize(player.direction);

                //next check, is we should choot the bullet
                if (mouseInput.isLeftPress && time - shoter.shootLastTime > shoter.shootCooldawn)
                {
                    shoter.shootLastTime = time;
                    if(ammo.bulletsCount > 0)
                    {
                        ammo.bulletsCount = ammo.bulletsCount - 1;
                        //instantiate the bullet and set it direction
                        Entity bulletEntity = cmdBuffer.Instantiate(player.bulletPrefab);
                        cmdBuffer.SetComponent<DirectionComponent>(bulletEntity, new DirectionComponent()
                        {
                            direction = player.direction
                        });
                        cmdBuffer.SetComponent<LifetimeComponent>(bulletEntity, new LifetimeComponent()
                        {
                            startTime = time,
                            lifeTime = player.bulletLifetime,
                        });
                        float2 bulletPosition = new float2(move.position.x, move.position.y)
                                        + 0.5f * (new float2(player.direction.x, player.direction.y));

                        cmdBuffer.AddComponent(bulletEntity, new LineMoveInitTag()
                        {
                            hostPosition = new float2(move.position.x, move.position.y)
                        });

                        cmdBuffer.SetComponent<LineMoveComponent>(bulletEntity, new LineMoveComponent()
                        {
                            startPoint = bulletPosition,
                            endPoint = bulletPosition,
                            currentPoint = bulletPosition,
                            isFreeLife = true
                        });
                        cmdBuffer.AddComponent<HostTypeComponent>(bulletEntity, new HostTypeComponent()
                        {
                            host = HostTypes.HOST_PLAYER
                        });
                    }
                }
            }).Run();

            cmdBuffer.Playback(EntityManager);
        }
    }
}
