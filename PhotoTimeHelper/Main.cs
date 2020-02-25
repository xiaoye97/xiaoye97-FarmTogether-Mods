using Harmony12;
using Logic.Farm;
using UnityEngine;
using System.Reflection;
using UnityModManagerNet;

namespace PhotoTimeHelper
{
    public static class Main
    {
        public static UnityModManager.ModEntry mod;
        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            mod = modEntry;
            HarmonyInstance.Create(mod.Info.Id).PatchAll(Assembly.GetExecutingAssembly());
            mod.OnGUI = OnGUI;
            return true;
        }

        public static bool IsLoadFarm()
        {
            if (StageScript.Instance == null) return false;
            if (!StageScript.Instance.Loaded) return false;
            return true;
        }

        #region 属性
        public static bool enabled = false;
        public static bool Enabled
        {
            get { return enabled; }
            set
            {
                if (enabled != value)
                {
                    if (value == true)
                    {
                        lockedProcess = StageScript.Instance.FarmData.SeasonProgress;
                        nextSeasonChange = Traverse.Create(StageScript.Instance.FarmData).Field("nextSeasonChange");
                        worldTicks = Traverse.Create(StageScript.Instance.FarmData).Field("worldTicks");
                    }
                    enabled = value;
                }
            }
        }
        public static float lockedProcess = 0.5f; //0 - 1 季节条
        public static Traverse nextSeasonChange;
        public static Traverse worldTicks;
        public static float realProcess
        {
            get
            {
                return 1f - ((uint)nextSeasonChange.GetValue() - (uint)worldTicks.GetValue()) / 1020f;
            }
        }
        #endregion
        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.Label("作者:xiaoye97 群内昵称:夜空之下");
            GUILayout.Label("FarmTogether交流群:973116708 bug反馈请加群找我");
            GUILayout.Label("本mod效果仅自己可见");
            if (mod.Enabled)
            {
                if (IsLoadFarm())
                {
                    Enabled = GUILayout.Toggle(Enabled, "启用并冻结时间");
                    if (Enabled)
                    {
                        //控制时间
                        GUILayout.BeginVertical("控制时间", GUI.skin.window);
                        GUILayout.Label($"拍照用时间百分比:{(lockedProcess * 100).ToString("f2")}%", GUILayout.Width(400));
                        lockedProcess = GUILayout.HorizontalSlider(lockedProcess, 0, 1);
                        GUILayout.Label($"真实时间百分比:{(realProcess * 100).ToString("f2")}%", GUILayout.Width(400));
                        GUILayout.HorizontalSlider(realProcess, 0, 1);
                        GUILayout.EndVertical();
                    }
                }
                else
                {
                    GUILayout.Label("未载入农场");
                }
            }
        }

        [HarmonyPatch(typeof(FarmData), "SeasonProgress", MethodType.Getter)]
        class ProcessPatch
        {
            public static bool Prefix(ref float __result)
            {
                if (mod.Enabled)
                {
                    if (IsLoadFarm() && Enabled)
                    {
                        __result = lockedProcess;
                        return false;
                    }
                }
                return true;
            }
        }
    }
}
