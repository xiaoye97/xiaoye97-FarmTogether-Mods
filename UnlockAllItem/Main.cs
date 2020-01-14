using System;
using Harmony12;
using Logic.Mode;
using Logic.Farm;
using UnityEngine;
using System.Reflection;
using UnityModManagerNet;

namespace UnlockAllItem
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
            GUILayout.Label("FarmTogether交流群:973116708 bug反馈请加群找我");
        }

        [HarmonyPatch(typeof(FarmManager), "IsUnlocked")]
        class UnlockPatch1
        {
            public static bool Prefix(ref UnlockState __result)
            {
                __result = UnlockState.Unlocked;
                return false;
            }
        }

        [HarmonyPatch(typeof(FarmData), "IsUnlocked", new Type[] { typeof(ShopItemDefinition) })]
        class UnlockPatch2
        {
            public static bool Prefix(ref UnlockState __result)
            {
                __result = UnlockState.Unlocked;
                return false;
            }
        }

        [HarmonyPatch(typeof(FarmData), "IsUnlocked", new Type[] { typeof(RecipeDefinition) })]
        class UnlockPatch3
        {
            public static bool Prefix(ref UnlockState __result)
            {
                __result = UnlockState.Unlocked;
                return false;
            }
        }
    }
}
