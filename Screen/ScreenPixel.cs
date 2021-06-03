using System.Drawing;

namespace Cooldowns.Screen
{
    public static class ScreenPixel
    {
        public static Color GetColor(int x, int y)
        {
            using var bitmap = new Bitmap(1, 1);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(new Point(x, y), new Point(0, 0), new Size(1, 1));
            }

            return bitmap.GetPixel(0, 0);
        }
    }
}