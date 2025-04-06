using EntityStates;
using RoR2;
using RoR2.Projectile;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PonePack.EntityStates.Hazel
{
    public class FireTurretSummon : BaseSkillState //GenericCharacterSpawnState ?
    {
        //private static int SpawnStateHash = Animator.StringToHash("Spawn");
        //private static int SpawnParamHash = Animator.StringToHash("Spawn.playbackRate");

        public static GameObject effectPrefab;
        public static GameObject hitEffectPrefab;
        [SerializeField]
        public GameObject projectilePrefab;
        [SerializeField]
        public float damageCoefficient = 1f;
        [SerializeField]
        public float force = 200f;
        public static float baseDuration = 1f;
        public static string throwMineSoundString;
        private float duration;

        public override void OnEnter()
        {
            base.OnEnter();

            //if (base.GetModelAnimator())
            //{
            //    base.PlayAnimation("Body", SpawnState.SpawnStateHash, SpawnState.SpawnParamHash, 1.5f);
            //}

            this.duration = baseDuration / this.attackSpeedStat;
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
                Debug.Log("SummonTurret projectile fired!");
                FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                {
                    projectilePrefab = PonePack.Survivors.Hazel.hazelTurretProjectile,
                    position = aimRay.origin,
                    rotation = Util.QuaternionSafeLookRotation(aimRay.direction),
                    owner = base.gameObject,
                    damage = this.damageStat * this.damageCoefficient,
                    force = this.force,
                    crit = Util.CheckRoll(this.critStat, base.characterBody.master)
                };
                ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            }
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
