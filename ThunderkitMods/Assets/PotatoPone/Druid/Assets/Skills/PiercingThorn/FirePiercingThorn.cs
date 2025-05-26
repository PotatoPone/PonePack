using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using EntityStates;
using RoR2.Projectile;

namespace Druid.EntityStates
{
    public class FirePiercingThorn : GenericProjectileBaseState
    {
        private static int FireFMJStateHash = Animator.StringToHash("FireFMJ");
        private static int FireFMJParamHash = Animator.StringToHash("FireFMJ.playbackRate");

        public override void PlayAnimation(float duration)
        {
            base.PlayAnimation(duration);
            //if (base.GetModelAnimator())
            //{
            //    base.PlayAnimation("Gesture, Additive", FireFMJ.FireFMJStateHash, FireFMJ.FireFMJParamHash, duration);
            //    base.PlayAnimation("Gesture, Override", FireFMJ.FireFMJStateHash, FireFMJ.FireFMJParamHash, duration);
            //}
        }

        public override void ModifyProjectileInfo(ref FireProjectileInfo fireProjectileInfo)
        {
            DamageTypeCombo damageTypeCombo = new DamageTypeCombo();
            damageTypeCombo.damageType = DamageType.Generic;
            damageTypeCombo.damageTypeExtended = DamageTypeExtended.Generic;
            damageTypeCombo.damageSource = DamageSource.Primary;

            fireProjectileInfo.damageTypeOverride = damageTypeCombo;
        }

        public override Ray ModifyProjectileAimRay(Ray aimRay)
        {
            TrajectoryAimAssist.ApplyTrajectoryAimAssist(ref aimRay, this.projectilePrefab, base.gameObject, 1f);
            return aimRay;
        }
    }
}
