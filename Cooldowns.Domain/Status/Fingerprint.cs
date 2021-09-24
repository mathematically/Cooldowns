using System.Collections.Generic;

namespace Cooldowns.Domain.Status
{
    public class Fingerprint<T> where T : notnull
    {
        public string Name { get; init; } = default!;
        public List<Testpoint<T>> Tests { get; init; } = new();
        public T MissingValue { get; init; } = default!;
    }
}