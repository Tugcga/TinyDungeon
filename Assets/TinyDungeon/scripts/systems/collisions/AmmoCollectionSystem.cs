using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace TD
{
    [UpdateAfter(typeof(PlayerPositionSystem))]
    public class AmmoCollectionSystem : SystemBase
    {
        EntityManager manager;

        protected override void OnCreate()
        {
            base.OnCreate();
            manager = EntityManager;
        }

        protected override void OnUpdate()
        {
            Entity playerEntity = GetSingletonEntity<PlayerComponent>();
            RadiusComponent radius = manager.GetComponentData<RadiusComponent>(playerEntity);
            AmmunitionComponent ammunition = manager.GetComponentData<AmmunitionComponent>(playerEntity);
            EntityCommandBuffer cmdBuffer = new EntityCommandBuffer(Allocator.Temp);
            Entities.ForEach((Entity entity, in Translation translation, in AmmoComponent ammo, in PlayerPositionComponent playerPosition) =>
            {
                if(playerPosition.isActive)
                {
                    if(math.distancesq(new float2(translation.Value.x, translation.Value.z), playerPosition.position) < radius.Value * radius.Value)
                    {
                        int newCount = ammunition.bulletsCount + ammo.count;
                        if(newCount > ammunition.maxBulletsCount)
                        {
                            newCount = ammunition.maxBulletsCount;
                        }
                        else if(newCount < 0)
                        {
                            newCount = 0;
                        }
                        cmdBuffer.SetComponent(playerEntity, new AmmunitionComponent()
                        {
                            maxBulletsCount = ammunition.maxBulletsCount,
                            bulletsCount = newCount
                        });

                        cmdBuffer.DestroyEntity(entity);
                    }
                }
            }).Run();

            cmdBuffer.Playback(manager);
        }
    }

}