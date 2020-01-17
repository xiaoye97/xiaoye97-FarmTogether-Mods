using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace FarmHarvestMonitor
{
    public class HarvestResult
    {
        public HarvestResult(byte playerSlot, FarmTileId[] tiles)
        {
            time = DateTime.Now;
            player = StageScript.Instance.GetPlayerFromSlot(playerSlot).PlayerSelection.Gamertag;
            farm = StageScript.Instance.FarmData.LocalFarmHeader.Name;
            foreach(var t in tiles)
            {
                var key = Localization.Get(StageScript.Instance.FarmData.GetTile(t).Contents.ShopItemDefinition.FullId);
                if (dict.ContainsKey(key))
                {
                    dict[key]++;
                }
                else
                {
                    dict.Add(key, 1);
                }
            }
        }

        public DateTime time;
        public string farm;
        public string player;
        public Dictionary<string, int> dict = new Dictionary<string, int>();

        public override string ToString()
        {
            string result = $"时间: {time.ToString()} 农场:{farm} 玩家: {player} 收获: ";
            foreach(var k in dict.Keys)
            {
                result += $"\n{k} {dict[k]}个";
            }
            return result;
        }

        public void ToFile()
        {
            string result = $"时间: {time.ToString()} 农场:{farm} 玩家: {player} 收获: ";
            foreach (var k in dict.Keys)
            {
                result += $"{k}{dict[k]}个 ";
            }
            StreamWriter sw = File.AppendText("收获记录.txt");
            sw.WriteLine(result);
            sw.Close();
            sw.Dispose();
        }
    }
}
