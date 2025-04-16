using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using R2API;
using System.Runtime.Serialization;
using RoR2.Skills;
using Assets.RoR2.Scripts.Platform;
using On;
using System;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;
using RoR2.Projectile;

namespace Druid
{
    public class DruidSurvivor : DruidContent
    {
        public static SurvivorDef survivorDef;

        //Healing Bomb
        public static GameObject healingBombProjectile;

        //Turret
        public static GameObject turretMaster;
        public static GameObject turretBody;
        public static GameObject turretProjectile;
        public static DeployableSlot turretDeployableSlot;

        //Skills
        public static SkillFamily primarySkillFamily;
        public static SkillFamily secondarySkillFamily;
        public static SkillFamily utilitySkillFamily;
        public static SkillFamily specialSkillFamily;
        public static SkillFamily turretPrimarySkillFamily;

        public static SkillDef turretSkillDef;

        public static void Initialize()
        {
            Debug.Log("Initializing Druid...");

            //Add survivorDef to ContentPack
            survivorDef = DruidBundle.LoadAsset<SurvivorDef>("DruidSurvivorDef");
            survivorDef.cachedName = "PotatoPone_Druid";
            DruidContentPack.survivorDefs.Add(new SurvivorDef[] { survivorDef });

            //Set the bodyPrefab's hurtbox layer and group
            HurtBoxGroup bodyPrefabHurboxGroup = survivorDef.bodyPrefab.GetComponent<HurtBoxGroup>();
            bodyPrefabHurboxGroup.mainHurtBox.gameObject.layer = LayerIndex.entityPrecise.intVal;
            bodyPrefabHurboxGroup.mainHurtBox.GetComponent<HurtBox>().hurtBoxGroup = bodyPrefabHurboxGroup;

            //Add bodyPrefab to ContentPack
            survivorDef.bodyPrefab.RegisterNetworkPrefab(); //Network register test
            //survivorDef.bodyPrefab.GetComponent<CharacterBody>().AddBuff(RoR2.JunkContent.Buffs.IgnoreFallDamage); //Immune to fall damage
            DruidContentPack.bodyPrefabs.Add(new GameObject[] { survivorDef.bodyPrefab });

            //Skills
            InitHealingBomb();
            InitSummonTurret();
            InitTeleport();
            InitSummon();

            //Skill Families
            primarySkillFamily = DruidBundle.LoadAsset<SkillFamily>("sfDruidPrimary");
            AddSkillFamily(primarySkillFamily);

            secondarySkillFamily = DruidBundle.LoadAsset<SkillFamily>("sfDruidSecondary");
            AddSkillFamily(secondarySkillFamily);

            utilitySkillFamily = DruidBundle.LoadAsset<SkillFamily>("sfDruidUtility");
            AddSkillFamily(utilitySkillFamily);

            specialSkillFamily = DruidBundle.LoadAsset<SkillFamily>("sfDruidSpecial");
            AddSkillFamily(specialSkillFamily);
        }

        private static void InitHealingBomb()
        {
            healingBombProjectile = DruidBundle.LoadAsset<GameObject>("HealingBombProjectile");
            healingBombProjectile.layer = LayerIndex.projectile.intVal;
            DruidContentPack.projectilePrefabs.Add(new GameObject[] { healingBombProjectile });

            DruidContentPack.entityStateTypes.Add(new System.Type[] { typeof(Druid.EntityStates.FireHealingBomb) });
            DruidContentPack.entityStateConfigurations.Add(new EntityStateConfiguration[] { DruidBundle.LoadAsset<EntityStateConfiguration>("Druid.EntityStates.FireHealingBomb") });
        }

