using RoR2;
using RoR2.Skills;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Druid
{
    [RequireComponent(typeof(CharacterBody))]
    [RequireComponent(typeof(InputBankTest))]
    [RequireComponent(typeof(TeamComponent))]
    public class DruidTracker : NetworkBehaviour
    {
        public GameObject trackingPrefab;
        public float maxTrackingDistance = 500f;
        public float maxTrackingAngle = 20f;
        public float trackerUpdateFrequency = 10f;

        private HurtBox trackingTarget;
        private CharacterBody characterBody;
        private TeamComponent teamComponent;
        private InputBankTest inputBank;
        private float trackerUpdateStopwatch;
        private Indicator indicator;
        private readonly BullseyeSearch search = new BullseyeSearch();

        private bool teleportDisabled = false;

        private void Awake()
        {
            if (this.trackingPrefab == null)
            {
                this.trackingPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/HuntressTrackingIndicator");
            }
            this.indicator = new Indicator(base.gameObject, this.trackingPrefab);
        }

        private void Start()
        {
            this.characterBody = base.GetComponent<CharacterBody>();
            this.inputBank = base.GetComponent<InputBankTest>();
            this.teamComponent = base.GetComponent<TeamComponent>();
        }

        public HurtBox GetTrackingTarget()
        {
            return this.trackingTarget;
        }

        private void OnEnable()
        {
            this.indicator.active = true;
        }

        private void OnDisable()
        {
            this.indicator.active = false;
        }

        private void FixedUpdate()
        {
            this.MyFixedUpdate(Time.fixedDeltaTime);
        }

        private void MyFixedUpdate(float deltaTime)
        {
            this.trackerUpdateStopwatch += deltaTime;
            if (this.trackerUpdateStopwatch >= 1f / this.trackerUpdateFrequency)
            {
                this.trackerUpdateStopwatch -= 1f / this.trackerUpdateFrequency;
                HurtBox hurtBox = this.trackingTarget;
                Ray aimRay = new Ray(this.inputBank.aimOrigin, this.inputBank.aimDirection);
                this.SearchForTarget(aimRay);
                this.indicator.targetTransform = (this.trackingTarget ? this.trackingTarget.transform : null);
            }

            if (base.hasAuthority)
            {
                SetTeleportSkill();
            }
        }

        private void SetTeleportSkill()
        {
            if (!this.characterBody.skillLocator.utility) return;

            //Disable the teleport skill
            if (this.characterBody.skillLocator.utility.skillDef.skillName == "DruidTeleport" || teleportDisabled == true)
            {
                DruidTeleportSkillDef teleportSkill = (DruidTeleportSkillDef)this.characterBody.skillLocator.utility.skillDef;

                if (this.trackingTarget)
                {
                    teleportSkill.SetHasTarget(true);
                    teleportDisabled = false;
                }
                else
                {
                    teleportSkill.SetHasTarget(false);
                    teleportDisabled = true;
                }
            }
        }

        private void SearchForTarget(Ray aimRay)
        {
            this.search.teamMaskFilter = TeamMask.none;
            this.search.teamMaskFilter.AddTeam(TeamIndex.Player);
            this.search.filterByLoS = true;
            this.search.searchOrigin = aimRay.origin;
            this.search.searchDirection = aimRay.direction;
            this.search.sortMode = BullseyeSearch.SortMode.Distance;
            this.search.maxDistanceFilter = this.maxTrackingDistance;
            this.search.maxAngleFilter = this.maxTrackingAngle;
            this.search.RefreshCandidates();
            this.search.FilterOutGameObject(base.gameObject);
            this.trackingTarget = this.search.GetResults().FirstOrDefault<HurtBox>();
        }
    }
}
