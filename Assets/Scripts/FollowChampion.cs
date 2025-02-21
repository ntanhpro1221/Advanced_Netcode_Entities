using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;

public class FollowChampion : MonoBehaviour {
    private void LateUpdate() {
        if (!Keyboard.current.spaceKey.isPressed) return;
        
        EntityQuery query = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<
                ChampionTag
              , GhostOwnerIsLocal
              , LocalTransform>()
            .Build(World.DefaultGameObjectInjectionWorld.EntityManager);
        if (query.CalculateEntityCount() == 0) return;
        LocalTransform champTrans = query.ToComponentDataArray<LocalTransform>(Allocator.Temp)[0];
        transform.position = new(
            champTrans.Position.x
          , transform.position.y
          , champTrans.Position.z - 7.5f);   
    }
}