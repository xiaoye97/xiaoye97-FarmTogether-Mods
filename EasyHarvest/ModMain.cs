using System;
using Harmony12;
using System.IO;
using Logic.Farm;
using UnityEngine;
using System.Text;
using System.Reflection;
using UnityModManagerNet;
using System.Collections.Generic;

namespace EasyHarvest
{
    public static class ModMain
    {
        public static UnityModManager.ModEntry mod;
        public static ModSetting setting;
        public static Queue<List<FarmTile>> workQueue = new Queue<List<FarmTile>>();
        public static bool startWork = false;
        public static WorkType startWorkType = WorkType.Harvest;
        public static string worktest = "";
        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            mod = modEntry;
            setting = UnityModManager.ModSettings.Load<ModSetting>(mod);
            HarmonyInstance.Create(modEntry.Info.Id).PatchAll(Assembly.GetExecutingAssembly());
            mod.OnGUI = OnGUI;
            mod.OnHideGUI = OnHideGUI;
            mod.OnSaveGUI = OnSaveGUI;
            mod.Info.DisplayName = $"一键收获({setting.harvestHotkey})/浇水喂食({setting.refillHotkey}) (本人所有mod均在群内免费发布，未授权任何二次售卖)";
            //if(GetH($"{mod.Path}info.json") != "206ac72410fd0c88019b4808ab465f30")
            //{
            //    worktest = "Mod文件校验不正确，请到交流群内下载";
            //}
            mod.Logger.Log(GetH($"{mod.Path}info.json"));
            return true;
        }

        public static bool CanWork()
        {
            if (!mod.Enabled) return false;
            if (StageScript.Instance == null) return false;
            if (!StageScript.Instance.Loaded) return false;
            if (!StageScript.Instance.LocalPlayer.IsFarmOwner) return false;
            if (!string.IsNullOrEmpty(worktest)) return false;
            return true;
        }

