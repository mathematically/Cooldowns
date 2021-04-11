// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace Cooldowns.Configuration
{
    public class App
    {
        public Toolbar Toolbar { get; set; } = new();
        public Key Q { get; set; } = new();
        public Key W { get; set; } = new();
        public Key E { get; set; } = new();
        public Key R { get; set; } = new();
    }
}