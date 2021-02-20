using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Tiny.Audio;

namespace TD
{
    [UpdateAfter(typeof(MouseInputSystem))]
    [UpdateAfter(typeof(KeyboardInputSystem))]
    [UpdateAfter(typeof(CameraRotateSystem))]
    [UpdateBefore(typeof(NavmeshSystem))]
    public class PlayerControlSystem : SystemBase
    {
        EntityManager manager;
        EntityQuery playerGroup;
        Random _playerRandom;

        protected override void OnCreate()
        {
            manager = World.EntityManager;
            playerGroup = manager.CreateEntityQuery(typeof(PlayerComponent), typeof(ShoterComponent), typeof(MovableComponent), typeof(AmmunitionComponent), typeof(PlayerAnimationsComponent),
                ComponentType.ReadOnly<PlayerSoundComponent>(),
                ComponentType.Exclude<DeadTag>());

            _playerRandom = new Random(128);

            RequireSingletonForUpdate<KeyboardInputComponent>();
            RequireSingletonForUpdate<MouseInputComponent>();
            RequireSingletonForUpdate<CameraDirectionComponent>();
            //RequireSingletonForUpdate<CollisionMap>();  // it seems that this component does not need

            RequireSingletonForUpdate<PlayerComponent>();

            base.OnCreate();
        }

        protected override void OnUpdate()
        {
            KeyboardInputComponent input = GetSingleton<KeyboardInputComponent>();
            MouseInputComponent mouseInput = GetSingleton<MouseInputComponent>();
            CameraDirectionComponent cameraDirection = GetSingleton<CameraDirectionComponent>();

            //we need this data for bullet instancing, so, get it before foreach process
            Entity playerEntity = GetSingletonEntity<PlayerComponent>();
            ShoterComponent playerShoter = manager.GetComponentData<ShoterComponent>(playerEntity);
            float3 weaponCornerPosition = manager.GetComponentData<LocalToWorld>(playerShoter.weaponCorner).Position;

            float dt = Time.DeltaTime;
            double time = Time.ElapsedTime;
            //PlayerComponent is singleton, but we use Bursten function for speed up
            Random playerRandom = _playerRandom;

            EntityCommandBuffer cmdBuffer = new EntityCommandBuffer(Allocator.Temp);
            Entities.WithNone<DeadTag>().ForEach((
                ref PlayerComponent player, 
                ref ShoterComponent shoter, 
                ref MovableComponent move, 
                ref AmmunitionComponent ammo,  
                ref PlayerAnimationsComponent playerAnim,
            in PlayerSoundComponent sound) =>
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

                bool oldMove = move.isMove;

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

                //check is we change move status (from move to non-move)
                if(!oldMove && move.isMove)
                {//start moving
                 //start move sound
                    cmdBuffer.AddComponent<AudioSourceStart>(sound.moveSound);
                }
                else if(oldMove && !move.isMove)
                {//finish moving
                    //stop move sound
                    cmdBuffer.AddComponent<AudioSourceStop>(sound.moveSound);  //ERROR: walk sound fade off after second, may be this is a bug...
                }

                player.direction = mouseInput.mouseGroundPosition - move.position;
                player.direction = math.normalize(player.direction);

                //check animations
                if(move.isMove)
                {
                    float2 moveDirection = move.nextPosition - move.position;
                    moveDirection = math.normalize(moveDirection);
                    float d = math.dot(moveDirection, player.direction);
                    if (d > math.SQRT2 / 2)
                    {
                        playerAnim.newAimation = 1;
                    }
                    else if(d < -math.SQRT2 / 2)
                    {
                        playerAnim.newAimation = 2;
                    }
                    else
                    {//this is left or right direction
                        //calculate cross product
                        float3 c = math.cross(new float3(moveDirection.x, 0.0f, moveDirection.y), new float3(player.direction.x, 0.0f, player.direction.y));
                        if(c.y < 0)
                        {
                            playerAnim.newAimation = 3;
                        }
                        else
                        {
                            playerAnim.newAimation = 4;
                        }
                    }
                }
                else
                {
                    playerAnim.newAimation = 0;
                }

                //next check, is we should choot the bullet
                if (mouseInput.isLeftPress && time - shoter.shootLastTime > shoter.shootCooldawn)
                {
                    shoter.shootLastTime = time;
                    if (ammo.bulletsCount > 0)
                    {
                        ammo.bulletsCount = ammo.bulletsCount - 1;
                        quaternion bulletRotation = quaternion.LookRotation(new float3(player.direction.x, 0.0f, player.direction.y), new float3(0f, 0f, 1f));

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
                        //instantiate flash
                        Entity flash = cmdBuffer.Instantiate(player.shotFlash);
                        cmdBuffer.SetComponent(flash, new LifetimeComponent() {startTime = time, lifeTime = player.flashLifetime });
                        cmdBuffer.AddComponent<LocalToParent>(flash);
                        cmdBuffer.AddComponent<Parent>(flash, new Parent() { Value = shoter.weaponCorner });
                        cmdBuffer.SetComponent(flash, new Translation() { Value = new float3(0.0f, 0.0f, 0.0f)});
                        cmdBuffer.SetComponent(flash, new Rotation() { Value = quaternion.EulerXYZ(playerRandom.NextFloat(0.0f, 6.28f), math.PI / 2, 0.0f) });

                        float2 bulletPosition = new float2(weaponCornerPosition.x, weaponCornerPosition.z);
                        cmdBuffer.SetComponent(bulletEntity, new HeightComponent() { Value = weaponCornerPosition.y});

                        cmdBuffer.AddComponent(bulletEntity, new LineMoveInitTag()
                        {
                            hostPosition = new float2(move.position.x, move.position.y)
                        });

                        cmdBuffer.SetComponent<Rotation>(bulletEntity, new Rotation() { Value = bulletRotation });

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
                        //play shot sound
                        cmdBuffer.AddComponent<AudioSourceStart>(sound.shotSound);
                    }
                    else
                    {
                        //play miss shot sound
                        cmdBuffer.AddComponent<AudioSourceStart>(sound.missShotSound);
                    }
                }
            }).Run();
                cmdBuffer.Playback(manager);
        }
    }
}
