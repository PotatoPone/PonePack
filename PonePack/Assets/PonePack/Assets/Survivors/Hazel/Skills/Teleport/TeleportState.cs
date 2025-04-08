using EntityStates;
using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PonePack.EntityStates.Hazel
{
    public class TeleportState : BaseSkillState
    {
        public static float barrierCoefficient = 0.25f;

        private HazelTracker hazelTracker;
        private HurtBox target;
        private float stopwatch;
        private float baseDuration = 0.25f;

        public override void OnEnter()
        {
            base.OnEnter();
            this.hazelTracker = base.GetComponent<HazelTracker>();

            if (this.hazelTracker && base.isAuthority)
            {
                this.target = this.hazelTracker.GetTrackingTarget();

                if (this.target)
                {
                    Teleport();
                }
            }
        }

        private void Teleport()
        {
            TeleportHelper.TeleportBody(base.characterBody, this.target.transform.position, true);

            if (this.target.healthComponent)
            {
                float barrierAmount = this.target.healthComponent.body.maxBarrier * barrierCoefficient;
                this.target.healthComponent.AddBarrier(barrierAmount);
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
