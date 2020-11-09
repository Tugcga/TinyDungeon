using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

namespace TD
{
    public class RotateSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var dt = Time.DeltaTime;
            Entities.ForEach((ref Rotation rot, ref RotateComponent rc) =>
            {
                rot.Value = math.mul(rot.Value, quaternion.RotateY(dt * rc.Speed));
            }).ScheduleParallel();
        }
    }

}
