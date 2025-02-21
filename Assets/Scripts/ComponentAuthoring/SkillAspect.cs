using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public readonly partial struct SkillAspect : IAspect {
    private readonly RefRO<SkillInputData>  _skillInput;
    private readonly RefRO<TeamTypeData>    _teamType;
    private readonly RefRO<LocalTransform>  _trans;
    private readonly RefRO<SkillPrefabData> _skillPrefab;

    public bool ShouldAoe => _skillInput.ValueRO.aoe.IsSet;
    public Entity AoeEntity => _skillPrefab.ValueRO.aoe;
    
    public bool ShouldProjectile => _skillInput.ValueRO.projectile.IsSet;
    public Entity ProjectileEntity => _skillPrefab.ValueRO.projectile;
    
    public float3 AttackPosition => _trans.ValueRO.Position;
    public TeamTypeData Team => _teamType.ValueRO;
}