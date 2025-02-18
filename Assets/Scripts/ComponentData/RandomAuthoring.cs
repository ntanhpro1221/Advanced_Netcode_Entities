using System;
using Unity.Entities;
using UnityEngine;

public struct RandomData : IComponentData {
    public Unity.Mathematics.Random random;

    public bool  Bool  => random.NextBool();
    public int   Int   => random.NextInt();
    public uint  UInt  => random.NextUInt();
    public float Float => random.NextFloat();
}

public class RandomAuthoring : MonoBehaviour {
    public class Baker : Baker<RandomAuthoring> {
        public override void Bake(RandomAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new RandomData {
                random = new((uint)(DateTime.Now.Ticks % uint.MaxValue))
            });
        }
    }
}