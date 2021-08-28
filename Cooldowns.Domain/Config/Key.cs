namespace Cooldowns.Domain.Config
{
    public class Key
    {
        public string Label { get; init; } = null!;
        public string ActionKey { get; init; } = null!;
        public string ModeKey { get; init; } = null!;
        public int DetectX { get; init; }
        public int DetectY { get; init; }
    }
}
