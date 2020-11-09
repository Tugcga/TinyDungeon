using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace TD
{
    public class RayTrackerSystem : SystemBase
    {
        EntityManager manager;
        EntityQuery mouseInputGroup;

        protected override void OnCreate()
        {
            manager = World.EntityManager;
            mouseInputGroup = manager.CreateEntityQuery(typeof(MouseInputComponent));

            base.OnCreate();
        }

        protected override void OnUpdate()
        {
            NativeArray<Entity> mouseInputs = mouseInputGroup.ToEntityArray(Allocator.TempJob);
            if (mouseInputs.Length > 0)
            {
                Entity entity = mouseInputs[0];
                MouseInputComponent mouseInput = manager.GetComponentData<MouseInputComponent>(entity);

                Entities.WithAll<RayTrackerComponent>().ForEach((ref Translation translation) =>
                {
                    translation.Value = new float3(mouseInput.mouseGroundPosition.x, 0.0f, mouseInput.mouseGroundPosition.y);
                }).Run();
            }

            mouseInputs.Dispose();
        }
    }
}