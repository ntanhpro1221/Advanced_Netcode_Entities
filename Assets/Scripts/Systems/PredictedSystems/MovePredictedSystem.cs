using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial struct MovePredictedSystem : ISystem {
    [BurstCompile]
    public void OnCreate(ref SystemState state) {
        state.RequireForUpdate(new EntityQueryBuilder(Allocator.Temp)
            .WithAll<
                MoveInputData
              , MoveData
              , LocalTransform
              , PhysicsVelocity
              , Simulate>()
            .Build(state.EntityManager));
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        new Job {
            deltaTime = SystemAPI.Time.DeltaTime
        }.ScheduleParallel();
    }

    [BurstCompile]
    [WithAll(typeof(Simulate))]
    public partial struct Job : IJobEntity {
        public float deltaTime;

        [BurstCompile]
        private void Execute(in  MoveInputData   moveInput
                           , in  MoveData        moveData
                           , ref LocalTransform  localTrans
                           , ref PhysicsVelocity velocity) {
            if (!moveInput.doneInit) return;
            
            // reset velocity very first
            velocity.Linear  = float3.zero;
            velocity.Angular = float3.zero;

            // move
            float3 moveDel = moveInput.moveTarget - localTrans.Position;
            float  moveDis = math.length(moveDel);
            if (moveDis > deltaTime * moveData.moveSpeed)
                velocity.Linear      = math.normalize(moveDel) * moveData.moveSpeed;
            else localTrans.Position = moveInput.moveTarget;

            // rotate
            if (moveDis > 0.001f) {
                quaternion rotateTarget = quaternion.LookRotation(moveDel, math.up());
                float3 rotateDel = math.Euler(math.mul(
                    rotateTarget
                  , math.inverse(localTrans.Rotation)));
                float rotateDis = math.length(rotateDel);
                if (rotateDis > deltaTime * moveData.rotateSpeed)
                    velocity.Angular     = math.normalize(rotateDel) * moveData.rotateSpeed;
                else localTrans.Rotation = rotateTarget;
            }
        }
    }
}