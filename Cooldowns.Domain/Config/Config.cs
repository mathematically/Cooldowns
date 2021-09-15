namespace Cooldowns.Domain.Config
{
    public class Config
    {
        public ToolbarConfig Toolbar{ get; } = new();
        public KeyConfig Q { get; } = new();
        public KeyConfig W { get; } = new();
        public KeyConfig E { get; } = new();
        public KeyConfig R { get; } = new();
    }
}