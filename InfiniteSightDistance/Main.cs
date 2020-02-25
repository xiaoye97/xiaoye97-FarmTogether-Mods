using Harmony12;
using UnityEngine;
using System.Reflection;
using UnityModManagerNet;

namespace InfiniteSightDistance
{
    public class Main
    {
        public static UnityModManager.ModEntry mod;
        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            mod = modEntry;
            HarmonyInstance.Create(mod.Info.Id).PatchAll(Assembly.GetExecutingAssembly());
            mod.OnGUI = OnGUI;
            return true;
        }

        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.Label("作者:xiaoye97 群内昵称:夜空之下");
            GUILayout.Label("FarmTogether交流群:973116708 bug反馈请加群找我");
            GUILayout.Label("本mod仅对自己生效");
        }

        [HarmonyPatch(typeof(FarmFenceView), "AutoDisabler.IAutoDisable.SetEnabled")]
        class FarmFenceViewPatch
        {
            public static void Prefix(ref bool value)
            {
                if (mod.Enabled) value = true;
            }
        }

        [HarmonyPatch(typeof(FarmTileContentsView), "AutoDisabler.IAutoDisable.SetEnabled")]
        class FarmTileContentsPatch
        {
            public static void Prefix(ref bool value)
            {
                if (mod.Enabled) value = true;
            }
        }

        [HarmonyPatch(typeof(BaseBuildingView), "AutoDisabler.IAutoDisable.SetEnabled")]
        class BaseBuildingViewPatch
        {
            public static void Prefix(ref bool value)
            {
                if (mod.Enabled) value = true;
            }
        }
    }
}
