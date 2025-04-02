using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using R2API;
using System.Runtime.Serialization;
using RoR2.Skills;
using Assets.RoR2.Scripts.Platform;

namespace PonePack.Survivors
{
    public class Hazel : PonePackContent
    {
        public static SurvivorDef survivorDef;

        public static GameObject hazelTurretMaster;
        public static GameObject hazelTurretBody;
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


            // Turret

            //Add TurretMaster
            hazelTurretMaster = bundle.LoadAsset<GameObject>("HazelTurretMaster");
            PonePackContentPack.masterPrefabs.Add(new GameObject[] { hazelTurretMaster });

            //Add TurretBody
            hazelTurretBody = bundle.LoadAsset<GameObject>("HazelTurretBody");
            hazelTurretBody.layer = LayerIndex.playerFakeActor.intVal;
            hazelTurretBody.GetComponent<ModelLocator>().modelTransform.GetComponent<HurtBoxGroup>().mainHurtBox.gameObject.layer = LayerIndex.entityPrecise.intVal; //Set the turretBody's hurtbox layer
            PonePackContentPack.bodyPrefabs.Add(new GameObject[] { hazelTurretBody });

            //Register new deployable
            DeployableAPI.GetDeployableSameSlotLimit deployableSameSlotLimit = delegate (CharacterMaster master, int intVal)
            {
                //TODO: Make this equal to the max stock count of the primary skill
                return primarySkillFamily.variants[0].skillDef.baseMaxStock;
            };
            hazelTurretDeployableSlot = DeployableAPI.RegisterDeployableSlot(deployableSameSlotLimit);

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

            PonePackContentPack.entityStateTypes.Add(new System.Type[] { typeof(PonePack.EntityStates.Hazel.HazelTurret.SpawnState) });
            PonePackContentPack.entityStateTypes.Add(new System.Type[] { typeof(PonePack.EntityStates.Hazel.HazelTurret.DeathState) });
            PonePackContentPack.entityStateTypes.Add(new System.Type[] { typeof(PonePack.EntityStates.Hazel.HazelTurret.DeployTurret) });
            PonePackContentPack.entityStateTypes.Add(new System.Type[] { typeof(PonePack.EntityStates.Hazel.HazelTurret.FireHazelTurret) });
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
