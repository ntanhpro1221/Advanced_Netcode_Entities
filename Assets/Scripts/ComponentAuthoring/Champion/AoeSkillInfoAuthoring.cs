using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[GhostComponent(OwnerSendType = SendToOwnerType.None)]
public struct AoeSkillInfoData : IComponentData {
    /// <summary>
    /// in ms
    /// </summary>
    public int coolDownTime;

    [GhostField] public NetworkTick startAtTick;
    [GhostField] public NetworkTick doneAtTick;

    public bool AllTickValid =>
        startAtTick.IsValid
     && doneAtTick.IsValid;
}

public class AoeSkillInfoAuthoring : MonoBehaviour {
    /// <summary>
    /// in ms
    /// </summary>
    [Tooltip("in ms")]
    public int coolDownTime;

    public class AoeSkillInfoBaker : Baker<AoeSkillInfoAuthoring> {
        public override void Bake(AoeSkillInfoAuthoring authoring) {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new AoeSkillInfoData { coolDownTime = authoring.coolDownTime });
        }
    }
}