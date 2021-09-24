using System;
using System.Collections.Generic;
using Cooldowns.Domain.Buttons;
using Cooldowns.Domain.Screen;
using NLog;

namespace Cooldowns.Domain.Status
{
    public sealed class StatusChecker<T> : IDisposable where T : notnull
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly IScreen screen;
        private readonly IDispatcher dispatcher;
        private readonly ICooldownTimer cooldownTimer;

        private List<Fingerprint<T>> Fingerprints { get; init; }

        private T? state;
        public event EventHandler<T>? StatusChanged;

        public StatusChecker(IScreen screen, IDispatcher dispatcher, ICooldownTimer cooldownTimer,
            List<Fingerprint<T>> fingerprints)
        {
            this.screen = screen;
            this.dispatcher = dispatcher;
            this.cooldownTimer = cooldownTimer;

            Fingerprints = fingerprints;

            cooldownTimer.Ticked += CooldownTimerOnTicked;
        }

        private void CooldownTimerOnTicked(object? sender, EventArgs e)
        {
            CheckState();
        }

        public void CheckState()
        {
            int fn = 0;
            foreach (var fingerprint in Fingerprints)
            {
                log.Debug($"Checking Fingerprint {fingerprint.Name} current={state}.");

                fn++;
                int tn = 0;
                foreach (var test in fingerprint.Tests)
                {
                    bool match = true;
                    log.Debug($"{fingerprint.Name} test #{tn++} for {test.State}");

                    for (int i = 0; i < test.Points.Count; i++)
                    {
                        var p = test.Points[i];
                        var c = test.Colors[i];

                        var px = screen.GetPixelColor(p.X, p.Y);

                        if (!Color.IsExactMatch(px, c))
                        {
                            log.Debug($"{fn}.{tn}.{i}.{test.State} test FAILED at {p} with {px} looking for {c}.");
                            match = false;
                            break;
                        }
                        else
                        {
                            log.Debug($"{fn}.{tn}.{i}.{test.State} test PASSED at {p} with {px}.");
                        }
                    }

                    if (match)
                    {
                        log.Debug($"{fn}.{tn}.{test.State} MATCHED.");
                        OnStatusChanged(test.State);
                        return;
                    }
                    else
                    {
                        log.Debug($"{fn}.{tn}.{test.State} DID NOT MATCH.");
                    }
                }

                // log.Debug($"Using MISSING Value {fingerprint.MissingValue}");
                // OnStatusChanged(fingerprint.MissingValue);
                return;
            }
        }

        private void OnStatusChanged(T state)
        {
            if (state.Equals(this.state)) return;
            this.state = state;
            dispatcher.Invoke(() => StatusChanged?.Invoke(this, this.state));
        }

        public void Dispose()
        {
            cooldownTimer.Ticked -= CooldownTimerOnTicked;
        }
    }
}