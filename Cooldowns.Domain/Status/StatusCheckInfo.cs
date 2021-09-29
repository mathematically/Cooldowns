using System.Collections.Generic;

namespace Cooldowns.Domain.Status
{
    public class StatusCheckInfo<T> where T : notnull
    {
        public string Name { get; }
        public T MissingValue { get; }
        public List<Fingerprint<T>> Fingerprints { get; }

        public StatusCheckInfo(T missingValue, List<Fingerprint<T>> fingerprints)
        {
            Name = nameof(T);
            MissingValue = missingValue;
            Fingerprints = fingerprints;
        }
    }
}