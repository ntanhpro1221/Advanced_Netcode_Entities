using Unity.Entities;
using Unity.NetCode;

public struct TeamTypeData : IComponentData {
    [GhostField] public TeamType value;
}