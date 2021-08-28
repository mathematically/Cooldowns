namespace Cooldowns.Domain.Config
{
    public class CooldownsApp
    {
        public Toolbar Toolbar { get; } = new();
        public Key Q { get; } = new();
        public Key W { get; } = new();
        public Key E { get; } = new();
        public Key R { get; } = new();
    }
}