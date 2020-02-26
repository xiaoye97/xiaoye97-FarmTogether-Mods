using System.IO;
using UnityEngine;
using UnityEngine.Video;
using System.Collections.Generic;

namespace VideoTV
{
    public class TV
    {
        public static GameObject ScreenPrefab; //预制体
        private GameObject screen; //屏幕
        private VideoPlayer video; //播放器
        private int nowIndex = 0; //当前视频索引
        private List<string> localVideoList = new List<string>(); //本地视频列表
        private bool loaded = false;
        public bool Loaded
        {
            get { return loaded && screen != null; }
        }
        public float Volume
        {
            get { return Main.setting.volume; }
            set
            {
                Main.setting.volume = value;
                video.SetDirectAudioVolume(0, value);
            }
        }
        private string tmpUrl = ""; //添加视频使用的缓存url

        public TV()
        {
            if (ScreenPrefab == null)
            {
                LoadResources();
            }
            var tv = GameObject.Find("ArmarioTV");
            if (tv == null)
            {
                Main.mod.Logger.Log("没有在场景内找到电视");
                return;
            }
            screen = GameObject.Instantiate(ScreenPrefab);
            screen.transform.position = tv.transform.parent.position;
            screen.transform.rotation = tv.transform.parent.rotation;
            screen.transform.parent = tv.transform.parent;
            screen.transform.Translate(-0.0124f, 0.905f, 0.243f);
            screen.transform.localScale = new Vector3(1.122f, 0.674f, 1f);
            video = screen.GetComponent<VideoPlayer>();
            video.SetDirectAudioVolume(0, Main.setting.volume);
            RefreshVideosList();
            loaded = true;
        }

        /// <summary>
        /// 界面
        /// </summary>
        public void TVGUI()
        {
            GUILayout.BeginVertical("电视", GUI.skin.window);
            GUILayout.Label($"当前播放的url:{video.url}");
            GUILayout.BeginHorizontal();
            GUILayout.Label("音量:", GUILayout.Width(100));
            Volume = GUILayout.HorizontalSlider(Volume, 0, 1);
            GUILayout.EndHorizontal();
            GUILayout.Space(30);
            //控制区
            if (GUILayout.Button($"上一个视频({Main.setting.上一个视频快捷键})", GUILayout.Height(50))) PlayBack();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button($"后退({Main.setting.后退快捷键})", GUILayout.ExpandWidth(true), GUILayout.Height(40))) ToLeft();
            if (GUILayout.Button($"{(video.isPlaying ? "暂停" : "播放")}({Main.setting.播放快捷键})", GUILayout.ExpandWidth(true), GUILayout.Height(40))) PlayOrPause();
            if (GUILayout.Button($"重新播放({Main.setting.重新播放快捷键})", GUILayout.ExpandWidth(true), GUILayout.Height(40))) RePlayVideo();
            if (GUILayout.Button($"快进({Main.setting.快进快捷键})", GUILayout.ExpandWidth(true), GUILayout.Height(40))) ToRight();
            GUILayout.EndHorizontal();
            if (GUILayout.Button($"下一个视频({Main.setting.下一个视频快捷键})", GUILayout.Height(50))) PlayNext();
            GUILayout.BeginHorizontal();
            GUILayout.Label("快进间隔(秒)", GUILayout.Width(200));
            float.TryParse(GUILayout.TextField(Main.setting.speedTime.ToString("f2"), GUILayout.Width(200)), out Main.setting.speedTime);
            GUILayout.EndHorizontal();
            GUILayout.Space(30);
            //本地视频库
            GUILayout.BeginVertical("本地视频库(Videos文件夹)", GUI.skin.window);
            if (GUILayout.Button("刷新")) RefreshVideosList();
            GUILayout.Label("视频列表:");
            for (int i = 0; i < localVideoList.Count; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.TextField(localVideoList[i]);
                if (GUILayout.Button("播放", GUILayout.Width(200)))
                {
                    nowIndex = i;
                    RePlayVideo();
                }
                GUILayout.EndHorizontal();
            }
            if(localVideoList.Count == 0)
            {
                GUILayout.Label("无本地视频，可在Mod文件夹下Videos文件夹中添加mp4视频");
            }
            GUILayout.EndVertical();
            GUILayout.Space(30);
            //外部视频库
            GUILayout.BeginVertical("外部视频库(手动添加)", GUI.skin.window);
            GUILayout.Label("添加视频链接:");
            GUILayout.BeginHorizontal();
            tmpUrl = GUILayout.TextField(tmpUrl);
            if(GUILayout.Button("添加", GUILayout.Width(100)))
            {
                Main.setting.videoUrlList.Add(tmpUrl);
            }
            GUILayout.EndHorizontal();
            GUILayout.Label("视频列表:");
            for (int i = 0; i < Main.setting.videoUrlList.Count; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.TextField(Main.setting.videoUrlList[i]);
                if (GUILayout.Button("播放", GUILayout.Width(200)))
                {
                    nowIndex = i + localVideoList.Count;
                    RePlayVideo();
                }
                if(Main.setting.videoUrlList[i].StartsWith("http"))
                {
                    if (StageScript.Instance.IsOnline)
                    {
                        if (GUILayout.Button("邀请一起看", GUILayout.Width(200)))
                        {
                            nowIndex = i + localVideoList.Count;
                            SendVideoInvite();
                        }
                    }
                }
                if (GUILayout.Button("删除", GUILayout.Width(100)))
                {
                    Main.setting.videoUrlList.RemoveAt(i);
                }
                GUILayout.EndHorizontal();
            }
            if(Main.setting.videoUrlList.Count == 0)
            {
                GUILayout.Label("无");
            }
            GUILayout.Label("邀请一起看视频要求视频地址必须为视频外链，也就是真实链接，不能直接用b站链接之类的。并且视频链接必须以http或者https开头。");
            GUILayout.EndVertical();
            GUILayout.EndVertical();
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        private static void LoadResources()
        {
            AssetBundle ab = AssetBundle.LoadFromFile($"{Main.mod.Path}xy.tvscreen");
            ScreenPrefab = ab.LoadAsset<GameObject>("TVVideoPlayer");
            ab.Unload(false);
        }

