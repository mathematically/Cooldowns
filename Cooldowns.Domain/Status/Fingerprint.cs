using System.Collections.Generic;
using System.Drawing;

namespace Cooldowns.Domain.Status
{
    public record Fingerprint<T>
    {
        public List<Point> Points { get; init; } = new();
        public List<Color> Colors  { get; init; } = new();
        public T State { get; init; }

        public Fingerprint(T state)
        {
            State = state;
        }
    }
}