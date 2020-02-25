using UnityEngine;
using UnityModManagerNet;

namespace SceneAnalyzer
{
    public class Main
    {
        public static UnityModManager.ModEntry mod;
        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            mod = modEntry;
            mod.OnGUI = OnGUI;
            return true;
        }

        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            if(GUILayout.Button("打开场景查看器"))
            {
                var window = new SceneAnalyzerWindow();
                window.Show();
            }
        }
    }
}
