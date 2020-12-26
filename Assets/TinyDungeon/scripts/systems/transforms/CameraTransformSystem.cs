using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace TD
{
    [UpdateAfter(typeof(CameraRotateSystem))]
    [UpdateAfter(typeof(PlayerPositionSystem))]
    public class CameraTransformSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.
                ForEach((ref Translation position, ref Rotation rotation, in CameraComponent camera, in PlayerPositionComponent playerPosition) =>
            {
                float3 newPosition = new float3(playerPosition.position.x + camera.distance * math.cos(camera.positionU) * math.cos(camera.positionV), 
                                                camera.distance * math.sin(camera.positionV) + camera.verticalHeight, 
                                                playerPosition.position.y + camera.distance * math.sin(camera.positionU) * math.cos(camera.positionV));
                position.Value = newPosition;
                float3 toVector = new float3(playerPosition.position.x, camera.verticalHeight, playerPosition.position.y) - newPosition;
                toVector = math.normalize(toVector);
                rotation.Value = quaternion.LookRotation(toVector, new float3(0, 1, 0));

            }).Run();
        }
    }

}
