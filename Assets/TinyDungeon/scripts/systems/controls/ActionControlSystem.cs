using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace TD
{
    public class ActionControlSystem : SystemBase
    {
        EntityManager manager;

        protected override void OnCreate()
        {
            manager = EntityManager;
            RequireSingletonForUpdate<UIGlobalStateComponent>();
        }

        protected override void OnUpdate()
        {
            UIGlobalStateComponent globalUI = GetSingleton<UIGlobalStateComponent>();
            bool isGearVisible = globalUI.isGearActive;
            bool shouldActivate = false;
            bool shouldDeactivate = false;

            Entities.
                ForEach((Entity entity, in GateSwitcherComponent switcher, in PlayerPositionComponent playerPos, in Translation swCenter) =>
            {
                if(math.distancesq(playerPos.position, new float2(swCenter.Value.x, swCenter.Value.z)) < switcher.radius * switcher.radius)
                {
                    //show the gear
                    shouldActivate = true;
                }
                else
                {
                    shouldDeactivate = true;
                }
            }).Run();
            
            if(!isGearVisible && shouldActivate)
            {
                Entities.
                WithoutBurst().
                WithAny<UIGearValueComponent>().
                ForEach((Entity entity, ref Translation translation) =>
                {
                    translation.Value = new float3(translation.Value.x, translation.Value.y, globalUI.activeUIHeight);
                    globalUI.isGearActive = true;
                }).Run();

                SetSingleton<UIGlobalStateComponent>(globalUI);
            }
            else if(isGearVisible && shouldDeactivate && !shouldActivate)
            {
                Entities.
                WithoutBurst().
                WithAny<UIGearValueComponent>().
                ForEach((Entity entity, ref Translation translation) =>
                {
                    translation.Value = new float3(translation.Value.x, translation.Value.y, globalUI.nonActiveUIHeight);
                    globalUI.isGearActive = false;
                }).Run();

                SetSingleton<UIGlobalStateComponent>(globalUI);
            }
        }
    }
}