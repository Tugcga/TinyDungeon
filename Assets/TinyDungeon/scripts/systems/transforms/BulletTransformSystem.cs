using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace TD
{
    [UpdateAfter(typeof(NavmeshSystem))]
    public class BulletTransformSystem : SystemBase
    {
        protected override void OnCreate()
        {
            base.OnCreate();
        }

        protected override void OnUpdate()
        {
            Entities.
#if USE_FOREACH_SYSTEM
#else
                WithoutBurst().
#endif
                WithAll<BulletComponent>().WithNone<DestroyBulletTag>().ForEach((ref Translation translate, in LineMoveComponent move, in HeightComponent height) =>
            {
                translate.Value = new float3(move.currentPoint.x, height.Value, move.currentPoint.y);
            }).Run();
        }
    }

}
