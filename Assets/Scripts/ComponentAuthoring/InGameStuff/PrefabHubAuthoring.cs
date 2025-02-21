using Unity.Entities;
using UnityEngine;

public struct PrefabHub : IComponentData {
    public Entity champion;
    public Entity soldier;
}

public class PrefabHubAuthoring : MonoBehaviour {
    [SerializeField] private GameObject champion;
    [SerializeField] private GameObject soldier;

    public class Baker : Baker<PrefabHubAuthoring> {
        public override void Bake(PrefabHubAuthoring authoring) {
            AddComponent(GetEntity(TransformUsageFlags.None), new PrefabHub {
                champion = GetEntity(authoring.champion, TransformUsageFlags.Dynamic)
              , soldier  = GetEntity(authoring.soldier,  TransformUsageFlags.Dynamic)
            });
        }
    }
}