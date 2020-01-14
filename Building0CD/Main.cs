using Harmony12;
using UnityEngine;
using System.Reflection;
using UnityModManagerNet;
using Logic.Farm.Buildings;

namespace Building0CD
{
    public static class Main
    {
        public static UnityModManager.ModEntry.ModLogger logger;
        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            logger = modEntry.Logger;
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            modEntry.OnGUI = OnGUI;
            return true;
        }

        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.Label("作者:xiaoye97 群内昵称:夜空之下");
            GUILayout.Label("FarmTogether交流群:973116708 bug反馈请加群找我");
            GUILayout.Label("本mod仅在自己农场生效");
        }

        /// <summary>
        /// 建筑补丁
        /// </summary>
        [HarmonyPatch(typeof(Building), "Tick")]
        class Building0CDPatch
        {
            public static void Postfix(Building __instance)
            {
                if (__instance.Definition.Interaction != BuildingDefinition.BuildingInteractions.None
                    && __instance.Definition.Interaction != BuildingDefinition.BuildingInteractions.Farmhand
                    && __instance.Definition.Interaction != BuildingDefinition.BuildingInteractions.PayFarmhands
                    && __instance.Definition.Interaction != BuildingDefinition.BuildingInteractions.Count
                    && __instance.Definition.Interaction != BuildingDefinition.BuildingInteractions.PeriodicHarvest)
                {
                    if (__instance.State == Building.BuildingState.Waiting)
                    {
                        if (StageScript.Instance.LocalPlayer.IsFarmOwner)
                        {
                            Traverse.Create(__instance).Field("waitTimeElapsed").SetValue(__instance.Definition.InteractionInterval);
                            Traverse.Create(__instance).Property("State").SetValue(Building.BuildingState.Ready);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 房屋补丁
        /// </summary>
        [HarmonyPatch(typeof(HouseBuilding), "Tick")]
        class Building0CDHousePatch
        {
            public static void Postfix(HouseBuilding __instance)
            {
                if (__instance.State == HouseBuilding.HouseConstructionState.Waiting)
                {
                    if (StageScript.Instance.LocalPlayer.IsFarmOwner)
                    {
                        Traverse.Create(__instance).Field("constructionTimeElapsed").SetValue(__instance.Definition.ConstructionStepInterval);
                    }
                }
            }
        }
    }
}
