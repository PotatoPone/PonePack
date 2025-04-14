using RoR2;
using RoR2.Projectile;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace PonePack
{
    public static class Utils
    {
        public static CharacterMaster ConstructTurret(CharacterBody builderBody, Vector3 position, Quaternion rotation, MasterCatalog.MasterIndex turretMasterIndex, DeployableSlot deployableSlot)
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
