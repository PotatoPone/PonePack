using EntityStates;
using RoR2;
using RoR2.Projectile;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Druid.EntityStates
{
    public class WaitForStick : BaseState
    {
        private protected ProjectileStickOnImpact projectileStickOnImpact { get; private set; }

        public override void OnEnter()
        {
            base.OnEnter();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (NetworkServer.active)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        [SerializeField]
        public string enterSoundString;
    }
}
