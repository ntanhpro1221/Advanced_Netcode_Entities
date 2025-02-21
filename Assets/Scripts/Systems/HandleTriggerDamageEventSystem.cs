using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;

[UpdateInGroup(typeof(PhysicsSystemGroup))]
[UpdateAfter(typeof(PhysicsSimulationGroup))]
public partial struct HandleTriggerDamageEventSystem : ISystem {
    private BufferLookup<AlreadyDamageBuffer>  alreadyDamageLookup;
    private BufferLookup<IncomingDamageBuffer> incomingDamageLookup;
    private ComponentLookup<TeamTypeData>      teamTypeLookup;
    private ComponentLookup<TriggerDamageData> triggerDamageLookup;

    public void OnCreate(ref SystemState state) {
        state.RequireForUpdate<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<SimulationSingleton>();

        alreadyDamageLookup  = state.GetBufferLookup<AlreadyDamageBuffer>();
        incomingDamageLookup = state.GetBufferLookup<IncomingDamageBuffer>();
        teamTypeLookup       = state.GetComponentLookup<TeamTypeData>();
        triggerDamageLookup  = state.GetComponentLookup<TriggerDamageData>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        alreadyDamageLookup.Update(ref state);
        incomingDamageLookup.Update(ref state);
        teamTypeLookup.Update(ref state);
        triggerDamageLookup.Update(ref state);

        state.Dependency = new Job {
            ecb = SystemAPI
                .GetSingleton<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged)
          , alreadyDamageLookup  = alreadyDamageLookup
          , incomingDamageLookup = incomingDamageLookup
          , teamTypeLookup       = teamTypeLookup
          , triggerDamageLookup  = triggerDamageLookup
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
            
            ecb.AppendToBuffer(sender, new AlreadyDamageBuffer { entity = receiver });
            ecb.AppendToBuffer(receiver, new IncomingDamageBuffer { value = triggerDamageLookup[sender].value });
        }
    }
}