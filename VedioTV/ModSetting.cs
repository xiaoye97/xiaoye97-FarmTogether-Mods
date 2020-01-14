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

        public float speedTime = 30;
        public List<string> videoUrlList = new List<string>();
    }
}
