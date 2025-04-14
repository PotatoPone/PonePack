using RoR2;
using RoR2.Items;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using BepInEx;
using R2API;
using System;
using System.Linq;
using IL.RoR2.DirectionalSearch;

namespace PonePack
{
    public class BaseModdedEquipment : CharacterBody.ItemBehavior
    {
        public CharacterBody characterBody;
        public TeamComponent teamComponent;
        public BullseyeSearch targetFinder = new BullseyeSearch();
        //public PickupSearch pickupSearch;
        public InputBankTest inputBank;

        public UserTargetInfo currentTarget;
        public Indicator targetIndicator;

        public struct UserTargetInfo
        {
            public UserTargetInfo(HurtBox source)
            {
                this.hurtBox = source;
                this.rootObject = (this.hurtBox ? this.hurtBox.healthComponent.gameObject : null);
                this.pickupController = null;
                this.transformToIndicateAt = (this.hurtBox ? this.hurtBox.transform : null);
            }

            public UserTargetInfo(GenericPickupController source)
            {
                this.pickupController = source;
                this.hurtBox = null;
                this.rootObject = (this.pickupController ? this.pickupController.gameObject : null);
                this.transformToIndicateAt = (this.pickupController ? this.pickupController.pickupDisplay.transform : null);
            }

            public readonly HurtBox hurtBox;
            public readonly GameObject rootObject;
            public readonly GenericPickupController pickupController;
            public readonly Transform transformToIndicateAt;
        }

        //private GenericPickupController FindPickupController(Ray aimRay, float maxAngle, float maxDistance, bool requireLoS, bool requireTransmutable)
        //{
        //    if (this.pickupSearch == null)
        //    {
        //        this.pickupSearch = new PickupSearch();
        //    }
        //    float num;
        //    aimRay = CameraRigController.ModifyAimRayIfApplicable(aimRay, base.gameObject, out num);
        //    this.pickupSearch.searchOrigin = aimRay.origin;
        //    this.pickupSearch.searchDirection = aimRay.direction;
        //    this.pickupSearch.minAngleFilter = 0f;
        //    this.pickupSearch.maxAngleFilter = maxAngle;
        //    this.pickupSearch.minDistanceFilter = 0f;
        //    this.pickupSearch.maxDistanceFilter = maxDistance + num;
        //    this.pickupSearch.filterByDistinctEntity = false;
        //    this.pickupSearch.filterByLoS = requireLoS;
        //    this.pickupSearch.sortMode = SortMode.DistanceAndAngle;
        //    this.pickupSearch.requireTransmutable = requireTransmutable;
        //    return this.pickupSearch.SearchCandidatesForSingleTarget<List<GenericPickupController>>(InstanceTracker.GetInstancesList<GenericPickupController>());
        //}

        private void Start()
        {
            this.characterBody = base.GetComponent<CharacterBody>();
            this.teamComponent = base.GetComponent<TeamComponent>();
            this.inputBank = base.GetComponent<InputBankTest>();
            this.targetIndicator = new Indicator(base.gameObject, null);
        }

        private void OnDestroy()
        {
            if (this.targetIndicator != null)
            {
                this.targetIndicator.active = false;
            }
        }

        private Ray GetAimRay()
        {
            return new Ray
            {
                direction = this.inputBank.aimDirection,
                origin = this.inputBank.aimOrigin
            };
        }

        private void ConfigureTargetFinderBase()
        {
            this.targetFinder.teamMaskFilter = TeamMask.allButNeutral;
            this.targetFinder.teamMaskFilter.RemoveTeam(this.teamComponent.teamIndex);
            this.targetFinder.sortMode = BullseyeSearch.SortMode.Angle;
            this.targetFinder.filterByLoS = true;
            float num;
            Ray ray = CameraRigController.ModifyAimRayIfApplicable(this.GetAimRay(), base.gameObject, out num);
            this.targetFinder.searchOrigin = ray.origin;
            this.targetFinder.searchDirection = ray.direction;
            this.targetFinder.maxAngleFilter = 10f;
            this.targetFinder.viewer = this.characterBody;
        }

        private void ConfigureTargetFinderForFriendlies()
        {
            this.ConfigureTargetFinderBase();
            this.targetFinder.teamMaskFilter = TeamMask.none;
            this.targetFinder.teamMaskFilter.AddTeam(this.teamComponent.teamIndex);
            this.targetFinder.RefreshCandidates();
            this.targetFinder.FilterOutGameObject(base.gameObject);
        }

