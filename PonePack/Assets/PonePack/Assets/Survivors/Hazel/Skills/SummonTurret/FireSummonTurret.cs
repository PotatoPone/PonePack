using EntityStates;
using RoR2;
using RoR2.Projectile;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PonePack.EntityStates.Hazel
{
    public class FireSummonTurret : GenericProjectileBaseState //GenericCharacterSpawnState ?
    {
        //private static int SpawnStateHash = Animator.StringToHash("Spawn");
        //private static int SpawnParamHash = Animator.StringToHash("Spawn.playbackRate");

        public override void ModifyProjectileInfo(ref FireProjectileInfo fireProjectileInfo)
        {
            DamageTypeCombo damageTypeCombo = new DamageTypeCombo();
            damageTypeCombo.damageType = DamageType.Generic;
            damageTypeCombo.damageTypeExtended = DamageTypeExtended.Generic;
            damageTypeCombo.damageSource = DamageSource.Secondary;

            fireProjectileInfo.damageTypeOverride = damageTypeCombo;
        }
    }
}
