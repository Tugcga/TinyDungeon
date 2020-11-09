using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

namespace TD
{
    [UpdateAfter(typeof(SearchControlSystem))]
    public class TowerLookTransformSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var dt = Time.DeltaTime;
            Entities.ForEach((ref Rotation rotation, in TowerLookComponent look) =>
            {
                rotation.Value = quaternion.LookRotation(new float3(math.sin(look.angle), 0.0f, math.cos(look.angle)), new float3(0.0f, 1.0f, 0.0f));
            }).Run();
        }
    }

}