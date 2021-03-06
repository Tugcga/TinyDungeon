﻿using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Tiny.Audio;

namespace TD
{
    [UpdateBefore(typeof(TowerLookTransformSystem))]
    [UpdateAfter(typeof(PlayerPositionSystem))]
    public class SearchControlSystem : SystemBase
    {
        EntityManager manager;
        EntityQuery searchGroup;

        protected override void OnCreate()
        {
            manager = World.EntityManager;
            searchGroup = manager.CreateEntityQuery(
                typeof(SearchPlayerComponent),
                typeof(ShoterComponent),
                typeof(RandomComponent),
                ComponentType.ReadOnly<TowerComponent>(),
                ComponentType.ReadOnly<Rotation>(),
                ComponentType.ReadOnly<Translation>(),
                ComponentType.ReadOnly<PlayerPositionComponent>(),
                ComponentType.ReadOnly<TowerSoundComponent>());
            //RequireSingletonForUpdate<CollisionMap>();
            base.OnCreate();
        }


        protected override void OnUpdate()
        {
            double time = Time.ElapsedTime;
            float dt = Time.DeltaTime;

            EntityCommandBuffer cmdBuffer = new EntityCommandBuffer(Allocator.Temp);
            Entities.
                ForEach((
                ref SearchPlayerComponent search, 
                ref ShoterComponent shoter, 
                ref MovableCollisionComponent collision,  //ERROR: blobAssetReferense should be in the separate component, because if in the component with entity data, then this entity does not exists
                ref TowerComponent tower, 
                in Rotation rotation, 
                in Translation translation, 
                in PlayerPositionComponent playerPosition, 
                in TowerSoundComponent towerSound) =>
            {
                TowerState oldState = search.state;
                ref CollisionMapBlobAsset collisiosnAsset = ref collision.collisionMap.Value;
                if (tower.isActive)
                {
                    float4 q = rotation.Value.value;
                    float2 forward = new float2(2 * (q.x * q.z + q.y * q.w), 1 - 2 * (q.y * q.y + q.z * q.z));
                    float2 orth = new float2(forward.y, -forward.x);

                    float2 searchForward = new float2(
                        forward.x * math.cos(search.angle) + forward.y * math.sin(search.angle),
                        forward.y * math.cos(search.angle) - forward.x * math.sin(search.angle));

                    //try to update plyaer visibility
                    if (time - search.lastCheckTime > search.checkPlayerTimeDelta)
                    {
                        search.toPlayer = new float2(playerPosition.position.x - translation.Value.x, playerPosition.position.y - translation.Value.z);
                        search.toPlayerdistance = math.length(search.toPlayer);
                        search.toPlayerDirection = math.normalize(search.toPlayer);
                        search.toPlayerAngle = math.acos(math.dot(search.toPlayerDirection, searchForward));

                        CollisionInfo info = collisiosnAsset.GetPoint(
                            new float2(translation.Value.x, translation.Value.z),
                            playerPosition.position, false);
                        search.isVisiblePlayer = !info.isCollide;

                        search.lastCheckTime = time;
                    }

                    if (!playerPosition.isActive)
                    {
                        if (search.state == TowerState.STATE_TARGET)
                        {
                            search.state = TowerState.STATE_RETURN_TO_SEARCH;
                        }
                    }

                    if (search.state == TowerState.STATE_SEARCH || search.state == TowerState.STATE_RETURN_TO_SEARCH)
                    {//rotate to search the player
                        if (search.state == TowerState.STATE_SEARCH)
                        {
                            if (search.searchDirection)
                            {//increase angle value
                                search.angle += search.searchRotateSpeed * dt;
                            }
                            else
                            {//decrease angle value
                                search.angle -= search.searchRotateSpeed * dt;
                            }
                            //clamp and change direction
                            if (search.angle > search.searchAngle)
                            {
                                search.searchDirection = false;
                                search.angle = search.searchAngle;
                            }
                            if (search.angle < -search.searchAngle)
                            {
                                search.searchDirection = true;
                                search.angle = -search.searchAngle;
                            }
                        }
                        else  // search.state == TowerState.STATE_RETURN_TO_SEARCH
                        {//return to the search interval
                            if (search.angle > -search.searchAngle && search.angle < search.searchAngle)
                            {
                                search.state = TowerState.STATE_SEARCH;
                            }
                            else
                            {
                                if (search.angle > search.searchAngle)
                                {
                                    search.angle -= search.searchRotateSpeed * dt;
                                }
                                else if (search.angle < -search.searchAngle)
                                {
                                    search.angle += search.searchRotateSpeed * dt;
                                }
                            }
                        }

                        cmdBuffer.SetComponent<TowerLookComponent>(search.rotateObject, new TowerLookComponent()
                        {
                            angle = search.angle,
                        });

                        //check is the player is visible to the tower
                        if (playerPosition.isActive && search.toPlayerdistance < search.visibleDistance && search.isVisiblePlayer)
                        {//the player on the visibla distance
                         //we should check the direction
                            if (search.toPlayerAngle <= search.visibleAngle)
                            {
                                search.state = TowerState.STATE_TARGET;
                            }
                        }
                    }
                    else if (search.state == TowerState.STATE_CHECK_DIRECTION)
                    {
                        if (playerPosition.isActive && search.toPlayerAngle <= search.visibleAngle && search.toPlayerdistance < search.visibleDistance)
                        {//tower see the player
                            search.state = TowerState.STATE_TARGET;
                        }
                        else
                        {//we should rotate the tower to the check direction
                            float toAngle = math.acos(math.dot(search.checkDirection, forward)) * (math.dot(search.checkDirection, orth) > 0 ? 1.0f : -1.0f);
                            float deltaAngle = dt * search.activeRotateSpeed;
                            if (math.abs(search.angle - toAngle) < deltaAngle)
                            {
                                search.angle = toAngle;
                            }
                            else
                            {
                                float modificator = (search.angle > math.PI / 2 && search.angle < math.PI && toAngle < -math.PI / 2 && toAngle > -math.PI) ||
                                (search.angle < -math.PI / 2 && search.angle > -math.PI && toAngle > math.PI / 2 && toAngle < math.PI) ? -1.0f : 1.0f;
                                search.angle = search.angle + deltaAngle * (toAngle < search.angle ? -1.0f * modificator : 1.0f * modificator);
                            }

                            //clamp search.angle
                            if (search.angle > math.PI)
                            {
                                search.angle -= 2 * math.PI;
                            }
                            else if (search.angle < -math.PI)
                            {
                                search.angle += 2 * math.PI;
                            }

                            //if angle is small, change the state
                            if (math.abs(search.angle - toAngle) < math.atan(tower.atackTargetRadius / search.toPlayerdistance))
                            {
                                search.state = TowerState.STATE_WAIT;
                                search.startWaitTime = time;
                            }
                            else
                            {
                                cmdBuffer.SetComponent(search.rotateObject, new TowerLookComponent()
                                {
                                    angle = search.angle,
                                });
                            }
                        }
                    }
                    else if (search.state == TowerState.STATE_WAIT)
                    {
                        if (playerPosition.isActive && search.toPlayerAngle <= search.visibleAngle && search.toPlayerdistance < search.visibleDistance)
                        {
                            search.state = TowerState.STATE_TARGET;
                        }
                        else
                        {
                            if (time - search.startWaitTime > tower.waitTime)
                            {
                                search.state = TowerState.STATE_RETURN_TO_SEARCH;
                            }
                        }
                    }
                    else if (search.state == TowerState.STATE_TARGET)
                    {//player in the vivisble
                     //also check visibility
                        if (search.toPlayerdistance > search.visibleDistance || !playerPosition.isActive || !search.isVisiblePlayer)
                        {//player is too far, start searching it
                            search.state = TowerState.STATE_RETURN_TO_SEARCH;
                        }
                        else
                        {//player is close, check visibility cone
                            if (search.toPlayerAngle > search.visibleAngle)
                            {
                                search.state = TowerState.STATE_RETURN_TO_SEARCH;
                            }
                            else
                            {//rotate to see a player at the center
                             //we should calculate angle between player and zAxis, and then set it as search.angle
                                float toAngle = math.acos(math.dot(search.toPlayerDirection, forward)) * (math.dot(search.toPlayerDirection, orth) > 0 ? 1.0f : -1.0f);

                                float deltaAngle = dt * search.activeRotateSpeed;
                                if (math.abs(search.angle - toAngle) < deltaAngle)
                                {
                                    search.angle = toAngle;
                                }
                                else
                                {
                                    float modificator = (search.angle > math.PI / 2 && search.angle < math.PI && toAngle < -math.PI / 2 && toAngle > -math.PI) ||
                                    (search.angle < -math.PI / 2 && search.angle > -math.PI && toAngle > math.PI / 2 && toAngle < math.PI) ? -1.0f : 1.0f;
                                    search.angle = search.angle + deltaAngle * (toAngle < search.angle ? -1.0f * modificator : 1.0f * modificator);
                                }

                                //clamp search.angle
                                if (search.angle > math.PI)
                                {
                                    search.angle -= 2 * math.PI;
                                }
                                else if (search.angle < -math.PI)
                                {
                                    search.angle += 2 * math.PI;
                                }

                                //if the difference is small, atack the player
                                if (time - shoter.shootLastTime > shoter.shootCooldawn && math.abs(search.angle - toAngle) < math.atan(tower.atackTargetRadius / search.toPlayerdistance))
                                {//shoot
                                    shoter.shootLastTime = time;
                                    Entity bulletEntity = cmdBuffer.Instantiate(tower.bulletPrefab);
                                    //---can't use entity, because there are too many arguments in the lambda expression. So, no animations here of the shot
                                    //---cmdBuffer.AddComponent<StartAnimationTag>(entity, new StartAnimationTag() { animationIndex = 0 });
                                    //instantiate flash
                                    Entity flash = cmdBuffer.Instantiate(tower.flashPrefab);
                                    cmdBuffer.SetComponent(flash, new LifetimeComponent() { startTime = time, lifeTime = tower.flashLifetime });
                                    cmdBuffer.AddComponent<LocalToParent>(flash);
                                    cmdBuffer.AddComponent<Parent>(flash, new Parent() { Value = shoter.weaponCorner });

                                    float2 bulletDirection = search.toPlayerDirection + (tower.random.NextFloat() * 2 * tower.shotDelta - tower.shotDelta) * (new float2(search.toPlayerDirection.y, -search.toPlayerDirection.x));
                                    bulletDirection = math.normalize(bulletDirection);
                                    quaternion bulletRotation = quaternion.LookRotation(new float3(bulletDirection.x, 0.0f, bulletDirection.y), new float3(0f, 0f, 1f));

                                    //set data
                                    cmdBuffer.SetComponent<Rotation>(bulletEntity, new Rotation() { Value = bulletRotation });
                                    cmdBuffer.SetComponent<DirectionComponent>(bulletEntity, new DirectionComponent()
                                    {
                                        direction = bulletDirection
                                    });
                                    cmdBuffer.SetComponent<LifetimeComponent>(bulletEntity, new LifetimeComponent()
                                    {
                                        startTime = time,
                                        lifeTime = tower.bulletLifetime,
                                    });
                                    cmdBuffer.AddComponent(bulletEntity, new LineMoveInitTag()
                                    {
                                        hostPosition = new float2(translation.Value.x, translation.Value.z)
                                    });

                                    float2 bulletPosition = new float2(tower.weaponCornerPosition.x, tower.weaponCornerPosition.z);
                                    cmdBuffer.SetComponent<HeightComponent>(bulletEntity, new HeightComponent() { Value = tower.weaponCornerPosition.y});
                                    cmdBuffer.SetComponent<LineMoveComponent>(bulletEntity, new LineMoveComponent()
                                    {
                                        startPoint = bulletPosition,
                                        endPoint = bulletPosition,
                                        currentPoint = bulletPosition,
                                        isFreeLife = true
                                    });
                                    cmdBuffer.AddComponent<HostTypeComponent>(bulletEntity, new HostTypeComponent()
                                    {
                                        host = HostTypes.HOST_ENEMY
                                    });
                                    //play shot sound
                                    cmdBuffer.AddComponent<AudioSourceStart>(towerSound.shotSound);
                                }
                                cmdBuffer.SetComponent(search.rotateObject, new TowerLookComponent()
                                {
                                    angle = search.angle,
                                });
                            }
                        }
                    }
                }
                
                //try to turn on/off alarm sound
                if((oldState == TowerState.STATE_WAIT || oldState == TowerState.STATE_SEARCH || oldState == TowerState.STATE_RETURN_TO_SEARCH) 
                    && (search.state == TowerState.STATE_TARGET))
                {
                    cmdBuffer.AddComponent<AudioSourceStop>(towerSound.alarmSound);  //ERROR: in web build this sound is still played after destroying the parent entity
                }
                else if((oldState == TowerState.STATE_TARGET) && 
                    (search.state == TowerState.STATE_WAIT || search.state == TowerState.STATE_SEARCH || search.state == TowerState.STATE_RETURN_TO_SEARCH))
                {
                    cmdBuffer.AddComponent<AudioSourceStop>(towerSound.alarmSound);  // and also here
                }

            }).Run();
            cmdBuffer.Playback(manager);
        }
    }
}