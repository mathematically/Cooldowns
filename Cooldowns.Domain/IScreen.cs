using System.Drawing;

namespace Cooldowns.Domain
{
    public interface IScreen
    {
        public Color GetPixelColor(int x, int y);
    }
}