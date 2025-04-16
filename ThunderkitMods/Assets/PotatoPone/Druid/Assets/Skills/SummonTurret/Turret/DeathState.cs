using EntityStates;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;

namespace Druid.EntityStates.DruidTurret
{
    public class DeathState : GenericCharacterDeath
    {

        public static float explosionRadius = 20f;
        public static float explosionProcCoefficient = 1;
        public static float explosionForce = 200f;

        [SerializeField]
        public GameObject initialExplosion;
        [SerializeField]
        public GameObject deathExplosion;
        private float deathDuration;

        private DruidTurretController druidTurretController;

        public override void PlayDeathAnimation(float crossfadeDuration = 0.1f)
        {
            Animator modelAnimator = base.GetModelAnimator();
            if (modelAnimator)
            {
                int layerIndex = modelAnimator.GetLayerIndex("Body");
                modelAnimator.PlayInFixedTime("Death", layerIndex);
                modelAnimator.Update(0f);
                this.deathDuration = modelAnimator.GetCurrentAnimatorStateInfo(layerIndex).length;
                if (this.initialExplosion)
                {
                    UnityEngine.Object.Instantiate<GameObject>(this.initialExplosion, base.transform.position, base.transform.rotation, base.transform);
                }
            }
        }

        public override bool shouldAutoDestroy
        {
            get
            {
                //return true;
                return false;
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge > this.deathDuration && NetworkServer.active)
            {
                if (this.deathExplosion)
                {
                    //EffectManager.SpawnEffect(this.deathExplosion, new EffectData
                    //{
                    //    origin = base.transform.position,
                    //    scale = 2f
                    //}, true);
                }

                Explode();
                EntityState.Destroy(base.gameObject);
            }
        }

        public override void OnEnter()
        {
            druidTurretController = base.gameObject.GetComponent<DruidTurretController>();

            base.OnEnter();
        }

        public override void OnExit()
        {
            base.DestroyModel();
            base.OnExit();
        }

        private void Explode()
        {
            float storedDamage = druidTurretController.GetStoredDamage();
            Debug.LogWarning("Damaging nearby enemies for " + storedDamage);

            BlastAttack blastAttack = new BlastAttack();
            blastAttack.radius = explosionRadius;
            blastAttack.procCoefficient = explosionProcCoefficient;
            blastAttack.position = base.transform.position;
            blastAttack.attacker = base.gameObject;
            blastAttack.crit = Util.CheckRoll(base.characterBody.crit, base.characterBody.master);
            blastAttack.baseDamage = storedDamage;
            blastAttack.falloffModel = BlastAttack.FalloffModel.None;
            blastAttack.damageType = DamageType.AOE;
            blastAttack.baseForce = explosionForce;
            blastAttack.teamIndex = TeamComponent.GetObjectTeam(base.gameObject);
            blastAttack.attackerFiltering = AttackerFiltering.NeverHitSelf;
            blastAttack.damageType.damageSource = DamageSource.Hazard;
            blastAttack.Fire();

            //List<HurtBox> foundHurtBoxes = new List<HurtBox>();

            //SphereSearch sphereSearch = new SphereSearch();
            //sphereSearch.mask = LayerIndex.entityPrecise.mask;
            //sphereSearch.origin = base.transform.position;
            //sphereSearch.radius = damageRadius;
            //sphereSearch.queryTriggerInteraction = QueryTriggerInteraction.UseGlobal;
            //sphereSearch.RefreshCandidates();
            //sphereSearch.FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(TeamComponent.GetObjectTeam(base.gameObject)));
            //sphereSearch.OrderCandidatesByDistance();
            //sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
            //sphereSearch.GetHurtBoxes(foundHurtBoxes);
            //sphereSearch.ClearCandidates();

            //DamageInfo damageInfo = new DamageInfo();
            //damageInfo.damage = storedDamage;

            //foreach (HurtBox hurtBox in foundHurtBoxes)
            //{
            //    if (!hurtBox.healthComponent) continue;
            //    if (hurtBox.healthComponent.alive == false) continue;

            //    //Add HealingFromDyingTurret ProcChainMask
            //    //hurtBox.healthComponent.Heal(storedHealing, default(ProcChainMask), true);
            //    hurtBox.healthComponent.TakeDamage();
            //}
        }
    }
}