        public static string GetH(string filename)
        {
            FileStream file = new FileStream(filename, FileMode.Open);
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(file);
            file.Close();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }

        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.BeginVertical("一键浇水收获", GUI.skin.window);
            GUILayout.Label("作者:xiaoye97 群内昵称:夜空之下");
            GUILayout.Label("FarmTogether交流群:973116708 bug反馈请加群找我");
            GUILayout.Label("本人所有mod均在群内免费发布，未授权任何二次售卖");
            if (!string.IsNullOrEmpty(worktest))
            {
                GUILayout.Label(worktest);
            }
            else
            {
                GUILayout.Label("一般设置");
                setting.hotKeyToggle = GUILayout.Toggle(setting.hotKeyToggle, "启用快捷键", GUILayout.Width(200));
                setting.logToggle = GUILayout.Toggle(setting.logToggle, "输出统计日志", GUILayout.Width(200));

                GUILayout.Label("一键收获设置");
                GUILayout.BeginHorizontal();
                setting.animalHarvestToggle = GUILayout.Toggle(setting.animalHarvestToggle, "动物", GUILayout.Width(70));
                setting.cropHarvestToggle = GUILayout.Toggle(setting.cropHarvestToggle, "作物", GUILayout.Width(70));
                setting.flowerHarvestToggle = GUILayout.Toggle(setting.flowerHarvestToggle, "花", GUILayout.Width(70));
                setting.treeHarvestToggle = GUILayout.Toggle(setting.treeHarvestToggle, "树", GUILayout.Width(70));
                setting.pondHarvestToggle = GUILayout.Toggle(setting.pondHarvestToggle, "鱼塘", GUILayout.Width(70));
                GUILayout.EndHorizontal();

                GUILayout.Label("一键浇水/喂食设置");
                GUILayout.BeginHorizontal();
                setting.animalRefillToggle = GUILayout.Toggle(setting.animalRefillToggle, "动物", GUILayout.Width(70));
                setting.cropRefillToggle = GUILayout.Toggle(setting.cropRefillToggle, "作物", GUILayout.Width(70));
                setting.flowerRefillToggle = GUILayout.Toggle(setting.flowerRefillToggle, "花", GUILayout.Width(70));
                GUILayout.EndHorizontal();
                if (CanWork())
                {
                    if (GUILayout.Button($"一键收获(快捷键{setting.harvestHotkey})"))
                    {
                        Harvest();
                    }
                    if (GUILayout.Button($"一键浇水/喂食(快捷键{setting.refillHotkey})"))
                    {
                        Refill();
                    }
                }
                GUILayout.EndVertical();
            }
        }

        public static void OnHideGUI(UnityModManager.ModEntry modEntry)
        {
            setting.Save(modEntry);
        }

        public static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            setting.Save(modEntry);
        }

        /// <summary>
        /// 一键收获
        /// </summary>
        public static void Harvest()
        {
            if (!CanWork()) return;
            if (!(setting.animalHarvestToggle || setting.cropHarvestToggle || setting.flowerHarvestToggle || setting.pondHarvestToggle || setting.treeHarvestToggle))
            {
                SendChat("[一键收获] 所有选项都已关闭，无法进行工作");
                return;
            }
            if (startWork)
            {
                SendChat("正在进行工作，请等待工作结束");
                return;
            }
            startWork = true;
            startWorkType = WorkType.Harvest;
            int animal = 0, crop = 0, flower = 0, tree = 0, pond = 0;
            List<FarmTile> workTiles = new List<FarmTile>();
            var farm = StageScript.Instance.FarmData;
            foreach(var chunk in farm.Chunks)
            {
                if (!chunk.IsUnlocked) continue;
                foreach(var tile in chunk.Tiles)
                {
                    if (tile == null || tile.Contents == null) continue;
                    switch (tile.Contents.Category)
                    {
                        case FarmTileContentsType.Animal: if (setting.animalHarvestToggle) TryWork(workTiles, tile.Contents, WorkType.Harvest, ref animal); break;
                        case FarmTileContentsType.Crop: if (setting.cropHarvestToggle) TryWork(workTiles, tile.Contents, WorkType.Harvest, ref crop); break;
                        case FarmTileContentsType.Flower: if (setting.flowerHarvestToggle) TryWork(workTiles, tile.Contents, WorkType.Harvest, ref flower); break;
                        case FarmTileContentsType.Tree: if (setting.treeHarvestToggle) TryWork(workTiles, tile.Contents, WorkType.Harvest, ref tree); break;
                        case FarmTileContentsType.Pond: if (setting.pondHarvestToggle) TryWork(workTiles, tile.Contents, WorkType.Harvest, ref pond); break;
                    }
                    //每一万个格子分成一个工作批次
                    if (workTiles.Count > 9999)
                    {
                        workQueue.Enqueue(workTiles);
                        workTiles = new List<FarmTile>();
                    }
                }
            }

            if (workTiles.Count > 0) workQueue.Enqueue(workTiles);

            string logmsg = "[一键收获] 准备进行收获，目标: ";
            if (setting.animalHarvestToggle) logmsg += animal + "动物 ";
            if (setting.cropHarvestToggle) logmsg += crop + "作物 ";
            if (setting.flowerHarvestToggle) logmsg += flower + "花 ";
            if (setting.pondHarvestToggle) logmsg += tree + "树 ";
            if (setting.treeHarvestToggle) logmsg += pond + "鱼池 ";
            SendChat(logmsg);
            if (workQueue.Count > 1)
            {
                SendChat("开始工作，目标过多，将分成" + workQueue.Count.ToString() + "个批次工作，期间请勿操作人物");
            }
        }

        /// <summary>
        /// 一键浇水喂食
        /// </summary>
        public static void Refill()
        {
            if (!CanWork()) return;
            if (!(setting.animalRefillToggle || setting.cropRefillToggle || setting.flowerRefillToggle))
            {
                SendChat("[一键浇水/喂食] 所有选项都已关闭，无法进行工作");
                return;
            }
            if (startWork)
            {
                SendChat("正在进行工作，请等待工作结束");
                return;
            }
            startWork = true;
            startWorkType = WorkType.Refill;
            int animal = 0, crop = 0, flower = 0;
            var farm = StageScript.Instance.FarmData;
            List<FarmTile> workTiles = new List<FarmTile>();
            foreach (var chunk in farm.Chunks)
            {
                if (!chunk.IsUnlocked) continue;
                foreach (var tile in chunk.Tiles)
                {
                    if (tile == null || tile.Contents == null) continue;
                    switch (tile.Contents.Category)
                    {
                        case FarmTileContentsType.Animal: if (setting.animalRefillToggle) TryWork(workTiles, tile.Contents, WorkType.Refill, ref animal); break;
                        case FarmTileContentsType.Crop: if (setting.cropRefillToggle) TryWork(workTiles, tile.Contents, WorkType.Refill, ref crop); break;
                        case FarmTileContentsType.Flower: if (setting.flowerRefillToggle) TryWork(workTiles, tile.Contents, WorkType.Refill, ref flower); break;
                    }
                    //每一万个格子分成一个工作批次
                    if (workTiles.Count > 9999)
                    {
                        workQueue.Enqueue(workTiles);
                        workTiles = new List<FarmTile>();
                    }
                }
            }

            if (workTiles.Count > 0) workQueue.Enqueue(workTiles);

            string logmsg = "[一键浇水喂食] 准备进行浇水/喂食，目标: ";
            if (setting.animalRefillToggle) logmsg += animal + "动物 ";
            if (setting.cropRefillToggle) logmsg += crop + "作物 ";
            if (setting.flowerRefillToggle) logmsg += flower + "花 ";
            SendChat(logmsg);
            if (workQueue.Count > 1)
            {
                SendChat("开始工作，目标过多，将分成" + workQueue.Count.ToString() + "个批次工作，期间请勿操作人物");
            }
        }

        /// <summary>
        /// 检查工作可行性
        /// </summary>
        /// <param name="workTiles">工作列表，如果可行将加进列表</param>
        /// <param name="contents">工作目标</param>
        /// <param name="workType">工作类型</param>
        /// <param name="count">统计计数</param>
        public static void TryWork(List<FarmTile> workTiles, FarmTileContents contents, WorkType workType, ref int count)
        {
            FailedActionReason far;
            if (contents.CanWork(StageScript.Instance.LocalPlayer, workType, out far))
            {
                workTiles.Add(contents.Tile);
                count++;
            }
        }

        /// <summary>
        /// 日志输出
        /// </summary>
        /// <param name="msg">消息</param>
        public static void SendChat(string msg)
        {
            if (setting.logToggle)
            {
                MilkUIChat chat = GameObject.FindObjectOfType<MilkUIChat>();
                if (chat != null)
                {
                    chat.Filter.FilterText(msg, 0);
                }
            }
            mod.Logger.Log(msg);
        }

        [HarmonyPatch(typeof(GameHud), "Update")]
        class InputPatch
        {
            public static void Postfix()
            {
                if (setting.hotKeyToggle)
                {
                    if(CanWork())
                    {
                        if(Input.GetKeyDown(setting.harvestHotkey)) Harvest();
                        else if(Input.GetKeyDown(setting.refillHotkey)) Refill();
                    }
                }
                if (workQueue.Count > 0)
                {
                    if (StageScript.Instance.LocalPlayer.State == PlayerScript.PlayerState.Idle)
                    {
                        StageScript.Instance.LocalPlayer.StartWorking(startWorkType, workQueue.Dequeue());
                    }
                }
                else if(startWork)
                {
                    SendChat("工作完毕");
                    startWork = false;
                }
            }
        }

        [HarmonyPatch(typeof(FloatingText), "Log", new Type[] {typeof(string), typeof(Vector3), typeof(string), typeof(float) })]
        class FloatingTextPtch
        {
            public static bool Prefix()
            {
                if(CanWork())
                {
                    if(startWork) return false;
                }
                return true;
            }
        }
    }
}
