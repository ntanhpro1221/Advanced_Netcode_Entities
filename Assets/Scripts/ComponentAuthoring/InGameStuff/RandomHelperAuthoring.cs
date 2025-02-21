using System;
using Unity.Entities;
using UnityEngine;

public struct RandomHelperData : IComponentData {
    public Unity.Mathematics.Random random;

    public bool  Bool  => random.NextBool();
    public int   Int   => random.NextInt();
    public uint  UInt  => random.NextUInt();
    public float Float => random.NextFloat();
}

public class RandomHelperAuthoring : MonoBehaviour {
    public class Baker : Baker<RandomHelperAuthoring> {
        public override void Bake(RandomHelperAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new RandomHelperData {
                random = new((uint)(DateTime.Now.Ticks % uint.MaxValue))
            });
        }
    }
}