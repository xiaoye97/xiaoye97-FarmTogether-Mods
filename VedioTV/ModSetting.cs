using UnityEngine;
using UnityModManagerNet;
using System.Collections.Generic;

namespace VideoTV
{
    public class ModSetting : UnityModManager.ModSettings
    {
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save<ModSetting>(this, modEntry);
        }

        public KeyCode 播放快捷键 = KeyCode.P;
        public KeyCode 重新播放快捷键 = KeyCode.Alpha8;
        public KeyCode 后退快捷键 = KeyCode.Alpha9;
        public KeyCode 快进快捷键 = KeyCode.Alpha0;
        public KeyCode 上一个视频快捷键 = KeyCode.UpArrow;
        public KeyCode 下一个视频快捷键 = KeyCode.DownArrow;

        /// <summary>
        /// 快进间隔
        /// </summary>
        public float speedTime = 30;
        /// <summary>
        /// 音量
        /// </summary>
        public float volume = 1f;
        /// <summary>
        /// 外部视频地址列表
        /// </summary>
        public List<string> videoUrlList = new List<string>();
    }
}
