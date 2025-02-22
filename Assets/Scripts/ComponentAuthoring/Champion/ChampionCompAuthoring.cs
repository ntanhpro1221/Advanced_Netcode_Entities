using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public struct ChampionTag : IComponentData { }

public struct ChampionChildRefData : IComponentData {
    public Entity bodyMesh;
}

public struct SkillInputData : IInputComponentData {
    [GhostField] public InputEvent aoe;
    [GhostField] public InputEvent projectile;
}

public struct SkillPrefabData : IComponentData {
    public Entity aoe;
    public Entity projectile;
}


public class ChampionCompAuthoring : MonoBehaviour {
    public GameObject bodyMesh;
    
    [Space]
    [Header("SKILL")]
    public GameObject aoePrefab;
    public GameObject projectilePrefab;

    public class ChampionComponentBaker : Baker<ChampionCompAuthoring> {
        public override void Bake(ChampionCompAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent<ChampionTag>(entity);

            AddComponent<SkillInputData>(entity);

            AddComponent(entity
              , new ChampionChildRefData {
                    bodyMesh = GetEntity(authoring.bodyMesh, TransformUsageFlags.Dynamic)
                });

            AddComponent(entity
              , new SkillPrefabData {
                    aoe        = GetEntity(authoring.aoePrefab,        TransformUsageFlags.Dynamic)
                  , projectile = GetEntity(authoring.projectilePrefab, TransformUsageFlags.Dynamic)
                });
        }
    }
}