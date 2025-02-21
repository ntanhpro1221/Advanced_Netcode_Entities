using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial struct AutoDestroySystem : ISystem {
    [BurstCompile]
    public void OnCreate(ref SystemState state) {
        state.RequireForUpdate<NetworkTime>();
        state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        var ecb = SystemAPI
            .GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);
        var curTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;
        foreach (var (
            destroyAtTick
          , trans
          , entity) in SystemAPI.Query<
                RefRO<DestroyAtTickData>
              , RefRW<LocalTransform>>()
            .WithEntityAccess().WithAll<Simulate>()) {
            if (!curTick.IsNewerThan(destroyAtTick.ValueRO.tick)
             && !curTick.Equals(destroyAtTick.ValueRO.tick)) continue;

            if (state.WorldUnmanaged.IsServer()) ecb.DestroyEntity(entity);
            else trans.ValueRW.Position = new float3(0, -99999f, 0);
        }
    }
}