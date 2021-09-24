using System.Collections.Generic;
using System.Drawing;

namespace Cooldowns.Domain.Status
{
    public record Testpoint<T>
    {
        public List<Point> Points { get; init; } = new();
        public List<Color> Colors  { get; init; } = new();
        public T State { get; init; } = default!;
    }
}