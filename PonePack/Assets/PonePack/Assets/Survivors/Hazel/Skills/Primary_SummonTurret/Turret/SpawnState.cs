using EntityStates;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PonePack.EntityStates.Hazel.HazelTurret
{
    public class SpawnState : BaseState //GenericCharacterSpawnState ?
    {
        public static float duration = 1f;
        //private static int SpawnStateHash = Animator.StringToHash("Spawn");
        //private static int SpawnParamHash = Animator.StringToHash("Spawn.playbackRate");

        public override void OnEnter()
        {
            base.OnEnter();

            //if (base.GetModelAnimator())
            //{
            //    base.PlayAnimation("Body", SpawnState.SpawnStateHash, SpawnState.SpawnParamHash, 1.5f);
            //}
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= SpawnState.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }
    }
}
