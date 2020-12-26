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
            bool isPressFront = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
            bool isPressBack = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
            bool isPressLeft = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
            bool isPressRight = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);

            bool isAny = Input.GetKeyDown(KeyCode.Space);
            bool isAction = Input.GetKeyDown(KeyCode.F);

            SetSingleton<KeyboardInputComponent>(new KeyboardInputComponent()
            {
                isPressFront = isPressFront,
                isPressBack = isPressBack,
                isPressLeft = isPressLeft,
                isPressRight = isPressRight,
                isPressAction = isAction,

                isPressAny = isAny
            });
        }
    }

}
