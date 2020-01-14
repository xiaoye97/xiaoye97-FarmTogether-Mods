using System;
using Logic.Farm;
using UnityEngine;
using System.Reflection;
using UnityModManagerNet;

namespace SuperStorage
{
    public static class Main
    {
        public static UnityModManager.ModEntry.ModLogger logger;
        public static bool restored = false;
        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            logger = modEntry.Logger;
            modEntry.OnGUI = OnGUI;
            foreach (var kv in ShopManager.ItemDictionary)
            {
                if (kv.Value is BuildingDefinition)
                {
                    var storage = (kv.Value as BuildingDefinition).Storage;
                    for (int i = 0; i < storage.Count; i++)
                    {
                        FarmResource r = storage[i];
                        r.Amount *= 100;
                        storage[i] = r;
                    }
                }
            }
            logger.Log("仓库容量提高100倍");
            return true;
        }

        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.Label("作者:xiaoye97 群内昵称:夜空之下");
            GUILayout.Label("FarmTogether交流群:973116708");
            GUILayout.Label("第一次使用需要在农场中按下 刷新容量 按钮");
            if (GUILayout.Button("刷新容量"))
            {
                RefreshStorage();
            }
            GUILayout.Label("如果需要卸载mod，请在农场内按下 复原容量 按钮，然后关闭游戏删除此mod");
            if (!restored)
            {
                if (GUILayout.Button("复原容量"))
                {
                    if (StageScript.Instance.FarmData != null)
                    {
                        foreach (var kv in ShopManager.ItemDictionary)
                        {
                            if (kv.Value is BuildingDefinition)
                            {
                                var storage = (kv.Value as BuildingDefinition).Storage;
                                for (int i = 0; i < storage.Count; i++)
                                {
                                    FarmResource r = storage[i];
                                    r.Amount /= 100;
                                    storage[i] = r;
                                }
                            }
                        }
                        RefreshStorage();
                        restored = true;
                    }
                    RefreshStorage();
                }
            }
            else
            {
                GUILayout.Label("容量已复原，如需再次开启10倍容量，请重新启动游戏");
            }
        }

        public static void RefreshStorage()
        {
            Type type = typeof(FarmData);
            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
            var m = type.GetMethod("RecalculateStorage", flags);
            var obj = m.Invoke(StageScript.Instance.FarmData, null);
        }
    }
}
