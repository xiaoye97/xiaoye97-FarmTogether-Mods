using Harmony12;
using System.IO;
using UnityEngine;
using System.Reflection;
using UnityEngine.Video;
using UnityModManagerNet;

namespace VideoTV
{
    public static class Main
    {
        public static UnityModManager.ModEntry.ModLogger logger;
        public static GameObject screen, tv;
        public static AudioSource screenAudio;
        public static VideoPlayer videoPlayer;
        public static GameObject screenPrefab;
        public static int selectToDel = 2, nowSelectVideo = 0;
        public static ModSetting setting;
        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            setting = UnityModManager.ModSettings.Load<ModSetting>(modEntry);
            logger = modEntry.Logger;
            HarmonyInstance.Create(modEntry.Info.Id).PatchAll(Assembly.GetExecutingAssembly());

            AssetBundle ab = AssetBundle.LoadFromMemory(File.ReadAllBytes("Mods/VideoTV/Resources/xy.tvscreen"));
            screenPrefab = ab.LoadAsset<GameObject>("TVVideoPlayer");
            ab.Unload(false);
            if (setting.videoUrlList.Count <= 0) setting.videoUrlList.Add(modEntry.Path + "Resources\\Video.mp4");
            else setting.videoUrlList[0] = modEntry.Path + "Resources\\Video.mp4";

            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnHideGUI = OnHideGUI;
            return true;
        }

        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.Label("作者:xiaoye97 群内昵称:夜空之下");
            GUILayout.Label("FarmTogether交流群:973116708 bug反馈请加群找我");
            GUILayout.BeginHorizontal();
            GUILayout.Label("视频路径库:");
            if(GUILayout.Button("增加新视频"))
            {
                setting.videoUrlList.Add("");
            }
            if (GUILayout.Button("删除指定位置的视频"))
            {
                if(selectToDel - 1 > 0)
                {
                    setting.videoUrlList.RemoveAt(selectToDel - 1);
                    if(nowSelectVideo >= setting.videoUrlList.Count)
                    {
                        nowSelectVideo = setting.videoUrlList.Count - 1;
                    }
                }
            }
            GUILayout.Label("要删除的视频序号");
            selectToDel = int.Parse(GUILayout.TextField(selectToDel.ToString()));
            Mathf.Clamp(selectToDel, 1, setting.videoUrlList.Count);
            GUILayout.EndHorizontal();
            GUILayout.BeginVertical();
            for(int i = 0; i < setting.videoUrlList.Count; i++)
            {
                setting.videoUrlList[i] = GUILayout.TextField(setting.videoUrlList[i]);
            }
            GUILayout.EndVertical();
            GUILayout.Label("快进间隔: (秒)");
            setting.speedTime = float.Parse(GUILayout.TextField(setting.speedTime.ToString()));
            
            if(screen != null)
            {
                if (GUILayout.Button("播放或停止 快捷键(K)"))
                {
                    PlayOrStopVideo();
                }
            }
            else
            {
                if (GUILayout.Button("寻找TV并实例化屏幕"))
                {
                    InitScreen();
                }
            }
        }

        public static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            setting.Save(modEntry);
        }

        public static void OnHideGUI(UnityModManager.ModEntry modEntry)
        {
            setting.Save(modEntry);
        }

        public static void SetScreenPosion(Transform screen)
        {
            screen.Translate(-0.0124f, 0.905f, 0.243f);
            screen.localScale = new Vector3(1.122f, 0.674f, 1f);
        }

        public static void InitScreen()
        {
            tv = GameObject.Find("ArmarioTV");
            if (tv != null)
            {
                logger.Log("找到了TV");
                screen = GameObject.Instantiate(screenPrefab);
                screen.transform.position = tv.transform.parent.position;
                screen.transform.rotation = tv.transform.parent.rotation;
                screen.transform.parent = tv.transform.parent;
                SetScreenPosion(screen.transform);
                screenAudio = screen.GetComponent<AudioSource>();
                videoPlayer = screen.GetComponent<VideoPlayer>();
            }
            else
            {
                logger.Log("未找到TV");
            }
        }

        public static void PlayOrStopVideo()
        {
            if(screen != null)
            {
                if(videoPlayer.isPlaying)
                {
                    videoPlayer.Stop();
                }
                else
                {
                    videoPlayer.url = setting.videoUrlList[nowSelectVideo];
                    videoPlayer.Play();
                }
            }
        }

        public static void RePlayVideo()
        {
            videoPlayer.Stop();
            videoPlayer.url = setting.videoUrlList[nowSelectVideo];
            videoPlayer.Play();
        }

        [HarmonyPatch(typeof(GameHud), "Update")]
        class InputPatch
        {
            public static void Postfix()
            {
                if(Input.GetKeyDown(KeyCode.P))
                {
                    if(screen != null)
                    {
                        PlayOrStopVideo();
                    }
                    else
                    {
                        InitScreen();
                    }
                }
                if(screen != null && videoPlayer != null)
                {
                    if(Input.GetKeyDown(KeyCode.Keypad8))
                    {
                        nowSelectVideo--;
                        if(nowSelectVideo < 0)
                        {
                            nowSelectVideo = setting.videoUrlList.Count - 1;
                        }
                        RePlayVideo();
                    }
                    else if(Input.GetKeyDown(KeyCode.Keypad2))
                    {
                        nowSelectVideo++;
                        if(nowSelectVideo >= setting.videoUrlList.Count)
                        {
                            nowSelectVideo = 0;
                        }
                        RePlayVideo();
                    }
                    if (videoPlayer.isPrepared)
                    {
                        if (Input.GetKeyDown(KeyCode.Keypad6))
                        {
                            videoPlayer.frame = (long)Mathf.Clamp(videoPlayer.frame + setting.speedTime * videoPlayer.frameRate, 0, videoPlayer.frameCount);
                        }
                        else if (Input.GetKeyDown(KeyCode.Keypad4))
                        {
                            videoPlayer.frame = (long)Mathf.Clamp(videoPlayer.frame - setting.speedTime * videoPlayer.frameRate, 0, videoPlayer.frameCount);
                        }
                    }
                }
            }
        }
    }
}
