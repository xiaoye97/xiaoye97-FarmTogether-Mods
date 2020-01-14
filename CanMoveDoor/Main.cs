using Harmony12;
using UnityEngine;
using Logic.Farm.House;
using UnityModManagerNet;

namespace CanMoveDoor
{
    public static class Main
    {
        public static bool patchFinish;
        public static HarmonyInstance harmony;
        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll();
            modEntry.OnGUI = OnGUI;
            return true;
        }

        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            InfoGUI(modEntry);
        }

        public static void InfoGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.BeginVertical(modEntry.Info.DisplayName, GUI.skin.window);
            GUILayout.Label("作者:xiaoye97", GUI.skin.box, GUILayout.Height(28));
            GUILayout.Label("群内昵称:夜空之下", GUI.skin.box, GUILayout.Height(28));
            GUILayout.Label("FarmTogether交流群:973116708", GUI.skin.box, GUILayout.Height(28));
            GUILayout.Label("bug反馈请加群找我", GUI.skin.box, GUILayout.Height(28));
            GUILayout.EndVertical();
        }

        public static bool CanBeMovePrefix(ref bool __result)
        {
            __result = true;
            return false;
        }

        /// <summary>
        /// 间接补丁
        /// </summary>
        [HarmonyPatch(typeof(HouseTile), "ContentsAny", MethodType.Getter)]
        class HouseTilePatch
        {
            public static void Postfix(HouseTileContents __result)
            {
                if (!patchFinish)
                {
                    if (__result.Category == HouseTileContentsType.InternalBuilding)
                    {
                        var type = __result.GetType();
                        var prop = type.GetProperty("CanBeMoved");
                        var o = prop.GetGetMethod();
                        harmony.Patch(o, new HarmonyMethod(typeof(Main).GetMethod("CanBeMovePrefix")));
                        patchFinish = true;
                    }
                }
            }
        }
    }
}
