using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public struct DestroyRegisterData : IComponentData {
    public float lifeTime;
}

public struct DestroyAtTickData : IComponentData {
    [GhostField] public NetworkTick tick;
}

public class AutoDestroyAuthoring : MonoBehaviour {
    [SerializeField] private float lifeTime;

    public class Baker : Baker<AutoDestroyAuthoring> {
        public override void Bake(AutoDestroyAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new DestroyRegisterData { lifeTime = authoring.lifeTime });
        }
    }
}