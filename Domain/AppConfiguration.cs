using WindowsInput.Native;

namespace Cooldowns.Domain
{
    public class AppConfiguration
    {
        public ToolbarConfig Toolbar { get; set; }
        public KeyConfig Q { get; set; }
        public KeyConfig W { get; set; }
        public KeyConfig E { get; set; }
        public KeyConfig R { get; set; }
    }
    
    public class ToolbarConfig
    {
        public int FontSize { get; set; }
    }

    public class KeyConfig
    {
        public bool Enabled { get; set; }
        public int Cooldown { get; set; }
        public bool Autocast { get; set; }
        public string AutocastKey { get; set; }
    }
}