using EntityStates;
using RoR2;
using RoR2.Projectile;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace PonePack.EntityStates.Hazel.HazelTurret
{
    public class WaitForStick : BaseState
    {
        private protected ProjectileStickOnImpact projectileStickOnImpact { get; private set; }
        //private protected EntityStateMachine armingStateMachine { get; private set; }

        public override void OnEnter()
        {
            base.OnEnter();
            this.projectileStickOnImpact = base.GetComponent<ProjectileStickOnImpact>();
            //this.armingStateMachine = EntityStateMachine.FindByCustomName(base.gameObject, "Arming");
            if (this.projectileStickOnImpact.enabled != true)
            {
                this.projectileStickOnImpact.enabled = true;
            }
            //Util.PlaySound(this.enterSoundString, base.gameObject);

            //if (NetworkServer.active)
            //{
            //    armingStateMachine.SetNextState(new MineArmingUnarmed());
            //}
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            //if (NetworkServer.active && this.shouldRevertToWaitForStickOnSurfaceLost && !this.projectileStickOnImpact.stuck)
            //{
            //    this.outer.SetNextState(new WaitForStickState());
            //}

            //Projectile has stuck and should spawn the turret
            if (NetworkServer.active && projectileStickOnImpact.stuck)
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
