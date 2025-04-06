using RoR2;
using RoR2.Projectile;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PonePack
{
    public class HealingBombController : MonoBehaviour
    {
        ProjectileDamage projectileDamage;
        ProjectileImpactExplosion projectileImpactExplosion;

        public float healingCoefficient = 1f;

        private void Awake()
        {
            this.projectileDamage = base.GetComponent<ProjectileDamage>();
            this.projectileImpactExplosion = base.GetComponent<ProjectileImpactExplosion>();
        }

        public void HealNerbyAllies(ProjectileImpactInfo impactInfo)
        {
            Debug.Log("Impact");

            List<HurtBox> foundHurtBoxes = new List<HurtBox>();
            SphereSearch sphereSearch = new SphereSearch();
            sphereSearch.mask = LayerIndex.entityPrecise.mask;
            sphereSearch.origin = this.transform.position;
            sphereSearch.radius = this.projectileImpactExplosion.blastRadius;
            sphereSearch.queryTriggerInteraction = QueryTriggerInteraction.UseGlobal;
            sphereSearch.RefreshCandidates();
            TeamMask teamMask = TeamMask.none;
            teamMask.AddTeam(TeamIndex.Player);
            sphereSearch.FilterCandidatesByHurtBoxTeam(teamMask);
            sphereSearch.OrderCandidatesByDistance();
            sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
            sphereSearch.GetHurtBoxes(foundHurtBoxes);
            sphereSearch.ClearCandidates();

            foreach (HurtBox hurtBox in foundHurtBoxes)
            {
                if (!hurtBox.healthComponent) continue;
                float healAmount = this.projectileDamage.damage * this.healingCoefficient;
                Debug.Log("Healing " + hurtBox.gameObject + " for " + healAmount);
                hurtBox.healthComponent.Heal(healAmount, default(ProcChainMask), true);
                //hurtBox.healthComponent.HealFraction(0.1f, default(ProcChainMask));
            }
        }
    }
}
