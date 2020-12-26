using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Tiny.Rendering;
using Unity.Tiny.Text;

namespace TD
{
    public class UIAmmoHealthValueSystem : SystemBase
    {
        EntityManager manager;

        int lastAmmo;
        int lastHealth;
        
        protected override void OnCreate()
        {
            base.OnCreate();
            manager = EntityManager;

            lastAmmo = -1;
            lastHealth = -1;

            RequireSingletonForUpdate<UIHealthValueComponent>();
            RequireSingletonForUpdate<UIBulletsValueComponent>();
            RequireSingletonForUpdate<PlayerComponent>();
        }

        protected override void OnUpdate()
        {
            Entity playerEntity = GetSingletonEntity<PlayerComponent>();
            LifeComponent playerLife = manager.GetComponentData<LifeComponent>(playerEntity);
            AmmunitionComponent playerAmmo = manager.GetComponentData<AmmunitionComponent>(playerEntity);

            if(playerLife.life != lastHealth)
            {
                //update ui data
                Entity healthEntity = GetSingletonEntity<UIHealthValueComponent>();
                TextLayout.SetEntityTextRendererString(EntityManager, healthEntity, math.max(0, playerLife.life).ToString());

                lastHealth = playerLife.life;
            }

            if(playerAmmo.bulletsCount != lastAmmo)
            {
                Entity ammoEntity = GetSingletonEntity<UIBulletsValueComponent>();
                TextLayout.SetEntityTextRendererString(EntityManager, ammoEntity, math.max(0, playerAmmo.bulletsCount).ToString());
                lastAmmo = playerAmmo.bulletsCount;
            }

        }
    }

}