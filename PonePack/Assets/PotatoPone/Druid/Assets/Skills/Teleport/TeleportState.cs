using EntityStates;
using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Druid.EntityStates
{
    public class TeleportState : BaseSkillState
    {
        //public static float barrierCoefficient = 0.25f;
        public static float healFraction = 0.5f;

        private DruidTracker druidTracker;
        private HurtBox target;
        private float stopwatch;
        private float baseDuration = 0f;

        public override void OnEnter()
        {
            base.OnEnter();
            this.druidTracker = base.GetComponent<DruidTracker>();

            if (this.druidTracker && base.isAuthority)
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
            Vector3 targetUp = this.target.transform.up;
            float distanceFromTarget = 2f;
            Vector3 teleportPosition = this.target.transform.position + (targetUp * distanceFromTarget); //Teleport 2 units away from the top of the target
            TeleportHelper.TeleportBody(base.characterBody, teleportPosition, true);

            if (this.target.healthComponent)
            {
                this.target.healthComponent.HealFraction(healFraction, default(ProcChainMask));
                healthComponent.HealFraction(healFraction, default(ProcChainMask));

                //float allyBarrierAmount = this.target.healthComponent.body.maxBarrier * barrierCoefficient;
                //this.target.healthComponent.AddBarrier(allyBarrierAmount);

                //float selfBarrierAmount = healthComponent.body.maxBarrier * barrierCoefficient;
                //healthComponent.AddBarrier(selfBarrierAmount);
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
