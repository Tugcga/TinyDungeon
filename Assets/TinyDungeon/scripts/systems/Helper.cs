using Unity.Entities;
using Unity.Tiny.Animation;
using Unity.Mathematics;

namespace TD
{
    public static class Helper
    {
        //copy from unity.com forum
        //may in future versions of the Tiny this is not neccessary
        public static void SelectClipAtIndex(World world, Entity entity, int index)
        {
            /////////////Stop() method////////////////////////
            var currentClip = world.EntityManager.GetComponentData<TinyAnimationPlayer>(entity).CurrentClip;
            var buffer = world.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>().CreateCommandBuffer();

            if (world.EntityManager.HasComponent<UpdateAnimationTimeTag>(currentClip))
                buffer.RemoveComponent<UpdateAnimationTimeTag>(currentClip);

            if (world.EntityManager.HasComponent<ApplyAnimationResultTag>(currentClip))
                buffer.RemoveComponent<ApplyAnimationResultTag>(currentClip);
            //world.EntityManager.SetComponentData(currentClip, new TinyAnimationTime { Value = 0f, InternalWorkTime = 0f});
            ////////////////////////////////////////////////////
            var clipsBuffer = world.EntityManager.GetBuffer<TinyAnimationClipRef>(entity);
            var selectedClip = clipsBuffer[index].Value;
            world.EntityManager.SetComponentData(entity, new TinyAnimationPlayer { CurrentClip = selectedClip, CurrentIndex = index });
            world.EntityManager.SetComponentData(selectedClip, new TinyAnimationTime { Value = 0f, InternalWorkTime = 0f });

            currentClip = selectedClip;

            ////////////////Play() method/////////////////////
            // Are we playing a stopped clip?
            if (!world.EntityManager.HasComponent<UpdateAnimationTimeTag>(currentClip) &&
                !world.EntityManager.HasComponent<ApplyAnimationResultTag>(currentClip))
            {
                var playbackInfo = world.EntityManager.GetComponentData<TinyAnimationPlaybackInfo>(currentClip);

                // Are we updating a cyclical wrap mode?
                if (playbackInfo.WrapMode == WrapMode.Loop || playbackInfo.WrapMode == WrapMode.PingPong)
                {
                    var startOffset = playbackInfo.CycleOffset * playbackInfo.Duration;
                    world.EntityManager.SetComponentData(currentClip, new TinyAnimationTime { Value = startOffset, InternalWorkTime = startOffset });
                }
            }

            buffer.AddComponent<UpdateAnimationTimeTag>(currentClip);
            buffer.AddComponent<ApplyAnimationResultTag>(currentClip);
            ////////////////////////////////////////////////
        }

        public static int GetSegmentIndex(int2 uv)
        {
            return GetSegmentIndex(uv.x, uv.y);
        }

        public static int GetSegmentIndex(int posU, int posV)
        {
            int absPosU = math.abs(posU);
            int absPosV = math.abs(posV);
            int squareIndex = math.max(absPosU, absPosV);
            int segmentIndex = -1;
            if (squareIndex == 0)
            {
                segmentIndex = 0;
            }
            else
            {
                int startSquareIndex = (2 * squareIndex - 1) * (2 * squareIndex - 1);
                if (posU == 0 && posV != 0)
                {
                    segmentIndex = posV > 0 ? startSquareIndex + 6 * squareIndex : startSquareIndex + 2 * squareIndex;
                }
                else if (posU != 0 && posV == 0)
                {
                    segmentIndex = posU > 0 ? startSquareIndex + 4 * squareIndex : startSquareIndex;
                }
                else if (posU < 0 && posV < 0)
                {
                    segmentIndex = startSquareIndex + absPosV + (absPosV > absPosU ? absPosV - absPosU : 0);
                }
                else if (posU < 0 && posV > 0)
                {
                    segmentIndex = startSquareIndex + 6 * squareIndex + absPosU + (absPosU > absPosV ? absPosU - absPosV : 0);
                }
                else if (posU > 0 && posV < 0)
                {
                    segmentIndex = startSquareIndex + 2 * squareIndex + posU + (absPosU > absPosV ? absPosU - absPosV : 0);
                }
                else if (posU > 0 && posV > 0)
                {
                    segmentIndex = startSquareIndex + 4 * squareIndex + posV + (absPosV > absPosU ? absPosV - absPosU : 0);
                }
                else
                {
                    segmentIndex = -1;
                }
            }
            return segmentIndex;
        }

        public static int GetSegmentIndexFromPosition(float2 position, float segmentSize)
        {
            float x = position.x / segmentSize + (position.x > 0 ? 1 : -1) * 0.5f;
            int u = (int)(x);

            float y = position.y / segmentSize + (position.y > 0 ? 1 : -1) * 0.5f;
            int v = (int)(y);

            return GetSegmentIndex(u, v);
        }

        public static int2 GetSegmentUVFromPosition(float2 position, float segmentSize)
        {
            float x = position.x / segmentSize + (position.x > 0 ? 1 : -1) * 0.5f;
            int u = (int)(x);

            float y = position.y / segmentSize + (position.y > 0 ? 1 : -1) * 0.5f;
            int v = (int)(y);

            return new int2(u, v);
        }

        public static int GetSegmentIndexFromPosition(float3 position, float segmentSize)
        {
            float x = position.x / segmentSize + (position.x > 0 ? 1 : -1) * 0.5f;
            int u = (int)(x);

            float y = position.z / segmentSize + (position.z > 0 ? 1 : -1) * 0.5f;
            int v = (int)(y);

            return GetSegmentIndex(u, v);
        }
    }
}