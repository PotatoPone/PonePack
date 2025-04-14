using RoR2.Networking;
using RoR2.Projectile;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using RoR2;
using Unity;

namespace Druid
{
    public class ProjectileSpawnTurret : NetworkBehaviour
    {
        bool turretSpawned = false;

        public void SpawnTurret(ProjectileImpactInfo impactInfo)
        {
            if (!NetworkServer.active) return;
            HurtBox hitHurtBox = impactInfo.collider.GetComponent<HurtBox>();
            if (hitHurtBox && hitHurtBox.healthComponent && hitHurtBox.healthComponent.body && hitHurtBox.healthComponent.body.GetComponent<StickToCharacter>()) return;
            if (turretSpawned == true) return;
            ProjectileController projectileController = base.gameObject.GetComponent<ProjectileController>();
            if (!projectileController) return;
            CharacterBody owner = projectileController.owner.GetComponent<CharacterBody>();
            if (!owner) return;

            GameObject turretBody = Druid.DruidSurvivor.turretBody;
            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, impactInfo.estimatedImpactNormal) * Quaternion.identity;
            MasterCatalog.MasterIndex masterIndex = MasterCatalog.FindMasterIndex(Druid.DruidSurvivor.turretMaster);
            DeployableSlot deployableSlot = Druid.DruidSurvivor.turretDeployableSlot;

            CharacterMaster newTurretMaster = ConstructTurret(owner, impactInfo.estimatedPointOfImpact, rotation, masterIndex, deployableSlot);

            turretSpawned = true;

            if (hitHurtBox && hitHurtBox.healthComponent && hitHurtBox.healthComponent.body)
            {
                newTurretMaster.GetBody().GetComponent<StickToCharacter>().Stick(hitHurtBox.healthComponent.body, impactInfo.collider.transform);
            }

            //test
            Destroy(base.gameObject);
        }

        private CharacterMaster ConstructTurret(CharacterBody builderBody, Vector3 position, Quaternion rotation, MasterCatalog.MasterIndex turretMasterIndex, DeployableSlot deployableSlot)
        {
            if ((bool)builderBody == false)
            {
                Debug.LogWarning("Unable to construct turret: No CharacterBody for Builder");
                return null;
            }
            CharacterMaster characterMaster = builderBody.master;
            if ((bool)characterMaster == false)
            {
                Debug.LogWarning("Unable to construct turret: No CharacterMaster for Builder");
                return null;
            }

            CharacterMaster newTurretMaster = new MasterSummon
            {
                masterPrefab = MasterCatalog.GetMasterPrefab(turretMasterIndex),
                position = position,
                rotation = rotation,
                summonerBodyObject = builderBody.gameObject,
                ignoreTeamMemberLimit = true,
                inventoryToCopy = characterMaster.inventory
            }.Perform();
            Deployable deployable = newTurretMaster.gameObject.AddComponent<Deployable>();
            deployable.onUndeploy = new UnityEvent();
            deployable.onUndeploy.AddListener(newTurretMaster.TrueKill);

            characterMaster.AddDeployable(deployable, deployableSlot);

            return newTurretMaster;
        }
    }
}
