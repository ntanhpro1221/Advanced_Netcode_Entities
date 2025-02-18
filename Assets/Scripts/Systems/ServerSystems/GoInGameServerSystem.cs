using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct GoInGameServerSystem : ISystem {
    private EntityQuery _goInGameRequestQuery;

    [BurstCompile]
    public void OnCreate(ref SystemState state) {
        state.RequireForUpdate<PrefabHub>();

        _goInGameRequestQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<
                GoInGameRequestRpc,
                ReceiveRpcCommandRequest>()
            .Build(state.EntityManager);
        state.RequireForUpdate(_goInGameRequestQuery);
        state.RequireForUpdate<RandomData>();
    }

    public void OnUpdate(ref SystemState state) {
        var ecb = state
            .World
            .GetExistingSystemManaged<EndSimulationEntityCommandBufferSystem>()
            .CreateCommandBuffer();

        var prefabHub = SystemAPI.GetSingleton<PrefabHub>();

        foreach (var (goInGameRequest,
            receiveRpcCommandRequest,
            entity) in SystemAPI.Query<
                RefRO<GoInGameRequestRpc>,
                RefRO<ReceiveRpcCommandRequest>>()
            .WithEntityAccess()) {
            // destroy request
            ecb.DestroyEntity(entity);

            // mark in game server-side
            ecb.AddComponent<NetworkStreamInGame>(receiveRpcCommandRequest.ValueRO.SourceConnection);

            // handle team type stuff
            TeamType teamType = goInGameRequest.ValueRO.teamType;
            switch (goInGameRequest.ValueRO.teamType) {
                case TeamType.AutoAssign:
                    var opponentQuery = new EntityQueryBuilder(Allocator.Temp)
                        .WithAll<TeamTypeData, ChampionTag>()
                        .Build(state.EntityManager);
                    teamType = opponentQuery.CalculateEntityCount() != 0
                        ? opponentQuery.ToComponentDataArray<TeamTypeData>(Allocator.Temp)[0].value.GetOpponentTeam()
                        : SystemAPI.GetSingleton<RandomData>().Bool
                            ? TeamType.Blue
                            : TeamType.Red;
                    break;
                case TeamType.Blue:      break;
                case TeamType.Red:       break;
                case TeamType.Spectator: continue;
                default:                 throw new ArgumentOutOfRangeException();
            }

            Entity champion = ecb.Instantiate(prefabHub.champion);
            ecb.SetName(champion, "Champion");

            // set spawn posision
            ecb.SetComponent(champion, LocalTransform.FromPosition(teamType switch {
                TeamType.Blue => new float3(-5, 0, 0)
              , TeamType.Red  => new float3(5,  0, 0)
              , _             => throw new ArgumentOutOfRangeException()
            }));

            // set owner of champion (so may be only that onwer can write to inputdata of the champion)
            ecb.SetComponent(champion, new GhostOwner {
                NetworkId = SystemAPI.GetComponent<NetworkId>(receiveRpcCommandRequest.ValueRO.SourceConnection).Value
            });

            // set team type
            ecb.SetComponent(champion, new TeamTypeData {
                value = teamType
            });

            // Link champion to client
            ecb.AppendToBuffer(receiveRpcCommandRequest.ValueRO.SourceConnection, new LinkedEntityGroup {
                Value = champion
            });

            Debug.Log("Server mark in-game: " + entity + " with team-type = " + goInGameRequest.ValueRO.teamType);
        }
    }
}