        private void ConfigureTargetFinderForEnemies()
        {
            this.ConfigureTargetFinderBase();
            this.targetFinder.teamMaskFilter = TeamMask.GetUnprotectedTeams(this.teamComponent.teamIndex);
            this.targetFinder.RefreshCandidates();
            this.targetFinder.FilterOutGameObject(base.gameObject);
        }

        public void UpdateTargets(EquipmentIndex targetingEquipmentIndex, bool userShouldAnticipateTarget)
        {
            bool configureForBosses = targetingEquipmentIndex == DLC1Content.Equipment.BossHunter.equipmentIndex;
            bool configureForEnemies = (targetingEquipmentIndex == RoR2Content.Equipment.Lightning.equipmentIndex || targetingEquipmentIndex == JunkContent.Equipment.SoulCorruptor.equipmentIndex || configureForBosses) && userShouldAnticipateTarget;
            bool configureForFriendlies = targetingEquipmentIndex == PonePack.Equipment.ShareItemPool.equipmentIndex && userShouldAnticipateTarget;
            bool configureForCharacters = configureForEnemies || configureForFriendlies;
            bool configureForItems = targetingEquipmentIndex == RoR2Content.Equipment.Recycle.equipmentIndex;
            if (configureForCharacters)
            {
                if (configureForEnemies)
                {
                    this.ConfigureTargetFinderForEnemies();
                }
                if (configureForFriendlies)
                {
                    this.ConfigureTargetFinderForFriendlies();
                }
                HurtBox source = null;
                if (configureForBosses)
                {
                    using (IEnumerator<HurtBox> enumerator = this.targetFinder.GetResults().GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            HurtBox hurtBox = enumerator.Current;
                            if (hurtBox && hurtBox.healthComponent && hurtBox.healthComponent.body)
                            {
                                DeathRewards component = hurtBox.healthComponent.body.gameObject.GetComponent<DeathRewards>();
                                if (component && component.bossDropTable && !hurtBox.healthComponent.body.HasBuff(RoR2Content.Buffs.Immune))
                                {
                                    source = hurtBox;
                                    break;
                                }
                            }
                        }
                        goto IL_134;
                    }
                }
                source = this.targetFinder.GetResults().FirstOrDefault<HurtBox>();
            IL_134:
                this.currentTarget = new UserTargetInfo(source);
            }
            else if (configureForItems)
            {
                //this.currentTarget = new UserTargetInfo(this.FindPickupController(this.GetAimRay(), 10f, 30f, true, targetingEquipmentIndex == RoR2Content.Equipment.Recycle.equipmentIndex));
            }
            else
            {
                this.currentTarget = default(UserTargetInfo);
            }

            GenericPickupController pickupController = this.currentTarget.pickupController;
            bool flag6 = this.currentTarget.transformToIndicateAt;

            if (this.targetIndicator == null) return;

            if (flag6)
            {
                if (targetingEquipmentIndex == RoR2Content.Equipment.Lightning.equipmentIndex)
                {
                    this.targetIndicator.visualizerPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/LightningIndicator");
                }
                else if (targetingEquipmentIndex == RoR2Content.Equipment.PassiveHealing.equipmentIndex)
                {
                    this.targetIndicator.visualizerPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/WoodSpriteIndicator");
                }
                else if (targetingEquipmentIndex == RoR2Content.Equipment.Recycle.equipmentIndex)
                {
                    if (!pickupController.Recycled)
                    {
                        this.targetIndicator.visualizerPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/RecyclerIndicator");
                    }
                    else
                    {
                        this.targetIndicator.visualizerPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/RecyclerBadIndicator");
                    }
                }
                else if (targetingEquipmentIndex == DLC1Content.Equipment.BossHunter.equipmentIndex)
                {
                    this.targetIndicator.visualizerPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/BossHunterIndicator");
                }
                else
                {
                    this.targetIndicator.visualizerPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/LightningIndicator");
                }
            }

            this.targetIndicator.active = flag6;
            this.targetIndicator.targetTransform = (flag6 ? this.currentTarget.transformToIndicateAt : null);
        }
    }
}