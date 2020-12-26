using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace TD
{
    [UpdateAfter(typeof(KeyboardInputSystem))]
    [UpdateBefore(typeof(PlayerControlSystem))]
    [UpdateBefore(typeof(CameraTransformSystem))]
    public class CameraRotateSystem : SystemBase
    {
        EntityManager manager;

        protected override void OnCreate()
        {
            manager = World.EntityManager;
            RequireSingletonForUpdate<MouseInputComponent>();

            base.OnCreate();
        }

        protected override void OnUpdate()
        {
            MouseInputComponent input = GetSingleton<MouseInputComponent>();

            Entities.
#if USE_FOREACH_SYSTEM
#else
                WithoutBurst().
#endif
                WithNone<CameraBlockRotationTag>().ForEach((ref CameraComponent camera, ref CameraDirectionComponent cameraDirection) =>
            {
                if (input.isRightPress)
                {
                    camera.positionU -= input.delta.x * input.deltaTime * camera.speedU;
                    camera.positionV -= input.delta.y * input.deltaTime * camera.speedV;
                }
                if (camera.positionV < camera.limitVMin)
                {
                    camera.positionV = camera.limitVMin;
                }
                else if (camera.positionV > camera.limitVMax)
                {
                    camera.positionV = camera.limitVMax;
                }

                //calculate forward directon of the camera
                float x = math.cos(camera.positionU) * math.cos(camera.positionV);
                //float y = math.sin(camera.positionV);
                float z = math.sin(camera.positionU) * math.cos(camera.positionV);
                float2 forward = new float2(-x, -z);
                forward = math.normalize(forward);
                cameraDirection.forward = forward;
                cameraDirection.right = new float2(forward.y, -forward.x);
            }).Run();
        }
    }

}