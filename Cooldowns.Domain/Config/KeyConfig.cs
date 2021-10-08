using Cooldowns.Domain.Buttons;

namespace Cooldowns.Domain.Config
{
    public class KeyConfig
    {
        public string Label { get; init; } = string.Empty;
        public ButtonType Type { get; init; } = ButtonType.Cooldown;
        public ButtonMode Mode { get; init; } = ButtonMode.Disabled;
        public string ActionKey { get; init; } = string.Empty;
        public int DetectX { get; init; }
        public int DetectY { get; init; }
    }
}
