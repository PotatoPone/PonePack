using EntityStates;
using RoR2.Projectile;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using System;

namespace PonePack.EntityStates.Hazel
{
    public class FireHealingBomb : GenericProjectileBaseState
    {
        public override void ModifyProjectileInfo(ref FireProjectileInfo fireProjectileInfo)
        {
            DamageTypeCombo damageTypeCombo = new DamageTypeCombo();
            damageTypeCombo.damageType = DamageType.Generic;
            damageTypeCombo.damageTypeExtended = DamageTypeExtended.Generic;
            damageTypeCombo.damageSource = DamageSource.Primary;

            fireProjectileInfo.damageTypeOverride = damageTypeCombo;
        }
    }
}
