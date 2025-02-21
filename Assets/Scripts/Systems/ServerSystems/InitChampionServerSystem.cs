using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

/// <summary>
/// Init [name | spawn pos | move target | team type] of champion
/// </summary>
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct InitChampionServerSystem : ISystem {
    private NativeList<TeamType> _teamTypeList;
    
    [BurstCompile]
    public void OnCreate(ref SystemState state) {
        state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate(new EntityQueryBuilder(Allocator.Temp)
            .WithAll<
                ChampionTag
              , NeedInitTag>()
            .Build(state.EntityManager));
        state.RequireForUpdate<RandomHelperData>();

        _teamTypeList = new(Allocator.Persistent);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state) {
        _teamTypeList.Dispose();
    }
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        var ecb = SystemAPI
            .GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);
        
        foreach (var (
            localTrans
          , ghostOwner
          , teamType
          , entity) in SystemAPI.Query<
                RefRW<LocalTransform>
              , RefRO<GhostOwner>
              , RefRW<TeamTypeData>>()
            .WithEntityAccess().WithAll<ChampionTag, NeedInitTag>()) {
            // remove init tag
            ecb.RemoveComponent<NeedInitTag>(entity);

            // handle team type
            switch (teamType.ValueRO.value) {
                case TeamType.AutoAssign:
                    teamType.ValueRW.value = _teamTypeList.Length != 0
                        ? _teamTypeList[0].GetOpponentTeam()
                        : SystemAPI.GetSingleton<RandomHelperData>().Bool
                            ? TeamType.Blue
                            : TeamType.Red;
                    break;
                case TeamType.Blue:      break;
                case TeamType.Red:       break;
                case TeamType.Spectator: continue;
                default:                 throw new ArgumentOutOfRangeException();
            }

            _teamTypeList.Add(teamType.ValueRO.value);

            // change name
            ecb.SetName(entity, $"Champion-{ghostOwner.ValueRO.NetworkId}");

            // set spawn position
            localTrans.ValueRW = LocalTransform.FromPosition(teamType.ValueRO.value switch {
                TeamType.Blue => new float3(-5, 0, 0)
              , TeamType.Red  => new float3(5,  0, 0)
              , _             => throw new ArgumentOutOfRangeException()
            });
        }
    }
}