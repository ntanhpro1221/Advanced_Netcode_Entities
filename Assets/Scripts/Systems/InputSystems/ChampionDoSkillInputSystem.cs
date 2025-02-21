using Unity.Entities;
using Unity.NetCode;

[UpdateInGroup(typeof(GhostInputSystemGroup))]
public partial class ChampionDoSkillInputSystem : SystemBase {
    private GlobalInput _input;

    protected override void OnCreate() {
        base.OnCreate();
        _input = new();
    }

    protected override void OnStartRunning() {
        base.OnStartRunning();

        _input.Enable();
    }

    protected override void OnStopRunning() {
        base.OnStopRunning();

        _input.Disable();
    }

    protected override void OnUpdate() {
        var newInputValue = new SkillInputData();
        if (_input.InGame.Skill_Aoe.WasPressedThisFrame()) 
            newInputValue.aoe.Set();
        if (_input.InGame.Skill_Projectile.WasPressedThisFrame())
            newInputValue.projectile.Set();
        
        foreach (var skillInput in SystemAPI.Query<RefRW<SkillInputData>>())
            skillInput.ValueRW = newInputValue;
    }
}