        /// <summary>
        /// 刷新视频列表
        /// </summary>
        public void RefreshVideosList()
        {
            localVideoList.Clear();
            DirectoryInfo videoFolder = new DirectoryInfo($"{Main.mod.Path}Videos\\");
            if(!videoFolder.Exists)
            {
                Main.mod.Logger.Log("视频库文件夹不存在");
                return;
            }
            var files = videoFolder.GetFiles("*.mp4");
            foreach(var file in files)
            {
                localVideoList.Add(file.FullName);
            }
        }

        /// <summary>
        /// 播放或暂停
        /// </summary>
        public void PlayOrPause()
        {
            if(video.isPlaying) //正在播放则暂停
            {
                video.Pause();
            }
            else
            {
                if(string.IsNullOrEmpty(video.url)) //如果url为空，则播放默认视频
                {
                    if(localVideoList.Count > 0)
                    {
                        video.url = localVideoList[0];
                        video.Play();
                    }
                }
                else //如果不为空，则继续播放
                {
                    video.Play();
                }
            }
        }

        /// <summary>
        /// 重新播放
        /// </summary>
        public void RePlayVideo()
        {
            video.Stop();
            video.url = GetUrl();
            video.Play();
        }

        /// <summary>
        /// 播放下一个视频
        /// </summary>
        public void PlayNext()
        {
            nowIndex++;
            RePlayVideo();
        }

        /// <summary>
        /// 播放上一个视频
        /// </summary>
        public void PlayBack()
        {
            nowIndex--;
            RePlayVideo();
        }

        /// <summary>
        /// 快进
        /// </summary>
        public void ToRight()
        {
            video.frame = (long)Mathf.Clamp(video.frame + Main.setting.speedTime * video.frameRate, 0, video.frameCount);
        }

        /// <summary>
        /// 后退
        /// </summary>
        public void ToLeft()
        {
            video.frame = (long)Mathf.Clamp(video.frame - Main.setting.speedTime * video.frameRate, 0, video.frameCount);
        }

        /// <summary>
        /// 得到视频链接
        /// </summary>
        public string GetUrl()
        {
            if(nowIndex < 0) //如果索越过左边界，则跳转到右边界
            {
                nowIndex = localVideoList.Count + Main.setting.videoUrlList.Count - 1;
                return GetUrl();
            }
            else if (nowIndex < localVideoList.Count) //如果索引在本地范围内，返回本地地址
            {
                return localVideoList[nowIndex];
            }
            else if (nowIndex < localVideoList.Count + Main.setting.videoUrlList.Count) //如果索引在外部视频范围内，返回外部视频地址
            {
                return Main.setting.videoUrlList[nowIndex - localVideoList.Count];
            }
            else //如果索引越过右边界，则跳转到左边界
            {
                nowIndex = 0;
                return GetUrl();
            }
        }

        /// <summary>
        /// 发送视频邀请
        /// </summary>
        public void SendVideoInvite()
        {
            MilkUIChat chat = GameObject.FindObjectOfType<MilkUIChat>();
            if (chat != null)
            {
                chat.Filter.FilterText($"[视频电视mod]一起来看:{GetUrl()}", 0);
            }
        }

        /// <summary>
        /// 响应视频邀请
        /// </summary>
        public void HandleVideoInvite(string url)
        {
            if (video.isPlaying) video.Stop();
            video.url = url;
            video.Play();
        }
    }
}
