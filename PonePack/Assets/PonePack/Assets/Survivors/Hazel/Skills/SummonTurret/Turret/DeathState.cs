using EntityStates;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;

namespace PonePack.EntityStates.Hazel.HazelTurret
{
    public class DeathState : GenericCharacterDeath
    {

        public static float healRadius = 20f;

        [SerializeField]
        public GameObject initialExplosion;
        [SerializeField]
        public GameObject deathExplosion;
        private float deathDuration;

        private HazelTurretController hazelTurretController;

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

                HealNearbyTeammates();

                EntityState.Destroy(base.gameObject);
            }
        }

        public override void OnEnter()
        {
            hazelTurretController = base.gameObject.GetComponent<HazelTurretController>();

            base.OnEnter();
        }

        public override void OnExit()
        {
            base.DestroyModel();
            base.OnExit();
        }

        private void HealNearbyTeammates()
        {
            //float storedHealing = hazelTurretController.GetStoredHealing();
            float storedHealing = hazelTurretController.GetStoredDamage();
            Debug.LogWarning("Healing nearby allies for " + storedHealing);
            List<HurtBox> foundHurtBoxes = new List<HurtBox>();

            SphereSearch sphereSearch = new SphereSearch();
            sphereSearch.mask = LayerIndex.entityPrecise.mask;
            sphereSearch.origin = base.transform.position;
            sphereSearch.radius = DeathState.healRadius;
            sphereSearch.queryTriggerInteraction = QueryTriggerInteraction.UseGlobal;
            sphereSearch.RefreshCandidates();
            TeamMask teamMask = TeamMask.none;
            teamMask.AddTeam(TeamIndex.Player); //Change to the turret's team, for the possibility of an enemy Hazel
            sphereSearch.FilterCandidatesByHurtBoxTeam(teamMask);
            sphereSearch.OrderCandidatesByDistance();
            sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
            sphereSearch.GetHurtBoxes(foundHurtBoxes);
            sphereSearch.ClearCandidates();

            foreach (HurtBox hurtBox in foundHurtBoxes)
            {
                if (!hurtBox.healthComponent) continue;
                if (hurtBox.healthComponent == base.healthComponent) continue; //Don't heal yourself
                if (hurtBox.healthComponent.alive == false) continue;

                //Add HealingFromDyingTurret ProcChainMask
                hurtBox.healthComponent.Heal(storedHealing, default(ProcChainMask), true);
            }
        }
    }
}
