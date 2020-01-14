using System;
using Harmony12;
using UnityEngine;
using System.Reflection;
using UnityModManagerNet;
using Logic.Farm.Buildings;

namespace AutoPayFarmhand
{
    public static class Main
    {
        public static UnityModManager.ModEntry.ModLogger logger;
        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            logger = modEntry.Logger;
            HarmonyInstance.Create(modEntry.Info.Id).PatchAll(Assembly.GetExecutingAssembly());
            modEntry.OnGUI = OnGUI;
            return true;
        }

        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.Label("作者:xiaoye97 群内昵称:夜空之下");
            GUILayout.Label("FarmTogether交流群:973116708");
            GUILayout.Label("本mod仅在自己农场生效");
        }

        [HarmonyPatch(typeof(Building), "Tick", new Type[] { typeof(uint) })]
        class PayFarmhand
        {
            public static void Postfix(Building __instance)
            {
                if (__instance.Definition.Interaction == BuildingDefinition.BuildingInteractions.Farmhand)
                {
                    FailedActionReason far;
                    bool canInteract = __instance.CanInteract(StageScript.Instance.LocalPlayer, out far);
                    if (canInteract)
                    {
                        if (StageScript.Instance.LocalPlayer.IsFarmOwner)
                        {
                            __instance.Work(StageScript.Instance.LocalPlayer, true, WorkType.Harvest, out far);
                        }
                    }
                }
            }
        }
    }
}
