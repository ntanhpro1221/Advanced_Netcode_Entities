using Unity.Entities;
using UnityEngine;

public struct ChampionTag : IComponentData { }

public struct ChampionChildRefData : IComponentData {
    public Entity bodyMesh;
}

public class ChampionCompAuthoring : MonoBehaviour {
    public GameObject bodyMesh;

    public class ChampionComponentBaker : Baker<ChampionCompAuthoring> {
        public override void Bake(ChampionCompAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent<ChampionTag>(entity);

            AddComponent(
                entity
              , new ChampionChildRefData {
                    bodyMesh = GetEntity(authoring.bodyMesh, TransformUsageFlags.Dynamic)
                });
        }
    }
}