using EntityStates;
using RoR2.Projectile;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using System;

namespace PonePack.EntityStates.Hazel
{
    public class FireHealingBomb : BaseSkillState
    {
        public static GameObject effectPrefab;
        public static GameObject hitEffectPrefab;
        public static float damageCoefficient = 1f;
        public static float healingCoefficient = 1f;
        public static float force = 200f;
        public static float baseDuration = 1f;

        private float duration;

        public override void OnEnter()
        {
            base.OnEnter();

            //On.RoR2.Projectile.ProjectileImpactExplosion.OnProjectileImpact += OnProjectileImpact;

            //if (base.GetModelAnimator())
            //{
            //    base.PlayAnimation("Body", SpawnState.SpawnStateHash, SpawnState.SpawnParamHash, 1.5f);
            //}
            Debug.Log("BaseDuration: " + FireHealingBomb.baseDuration);
            this.duration = FireHealingBomb.baseDuration / this.attackSpeedStat;
            Debug.Log("Duration: " + this.duration);
            Ray aimRay = base.GetAimRay();
            base.StartAimMode(aimRay, 2f, false);


            //if (base.GetModelAnimator())
            //{
            //    float num = this.duration * 0.3f;
            //    base.PlayCrossfade("Gesture, Additive", "FireMineRight", "FireMine.playbackRate", this.duration + num, 0.05f);
            //}
            //string muzzleName = "MuzzleCenter";
            //if (effectPrefab)
            //{
            //    EffectManager.SimpleMuzzleFlash(effectPrefab, base.gameObject, muzzleName, false);
            //}
            if (base.isAuthority)
            {
                Debug.Log("HealingBomb projectile fired!");
                FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                {
                    projectilePrefab = PonePack.Survivors.Hazel.hazelHealingBombProjectile,
                    position = aimRay.origin,
                    rotation = Util.QuaternionSafeLookRotation(aimRay.direction),
                    owner = base.gameObject,
                    damage = this.damageStat * FireHealingBomb.damageCoefficient,
                    force = FireHealingBomb.force,
                    crit = Util.CheckRoll(this.critStat, base.characterBody.master)
                };
                ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge > this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
