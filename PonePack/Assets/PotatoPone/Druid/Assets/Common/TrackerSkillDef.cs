using RoR2.Skills;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using JetBrains.Annotations;

namespace Druid
{
    [CreateAssetMenu(menuName = "PotatoPone/TrackerSkillDef")]
    public class TrackerSkillDef : SkillDef
    {
        private bool hasTarget = false;

        public void SetHasTarget(bool hasTarget)
        {
            this.hasTarget = hasTarget;
        }

        public override bool CanExecute([NotNull] GenericSkill skillSlot)
        {
            if (hasTarget == false)
            {
                return false;
            }
            return base.CanExecute(skillSlot);
        }

        public override bool IsReady([NotNull] GenericSkill skillSlot)
        {
            if (hasTarget == false)
            {
                return false;
            }
            return base.IsReady(skillSlot);
        }
    }
}
