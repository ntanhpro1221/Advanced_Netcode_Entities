using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

/// <summary>
/// Init [team color | move input] of champion]
/// </summary>
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
          , moveInput
          , localTrans
          , childEntity
          , entity) in SystemAPI.Query<
                RefRO<TeamTypeData>
              , RefRW<MoveInputData>
              , RefRO<LocalTransform>
              , RefRO<ChampionChildRefData>>()
            .WithEntityAccess().WithAll<ChampionTag, NeedInitTag>()) {
            // remove init tag
            ecb.RemoveComponent<NeedInitTag>(entity);

            // set name
            ecb.SetName(entity, "My-Champion");

            // set team color
            float4 teamColor = teamType.ValueRO.value switch {
                TeamType.Blue => new float4(0.0f, 0.0f, 1.0f, 1.0f)
              , TeamType.Red  => new float4(1.0f, 0.0f, 0.0f, 1.0f)
              , _             => default
            };
            var materialColor = SystemAPI.GetComponentRW<URPMaterialPropertyBaseColor>(childEntity.ValueRO.bodyMesh);
            materialColor.ValueRW.Value = teamColor;

            // init move input data
            moveInput.ValueRW.moveTarget = localTrans.ValueRO.Position;
            
            // mark done init
            moveInput.ValueRW.doneInit = true;
        }
    }
}