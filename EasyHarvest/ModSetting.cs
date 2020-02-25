using UnityEngine;
using UnityModManagerNet;

namespace EasyHarvest
{
    public class ModSetting : UnityModManager.ModSettings
    {
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save<ModSetting>(this, modEntry);
        }

        public bool animalHarvestToggle = true;
        public bool cropHarvestToggle = true;
        public bool flowerHarvestToggle = true;
        public bool treeHarvestToggle = true;
        public bool pondHarvestToggle = true;
        public bool animalRefillToggle = true;
        public bool cropRefillToggle = true;
        public bool flowerRefillToggle = true;
        public bool hotKeyToggle = true;
        public bool logToggle = true;
        public bool allowHarvestToggle = false;
        public bool allowRefillToggle = false;
        public KeyCode harvestHotkey = KeyCode.E;
        public KeyCode refillHotkey = KeyCode.R;
    }
}
