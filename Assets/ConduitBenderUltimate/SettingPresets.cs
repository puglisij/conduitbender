using System;


namespace CB
{
    [Serializable]
    public class SettingPresets
    {
        /// <summary> Preset values are in inches </summary>
        public KeyFloatSet[] conduitDiameter;
        public KeyFloatSet[] benderRadius;
    }
}

