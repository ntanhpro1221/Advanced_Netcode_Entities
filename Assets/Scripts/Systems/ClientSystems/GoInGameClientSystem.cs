using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
public partial struct GoInGameClientSystem : ISystem {
    private EntityQuery _pendingNetworkIdQuery;

    [BurstCompile]
    public void OnCreate(ref SystemState state) {
        state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<ClientInitGameData>();
        _pendingNetworkIdQuery = new EntityQueryBuilder(Allocator.Temp)
                                 .WithAll<NetworkId>()
                                 .WithNone<NetworkStreamInGame>()
                                 .Build(state.EntityManager);

        state.RequireForUpdate(_pendingNetworkIdQuery);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        var ecb = SystemAPI
            .GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        var teamType = SystemAPI.GetSingleton<ClientInitGameData>().teamType;

        foreach (var entity in _pendingNetworkIdQuery.ToEntityArray(Allocator.Temp)) {
            // mark in game client-side
            ecb.AddComponent<NetworkStreamInGame>(entity);

            // request in game server-side
            var rpcEntity = ecb.CreateEntity();
            ecb.AddComponent(rpcEntity, new GoInGameRequestRpc {
                teamType = teamType
            });
            ecb.AddComponent<SendRpcCommandRequest>(rpcEntity);
        }
    }
}