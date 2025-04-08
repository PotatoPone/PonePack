using RoR2.Networking;
using RoR2.Projectile;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using RoR2;
using Unity;

namespace PonePack
{
    public class ProjectileSpawnTurret : NetworkBehaviour
    {
        bool turretSpawned = false;

        private void Start()
        {
            Debug.LogWarning("SpawnTurret Projectile Started!");
        }

        public void SpawnTurret(ProjectileImpactInfo impactInfo)
        {
            Debug.Log("SpawnTurret called!");
            Debug.Log("NetworkServer.active: " + NetworkServer.active);
            if (!NetworkServer.active) return;
            HurtBox hitHurtBox = impactInfo.collider.GetComponent<HurtBox>();
            if (hitHurtBox && hitHurtBox.healthComponent && hitHurtBox.healthComponent.body && hitHurtBox.healthComponent.body.GetComponent<StickToCharacter>())
            {
                Debug.Log("Hit a StickToCharacter component. Ignoring");
                return;
            }
            if (turretSpawned == true) return;
            turretSpawned = true;

            ProjectileController projectileController = base.gameObject.GetComponent<ProjectileController>();
            if (!projectileController) return;
            CharacterBody owner = projectileController.owner.GetComponent<CharacterBody>();
            if (!owner) return;

            GameObject turretBody = PonePack.Survivors.Hazel.hazelTurretBody;
            //Quaternion rotation = Quaternion.FromToRotation(turretBody.transform.up, impactInfo.estimatedImpactNormal) * turretBody.transform.rotation;
            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, impactInfo.estimatedImpactNormal) * Quaternion.identity;
            MasterCatalog.MasterIndex masterIndex = MasterCatalog.FindMasterIndex(PonePack.Survivors.Hazel.hazelTurretMaster);
            DeployableSlot deployableSlot = PonePack.Survivors.Hazel.hazelTurretDeployableSlot;

            CharacterMaster newTurretMaster = PonePack.Utils.ConstructTurret(owner, impactInfo.estimatedPointOfImpact, rotation, masterIndex, deployableSlot);
            //CharacterMaster newTurretMaster = ConstructTurret(owner, impactInfo, MasterCatalog.FindMasterIndex(PonePack.Survivors.Hazel.hazelTurretMaster));

            if (hitHurtBox && hitHurtBox.healthComponent && hitHurtBox.healthComponent.body)
            {
                newTurretMaster.GetBody().GetComponent<StickToCharacter>().Stick(hitHurtBox.healthComponent.body, impactInfo.collider.transform);
            }

            //test
            Destroy(base.gameObject);
        }

        //private CharacterMaster ConstructTurret(CharacterBody builderBody, ProjectileImpactInfo impactInfo, MasterCatalog.MasterIndex turretMasterIndex)
        //{
        //    if ((bool)builderBody == false)
        //    {
        //        Debug.LogWarning("Unable to construct turret: No CharacterBody for Builder");
        //        return null;
        //    }
        //    CharacterMaster characterMaster = builderBody.master;
        //    if ((bool)characterMaster == false)
        //    {
        //        Debug.LogWarning("Unable to construct turret: No CharacterMaster for Builder");
        //        return null;
        //    }

        //    GameObject turretBody = PonePack.Survivors.Hazel.hazelTurretBody;
        //    Quaternion newRotation = Quaternion.FromToRotation(turretBody.transform.up, impactInfo.estimatedImpactNormal) * turretBody.transform.rotation;

        //    CharacterMaster newTurretMaster = new MasterSummon
        //    {
        //        masterPrefab = MasterCatalog.GetMasterPrefab(turretMasterIndex),
        //        position = impactInfo.estimatedPointOfImpact,
        //        rotation = newRotation,
        //        summonerBodyObject = builderBody.gameObject,
        //        ignoreTeamMemberLimit = true,
        //        inventoryToCopy = characterMaster.inventory
        //    }.Perform();
        //    Deployable deployable = newTurretMaster.gameObject.AddComponent<Deployable>();
        //    deployable.onUndeploy = new UnityEvent();
        //    deployable.onUndeploy.AddListener(newTurretMaster.TrueKill);
        //    characterMaster.AddDeployable(deployable, PonePack.Survivors.Hazel.hazelTurretDeployableSlot);

        //    return newTurretMaster;
        //}
    }
}
