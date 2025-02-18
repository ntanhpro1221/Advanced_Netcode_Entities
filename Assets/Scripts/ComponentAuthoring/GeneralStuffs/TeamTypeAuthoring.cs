using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public struct TeamTypeData : IComponentData {
    [GhostField] public TeamType value;
}

public class TeamTypeAuthoring : MonoBehaviour {
    public TeamType value;

    public class Baker : Baker<TeamTypeAuthoring> {
        public override void Bake(TeamTypeAuthoring authoring) {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new TeamTypeData { value = authoring.value });
        }
    }
}