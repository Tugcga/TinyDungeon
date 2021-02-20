using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Tiny.Audio;

namespace TD
{
    [UpdateAfter(typeof(BulletCollisionSystem))]
    public class LifeControlSystem : SystemBase
    {
        EntityManager manager;
        EntityQuery lifeQuert;
        EntityQuery damagebleQuert;
        Random random;

        protected override void OnCreate()
        {
            manager = World.EntityManager;
            lifeQuert = manager.CreateEntityQuery(typeof(LifeComponent), typeof(Translation), ComponentType.Exclude<DeadTag>());
            damagebleQuert = manager.CreateEntityQuery(typeof(LifeComponent), typeof(Translation), typeof(RadiusComponent), ComponentType.Exclude<DeadTag>());

            random = new Random(1);  // the random is different for each call, because it does not copy to the parellel execution
            RequireSingletonForUpdate<CollisionMap>();
            base.OnCreate();
        }

        protected override void OnUpdate()
        {
            NativeArray<Entity> entities = lifeQuert.ToEntityArray(Allocator.TempJob);
            NativeArray<Entity> targets = damagebleQuert.ToEntityArray(Allocator.TempJob);

            ref CollisionMapBlobAsset map = ref GetSingleton<CollisionMap>().collisionMap.Value;
            EntityCommandBuffer cmdBuffer = new EntityCommandBuffer(Allocator.Temp);
            for (int i = 0; i < entities.Length; i++)
            {
                Entity entity = entities[i];
                LifeComponent life = manager.GetComponentData<LifeComponent>(entity);
                if(life.life <= 0)
                {//we should destriy the entity
                    if(manager.HasComponent<ExplodedItemComponent>(entity))
                    {//we should emit the explosion and apply damage to each entity in the damage radius
                        ExplodedItemComponent explodeComponent = manager.GetComponentData<ExplodedItemComponent>(entity);
                        Translation entityTranslation = manager.GetComponentData<Translation>(entity);

                        Entity explosionEntity = manager.Instantiate(explodeComponent.explosionPrefab);
                        cmdBuffer.SetComponent<Translation>(explosionEntity, new Translation()
                        {
                            Value = entityTranslation.Value
                        });
                        LifetimeComponent explosionLifetime = manager.GetComponentData<LifetimeComponent>(explosionEntity);
                        cmdBuffer.SetComponent<LifetimeComponent>(explosionEntity, new LifetimeComponent()
                        {
                            lifeTime = explosionLifetime.lifeTime,
                            startTime = Time.ElapsedTime
                        });

                        float2 explodePosition = new float2(entityTranslation.Value.x, entityTranslation.Value.z);

                        //iterate throw targets
                        for(int j = 0; j < targets.Length; j++)
                        {
                            Entity target = targets[j];
                            if(target != entity && manager.HasComponent<Translation>(target))
                            {
                                Translation targetTranslation = manager.GetComponentData<Translation>(target);
                                float2 targetPosition = new float2(targetTranslation.Value.x, targetTranslation.Value.z);
                                RadiusComponent targetRaidus = manager.GetComponentData<RadiusComponent>(target);
                                LifeComponent targetLife = manager.GetComponentData<LifeComponent>(target);

                                if (math.distancesq(targetPosition, explodePosition) < (explodeComponent.damageRadius + targetRaidus.Value) * (explodeComponent.damageRadius + targetRaidus.Value))
                                {//apply damage
                                    //UnityEngine.Debug.Log("Add delay damage at start=" + Time.ElapsedTime.ToString() + " damage=" + explodeComponent.damage.ToString() + " delay=" + ((explodeComponent.delayMaximum - explodeComponent.delayMinimum) * random.NextFloat() + explodeComponent.delayMinimum).ToString());
                                    cmdBuffer.AddComponent(target, new DelayDamageComponent()
                                    {
                                        startTime = Time.ElapsedTime,
                                        damage = explodeComponent.damage,
                                        delay = (explodeComponent.delayMaximum - explodeComponent.delayMinimum) * random.NextFloat() + explodeComponent.delayMinimum
                                    }); 
                                }
                            }
                        }
                    }

                    if(manager.HasComponent<PlayerComponent>(entity) && !manager.HasComponent<DeadTag>(entity))
                    {//set player dead
                        cmdBuffer.AddComponent(entity, typeof(DeadTag));

                        //add cooldawn component
                        cmdBuffer.AddComponent<DeadCooldawnComponent>(entity, new DeadCooldawnComponent()
                        {
                            startTime = Time.ElapsedTime,
                            delayTime = 5.0f  // set dead delay to 5 seconds
                        });

                        //set dead animation
                        PlayerAnimationsComponent playerAnim = manager.GetComponentData<PlayerAnimationsComponent>(entity);
                        playerAnim.newAimation = 5;
                        cmdBuffer.SetComponent(entity, playerAnim);

                        //stop walk sound
                        PlayerSoundComponent playerSound = manager.GetComponentData<PlayerSoundComponent>(entity);
                        cmdBuffer.AddComponent<AudioSourceStop>(playerSound.moveSound);
                    }
                    else
                    {
                        //turn off alarm sound
                        if (manager.HasComponent<TowerComponent>(entity))
                        {
                            TowerSoundComponent towerSound = manager.GetComponentData<TowerSoundComponent>(entity);
                            cmdBuffer.AddComponent<AudioSourceStop>(towerSound.alarmSound);
                        }

                        if(manager.HasComponent<CollisionEdgesSetComponent>(entity))
                        {//entity contains data about correspondign collision edges
                            //before we destroy the entity, we should deactivate these edges
                            CollisionEdgesSetComponent indexes = manager.GetComponentData<CollisionEdgesSetComponent>(entity);
                            //process for all non-zero indexes
                            //ERROR: may be here we change some data, but other entity access to the same data, and collision appears
                            map.Deactivate(indexes);
                        }
                        cmdBuffer.DestroyEntity(entity);
                    }
                }
            }

            entities.Dispose();
            targets.Dispose();

            cmdBuffer.Playback(manager);
        }
    }
}
