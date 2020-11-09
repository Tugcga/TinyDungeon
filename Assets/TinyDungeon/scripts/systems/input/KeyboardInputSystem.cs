using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

#if UNITY_DOTSPLAYER
using Unity.Tiny.Input;
#else
using UnityEngine;
#endif

namespace TD
{
    [UpdateBefore(typeof(CameraRotateSystem))]
    [UpdateBefore(typeof(PlayerControlSystem))]
    public class KeyboardInputSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireSingletonForUpdate<KeyboardInputComponent>();
            base.OnCreate();
        }

        protected override void OnUpdate()
        {
#if UNITY_DOTSPLAYER
            InputSystem Input = World.GetExistingSystem<InputSystem>();
#else
            
#endif
            bool isPressFront = Input.GetKey(KeyCode.W);
            bool isPressBack = Input.GetKey(KeyCode.S);
            bool isPressLeft = Input.GetKey(KeyCode.A);
            bool isPressRight = Input.GetKey(KeyCode.D);

            bool isAction = Input.GetKeyDown(KeyCode.F);

            float dt = Time.DeltaTime;

            SetSingleton<KeyboardInputComponent>(new KeyboardInputComponent()
            {
                isPressFront = isPressFront,
                isPressBack = isPressBack,
                isPressLeft = isPressLeft,
                isPressRight = isPressRight,
                isPressAction = isAction
            });
        }
    }

}
