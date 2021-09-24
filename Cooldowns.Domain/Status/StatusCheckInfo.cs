using System.Collections.Generic;

namespace Cooldowns.Domain.Status
{
    public class StatusCheckInfo<T> where T : notnull
    {
        public string Name { get; }
        public T MissingValue { get; }
        public Fingerprint<T> HasState { get; }
        public List<Fingerprint<T>> StateValues { get; }

        public StatusCheckInfo(T missingValue, Fingerprint<T> hasState, List<Fingerprint<T>> stateValues)
        {
            Name = typeof(T).FullName ?? string.Empty;
            MissingValue = missingValue;
            HasState = hasState;
            StateValues = stateValues;
        }
    }
}