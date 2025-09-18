using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Shared.JsonNS;
using Newtonsoft.Json;

namespace ExtraCrestSlots
{
    public class Settings
    {
        [JsonIgnore] public string? FilePath;

        [JsonInclude] public int Version = 1;

        [JsonInclude]
        public List<SettingCrest> Slots = new()
        {
            new() { CrestID = "ALL", SlotCount = 6, PositionX = -4.5f, PositionY = -3.5f },
        };

        public void Save()
        {
            JsonTool.SerializeFile(FilePath!, this);
        }

        public static Settings Load()
        {
            string filePath = Path.Combine(BepInEx.Paths.ConfigPath, "ExtraCrestSlots.json");
            if (JsonTool.DeserializeFile(filePath, out _state))
            {
                _state.FilePath = filePath;
            }
            else
            {
                _state = new();
                _state.FilePath = filePath;
                _state.Save();
            }
            return _state;
        }

        private static Settings? _state;
        public static Settings State => _state ?? Load();
    }
}
