using System;
using System.Linq;
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
            if (HasState())
            {
                IdentifyExactState();
            }
        }

        private bool HasState()
        {
            var hasState = statusCheckInfo.HasState;

            for (int i = 0; i < hasState.Points.Count; i++)
            {
                var p = hasState.Points[i];
                var c = hasState.Colors[i];

                var px = screen.GetPixelColor(p.X, p.Y);

                if (!Color.IsExactMatch(px, c))
                {
                    log.Debug($"{statusCheckInfo.Name} status fingerprint NOT FOUND, state set to {statusCheckInfo.MissingValue}");
                    OnStatusChanged(statusCheckInfo.MissingValue);
                    return false;
                }
            }

            log.Debug($"{statusCheckInfo.Name} status fingerprint FOUND will check for value");
            return true;
        }

        private void IdentifyExactState()
        {
            log.Debug($"Checking StatusCheck {statusCheckInfo.Name} current={state}.");

            foreach (var v in statusCheckInfo.StateValues.Where(HasStateValue))
            {
                log.Debug($"{statusCheckInfo.Name} MATCHED was {state} now {v.State}.");
                state = v.State;
                OnStatusChanged(v.State);
                return;
            }
        }

        private bool HasStateValue(Fingerprint<T> fingerprint)
        {
            for (int i = 0; i < fingerprint.Points.Count; i++)
            {
                var p = fingerprint.Points[i];
                var c = fingerprint.Colors[i];

                var px = screen.GetPixelColor(p.X, p.Y);

                if (!Color.IsExactMatch(px, c))
                {
                    log.Debug($"FAILED at {p} with {px} looking for {c} not {fingerprint.State}");
                    return false;
                }
            }

            return true;
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