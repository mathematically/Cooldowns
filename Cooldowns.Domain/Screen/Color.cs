namespace Cooldowns.Domain.Screen
{
    public static class Color
    {
        private static readonly int colorTolerance = 5;

        public static bool IsMatch(System.Drawing.Color pixel, System.Drawing.Color target)
        {
            return pixel.R > target.R - colorTolerance && pixel.R < target.R + colorTolerance &&
                   pixel.G > target.G - colorTolerance && pixel.G < target.G + colorTolerance &&
                   pixel.B > target.B - colorTolerance && pixel.B < target.B + colorTolerance;
        }

        public static bool IsExactMatch(System.Drawing.Color pixel, System.Drawing.Color target)
        {
            return pixel.ToArgb().Equals(target.ToArgb());
        }
    }
}