using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Tiny.Audio;


namespace TD
{
    [UpdateAfter(typeof(PlayerPositionSystem))]
    public class AmmoCollectionSystem : SystemBase
    {
        EntityManager manager;
        EntityQuery ammoGroup;

        protected override void OnCreate()
        {
            base.OnCreate();
            manager = EntityManager;
            ammoGroup = manager.CreateEntityQuery(ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly <AmmoComponent>(), ComponentType.ReadOnly<PlayerPositionComponent>(), ComponentType.ReadOnly<RadiusComponent>());

            RequireSingletonForUpdate<PlayerComponent>();
        }

        protected override void OnUpdate()
        {
            Entity playerEntity = GetSingletonEntity<PlayerComponent>();
            PlayerSoundComponent playerSound = manager.GetComponentData<PlayerSoundComponent>(playerEntity);
            RadiusComponent radius = manager.GetComponentData<RadiusComponent>(playerEntity);
            AmmunitionComponent ammunition = manager.GetComponentData<AmmunitionComponent>(playerEntity);

            EntityCommandBuffer cmdBuffer = new EntityCommandBuffer(Allocator.Temp);
            Entities.ForEach((Entity entity, in Translation translation, in AmmoComponent ammo, in PlayerPositionComponent playerPosition, in RadiusComponent ammoRadius) =>
            {
                if (playerPosition.isActive)
                {
                    if (math.distancesq(new float2(translation.Value.x, translation.Value.z), playerPosition.position) < (radius.Value + ammoRadius.Value) * (radius.Value + ammoRadius.Value))
                    {
                        int newCount = ammunition.bulletsCount + ammo.count;
                        if (newCount > ammunition.maxBulletsCount)
                        {
                            newCount = ammunition.maxBulletsCount;
                        }
                        else if (newCount < 0)
                        {
                            newCount = 0;
                        }

                        cmdBuffer.SetComponent(playerEntity, new AmmunitionComponent()
                        {
                            maxBulletsCount = ammunition.maxBulletsCount,
                            bulletsCount = newCount
                        });
                        //play pickup sound
                        cmdBuffer.AddComponent<AudioSourceStart>(playerSound.ammoPickupSound);

                        cmdBuffer.DestroyEntity(entity);
                    }
                }
            }).Run();

            cmdBuffer.Playback(manager);
        }
    }

}