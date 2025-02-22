using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

namespace Systems.PredictedSystems {
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup), OrderLast = true)]
    public partial struct ApplyFrameDamageSystem : ISystem {
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<NetworkTime>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (
                incomingDamageBuffer
              , health
              , entity) in SystemAPI.Query<
                    DynamicBuffer<IncomingDamageBuffer>
                  , RefRW<HealthData>>()
                .WithEntityAccess().WithAll<Simulate>()) {

                // calc total damage
                int totalDamage = 0;
                foreach (var incomingDamage in incomingDamageBuffer)
                    totalDamage += incomingDamage.value;
                
                // clear damage buffer
                incomingDamageBuffer.Clear();
                
                // apply damage
                health.ValueRW.current -= totalDamage;
                
                // die handle (destroy)
                if (health.ValueRW.current <= 0) {
                    ecb.AddComponent<AutoDestroyTag>(entity);
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}