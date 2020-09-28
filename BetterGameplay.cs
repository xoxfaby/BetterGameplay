using System;

using BepInEx;
using RoR2;
using R2API.Utils;
using UnityEngine;
using System.Collections;

namespace BetterGameplay
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.xoxfaby.BetterGameplay", "BetterGameplay", "1.0.0")]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    public class BetterGameplay : BaseUnityPlugin
    {
        char[] collisionMap = "11111111100010000000110011001000".ToCharArray();
                                         
        public void Awake()
        {
            for(int i = 0; i < 32; i++)
            {
                Physics.IgnoreLayerCollision(22, i, collisionMap[31 - i] == '0' ? true : false) ;
            }
            On.RoR2.PickupDropletController.Start += (orig, self) =>
            {
                orig(self);
                self.gameObject.layer = 22;
            };
            On.RoR2.MapZone.TryZoneStart += (orig, self, collider) => {
                if (self.zoneType == MapZone.ZoneType.OutOfBounds)
                {
                    if (collider.GetComponent<PickupDropletController>() || collider.GetComponent<GenericPickupController>())
                    {
                        SpawnCard spawnCard = ScriptableObject.CreateInstance<SpawnCard>();
                        spawnCard.hullSize = HullClassification.Human;
                        spawnCard.nodeGraphType = RoR2.Navigation.MapNodeGroup.GraphType.Ground;
                        spawnCard.prefab = Resources.Load<GameObject>("SpawnCards/HelperPrefab");
                        GameObject gameObject = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(spawnCard, new DirectorPlacementRule
                        {
                            placementMode = DirectorPlacementRule.PlacementMode.NearestNode,
                            position = collider.transform.position
                        }, RoR2Application.rng));
                        if (gameObject)
                        {
                            Debug.Log("TP Item Back");
                            TeleportHelper.TeleportGameObject(collider.gameObject, gameObject.transform.position);
                            UnityEngine.Object.Destroy(gameObject);
                        }
                        UnityEngine.Object.Destroy(spawnCard);
                    }
                }

                orig(self, collider);
            };

            On.RoR2.EquipmentSlot.ExecuteIfReady += (orig, self) =>
            {
                if((self.inventory != null) ? self.inventory.GetItemCount(ItemIndex.AutoCastEquipment) > 0 : false)
                {
                    if(SceneInfo.instance.sceneDef.nameToken == "MAP_BAZAAR_TITLE")
                    {
                        return false;
                    }
                }
                return orig(self);
            };

            On.RoR2.Inventory.UpdateEquipment += (orig, self) =>
            {

                if (self.GetItemCount(ItemIndex.AutoCastEquipment) > 0)
                {
                    if (SceneInfo.instance.sceneDef.nameToken == "MAP_BAZAAR_TITLE")
                    {
                        for(int i = 0; i < self.equipmentStateSlots.Length; i++)
                        {
                            self.equipmentStateSlots[0].chargeFinishTime.t += Time.deltaTime;
                        }
                        return;
                    }
                }
                orig(self);
            };
        }
    }
}
