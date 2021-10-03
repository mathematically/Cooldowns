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
            if (HasStatus())
            {
                log.Debug($"{statusCheckInfo.Name} status FOUND checking for current value.");
                var exactState = IdentifyExactState();

                if (exactState.Equals(statusCheckInfo.MissingValue))
                {
                    // We are either misconfigured or are in some indeterminate state so don't raise event
                    log.Debug($"Couldn't find status for {statusCheckInfo.Name} status remains unchanged at {state}");
                    return;

                }

                log.Debug($"{statusCheckInfo.Name} has status {exactState}.");
                state = exactState;
                OnStatusChanged(state);
            }
            else
            {
                log.Debug($"{statusCheckInfo.Name} status fingerprint NOT FOUND, state set to {statusCheckInfo.MissingValue}");
                state = statusCheckInfo.MissingValue;
                OnStatusChanged(statusCheckInfo.MissingValue);
            }
        }

        private bool HasStatus()
        {
            return CheckScreenForStatus(statusCheckInfo.StatusFingerprint);
        }

        private T IdentifyExactState()
        {
            log.Debug($"Checking StatusCheck {statusCheckInfo.Name} current={state}.");

            foreach (var fingerprint in statusCheckInfo.StatusValueFingerprints.Where(CheckScreenForStatus))
            {
                log.Debug($"{statusCheckInfo.Name} MATCHED was {state} now {fingerprint.State}.");
                return fingerprint.State;
            }

            return statusCheckInfo.MissingValue;
        }

        private bool CheckScreenForStatus(Fingerprint<T> fingerprint)
        {
            for (int i = 0; i < fingerprint.Points.Count; i++)
            {
                var p = fingerprint.Points[i];
                var c = fingerprint.Colors[i];

                var px = screen.GetPixelColor(p.X, p.Y);

                if (!Color.IsExactMatch(px, c))
                {
                    return false;
                }
            }

            return true;
        }

        private void OnStatusChanged(T state)
        {
            dispatcher.BeginInvoke(() => StatusChanged?.Invoke(this, state));
        }

        public void Dispose()
        {
            cooldownTimer.Ticked -= CooldownTimerOnTicked;
        }
    }
}