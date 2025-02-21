using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(PhysicsSimulationGroup))]
[UpdateBefore(typeof(EndFixedStepSimulationEntityCommandBufferSystem))]
public partial struct HandleTriggerDamageEventSystem : ISystem {
    public void OnCreate(ref SystemState state) {
        state.RequireForUpdate<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<SimulationSingleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        state.Dependency = new Job {
            ecb = SystemAPI
                .GetSingleton<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged)
          , alreadyDamageLookup  = state.GetBufferLookup<AlreadyDamageBuffer>()
          , incomingDamageLookup = state.GetBufferLookup<IncomingDamageBuffer>()
          , teamTypeLookup       = state.GetComponentLookup<TeamTypeData>()
          , triggerDamageLookup  = state.GetComponentLookup<TriggerDamageData>()
        }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
    }

    [BurstCompile]
    public partial struct Job : ITriggerEventsJob {
        public EntityCommandBuffer ecb;

        [ReadOnly] public BufferLookup<AlreadyDamageBuffer>  alreadyDamageLookup;
        [ReadOnly] public BufferLookup<IncomingDamageBuffer> incomingDamageLookup;
        [ReadOnly] public ComponentLookup<TeamTypeData>      teamTypeLookup;
        [ReadOnly] public ComponentLookup<TriggerDamageData> triggerDamageLookup;

        public void Execute(TriggerEvent triggerEvent) {
            Debug.Log("trigger");
            // not have team
            if (!teamTypeLookup.HasComponent(triggerEvent.EntityA)
             || !teamTypeLookup.HasComponent(triggerEvent.EntityB)) return;

            //same team
            if (teamTypeLookup[triggerEvent.EntityA].value
             == teamTypeLookup[triggerEvent.EntityB].value) return;

            if (ValidateDamageOrder(triggerEvent.EntityA, triggerEvent.EntityB))
                Handle(triggerEvent.EntityA, triggerEvent.EntityB);
            if (ValidateDamageOrder(triggerEvent.EntityB, triggerEvent.EntityA))
                Handle(triggerEvent.EntityB, triggerEvent.EntityA);
        }

        private bool ValidateDamageOrder(in Entity sender, in Entity receiver) =>
            triggerDamageLookup.HasComponent(sender)
         && alreadyDamageLookup.HasBuffer(sender)
         && incomingDamageLookup.HasBuffer(receiver);

        private void Handle(in Entity sender, in Entity receiver) {
            // already damaged
            var alreadyDamageList = alreadyDamageLookup[sender];
            foreach (var alreadyDamage in alreadyDamageList)
                if (alreadyDamage.entity == receiver)
                    return;
            Debug.Log("append to buffer");
            ecb.AppendToBuffer(receiver, new IncomingDamageBuffer { value = triggerDamageLookup[sender].value });
        }
    }
}