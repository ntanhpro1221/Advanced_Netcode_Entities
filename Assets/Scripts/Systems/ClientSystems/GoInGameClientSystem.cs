using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
public partial struct GoInGameClientSystem : ISystem {
    private EntityQuery pendingNetworkIdQuery;

    [BurstCompile]
    public void OnCreate(ref SystemState state) {
        pendingNetworkIdQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<NetworkId>()
            .WithNone<NetworkStreamInGame>()
            .Build(state.EntityManager);

        state.RequireForUpdate(pendingNetworkIdQuery);
        state.RequireForUpdate<TeamTypeInfo>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        var ecb = state
            .World
            .GetExistingSystemManaged<EndSimulationEntityCommandBufferSystem>()
            .CreateCommandBuffer();

        TeamType teamType = SystemAPI.GetSingleton<TeamTypeInfo>().teamType;

        foreach (Entity entity in pendingNetworkIdQuery.ToEntityArray(Allocator.Temp)) {
            // mark in game client-side
            ecb.AddComponent<NetworkStreamInGame>(entity);

            // request in game server-side
            Entity rpcEntity = ecb.CreateEntity();
            ecb.AddComponent(rpcEntity, new GoInGameRequestRpc {
                teamType = teamType,
            });
            ecb.AddComponent<SendRpcCommandRequest>(rpcEntity);
        }
    }
}
