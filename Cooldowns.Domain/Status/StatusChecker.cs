using System;
using Cooldowns.Domain.Screen;
using Cooldowns.Domain.Timer;
using NLog;

namespace Cooldowns.Domain.Status
{
    public sealed class StatusChecker<T> : IDisposable where T : notnull
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly IScreen screen;
        private readonly IDispatcher dispatcher;
        private readonly ICooldownTimer cooldownTimer;
        private readonly StatusCheckInfo<T> statusCheckInfo;

        private T state;
        public event EventHandler<T>? StatusChanged;

        public StatusChecker(IScreen screen, IDispatcher dispatcher, ICooldownTimer cooldownTimer, StatusCheckInfo<T> statusCheckInfo)
        {
            this.screen = screen;
            this.dispatcher = dispatcher;
            this.cooldownTimer = cooldownTimer;
            this.statusCheckInfo = statusCheckInfo;
            this.state = statusCheckInfo.MissingValue;

            cooldownTimer.Ticked += CooldownTimerOnTicked;
        }

        private void CooldownTimerOnTicked(object? sender, EventArgs e)
        {
            CheckState();
        }

        private const int MissLimit = 5;
        private int missCount;

        private void CheckState()
        {
            log.Debug($"Checking StatusCheck {statusCheckInfo.Name} current={state}.");

            foreach (var fingerprint in statusCheckInfo.Fingerprints)
            {
                bool match = true;
                log.Debug($"{statusCheckInfo.Name} test for {fingerprint.State}");

                for (int i = 0; i < fingerprint.Points.Count; i++)
                {
                    var p = fingerprint.Points[i];
                    var c = fingerprint.Colors[i];

                    var px = screen.GetPixelColor(p.X, p.Y);

                    if (Color.IsExactMatch(px, c))
                    {
                        log.Debug($"P{i}.{fingerprint.State} test PASSED at {p} with {px}.");
                    }
                    else
                    {
                        log.Debug($"P{i}.{fingerprint.State} test FAILED at {p} with {px} looking for {c}.");
                        match = false;
                        break;
                    }
                }

                if (match)
                {
                    log.Debug($"{statusCheckInfo.Name} MATCHED was {state} now {fingerprint.State}.");
                    state = fingerprint.State;
                    missCount = 0;
                    OnStatusChanged(fingerprint.State);
                    return;
                }

                log.Debug($"{statusCheckInfo.Name}.{fingerprint.State} DID NOT MATCH still {state}");
                missCount++;
            }

            if (missCount > MissLimit)
            {
                log.Debug($"Got {missCount} misses forcing {state} to MISSING Value {statusCheckInfo.MissingValue}");
                OnStatusChanged(statusCheckInfo.MissingValue);
                state = statusCheckInfo.MissingValue;
                missCount = 0;
            }
        }

        private void OnStatusChanged(T state)
        {
            dispatcher.Invoke(() => StatusChanged?.Invoke(this, state));
        }

        public void Dispose()
        {
            cooldownTimer.Ticked -= CooldownTimerOnTicked;
        }
    }
}