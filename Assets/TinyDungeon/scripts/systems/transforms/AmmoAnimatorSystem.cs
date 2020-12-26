using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace TD
{
    public class AmmoAnimatorSystem : SystemBase
    {
        protected override void OnCreate()
        {
            base.OnCreate();
        }

        protected override void OnUpdate()
        {
            double time = Time.ElapsedTime;
            float dt = Time.DeltaTime;
            Entities.
#if USE_FOREACH_SYSTEM
#else
                WithoutBurst().
#endif
                ForEach((ref Translation translate, ref Rotation rotation, ref Scale scale, in AmmoAnimatorComponent anim) =>
            {
                translate.Value = new float3(translate.Value.x, 
                    math.sin((float)time * anim.heightFrequency) * (anim.maxHeight - anim.minHeight) / 2 + (anim.minHeight + anim.maxHeight) / 2, 
                    translate.Value.z);
                rotation.Value = math.mul(rotation.Value, quaternion.RotateY(dt * anim.rotationSpeed));

                scale.Value = math.sin((float)time * anim.scaleFrequency) * (anim.maxScale - anim.minScale) / 2 + (anim.minScale + anim.maxScale) / 2;
            }).Run();
        }
    }

}