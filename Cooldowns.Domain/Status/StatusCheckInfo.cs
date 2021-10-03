using System.Collections.Generic;

namespace Cooldowns.Domain.Status
{
    public class StatusCheckInfo<T> where T : notnull
    {
        public string Name { get; }
        public T MissingValue { get; }
        public Fingerprint<T> StatusFingerprint { get; }
        public List<Fingerprint<T>> StatusValueFingerprints { get; }

        public StatusCheckInfo(T missingValue, Fingerprint<T> statusFingerprint, List<Fingerprint<T>> statusValueFingerprints)
        {
            Name = typeof(T).FullName ?? string.Empty;
            MissingValue = missingValue;
            StatusFingerprint = statusFingerprint;
            StatusValueFingerprints = statusValueFingerprints;
        }
    }
}