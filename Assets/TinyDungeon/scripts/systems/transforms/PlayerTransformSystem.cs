using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace TD
{
    /*
     * Place player entity in the 3d-space with respect to it position and direction
     */
    [UpdateAfter(typeof(NavmeshSystem))]
    public class PlayerTransformSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            //in fact PlayerComponent is singleton, but here we use Bursted function
            Entities.WithNone<DeadTag>().ForEach((ref Translation translation, ref Rotation rotation, in PlayerComponent player, in MovableComponent move) =>
            {
                translation.Value = new float3(move.position.x, 0f, move.position.y);
                rotation.Value = quaternion.LookRotation(new float3(player.direction.x, 0.0f, player.direction.y), new float3(0.0f, 1.0f, 0.0f));
            }).Run();
        }
    }

}