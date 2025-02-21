using System;
using Unity.Entities;
using UnityEngine;

public struct TriggerDamageData : IComponentData {
    public int value;
}

public struct AlreadyDamageBuffer : IBufferElementData {
    public Entity entity;
}

public class TriggerDamageAuthoring : MonoBehaviour {
    public int value;
    
    public class Baker : Baker<TriggerDamageAuthoring> {
        public override void Bake(TriggerDamageAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new TriggerDamageData { value = authoring.value });
            AddBuffer<AlreadyDamageBuffer>(entity);
        }
    }
}