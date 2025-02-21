﻿using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial struct ExecuteSkillCommandSystem : ISystem {
    [BurstCompile]
    public void OnCreate(ref SystemState state) {
        state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<NetworkTime>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        var ecb = SystemAPI
            .GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        
        if (!SystemAPI.GetSingleton<NetworkTime>().IsFirstTimeFullyPredictingTick) return;

        foreach (var skill in SystemAPI.Query<SkillAspect>().WithAll<Simulate>()) {
            if (skill.ShouldAoe) {
                Entity entity = ecb.Instantiate(skill.AoeEntity);
                ecb.SetComponent(entity, LocalTransform.FromPositionRotationScale(
                    skill.AttackPosition
                  , quaternion.identity
                  , 5));
                ecb.SetComponent(entity, skill.Team);
            }

            if (skill.ShouldProjectile) {
                Entity entity = ecb.Instantiate(skill.ProjectileEntity);
                ecb.SetComponent(entity, LocalTransform.FromPosition(skill.AttackPosition));
                ecb.SetComponent(entity, skill.Team);
            }
        }
    }
}

