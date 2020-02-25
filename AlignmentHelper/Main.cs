using System;
using Harmony12;
using UnityEngine;
using Milkstone.Utils;
using System.Reflection;
using UnityModManagerNet;

namespace AlignmentHelper
{
    public static class Main
    {
        public static UnityModManager.ModEntry mod;
        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            mod = modEntry;
            HarmonyInstance.Create(modEntry.Info.Id).PatchAll(Assembly.GetExecutingAssembly());
            mod.OnGUI = OnGUI;
            return true;
        }

        /// <summary>
        /// 农场是否载入
        /// </summary>
        public static bool IsLoadFarm()
        {
            if (StageScript.Instance == null) return false;
            if (!StageScript.Instance.Loaded) return false;
            return true;
        }

        #region 属性
        public static bool showCenter = true;
        public static bool ShowCenter
        {
            get { return showCenter; }
            set
            {
                if (showCenter != value)
                {
                    showCenter = value;
                    Refresh();
                }
            }
        }
        public static bool showBorder = true;
        public static bool ShowBorder
        {
            get { return showBorder; }
            set
            {
                if (showBorder != value)
                {
                    showBorder = value;
                    Refresh();
                }
            }
        }
        public static bool showDiagonal = true;
        public static bool ShowDiagonal
        {
            get { return showDiagonal; }
            set
            {
                if (showDiagonal != value)
                {
                    showDiagonal = value;
                    Refresh();
                }
            }
        }
        public static bool show = false;
        #endregion
        public static GameObject AlignmentHelperCenterRoot, AlignmentHelperBorderRoot, AlignmentHelperDiagonalRoot;
        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.Label("作者:xiaoye97 群内昵称:夜空之下");
            GUILayout.Label("FarmTogether交流群:973116708 bug反馈请加群找我");
            if (mod.Enabled)
            {
                if (IsLoadFarm())
                {
                    GUILayout.BeginHorizontal();
                    ShowCenter = GUILayout.Toggle(ShowCenter, "区块中心", GUILayout.Width(100));
                    ShowBorder = GUILayout.Toggle(ShowBorder, "区块边界", GUILayout.Width(100));
                    ShowDiagonal = GUILayout.Toggle(ShowDiagonal, "世界对角线", GUILayout.Width(200));
                    GUILayout.EndHorizontal();
                    if (GUILayout.Button((show && (AlignmentHelperCenterRoot || AlignmentHelperBorderRoot || AlignmentHelperDiagonalRoot) ? "关闭" : "开启") + "显示", GUILayout.Width(200)))
                    {
                        show = !(show && (AlignmentHelperCenterRoot || AlignmentHelperBorderRoot || AlignmentHelperDiagonalRoot));
                        Refresh();
                    }
                }
                else
                {
                    GUILayout.Label("农场未加载，请在载入农场后操作");
                }
            }
        }

        /// <summary>
        /// 刷新显示
        /// </summary>
        public static void Refresh()
        {
            try
            {
                Int2 count = GameGlobals.FarmChunkSize; // 7 7
                Vector2 size = GameGlobals.ChunkWorldSize; // 80 40
                Vector2 worldSize = GameGlobals.FarmWorldSize; // 560 280

                //区块中心
                if (show && showCenter)
                {
                    if (AlignmentHelperCenterRoot == null)
                    {
                        AlignmentHelperCenterRoot = new GameObject("AlignmentHelperCenterRoot");
                        foreach (var chunk in StageScript.Instance.FarmData.Chunks)
                        {
                            var pos = chunk.BaseChunkPosition;
                            mod.Logger.Log(pos.ToString());
                            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                            go.transform.localScale = new Vector3(0.1f, 10, 0.1f);
                            go.transform.position = new Vector3(pos.x + GameGlobals.ChunkSize.x, 5, pos.y + GameGlobals.ChunkSize.y);
                            go.GetComponent<Collider>().enabled = false;
                            go.transform.parent = AlignmentHelperCenterRoot.transform;
                        }
                    }
                    else
                    {
                        AlignmentHelperCenterRoot.SetActive(true);
                    }
                }
                else
                {
                    if (AlignmentHelperCenterRoot != null) AlignmentHelperCenterRoot.SetActive(false);
                }

                //区块边界
                if (show && ShowBorder)
                {
                    if (AlignmentHelperBorderRoot == null)
                    {
                        AlignmentHelperBorderRoot = new GameObject("AlignmentHelperBorderRoot");
                        for (int x = 0; x <= count.x; x++)
                        {
                            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            go.GetComponent<Collider>().enabled = false;
                            go.transform.localScale = new Vector3(0.1f, 0.1f, worldSize.y);
                            go.transform.position = new Vector3(x * size.x, 1, worldSize.y / 2);
                            go.transform.parent = AlignmentHelperBorderRoot.transform;
                        }
                        for (int z = 0; z <= count.y; z++)
                        {
                            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            go.GetComponent<Collider>().enabled = false;
                            go.transform.localScale = new Vector3(count.x * size.x, 0.1f, 0.1f);
                            go.transform.position = new Vector3(worldSize.x / 2, 1, z * size.y);
                            go.transform.parent = AlignmentHelperBorderRoot.transform;
                        }
                    }
                    else
                    {
                        AlignmentHelperBorderRoot.SetActive(true);
                    }
                }
                else
                {
                    if (AlignmentHelperBorderRoot != null) AlignmentHelperBorderRoot.SetActive(false);
                }


                //世界对角线
                if (show && showDiagonal)
                {
                    if (AlignmentHelperDiagonalRoot == null)
                    {
                        AlignmentHelperDiagonalRoot = new GameObject("AlignmentHelperDiagonalRoot");
                        float lineLen = Mathf.Sqrt(worldSize.x * worldSize.x + worldSize.y * worldSize.y);
                        var linea = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        linea.GetComponent<Collider>().enabled = false;
                        var lineb = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        lineb.GetComponent<Collider>().enabled = false;
                        linea.transform.localScale = new Vector3(lineLen, 0.1f, 0.1f);
                        lineb.transform.localScale = new Vector3(lineLen, 0.1f, 0.1f);
                        linea.transform.position = new Vector3(worldSize.x / 2, 1, worldSize.y / 2);
                        lineb.transform.position = new Vector3(worldSize.x / 2, 1, worldSize.y / 2);
                        linea.transform.Rotate(Vector3.up, 26.565f);
                        lineb.transform.Rotate(Vector3.up, -26.565f);
                        linea.transform.parent = AlignmentHelperDiagonalRoot.transform;
                        lineb.transform.parent = AlignmentHelperDiagonalRoot.transform;
                    }
                    else
                    {
                        AlignmentHelperDiagonalRoot.SetActive(true);
                    }
                }
                else
                {
                    if (AlignmentHelperDiagonalRoot != null) AlignmentHelperDiagonalRoot.SetActive(false);
                }

            }
            catch (Exception e)
            {
                mod.Logger.Log(e.Message);
                mod.Logger.Log(e.StackTrace);
            }
        }
    }
}
