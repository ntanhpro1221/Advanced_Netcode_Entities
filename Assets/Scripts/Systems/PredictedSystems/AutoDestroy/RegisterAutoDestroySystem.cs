using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial struct RegisterAutoDestroySystem : ISystem {
    [BurstCompile]
    public void OnCreate(ref SystemState state) {
        state.RequireForUpdate<NetworkTime>();
    }

    public void OnUpdate(ref SystemState state) {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        var tickRate = NetCodeConfig.Global.ClientServerTickRate.SimulationTickRate;
        var curTick  = SystemAPI.GetSingleton<NetworkTime>().ServerTick;
        
        foreach (var (
            destroyRegisterData
          , entity) in SystemAPI.Query<
                RefRO<DestroyRegisterData>>()
            .WithEntityAccess().WithAll<Simulate>()) {
            ecb.RemoveComponent<DestroyRegisterData>(entity);

            var destroyTick = curTick;
            destroyTick.Add((uint)(destroyRegisterData.ValueRO.lifeTime * tickRate));
            ecb.AddComponent(entity, new DestroyAtTickData { tick = destroyTick });
        }
        
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}