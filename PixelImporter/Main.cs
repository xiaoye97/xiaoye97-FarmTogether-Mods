using System;
using Harmony12;
using Logic.Farm;
using UnityEngine;
using Milkstone.Utils;
using System.Reflection;
using UnityModManagerNet;
using Logic.Farm.Contents;
using System.Runtime.InteropServices;

namespace PixelImporter
{
    public static class Main
    {
        public static UnityModManager.ModEntry mod;
        public static Texture2D pic = null;
        public static Texture2D prePic = null; //预览图
        public static float yu = 0.7f; //灰度阈值
        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            mod = modEntry;
            mod.OnGUI = OnGUI;
            WWW www = new WWW($"file://{mod.Path}default.png");
            pic = www.texture;
            prePic = FixPic(pic);
            return true;
        }

        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.Label("作者:xiaoye97 群内昵称:夜空之下");
            GUILayout.Label("FarmTogether交流群:973116708 bug反馈请加群找我");
            GUILayout.BeginVertical("使用说明", GUI.skin.window);
            GUILayout.Label("从外部软件如PS，Aseprite等制作一张宽280，高140的黑白图片，黑色为要放置的点");
            GUILayout.Label("在农场内点击下方 打开文件 按钮选择png图片，然后点击导入即可");
            GUILayout.Label("如果要覆盖的位置有其他物品，则不会覆盖，直接忽略");
            GUILayout.EndVertical();
            if (GUILayout.Button("打开文件", GUILayout.Height(60)))
            {
                OpenFileName openFileName = new OpenFileName();
                openFileName.structSize = Marshal.SizeOf(openFileName);
                openFileName.filter = "PNG图片\0*.png";
                openFileName.file = new string(new char[256]);
                openFileName.maxFile = openFileName.file.Length;
                openFileName.fileTitle = new string(new char[64]);
                openFileName.maxFileTitle = openFileName.fileTitle.Length;
                openFileName.initialDir = mod.Path; //默认路径
                openFileName.title = "打开像素图";
                openFileName.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000008;

                if (LocalDialog.GetSaveFileName(openFileName))
                {
                    WWW www = new WWW($"file://{openFileName.file}");
                    pic = www.texture;
                    prePic = FixPic(pic);
                }
            }
            if (pic != null)
            {
                GUILayout.BeginVertical("图片预览", GUI.skin.window);
                GUILayout.Box(pic);
                GUILayout.Label($"灰度阈值 {yu.ToString("f3")}");
                yu = GUILayout.HorizontalSlider(yu, 0, 1);
                if (GUILayout.Button("刷新"))
                {
                    prePic = FixPic(pic);
                }
                GUILayout.Label("结果预览↓");
                GUILayout.Box(prePic);
                if (pic.width == 280 && pic.height == 140)
                {
                    if (StageScript.Instance != null && StageScript.Instance.Loaded)
                    {
                        if (StageScript.Instance.LocalPlayer.IsFarmOwner)
                        {
                            if (GUILayout.Button("导入", GUILayout.Height(60)))
                            {
                                Fill("RoadGrass", pic);
                            }
                        }
                        else GUILayout.Label("这不是你的农场，不能导入像素画");
                    }
                    else GUILayout.Label("存档未载入，无法打开像素画导入器");
                }
                else GUILayout.Label($"图片大小必须为280x140，当前图片大小为{pic.width}x{pic.height}，无法导入");
                GUILayout.EndVertical();
            }
        }

        /// <summary>
        /// 转换int到FarmTileId
        /// </summary>
        public static FarmTileId GetId(int x, int y)
        {
            return new FarmTileId(new Int2(x, y));
        }

        /// <summary>
        /// 预览图
        /// </summary>
        public static Texture2D FixPic(Texture2D o)
        {
            int n = 4; //放大倍数
            Texture2D t = new Texture2D(o.width * n, o.height * n, o.format, false);

            for (int x = 0; x < o.width; x++)
            {
                for (int y = 0; y < o.height; y++)
                {
                    if (IsNearBlack(o.GetPixel(x, y)))
                    {
                        for (int nx = 0; nx < n; nx++)
                        {
                            for (int ny = 0; ny < n; ny++)
                            {
                                t.SetPixel(x * n + nx, y * n + ny, Color.black);
                            }
                        }
                    }
                    else
                    {
                        for (int nx = 0; nx < n; nx++)
                        {
                            for (int ny = 0; ny < n; ny++)
                            {
                                t.SetPixel(x * n + nx, y * n + ny, Color.white);
                            }
                        }
                    }
                }
            }
            t.Apply(false);
            return t;
        }

        public static bool IsNearBlack(Color c)
        {
            if (c.r > yu) return false;
            if (c.g > yu) return false;
            if (c.b > yu) return false;
            if (Mathf.Abs(c.r - c.g) > 0.05f) return false;
            if (Mathf.Abs(c.r - c.b) > 0.05f) return false;
            if (Mathf.Abs(c.g - c.b) > 0.05f) return false;
            return true;
        }

        delegate void FillAction(FarmTile tile);
        public static bool startFill = false;
        public static void Fill(string id, Texture2D pic)
        {
            //判断尺寸
            var farm = StageScript.Instance.FarmData;
            mod.Logger.Log($"像素画尺寸 w:{pic.width} h:{pic.height}");
            if (pic.width != 280 || pic.height != 140)
            {
                mod.Logger.Log("像素画尺寸不等于280x140，停止工作");
                return;
            }
            //判断物品
            FarmItemDefinition fillObj = null;
            foreach (var kv in ShopManager.ItemDictionary)
            {
                if (kv.Value.FullId == id)
                {
                    mod.Logger.Log($"找到了ID为{id}的物品，名字为{Localization.Get(id)}");
                    fillObj = kv.Value;
                    break;
                }
            }
            if (fillObj == null)
            {
                mod.Logger.Log("未找到物品");
                return;
            }
            FailedActionReason far;
            int workCount = 0;
            FillAction action = null;
            Type contentType = null;

            switch (fillObj.Category)
            {
                case ShopItemType.Crop:
                    contentType = Assembly.Load("Assembly-CSharp").GetType("Logic.Farm.Contents.FarmCrop");
                    action = new FillAction((tile) =>
                    {
                        if (tile.CheckEmpty(out far))
                        {
                            FarmTileContents content = Activator.CreateInstance(contentType, fillObj, tile) as FarmTileContents;
                            Traverse.Create(content).Field("state").SetValue(byte.Parse("1")); //设置作物为成熟状态
                            SetContent(tile, content);
                            workCount++;
                        }
                    });
                    break;
                case ShopItemType.Tree:
                    action = new FillAction((tile) =>
                    {
                        if (tile.CheckEmpty(out far))
                        {
                            SetContent(tile, new FarmTree(fillObj as TreeDefinition, tile));
                            workCount++;
                        }
                    });
                    break;
                case ShopItemType.Animal:
                    action = new FillAction((tile) =>
                    {
                        if (tile.CheckEmpty(out far))
                        {
                            farm.PlaceAnimal(fillObj as AnimalDefinition, tile.TileId, out far);
                            workCount++;
                        }
                    });
                    break;
                case ShopItemType.Pond:
                    action = new FillAction((tile) =>
                    {
                        if (tile.CheckEmpty(out far))
                        {
                            farm.PlacePond(fillObj as PondDefinition, tile.TileId, out far);
                            workCount++;
                        }
                    });
                    break;
                case ShopItemType.Building:
                case ShopItemType.House:
                case ShopItemType.Decoration:
                    action = new FillAction((tile) =>
                    {
                        if (farm.CanPlaceBuilding(fillObj as BaseBuildingDefinition, tile.TileId, 0, out far))
                        {
                            farm.PlaceBuilding(fillObj as BaseBuildingDefinition, tile.TileId, 0, farm.GetNextBuildingId(), out far);
                            workCount++;
                        }
                    });
                    break;
                case ShopItemType.Road:
                    contentType = Assembly.Load("Assembly-CSharp").GetType("Logic.Farm.Contents.FarmRoad");
                    action = new FillAction((tile) =>
                    {
                        if (tile.CheckEmpty(out far))
                        {
                            SetContent(tile, Activator.CreateInstance(contentType, fillObj, tile) as FarmTileContents);
                            workCount++;
                        }
                    });
                    break;
                case ShopItemType.Flower:
                    contentType = Assembly.Load("Assembly-CSharp").GetType("Logic.Farm.Contents.FarmFlower");
                    action = new FillAction((tile) =>
                    {
                        if (tile.CheckEmpty(out far))
                        {
                            FarmTileContents content = Activator.CreateInstance(contentType, fillObj, tile) as FarmTileContents;
                            Traverse.Create(content).Field("state").SetValue(1); //设置花为成熟状态
                            SetContent(tile, content);
                            workCount++;
                        }
                    });
                    break;
            }
            if (action != null)
            {
                startFill = true;
                for (int x = 0; x < 280; x++)
                {
                    for (int y = 0; y < 140; y++)
                    {
                        if (IsNearBlack(pic.GetPixel(x, y)))
                        {
                            var tile = farm.GetTile(GetId(x, y));
                            if (tile != null)
                            {
                                action(tile);
                            }
                        }
                    }
                }
                startFill = false;
                mod.Logger.Log($"导入完毕，共填充{workCount}个地块");
            }
            else
            {
                mod.Logger.Log($"填充失败，未定义此类物品的填充方法");
            }
        }

        public static void SetContent(FarmTile tile, FarmTileContents content)
        {
            var ct = Traverse.Create(tile).Field("contents");
            ct.SetValue(content);
            if (content != null)
            {
                FarmTileContentsType category = content.Category;
                var tsc = Traverse.Create(tile).Field("tileState");
                switch (category)
                {
                    case FarmTileContentsType.Crop:
                        tsc.SetValue(FarmTileState.Plow);
                        break;
                    default:
                        if (category != FarmTileContentsType.Flower)
                        {
                            tsc.SetValue(FarmTileState.Building);
                        }
                        else
                        {
                            tsc.SetValue(FarmTileState.Flower);
                        }
                        break;
                    case FarmTileContentsType.Animal:
                        tsc.SetValue(FarmTileState.Animal);
                        break;
                    case FarmTileContentsType.Pond:
                        tsc.SetValue(FarmTileState.Pond);
                        break;
                }
            }
        }

        [HarmonyPatch(typeof(FarmTile), "SetContent")]
        class FarmTileSetContentPatch
        {
            public static bool Prefix(FarmTile __instance, FarmTileContents content)
            {
                if (mod.Enabled)
                {
                    if (startFill)
                    {
                        SetContent(__instance, content);
                        return false;
                    }
                }
                return true;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class OpenFileName
        {
            public int structSize = 0;
            public IntPtr dlgOwner = IntPtr.Zero;
            public IntPtr instance = IntPtr.Zero;
            public String filter = null;
            public String customFilter = null;
            public int maxCustFilter = 0;
            public int filterIndex = 0;
            public String file = null;
            public int maxFile = 0;
            public String fileTitle = null;
            public int maxFileTitle = 0;
            public String initialDir = null;
            public String title = null;
            public int flags = 0;
            public short fileOffset = 0;
            public short fileExtension = 0;
            public String defExt = null;
            public IntPtr custData = IntPtr.Zero;
            public IntPtr hook = IntPtr.Zero;
            public String templateName = null;
            public IntPtr reservedPtr = IntPtr.Zero;
            public int reservedInt = 0;
            public int flagsEx = 0;
        }

        public class LocalDialog
        {
            //链接指定系统函数       打开文件对话框
            [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
            public static extern bool GetOpenFileName([In, Out] OpenFileName ofn);
            public static bool GetOFN([In, Out] OpenFileName ofn)
            {
                return GetOpenFileName(ofn);
            }

            //链接指定系统函数        另存为对话框
            [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
            public static extern bool GetSaveFileName([In, Out] OpenFileName ofn);
            public static bool GetSFN([In, Out] OpenFileName ofn)
            {
                return GetSaveFileName(ofn);
            }
        }
    }
}
