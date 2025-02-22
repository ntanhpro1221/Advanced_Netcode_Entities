using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;

[UpdateInGroup(typeof(PhysicsSystemGroup))]
[UpdateAfter(typeof(PhysicsSimulationGroup))]
public partial struct HandleTriggerDamageEventSystem : ISystem {
    private BufferLookup<AlreadyDamageBuffer>  _alreadyDamageLookup;
    private BufferLookup<IncomingDamageBuffer> _incomingDamageLookup;
    private ComponentLookup<TeamTypeData>      _teamTypeLookup;
    private ComponentLookup<TriggerDamageData> _triggerDamageLookup;

    public void OnCreate(ref SystemState state) {
        state.RequireForUpdate<SimulationSingleton>();

        _alreadyDamageLookup  = state.GetBufferLookup<AlreadyDamageBuffer>();
        _incomingDamageLookup = state.GetBufferLookup<IncomingDamageBuffer>();
        _teamTypeLookup       = state.GetComponentLookup<TeamTypeData>();
        _triggerDamageLookup  = state.GetComponentLookup<TriggerDamageData>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        _alreadyDamageLookup.Update(ref state);
        _incomingDamageLookup.Update(ref state);
        _teamTypeLookup.Update(ref state);
        _triggerDamageLookup.Update(ref state);

        var ecb = new EntityCommandBuffer(Allocator.TempJob);
        
        new Job {
            ecb = ecb
          , alreadyDamageLookup  = _alreadyDamageLookup
          , incomingDamageLookup = _incomingDamageLookup
          , teamTypeLookup       = _teamTypeLookup
          , triggerDamageLookup  = _triggerDamageLookup
        }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency).Complete();
        
        ecb.Playback(state.EntityManager); 
        ecb.Dispose();
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