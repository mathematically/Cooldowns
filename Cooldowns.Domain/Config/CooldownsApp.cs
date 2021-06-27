namespace Cooldowns.Domain.Config
{
    public class CooldownsApp
    {
        public Toolbar Toolbar { get; set; } = new();
        public Key Q { get; set; } = new();
        public Key W { get; set; } = new();
        public Key E { get; set; } = new();
        public Key R { get; set; } = new();
    }
}