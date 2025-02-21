using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public struct HealthData : IComponentData {
    public              int max;
    [GhostField] public int current;
}

public struct IncomingDamageBuffer : IBufferElementData {
    public int value;
}

public class DamageableAuthoring : MonoBehaviour {
    [SerializeField] private int maxHealth;

    private class Baker : Baker<DamageableAuthoring> {
        public override void Bake(DamageableAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity
              , new HealthData {
                    current = authoring.maxHealth
                  , max     = authoring.maxHealth
                });

            AddBuffer<IncomingDamageBuffer>(entity);
        }
    }
}