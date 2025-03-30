using EntityStates;
using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PonePack.EntityStates.Hazel.HazelTurret
{
    public class FireHazelTurret : BaseState
    {
        public static GameObject effectPrefab;
        public static GameObject hitEffectPrefab;
        public static GameObject tracerEffectPrefab;
        public static string attackSoundString;
        public static float damageCoefficient;
        public static float force;
        public static float minSpread;
        public static float maxSpread;
        public static int bulletCount;
        public static float baseDuration = 2f;
        public int bulletCountCurrent = 1;
        private float duration;
        private static int FireGaussStateHash = Animator.StringToHash("FireGauss");
        private static int FireGaussParamHash = Animator.StringToHash("FireGauss.playbackRate");

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = FireHazelTurret.baseDuration / this.attackSpeedStat;
            Util.PlaySound(FireHazelTurret.attackSoundString, base.gameObject);
            Ray aimRay = base.GetAimRay();
            base.StartAimMode(aimRay, 2f, false);
            base.PlayAnimation("Gesture", FireHazelTurret.FireGaussStateHash, FireHazelTurret.FireGaussParamHash, this.duration);
            string muzzleName = "Muzzle";
            if (FireHazelTurret.effectPrefab)
            {
                EffectManager.SimpleMuzzleFlash(FireHazelTurret.effectPrefab, base.gameObject, muzzleName, false);
            }
            if (base.isAuthority)
            {
                BulletAttack bulletAttack = new BulletAttack();
                bulletAttack.owner = base.gameObject;
                bulletAttack.weapon = base.gameObject;
                bulletAttack.origin = aimRay.origin;
                bulletAttack.aimVector = aimRay.direction;
                bulletAttack.minSpread = FireHazelTurret.minSpread;
                bulletAttack.maxSpread = FireHazelTurret.maxSpread;
                bulletAttack.bulletCount = 1U;
                bulletAttack.damage = FireHazelTurret.damageCoefficient * this.damageStat;
                bulletAttack.force = FireHazelTurret.force;
                bulletAttack.tracerEffectPrefab = FireHazelTurret.tracerEffectPrefab;
                bulletAttack.muzzleName = muzzleName;
                bulletAttack.hitEffectPrefab = FireHazelTurret.hitEffectPrefab;
                bulletAttack.isCrit = Util.CheckRoll(this.critStat, base.characterBody.master);
                bulletAttack.HitEffectNormal = false;
                bulletAttack.radius = 0.15f;
                bulletAttack.damageType.damageSource = DamageSource.Primary;
                bulletAttack.Fire();

                Debug.Log("Fired with damage: " + bulletAttack.damage);
                Debug.Log("damageCoefficient: " + FireHazelTurret.damageCoefficient);
                Debug.Log("damageStat: " + this.damageStat);
                Debug.Log("characterBody.damage: " + this.characterBody.damage);
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge >= this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}