﻿using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace TD
{
    public class DeadCooldawnSystem : SystemBase
    {
        //EntityQuery playerGroup;
        EntityManager manager;

        protected override void OnCreate()
        {
            manager = World.EntityManager;
            //playerGroup = manager.CreateEntityQuery(typeof(PlayerComponent), typeof(LifeComponent), typeof(DeadCooldawnComponent), typeof(DeadTag));
            base.OnCreate();
        }

        protected override void OnUpdate()
        {
            double time = Time.ElapsedTime;

            EntityCommandBuffer cmdBuffer = new EntityCommandBuffer(Allocator.Temp);
            //PlayerComponent is singleton, but use Bursted code for speed up
            Entities.WithAll<DeadTag>().ForEach((Entity entity, in PlayerComponent player, in LifeComponent life, in DeadCooldawnComponent cooldawn) =>
            {
                if (time - cooldawn.startTime > cooldawn.delayTime)
                {
                    cmdBuffer.SetComponent(entity, new LifeComponent()
                    {
                        life = life.maxLife,
                        maxLife = life.maxLife
                    });

                    cmdBuffer.RemoveComponent<DeadTag>(entity);
                    cmdBuffer.RemoveComponent<DeadCooldawnComponent>(entity);
                }
            }).Run();

            cmdBuffer.Playback(manager);

            //PlayerComponent is singleton, but it will be better to create EntityQuery for group with DeadTag and DeadCooldawnComponent components
            //because at most time this query is empty
            /*NativeArray<Entity> players = playerGroup.ToEntityArray(Allocator.TempJob);

            if(players.Length > 0)
            {
                //use only the first entity, because there are no other (PlayerComponent is singleton)
                Entity playerEntity = players[0];

                DeadCooldawnComponent cooldawn = manager.GetComponentData<DeadCooldawnComponent>(playerEntity);
                if(time - cooldawn.startTime > cooldawn.delayTime)
                {
                    //set full life
                    LifeComponent life = manager.GetComponentData<LifeComponent>(playerEntity);
                    manager.SetComponentData<LifeComponent>(playerEntity, new LifeComponent()
                    {
                        life = life.maxLife,
                        maxLife = life.maxLife
                    });

                    //delete dead tag
                    manager.RemoveComponent<DeadTag>(playerEntity);
                    manager.RemoveComponent<DeadCooldawnComponent>(playerEntity);
                }
            }

            players.Dispose();*/
        }
    }
}