        private static void InitSummonTurret()
        {
            //Add TurretMaster
            turretMaster = DruidBundle.LoadAsset<GameObject>("TurretMaster");
            DruidContentPack.masterPrefabs.Add(new GameObject[] { turretMaster });

            //Add TurretBody
            turretBody = DruidBundle.LoadAsset<GameObject>("TurretBody");
            turretBody.layer = LayerIndex.playerFakeActor.intVal;
            turretBody.GetComponent<ModelLocator>().modelTransform.GetComponent<HurtBoxGroup>().mainHurtBox.gameObject.layer = LayerIndex.entityPrecise.intVal; //Set the turretBody's hurtbox layer
            DruidContentPack.bodyPrefabs.Add(new GameObject[] { turretBody });

            //Add TurretProjectile
            turretProjectile = DruidBundle.LoadAsset<GameObject>("TurretProjectile");
            turretProjectile.layer = LayerIndex.projectile.intVal;
            DruidContentPack.projectilePrefabs.Add(new GameObject[] { turretProjectile });

            //Register new deployable
            DeployableAPI.GetDeployableSameSlotLimit deployableSameSlotLimit = delegate (CharacterMaster master, int intVal)
            {
                int baseStock = secondarySkillFamily.variants[0].skillDef.baseMaxStock;
                int backupMagCount = master.inventory.GetItemCount(RoR2.RoR2Content.Items.SecondarySkillMagazine);
                int newStock = baseStock + backupMagCount;
                return newStock;
            };
            turretDeployableSlot = DeployableAPI.RegisterDeployableSlot(deployableSameSlotLimit);

            //Add EntityStates
            DruidContentPack.entityStateTypes.Add(new System.Type[] { typeof(Druid.EntityStates.FireSummonTurret) });
            DruidContentPack.entityStateTypes.Add(new System.Type[] { typeof(Druid.EntityStates.WaitForStick) });
            DruidContentPack.entityStateTypes.Add(new System.Type[] { typeof(Druid.EntityStates.DeployTurret) });
            DruidContentPack.entityStateTypes.Add(new System.Type[] { typeof(Druid.EntityStates.DruidTurret.SpawnState) });
            DruidContentPack.entityStateTypes.Add(new System.Type[] { typeof(Druid.EntityStates.DruidTurret.DeathState) });
            DruidContentPack.entityStateTypes.Add(new System.Type[] { typeof(Druid.EntityStates.DruidTurret.FireTurret) });

            //Add EntityStateConfigs
            DruidContentPack.entityStateConfigurations.Add(new EntityStateConfiguration[] { DruidBundle.LoadAsset<EntityStateConfiguration>("Druid.EntityStates.FireSummonTurret") });
            DruidContentPack.entityStateConfigurations.Add(new EntityStateConfiguration[] { DruidBundle.LoadAsset<EntityStateConfiguration>("Druid.EntityStates.DruidTurret.FireTurret") });
            DruidContentPack.entityStateConfigurations.Add(new EntityStateConfiguration[] { DruidBundle.LoadAsset<EntityStateConfiguration>("Druid.EntityStates.DruidTurret.SpawnState") });
            DruidContentPack.entityStateConfigurations.Add(new EntityStateConfiguration[] { DruidBundle.LoadAsset<EntityStateConfiguration>("Druid.EntityStates.DruidTurret.DeathState") });

            //Add SkillFamily
            turretPrimarySkillFamily = DruidBundle.LoadAsset<SkillFamily>("TurretPrimaryFamily");
            AddSkillFamily(turretPrimarySkillFamily);
        }

        private static void InitTeleport()
        {
            //Add EntityStates
            DruidContentPack.entityStateTypes.Add(new System.Type[] { typeof(Druid.EntityStates.TeleportState) });

            //Add EntityStateConfigs
            DruidContentPack.entityStateConfigurations.Add(new EntityStateConfiguration[] { DruidBundle.LoadAsset<EntityStateConfiguration>("Druid.EntityStates.TeleportState") });
        }

        private static void InitSummon()
        {

        }

        private static void AddSkillFamily(SkillFamily skillFamily)
        {
            foreach (SkillFamily.Variant skillFamilyVariant in skillFamily.variants)
            {
                DruidContentPack.skillDefs.Add(new SkillDef[] { skillFamilyVariant.skillDef });
            }
        }
    }
}
