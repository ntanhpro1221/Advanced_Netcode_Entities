using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial struct AutoDestroyPredictedSystem : ISystem {
    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (
            trans
          , entity) in SystemAPI.Query<
                RefRW<LocalTransform>>()
            .WithEntityAccess().WithAll<Simulate, AutoDestroyTag>()) {

            if (state.WorldUnmanaged.IsServer()) ecb.DestroyEntity(entity);
            else trans.ValueRW.Position = new float3(0, -99999f, 0);
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}