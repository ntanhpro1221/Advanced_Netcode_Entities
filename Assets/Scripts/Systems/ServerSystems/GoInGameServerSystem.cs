using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct GoInGameServerSystem : ISystem {
    private EntityQuery goInGameRequestQuery;

    [BurstCompile]
    public void OnCreate(ref SystemState state) {
        goInGameRequestQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<
                GoInGameRequestRpc,
                ReceiveRpcCommandRequest>()
            .Build(state.EntityManager);

        state.RequireForUpdate(goInGameRequestQuery);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        var ecb = state
            .World
            .GetExistingSystemManaged<EndSimulationEntityCommandBufferSystem>()
            .CreateCommandBuffer();

        foreach (var (
            goInGameRequest,
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
            Debug.Log("Server mark in-game: " + entity + " with team-type = " + goInGameRequest.ValueRO.teamType);
        } 
    }
}
