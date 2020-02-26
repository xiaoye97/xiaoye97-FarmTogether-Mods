using Harmony12;
using UnityEngine;
using System.Reflection;
using UnityModManagerNet;

namespace VideoTV
{
    public static class Main
    {
        public static ModSetting setting;
        public static UnityModManager.ModEntry mod;
        public static TV tv;
        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            mod = modEntry;
            setting = UnityModManager.ModSettings.Load<ModSetting>(mod);
            HarmonyInstance.Create(modEntry.Info.Id).PatchAll(Assembly.GetExecutingAssembly());
            mod.OnGUI = OnGUI;
            mod.OnSaveGUI = OnSaveGUI;
            mod.OnHideGUI = OnSaveGUI;
            mod.Info.DisplayName = "视频电视 (本人所有mod均在群内免费发布，未授权任何二次售卖)";
            return true;
        }

        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.Label("作者:xiaoye97 群内昵称:夜空之下");
            GUILayout.Label("FarmTogether交流群:973116708 bug反馈请加群找我");
            if(CanWork())
            {
                if(tv != null && tv.Loaded)
                {
                    tv.TVGUI();
                }
                else
                {
                    if(GUILayout.Button("实例化电视", GUILayout.Height(60)))
                    {
                        tv = new TV();
                        tv.PlayOrPause();
                    }
                }
            }
            else
            {
                GUILayout.Label("存档未载入，请在农场房间内使用");
            }
        }

        public static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            setting.Save(modEntry);
        }

        public static bool CanWork()
        {
            if (!mod.Enabled) return false;
            if (StageScript.Instance == null) return false;
            if (!StageScript.Instance.Loaded) return false;
            return true;
        }

        [HarmonyPatch(typeof(GameHud), "Update")]
        class InputPatch
        {
            public static void Postfix()
            {
                if (!CanWork()) return;

                if(Input.GetKeyDown(setting.播放快捷键))
                {
                    if(tv != null && tv.Loaded)
                    {
                        tv.PlayOrPause();
                    }
                    else
                    {
                        tv = new TV();
                        tv.PlayOrPause();
                    }
                }
                if(tv != null && tv.Loaded)
                {
                    if (Input.GetKeyDown(setting.上一个视频快捷键)) tv.PlayBack();
                    if (Input.GetKeyDown(setting.下一个视频快捷键)) tv.PlayNext();
                    if (Input.GetKeyDown(setting.后退快捷键)) tv.ToLeft();
                    if (Input.GetKeyDown(setting.快进快捷键)) tv.ToRight();
                    if (Input.GetKeyDown(setting.重新播放快捷键)) tv.RePlayVideo();
                }
            }
        }

        [HarmonyPatch(typeof(MilkUIChat), "PostMessage")]
        class ChatPatch
        {
            public static void Postfix(string msg)
            {
                if (!CanWork()) return;
                if (!msg.Contains("[视频电视mod]一起来看:")) return;
                int start = msg.IndexOf("[视频电视mod]一起来看:");
                string url = msg.Substring(start + "[视频电视mod]一起来看:".Length);
                mod.Logger.Log($"收到邀请{url}");
                if(tv == null || !tv.Loaded)
                {
                    tv = new TV();
                }
                if(tv != null && tv.Loaded)
                {
                    tv.HandleVideoInvite(url);
                }
                else
                {
                    mod.Logger.Log("响应邀请失败，当前位置没有电视");
                }
            }
        }
    }
}
