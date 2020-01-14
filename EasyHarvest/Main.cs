using Harmony12;
using Logic.Farm;
using UnityEngine;
using System.Reflection;
using UnityModManagerNet;
using System.Collections.Generic;

namespace EasyHarvest
{
    public static class Main
    {
        public static ModSetting setting;
        public static UnityModManager.ModEntry.ModLogger logger;
        public static Queue<List<FarmTile>> workQueue = new Queue<List<FarmTile>>();
        public static bool startWork = false;
        public static WorkType startWorkType = WorkType.Harvest;
        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            setting = UnityModManager.ModSettings.Load<ModSetting>(modEntry);
            logger = modEntry.Logger;
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            modEntry.OnGUI = OnGUI;
            modEntry.OnHideGUI = OnHideGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            return true;
        }

        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.Label("作者:xiaoye97 群内昵称:夜空之下");
            GUILayout.Label("FarmTogether交流群:973116708 bug反馈请加群找我");
            GUILayout.Label("在其他人的农场，你必须拥有全部权限才能进行一键操作");

            GUILayout.Label("一般设置");
            setting.hotKeyToggle = GUILayout.Toggle(setting.hotKeyToggle, "启用快捷键", GUILayout.Width(100));
            setting.logToggle = GUILayout.Toggle(setting.logToggle, "输出统计日志", GUILayout.Width(100));

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
            if (StageScript.Instance != null && StageScript.Instance.FarmData != null)
            {
                if (GUILayout.Button("一键收获(快捷键E)"))
                {
                    Harvest();
                }
                if (GUILayout.Button("一键浇水/喂食(快捷键R)"))
                {
                    Refill();
                }
            }
            else
            {
                GUILayout.Label("进入农场后解锁一键收获按钮(快捷键E)和一键浇水喂食按钮(快捷键R)");
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
            if (StageScript.Instance.IsOnline)
            {
                if (StageScript.Instance.LocalPlayer.Permissions != PlayerPermissions.Full)
                {
                    SendChat("[一键收获] 没有权限，你必须拥有农场全部权限才能进行此操作");
                    return;
                }
            }
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
            FarmChunk[,] chunks = StageScript.Instance.FarmData.Chunks;
            List<FarmTile> workTiles = new List<FarmTile>();
            for (int i = 0; i < chunks.GetLength(0); i++)
            {
                for (int j = 0; j < chunks.GetLength(1); j++)
                {
                    if (chunks[i, j] != null)
                    {
                        if (chunks[i, j].IsUnlocked)
                        {
                            for (int x = 0; x < chunks[i, j].Tiles.GetLength(0); x++)
                            {
                                for (int y = 0; y < chunks[i, j].Tiles.GetLength(1); y++)
                                {
                                    if (chunks[i, j].Tiles[x, y] != null)
                                    {
                                        FarmTileContents contents = chunks[i, j].Tiles[x, y].Contents;
                                        if (contents != null)
                                        {
                                            switch (contents.Category)
                                            {
                                                case FarmTileContentsType.Animal: if (setting.animalHarvestToggle) TryWork(workTiles, contents, WorkType.Harvest, ref animal); break;
                                                case FarmTileContentsType.Crop: if (setting.cropHarvestToggle) TryWork(workTiles, contents, WorkType.Harvest, ref crop); break;
                                                case FarmTileContentsType.Flower: if (setting.flowerHarvestToggle) TryWork(workTiles, contents, WorkType.Harvest, ref flower); break;
                                                case FarmTileContentsType.Tree: if (setting.treeHarvestToggle) TryWork(workTiles, contents, WorkType.Harvest, ref tree); break;
                                                case FarmTileContentsType.Pond: if (setting.pondHarvestToggle) TryWork(workTiles, contents, WorkType.Harvest, ref pond); break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    //每一万个格子分成一个工作批次
                    if (workTiles.Count > 10000)
                    {
                        workQueue.Enqueue(workTiles);
                        workTiles = new List<FarmTile>();
                    }
                }
            }
            if (workTiles.Count > 0)
            {
                workQueue.Enqueue(workTiles);
            }
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
            if (StageScript.Instance.IsOnline)
            {
                if (StageScript.Instance.LocalPlayer.Permissions != PlayerPermissions.Full)
                {
                    SendChat("[一键浇水/喂食] 没有权限，你必须拥有农场全部权限才能进行此操作");
                    return;
                }
            }
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
            FarmChunk[,] chunks = StageScript.Instance.FarmData.Chunks;
            List<FarmTile> workTiles = new List<FarmTile>();
            for (int i = 0; i < chunks.GetLength(0); i++)
            {
                for (int j = 0; j < chunks.GetLength(1); j++)
                {
                    if (chunks[i, j] != null)
                    {
                        if (chunks[i, j].IsUnlocked)
                        {
                            for (int x = 0; x < chunks[i, j].Tiles.GetLength(0); x++)
                            {
                                for (int y = 0; y < chunks[i, j].Tiles.GetLength(1); y++)
                                {
                                    if (chunks[i, j].Tiles[x, y] != null)
                                    {
                                        FarmTileContents contents = chunks[i, j].Tiles[x, y].Contents;
                                        if (contents != null)
                                        {
                                            switch (contents.Category)
                                            {
                                                case FarmTileContentsType.Animal: if (setting.animalRefillToggle) TryWork(workTiles, contents, WorkType.Refill, ref animal); break;
                                                case FarmTileContentsType.Crop: if (setting.cropRefillToggle) TryWork(workTiles, contents, WorkType.Refill, ref crop); break;
                                                case FarmTileContentsType.Flower: if (setting.flowerRefillToggle) TryWork(workTiles, contents, WorkType.Refill, ref flower); break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (workTiles.Count > 10000)
                    {
                        workQueue.Enqueue(workTiles);
                        workTiles = new List<FarmTile>();
                    }
                }
            }
            if (workTiles.Count > 0)
            {
                workQueue.Enqueue(workTiles);
            }
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
            logger.Log(msg);
        }

        [HarmonyPatch(typeof(GameHud), "Update")]
        class InputPatch
        {
            public static void Postfix()
            {
                if (setting.hotKeyToggle)
                {
                    if (StageScript.Instance != null && StageScript.Instance.FarmData != null)
                    {
                        if (Input.GetKeyDown(KeyCode.E))
                        {
                            Harvest();
                        }
                        if (Input.GetKeyDown(KeyCode.R))
                        {
                            Refill();
                        }
                    }
                }
                if (workQueue.Count > 0)
                {
                    if (StageScript.Instance.LocalPlayer.State == PlayerScript.PlayerState.Idle)
                    {
                        StageScript.Instance.LocalPlayer.StartWorking(startWorkType, workQueue.Dequeue());
                    }
                }
                else
                {
                    if (startWork)
                    {
                        if (StageScript.Instance.LocalPlayer.IsFarmOwner)
                        {
                            SendChat("工作完毕");
                        }
                        else
                        {
                            SendChat("工作请求发送完毕");
                        }
                        startWork = false;
                    }
                }
            }
        }
    }
}
