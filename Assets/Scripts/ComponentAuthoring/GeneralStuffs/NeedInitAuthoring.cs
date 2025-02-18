using Unity.Entities;
using UnityEngine;

public struct NeedInitTag: IComponentData { }

public class NeedInitAuthoring : MonoBehaviour {
    public class Baker : Baker<NeedInitAuthoring> {
        public override void Bake(NeedInitAuthoring authoring) {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<NeedInitTag>(entity);
        }
    }
}