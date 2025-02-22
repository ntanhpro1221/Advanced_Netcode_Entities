using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

public readonly partial struct SkillAspect : IAspect {
    private readonly RefRO<SkillInputData>   _skillInput;
    private readonly RefRO<TeamTypeData>     _teamType;
    private readonly RefRO<LocalTransform>   _trans;
    private readonly RefRO<SkillPrefabData>  _skillPrefab;
    public readonly  RefRW<AoeSkillInfoData> aoeSkillInfo;

    public bool ShouldAoe(NetworkTick curTick) {
        if (!_skillInput.ValueRO.aoe.IsSet) return false;
        if (!aoeSkillInfo.ValueRO.AllTickValid) return true;
        return curTick.IsValid
         && curTick.IsNewerThan(aoeSkillInfo.ValueRO.doneAtTick);
    }

    public Entity AoeEntity => _skillPrefab.ValueRO.aoe;
    
    public bool ShouldProjectile => _skillInput.ValueRO.projectile.IsSet;
    public Entity ProjectileEntity => _skillPrefab.ValueRO.projectile;
    
    public float3 AttackPosition => _trans.ValueRO.Position;
    public TeamTypeData Team => _teamType.ValueRO;
}