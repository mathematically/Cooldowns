// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace Cooldowns.Configuration
{
    public class Key
    {
        public string Label { get; set; } = null!;
        public bool Enabled { get; set; }
        public int Cooldown { get; set; }
        public bool AutoDetectCooldown { get; set; }
        public int DetectX { get; set; }
        public int DetectY { get; set; }
        public bool Autocast { get; set; }
        public string AutocastKey { get; set; } = null!;
    }
}