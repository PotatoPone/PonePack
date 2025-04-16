using EntityStates;
using RoR2;
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

        private DruidTracker druidTracker;
        private HurtBox target;
        private float stopwatch;
        private float baseDuration = 0f;

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
                this.target.healthComponent.HealFraction(healFraction, default(ProcChainMask));
                healthComponent.HealFraction(healFraction, default(ProcChainMask));
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
