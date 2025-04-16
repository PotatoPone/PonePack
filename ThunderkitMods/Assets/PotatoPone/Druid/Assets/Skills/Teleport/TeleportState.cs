using EntityStates;
using RoR2;
using RoR2.Projectile;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Druid.EntityStates
{
    public class TeleportState : BaseSkillState
    {
        //public static float barrierCoefficient = 0.25f;
        public static float healFraction = 0.5f;
        public static float healRadius = 20f;

        private DruidTracker druidTracker;
        private HurtBox target;
        private float stopwatch;
        private float baseDuration = 0f;
        private List<HurtBox> alliesToHeal = new List<HurtBox>();

        public override void OnEnter()
        {
            base.OnEnter();
            this.druidTracker = base.GetComponent<DruidTracker>();

            if (this.druidTracker)
            {
                this.target = this.druidTracker.GetTrackingTarget();

                if (this.target)
                {
                    Teleport();
                }
            }
        }

        private void Teleport()
        {
            if (base.isAuthority)
            {
                Vector3 targetUp = this.target.transform.up;
                float distanceFromTarget = 2f;
                Vector3 teleportPosition = this.target.transform.position + (targetUp * distanceFromTarget); //Teleport 2 units away from the top of the target

                TeleportHelper.TeleportBody(base.characterBody, teleportPosition, true);
            }

            if (this.target.healthComponent && NetworkServer.active)
            {
                //this.target.healthComponent.HealFraction(healFraction, default(ProcChainMask));
                healthComponent.HealFraction(healFraction, default(ProcChainMask));
                HealNerbyAllies(this.target.transform.position);
            }
        }

        public void HealNerbyAllies(Vector3 origin)
        {
            SphereSearch sphereSearch = new SphereSearch();
            sphereSearch.mask = LayerIndex.entityPrecise.mask;
            sphereSearch.origin = origin;
            sphereSearch.radius = healRadius;
            sphereSearch.queryTriggerInteraction = QueryTriggerInteraction.UseGlobal;
            sphereSearch.RefreshCandidates();
            TeamMask teamMask = TeamMask.none;
            teamMask.AddTeam(TeamComponent.GetObjectTeam(base.gameObject));
            sphereSearch.FilterCandidatesByHurtBoxTeam(teamMask);
            sphereSearch.OrderCandidatesByDistance();
            sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
            sphereSearch.GetHurtBoxes(alliesToHeal);
            sphereSearch.ClearCandidates();

            foreach (HurtBox hurtBox in alliesToHeal)
            {
                if (!hurtBox.healthComponent) continue;
                if (hurtBox.healthComponent == healthComponent) continue; //Don't heal yourself twice

                hurtBox.healthComponent.HealFraction(healFraction, default(ProcChainMask));
                Util.PlaySound("Play_item_proc_TPhealingNova_hitPlayer", hurtBox.healthComponent.body.gameObject);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            this.stopwatch += base.GetDeltaTime();

            if (this.stopwatch >= this.baseDuration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }
}
