using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtraCrestSlots
{
    public class SettingCrest
    {
        public string? CrestID;
        public int SlotCount = 1;
        public float PositionX;
        public float PositionY;
        public SlotType SlotType;
        public bool? IsLocked;
        public int? OverrideSlot;
    }

    public enum SlotType
    {
        Auto,
        Blue,
        Yellow,
        RedUp,
        RedNeutral,
        RedDown,
        WhiteUp,
        WhiteNeutral,
        WhiteDown,
    }
}
