using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial struct HandleDestroyAtTickSystem : ISystem {
    [BurstCompile]
    public void OnCreate(ref SystemState state) {
        state.RequireForUpdate<NetworkTime>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        var curTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;
        foreach (var (
            destroyAtTick
          , entity) in SystemAPI.Query<
                RefRO<DestroyAtTickData>>()
            .WithEntityAccess().WithAll<Simulate>()) {
            
            if (!curTick.IsNewerThan(destroyAtTick.ValueRO.tick)
             && !curTick.Equals(destroyAtTick.ValueRO.tick)) continue;
            
            ecb.RemoveComponent<DestroyAtTickData>(entity);
            ecb.AddComponent<AutoDestroyTag>(entity);
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}