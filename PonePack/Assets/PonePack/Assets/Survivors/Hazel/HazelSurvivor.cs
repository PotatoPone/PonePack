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

namespace PonePack.Survivors
{
    public class Hazel : PonePackContent
    {
        public static SurvivorDef survivorDef;

        //Healing Bomb
        public static GameObject hazelHealingBombProjectile;

        //Turret
        public static GameObject hazelTurretMaster;
        public static GameObject hazelTurretBody;
        public static GameObject hazelTurretProjectile;
        public static DeployableSlot hazelTurretDeployableSlot;

        //Skills
        public static SkillFamily primarySkillFamily;
        public static SkillFamily secondarySkillFamily;
        public static SkillFamily utilitySkillFamily;
        public static SkillFamily specialSkillFamily;

        public static SkillDef hazelTurretSkillDef;

        public static void Initialize(AssetBundle bundle)
        {
            Debug.Log("!!! Initializing Hazel...");

            //Add survivorDef to ContentPack
            survivorDef = bundle.LoadAsset<SurvivorDef>("HazelSurvivorDef");
            survivorDef.cachedName = "PotatoPone_Hazel";
            PonePackContentPack.survivorDefs.Add(new SurvivorDef[] { survivorDef });

            //Set the bodyPrefab's hurtbox layer and group
            HurtBoxGroup bodyPrefabHurboxGroup = survivorDef.bodyPrefab.GetComponent<HurtBoxGroup>();
            bodyPrefabHurboxGroup.mainHurtBox.gameObject.layer = LayerIndex.entityPrecise.intVal;
            bodyPrefabHurboxGroup.mainHurtBox.GetComponent<HurtBox>().hurtBoxGroup = bodyPrefabHurboxGroup;

            //Add bodyPrefab to ContentPack
            survivorDef.bodyPrefab.RegisterNetworkPrefab(); //Network register test
            PonePackContentPack.bodyPrefabs.Add(new GameObject[] { survivorDef.bodyPrefab });


            InitHealingBomb(bundle);
            InitTurret(bundle);
            InitTeleport(bundle);
            InitSummon(bundle);

            //Skills
            primarySkillFamily = bundle.LoadAsset<SkillFamily>("sfHazelPrimary");
            AddSkillFamily(primarySkillFamily);

            secondarySkillFamily = bundle.LoadAsset<SkillFamily>("sfHazelSecondary");
            AddSkillFamily(secondarySkillFamily);

            utilitySkillFamily = bundle.LoadAsset<SkillFamily>("sfHazelUtility");
            AddSkillFamily(utilitySkillFamily);

            specialSkillFamily = bundle.LoadAsset<SkillFamily>("sfHazelSpecial");
            AddSkillFamily(specialSkillFamily);

            hazelTurretSkillDef = bundle.LoadAsset<SkillDef>("HazelTurretPrimarySkill");
            PonePackContentPack.skillDefs.Add(new SkillDef[] { hazelTurretSkillDef });
        }

        private static void InitHealingBomb(AssetBundle bundle)
        {
            hazelHealingBombProjectile = bundle.LoadAsset<GameObject>("HazelHealingBombProjectile");
            hazelHealingBombProjectile.layer = LayerIndex.projectile.intVal;
            PonePackContentPack.projectilePrefabs.Add(new GameObject[] { hazelHealingBombProjectile });

            PonePackContentPack.entityStateTypes.Add(new System.Type[] { typeof(PonePack.EntityStates.Hazel.FireHealingBomb) });
        }

        private static void InitTurret(AssetBundle bundle)
        {
            //Add TurretMaster
            hazelTurretMaster = bundle.LoadAsset<GameObject>("HazelTurretMaster");
            PonePackContentPack.masterPrefabs.Add(new GameObject[] { hazelTurretMaster });

            //Add TurretBody
            hazelTurretBody = bundle.LoadAsset<GameObject>("HazelTurretBody");
            hazelTurretBody.layer = LayerIndex.playerFakeActor.intVal;
            //hazelTurretBody.layer = LayerIndex.entityPrecise.intVal;
            hazelTurretBody.GetComponent<ModelLocator>().modelTransform.GetComponent<HurtBoxGroup>().mainHurtBox.gameObject.layer = LayerIndex.entityPrecise.intVal; //Set the turretBody's hurtbox layer
            PonePackContentPack.bodyPrefabs.Add(new GameObject[] { hazelTurretBody });

            //Add TurretProjectile
            hazelTurretProjectile = bundle.LoadAsset<GameObject>("HazelTurretProjectile");
            hazelTurretProjectile.layer = LayerIndex.projectile.intVal;
            PonePackContentPack.projectilePrefabs.Add(new GameObject[] { hazelTurretProjectile });

            //Register new deployable
            DeployableAPI.GetDeployableSameSlotLimit deployableSameSlotLimit = delegate (CharacterMaster master, int intVal)
            {
                int baseStock = secondarySkillFamily.variants[0].skillDef.baseMaxStock;
                int backupMagCount = master.inventory.GetItemCount(RoR2.RoR2Content.Items.SecondarySkillMagazine);
                int newStock = baseStock + backupMagCount;
                return newStock;
            };
            hazelTurretDeployableSlot = DeployableAPI.RegisterDeployableSlot(deployableSameSlotLimit);

            PonePackContentPack.entityStateTypes.Add(new System.Type[] { typeof(PonePack.EntityStates.Hazel.FireTurretSummon) });
            PonePackContentPack.entityStateTypes.Add(new System.Type[] { typeof(PonePack.EntityStates.Hazel.HazelTurret.DeathState) });
            PonePackContentPack.entityStateTypes.Add(new System.Type[] { typeof(PonePack.EntityStates.Hazel.HazelTurret.WaitForStick) });
            PonePackContentPack.entityStateTypes.Add(new System.Type[] { typeof(PonePack.EntityStates.Hazel.HazelTurret.DeployTurret) });
            PonePackContentPack.entityStateTypes.Add(new System.Type[] { typeof(PonePack.EntityStates.Hazel.HazelTurret.FireHazelTurret) });
        }

        private static void InitTeleport(AssetBundle bundle)
        {

        }

        private static void InitSummon(AssetBundle bundle)
        {

        }

        private static void AddSkillFamily(SkillFamily skillFamily)
        {
            foreach (SkillFamily.Variant skillFamilyVariant in skillFamily.variants)
            {
                PonePackContentPack.skillDefs.Add(new SkillDef[] { skillFamilyVariant.skillDef });
            }
        }
    }
}
