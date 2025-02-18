using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
public partial struct InitChampionClientSystem : ISystem {
    [BurstCompile]
    public void OnCreate(ref SystemState state) {
        state.RequireForUpdate(new EntityQueryBuilder(Allocator.Temp)
            .WithAll<
                ChampionTag
              , NeedInitTag>()
            .Build(state.EntityManager));
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        var ecb = state
            .World
            .GetExistingSystemManaged<EndSimulationEntityCommandBufferSystem>()
            .CreateCommandBuffer();

        foreach (var (
            teamType
          , childEntity
          , entity) in SystemAPI.Query<
                RefRO<TeamTypeData>
              , RefRO<ChampionChildEntityData>>()
            .WithEntityAccess().WithAll<NeedInitTag>()) {
            ecb.RemoveComponent<NeedInitTag>(entity);

            float4 teamColor = teamType.ValueRO.value switch {
                TeamType.Blue => new float4(0.0f, 0.0f, 1.0f, 1.0f)
              , TeamType.Red  => new float4(1.0f, 0.0f, 0.0f, 1.0f)
              , _             => default
            }; 

            var materialColor = SystemAPI.GetComponentRW<URPMaterialPropertyBaseColor>(childEntity.ValueRO.bodyMesh);
            materialColor.ValueRW.Value = teamColor;
        }
    }
}