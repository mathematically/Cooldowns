namespace Cooldowns.Domain.Config
{
    public class Config
    {
        public ToolbarConfig Toolbar { get; init; } = new();
        public KeyConfig Q { get; init; } = null!;
        public KeyConfig W { get; init; } = null!;
        public KeyConfig E { get; init; } = null!;
        public KeyConfig R { get; init; } = null!;
        public int IntervalMilliseconds { get; init; } = 100;
    }
}