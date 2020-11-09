using Unity.Mathematics;
using Unity.Entities;
using Unity.Collections;
using Unity.Core;
using Unity.Tiny;
using Unity.Tiny.Assertions;
using Unity.Transforms;

using Unity.Tiny.Input;
using Unity.Tiny.Rendering;
using Unity.Collections.LowLevel.Unsafe;
using System;

#if UNITY_DOTSPLAYER
using Unity.Tiny.Input;
#else
using UnityEngine;
#endif

namespace TD
{
    [UpdateBefore(typeof(PlayerControlSystem))]
    public class MouseInputSystem : SystemBase
    {
        ScreenToWorld s2w;

        protected override void OnCreate()
        {
            s2w = World.GetExistingSystem<ScreenToWorld>();
            RequireSingletonForUpdate<MouseInputComponent>();
            
            base.OnCreate();
        }

        protected override void OnUpdate()
        {
#if UNITY_DOTSPLAYER
            InputSystem Input = World.GetExistingSystem<InputSystem>();
            float2 newPosition = Input.GetInputPosition();
#else
            float2 newPosition = new float2(Input.mousePosition.x, Input.mousePosition.y);
#endif
            float3 rayOrigin = new float3(0, 1, 0);
            float3 rayDirection = new float3(0, -1, 1);
#if UNITY_DOTSPLAYER
            s2w.ScreenSpaceToWorldSpaceRay(newPosition, out rayOrigin, out rayDirection);
#endif

            bool isLeftPress = Input.GetMouseButton(0);
            bool isRightPress = Input.GetMouseButton(1);
            bool isLeftPressStart = Input.GetMouseButtonDown(0);
            bool isRightPressStart = Input.GetMouseButtonDown(1);

            float dt = Time.DeltaTime;
            MouseInputComponent mouseInput = GetSingleton<MouseInputComponent>();
            mouseInput.isLeftPress = isLeftPress;
            mouseInput.isRightPress = isRightPress;

            mouseInput.delta = newPosition - mouseInput.currentPosition;
            mouseInput.currentPosition = newPosition;
            mouseInput.deltaTime = dt;

            if (isLeftPressStart)
            {
                mouseInput.leftPressPosition = mouseInput.currentPosition;
            }

            if (isRightPressStart)
            {
                mouseInput.rightPressPosition = mouseInput.currentPosition;
            }

            //calculate ground position
            mouseInput.mouseGroundPosition = new float2(rayOrigin.x - rayOrigin.y * rayDirection.x / rayDirection.y, rayOrigin.z - rayOrigin.y * rayDirection.z / rayDirection.y);

            SetSingleton<MouseInputComponent>(mouseInput);
        }
    }

}
