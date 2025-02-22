using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial struct ExecuteSkillCommandSystem : ISystem {
    [BurstCompile]
    public void OnCreate(ref SystemState state) {
        state.RequireForUpdate<NetworkTime>();
    }

    public void OnUpdate(ref SystemState state) {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        var networkTime = SystemAPI.GetSingleton<NetworkTime>();
        if (!networkTime.IsFirstTimeFullyPredictingTick) return;
        var curTick = networkTime.ServerTick;
        
        foreach (var skill in SystemAPI.Query<SkillAspect>().WithAll<Simulate>()) {
            if (skill.ShouldAoe(curTick)) {
                Entity entity = ecb.Instantiate(skill.AoeEntity);
                ecb.SetComponent(entity, LocalTransform.FromPositionRotationScale(
                    skill.AttackPosition
                  , quaternion.identity
                  , 5));
                ecb.SetComponent(entity, skill.Team);
                
                // cooldown
                var doneTick = networkTime.ServerTick;
                doneTick.Add((uint)(skill.aoeSkillInfo.ValueRO.coolDownTime * NetCodeConfig.Global.ClientServerTickRate.SimulationTickRate));
                skill.aoeSkillInfo.ValueRW.doneAtTick = doneTick;
                skill.aoeSkillInfo.ValueRW.startAtTick = networkTime.ServerTick;
            }

            if (skill.ShouldProjectile) {
                Entity entity = ecb.Instantiate(skill.ProjectileEntity);
                ecb.SetComponent(entity, LocalTransform.FromPosition(skill.AttackPosition));
                ecb.SetComponent(entity, skill.Team);
            }
        }
        
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}

