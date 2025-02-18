using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using UnityEngine;
using UnityEngine.InputSystem;

[UpdateInGroup(typeof(GhostInputSystemGroup))]
public partial class ChampionInputSystem : SystemBase {
    private GlobalInput     _input;
    private CollisionFilter _groundCastFilter;
    private EntityQuery     _ownerChampionQuery;

    private void OnClick(InputAction.CallbackContext context) {
        if (_ownerChampionQuery.CalculateEntityCount() == 0) return;

        var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
        var ray = Camera.main!.ScreenPointToRay(
            Mouse.current?.position.value
         ?? Touchscreen.current.touches[0].position.value);

        var rayCastInput = new RaycastInput {
            Start  = ray.GetPoint(0)
          , End    = ray.GetPoint(9999)
          , Filter = _groundCastFilter
        };
        if (collisionWorld.CastRay(rayCastInput, out var hit)) {
            float3 moveTarget = hit.Position;
            moveTarget.y = 0;

            _ownerChampionQuery.GetSingletonRW<MoveInputData>().ValueRW.moveTarget = moveTarget;
        }
    }

    protected override void OnCreate() {
        base.OnCreate();
        _input = new();
    }

    protected override void OnStartRunning() {
        base.OnStartRunning();

        _input.Enable();
        _input.InGame.Click.performed += OnClick;

        _groundCastFilter = new() {
            BelongsTo    = (uint)LayerMaskHelper.RayCast
          , CollidesWith = (uint)LayerMaskHelper.Ground
        };
        _ownerChampionQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<
                ChampionTag
              , GhostOwnerIsLocal>()
            .WithAllRW<
                MoveInputData>()
            .WithNone<
                NeedInitTag>()
            .Build(EntityManager);
    }

    protected override void OnStopRunning() {
        base.OnStopRunning();

        _input.InGame.Click.performed -= OnClick;
        _input.Disable();
    }

    protected override void OnUpdate() { }
}