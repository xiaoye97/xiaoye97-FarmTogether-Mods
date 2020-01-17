using Harmony12;
using UnityEngine;
using System.Reflection;
using UnityModManagerNet;

namespace FarmHarvestMonitor
{
    public static class Main
    {
        public static UnityModManager.ModEntry.ModLogger logger;
        public static UnityModManager.ModEntry entry;
        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            entry = modEntry;
            logger = modEntry.Logger;
            
            HarmonyInstance.Create(modEntry.Info.Id).PatchAll(Assembly.GetExecutingAssembly());
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
            GUILayout.Label("本mod仅在非本地模式的自己农场中生效");
            GUILayout.Label("输出的文件日志如果显示为乱码，请选择编码为UTF-8即可正常显示");
            GUILayout.EndVertical();
        }

        [HarmonyPatch(typeof(FWFNetworkLogicBehaviour), "CmdStartWork")]
        class WorkPatch
        {
            public static void Postfix(byte playerSlot, WorkType workType, FarmTileId[] tiles)
            {
                if (!BaseStageScript.HasInstance)
                {
                    return;
                }
                if (StageScript.Instance.LocalPlayer.IsFarmOwner)
                {
                    if(workType == WorkType.Harvest)
                    {
                        HarvestResult result = new HarvestResult(playerSlot, tiles);
                        logger.Log(result.ToString());
                        result.ToFile();
                    }
                }
            }
        }
    }
}
