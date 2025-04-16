using EntityStates;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Networking;
using RoR2.Orbs;
using RoR2.Projectile;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;


namespace Druid.EntityStates.DruidTurret
{
    public class FireTurret : BaseSkillState
    {
        [SerializeField]
        public float orbDamageCoefficient;
        [SerializeField]
        public float orbProcCoefficient;
        [SerializeField]
        public string muzzleString;
        [SerializeField]
        public GameObject muzzleflashEffectPrefab;
        [SerializeField]
        public string attackSoundString;
        [SerializeField]
        public float baseDuration;

        private float duration;
        protected bool isCrit;
        private HurtBox initialTarget;
        private ChildLocator childLocator;
        private Animator animator;
        private static int FireStateHash = Animator.StringToHash("Fire");
        private static int FireParamHash = Animator.StringToHash("Fire.playbackRate");
        private float maxDistance;
        //public static GameObject effectPrefab;
        //public static string attackSoundString;

        public override void OnEnter()
        {
            base.OnEnter();

            Transform modelTransform = base.GetModelTransform();
            if (modelTransform)
            {
                this.childLocator = modelTransform.GetComponent<ChildLocator>();
                this.animator = modelTransform.GetComponent<Animator>();
            }

            Util.PlayAttackSpeedSound(this.attackSoundString, base.gameObject, this.attackSpeedStat);

            this.duration = this.baseDuration / this.attackSpeedStat;

            if (base.characterBody)
            {
                base.characterBody.SetAimTimer(this.duration + 1f);
                this.SetSkillDriverMaxDistance();
            }

            base.PlayCrossfade("Body", FireTurret.FireStateHash, FireTurret.FireParamHash, this.duration, this.duration * 0.2f / this.attackSpeedStat);
            //base.PlayAnimation("Body", FireTurret.FireStateHash, FireTurret.FireParamHash, this.duration);

            this.isCrit = Util.CheckRoll(base.characterBody.crit, base.characterBody.master);

            this.initialTarget = this.GetNearestEnemy();
            this.FireOrb();
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        private void FireOrb()
        {
            if (!base.isAuthority) return;

            GenericDamageOrb genericDamageOrb = new DruidTurretOrb();
            genericDamageOrb.damageValue = base.characterBody.damage * this.orbDamageCoefficient;
            genericDamageOrb.isCrit = this.isCrit;
            genericDamageOrb.teamIndex = TeamComponent.GetObjectTeam(base.gameObject);
            genericDamageOrb.attacker = base.gameObject;
            genericDamageOrb.procCoefficient = this.orbProcCoefficient;
            genericDamageOrb.damageType.damageSource = DamageSource.Primary;
            HurtBox hurtBox = this.initialTarget;
            if (hurtBox)
            {
                //Transform transform = this.childLocator.FindChild(this.muzzleString);
                //EffectManager.SimpleMuzzleFlash(this.muzzleflashEffectPrefab, base.gameObject, this.muzzleString, true);
                //genericDamageOrb.origin = transform.position;
                genericDamageOrb.origin = base.GetAimRay().origin;
                genericDamageOrb.target = hurtBox;
                OrbManager.instance.AddOrb(genericDamageOrb);
            }

        }

        private HurtBox GetNearestEnemy()
        {
            List<HurtBox> foundHurtBoxes = new List<HurtBox>();

            SphereSearch sphereSearch = new SphereSearch();
            sphereSearch.mask = LayerIndex.entityPrecise.mask;
            sphereSearch.origin = base.transform.position;
            sphereSearch.radius = this.maxDistance;
            sphereSearch.queryTriggerInteraction = QueryTriggerInteraction.UseGlobal;
            sphereSearch.RefreshCandidates();
            sphereSearch.FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(TeamComponent.GetObjectTeam(base.gameObject)));
            sphereSearch.OrderCandidatesByDistance();
            sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
            sphereSearch.GetHurtBoxes(foundHurtBoxes);
            sphereSearch.ClearCandidates();

            return foundHurtBoxes.FirstOrDefault<HurtBox>();
        }

        private void SetSkillDriverMaxDistance()
        {
            AISkillDriver[] skillDrivers = base.characterBody.master.GetComponents<AISkillDriver>();
            foreach (AISkillDriver skillDriver in skillDrivers)
            {
                if (skillDriver.customName == "FireAtEnemy")
                {
                    this.maxDistance = skillDriver.maxDistance;
                    return;
                }
            }
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
            return InterruptPriority.PrioritySkill;
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            writer.Write(HurtBoxReference.FromHurtBox(this.initialTarget));
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            this.initialTarget = reader.ReadHurtBoxReference().ResolveHurtBox();
        }
    }
}