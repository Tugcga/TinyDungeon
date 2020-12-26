using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace TD
{
    public class TowerWeaponCornerSystem : SystemBase
    {
        EntityManager manager;
        EntityQuery towersGroup;

        protected override void OnCreate()
        {
            manager = EntityManager;
            towersGroup = manager.CreateEntityQuery(typeof(TowerComponent));

            base.OnCreate();

        }

        protected override void OnUpdate()
        {
            NativeArray<Entity> towers = towersGroup.ToEntityArray(Allocator.Temp);
            for(int i = 0; i < towers.Length; i++)
            {
                Entity e = towers[i];
                TowerComponent tower = manager.GetComponentData<TowerComponent>(e);
                tower.weaponCornerPosition = manager.GetComponentData<LocalToWorld>(tower.weaponCorner).Position;

                manager.SetComponentData(e, tower);
            }
            towers.Dispose();
        }
    }
}