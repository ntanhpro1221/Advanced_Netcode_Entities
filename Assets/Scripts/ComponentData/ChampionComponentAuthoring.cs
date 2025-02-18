using Unity.Entities;
using UnityEngine;

public struct ChampionTag : IComponentData { }

public struct NeedInitTag: IComponentData { }

public struct ChampionChildEntityData : IComponentData {
    public Entity bodyMesh;
}

public class ChampionComponentAuthoring : MonoBehaviour {
    public GameObject bodyMesh;

    public class ChampionComponentBaker : Baker<ChampionComponentAuthoring> {
        public override void Bake(ChampionComponentAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity
              , new ComponentTypeSet(
                    typeof(TeamTypeData)
                  , typeof(ChampionTag)
                  , typeof(NeedInitTag)));

            AddComponent(entity
              , new ChampionChildEntityData {
                    bodyMesh = GetEntity(authoring.bodyMesh, TransformUsageFlags.None)
                });
        }
    }
}