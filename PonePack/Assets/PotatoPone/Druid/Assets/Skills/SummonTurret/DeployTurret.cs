using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using EntityStates;
using RoR2.Networking;
using UnityEngine.Events;
using UnityEngine.Networking;
using Unity;
using R2API.Networking.Interfaces;
using RoR2.Projectile;
using static UnityEngine.UI.GridLayoutGroup;

namespace Druid.EntityStates
{
    public class DeployTurret : BaseState
    {
        public static float baseDuration = 0.1f;

        private float duration;

        public override void OnEnter()
        {
            base.OnEnter();

            this.duration = DeployTurret.baseDuration / this.attackSpeedStat;
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
                this.outer.SetNextState(new Idle());
                return;
            }
        }
    }
}
