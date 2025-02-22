using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.UI;

public class AoeSkillCooldownUI : MonoBehaviour {
    [SerializeField] private Image coolDownCover;

    private EntityQuery _skillInfoQuery;
    private EntityQuery _networkTimeQuery;
    
    private void Start() {
        _skillInfoQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<
                AoeSkillInfoData
              , ChampionTag
              , GhostOwnerIsLocal>()
            .WithNone<NeedInitTag>()
            .Build(World.DefaultGameObjectInjectionWorld.EntityManager);

        _networkTimeQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<NetworkTime>()
            .Build(World.DefaultGameObjectInjectionWorld.EntityManager);
    }

    private void LateUpdate() {
        coolDownCover.fillAmount = 0;

        if (_skillInfoQuery.CalculateEntityCount() == 0) return;

        var skillInfo = _skillInfoQuery.GetSingleton<AoeSkillInfoData>();
        if (!skillInfo.AllTickValid) return;

        var curTick = _networkTimeQuery.GetSingleton<NetworkTime>().ServerTick;
        if (curTick.IsNewerThan(skillInfo.doneAtTick)) return;

        coolDownCover.fillAmount =
            1 - (float)curTick.TicksSince(skillInfo.startAtTick) / skillInfo.doneAtTick.TicksSince(skillInfo.startAtTick);

    }
}
