using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct GoInGameServerSystem : ISystem {
    private EntityQuery _goInGameRequestQuery;

    [BurstCompile]
    public void OnCreate(ref SystemState state) {
        state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<PrefabHub>();

        _goInGameRequestQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<
                GoInGameRequestRpc,
                ReceiveRpcCommandRequest>()
            .Build(state.EntityManager);
        state.RequireForUpdate(_goInGameRequestQuery);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        var ecb = SystemAPI
            .GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        var prefabHub = SystemAPI.GetSingleton<PrefabHub>();

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

            // spawn client's champion
            Entity champEntity = ecb.Instantiate(prefabHub.champion);
            ecb.SetComponent(champEntity, new TeamTypeData { value = goInGameRequest.ValueRO.teamType });
            ecb.SetComponent( // set ghost owner
                champEntity
              , new GhostOwner {
                    NetworkId = SystemAPI.GetComponent<NetworkId>(receiveRpcCommandRequest.ValueRO.SourceConnection).Value
                });
            ecb.AppendToBuffer( // bind to client (disappear when client disconnect)
                receiveRpcCommandRequest.ValueRO.SourceConnection
              , new LinkedEntityGroup {
                    Value = champEntity
                });
        }
    }
}