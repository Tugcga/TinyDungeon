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
#if USE_FOREACH_SYSTEM
            EntityCommandBuffer cmdBuffer = new EntityCommandBuffer(Allocator.Temp);
            Entities.ForEach((Entity entity, in Translation translation, in AmmoComponent ammo, in PlayerPositionComponent playerPosition, in RadiusComponent ammoRadius) =>
            {
#else
            NativeArray<Entity> entities = ammoGroup.ToEntityArray(Allocator.Temp);
            for (int i = 0; i < entities.Length; i++)
            {
                Entity entity = entities[i];
                Translation translation = manager.GetComponentData<Translation>(entity);
                AmmoComponent ammo = manager.GetComponentData<AmmoComponent>(entity);
                PlayerPositionComponent playerPosition = manager.GetComponentData<PlayerPositionComponent>(entity);
                RadiusComponent ammoRadius = manager.GetComponentData<RadiusComponent>(entity);
#endif
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
#if USE_FOREACH_SYSTEM
                        cmdBuffer.SetComponent(playerEntity, new AmmunitionComponent()
                        {
                            maxBulletsCount = ammunition.maxBulletsCount,
                            bulletsCount = newCount
                        });
                        //play pickup sound
                        cmdBuffer.AddComponent<AudioSourceStart>(playerSound.ammoPickupSound);

                        cmdBuffer.DestroyEntity(entity);
#else
                        manager.SetComponentData(playerEntity, new AmmunitionComponent()
                        {
                            maxBulletsCount = ammunition.maxBulletsCount,
                            bulletsCount = newCount
                        });
                        //play pickup sound
                        manager.AddComponent<AudioSourceStart>(playerSound.ammoPickupSound);

                        manager.DestroyEntity(entity);
#endif
                    }
                }
#if USE_FOREACH_SYSTEM
            }).Run();

            cmdBuffer.Playback(manager);
#else
            }
            entities.Dispose();
#endif
        }
    }

}