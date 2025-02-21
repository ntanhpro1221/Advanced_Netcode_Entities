using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

public struct MoveInputData : IInputComponentData {
    [GhostField(Quantization = 0)] public float3 moveTarget;
    [GhostField]                   public bool   doneInit;
}

public struct MoveData : IComponentData {
    public float  moveSpeed;
    public float  rotateSpeed;
}

public class MoveableAuthoring : MonoBehaviour {
    public float  moveSpeed;
    public float  rotateSpeed;

    public class Baker : Baker<MoveableAuthoring> {
        public override void Bake(MoveableAuthoring authoring) {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            
            AddComponent<MoveInputData>(entity);
            
            AddComponent(entity, new MoveData {
                moveSpeed   = authoring.moveSpeed
              , rotateSpeed = authoring.rotateSpeed
            });
        }
    }
}
