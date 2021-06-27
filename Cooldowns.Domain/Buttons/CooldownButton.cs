// ReSharper disable TemplateIsNotCompileTimeConstantProblem

using System;
using System.Drawing;
using System.Threading;
using WindowsInput.Native;
using Cooldowns.Domain.Config;
using Cooldowns.Domain.Keyboard;
using NLog;

namespace Cooldowns.Domain.Buttons
{
    public sealed class CooldownButton
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();

        private static bool IsSkillAvailable(Color p) => p.R == 255 && p.G == 255 && p.B == 255;
        private static bool IsSkillOnCooldown(Color p) => p.R == 17 && p.G == 17 && p.B == 21;
        
        private const int AutoCheckInterval = 100;
        private const int AutoCheckDelay = 1000;

        private readonly IScreen screen;
        private readonly IDispatcher dispatcher;
        private readonly Key key;
        private readonly VirtualKeyCode autocastKeyCode;
        
        private CooldownButtonState buttonState;
        private Timer? timer;

        private readonly KeyboardSimulator keyboard = new();
        
        // ReSharper disable once ContextualLoggerProblem
        public CooldownButton(IScreen screen, IDispatcher dispatcher, Key key)
        {
            this.screen = screen;
            this.dispatcher = dispatcher;
            this.key = key;
            
            autocastKeyCode = Enum.Parse<VirtualKeyCode>(key.AutocastKey);
            
            // todo what are the rules for these settings? What combos are ok? Do we need cooldown at all now?

            if (key.Autocast)
            {
                log.Debug($"Key {key.Label} will auto cast as {autocastKeyCode}");
            }

            if (key.AutoDetectCooldown)
            {
                log.Debug($"Key {key.Label} will auto detect cooldonws based on screen pixels");
                timer = TimerFactory(AutoCheckInterval, AutoCheckDelay);
            }

            OnButtonStateChanged(key.Enabled ? CooldownButtonState.Up : CooldownButtonState.Disabled);
        }

        private Timer TimerFactory(int period, int dueTime)
        {
            return new(OnCooldownEnded, key.Label, dueTime, period);
        }

        public event EventHandler<CooldownButtonState>? ButtonStateChanged;

        private void OnButtonStateChanged(CooldownButtonState state)
        {
            buttonState = state;
            ButtonStateChanged?.Invoke(this, buttonState);
        }

        public void Press()
        {
            if (buttonState == CooldownButtonState.Disabled || key.AutoDetectCooldown) return;

            switch (key.Autocast)
            {
                case true when buttonState == CooldownButtonState.AutoCasting:
                    log.Debug($"Auto cast stopped for {key.Label} will be disabled on next timer end.");
                    OnButtonStateChanged(CooldownButtonState.OnCooldown);
                    
                    break;
                
                case true when buttonState == CooldownButtonState.Up:
                    log.Debug($"Enabling auto cast for {key.Label} as {autocastKeyCode}.");
                    timer = TimerFactory(key.Cooldown, key.Cooldown);
                    OnButtonStateChanged(CooldownButtonState.AutoCasting);
                    
                    break;
                
                default:
                    if (buttonState == CooldownButtonState.Up)
                    {
                        log.Debug($"Creating one shot timer for {key.Label}.");
                        timer = TimerFactory(Timeout.Infinite, key.Cooldown);
                        OnButtonStateChanged(CooldownButtonState.OnCooldown);
                    }

                    break;
            }
        }

        private void OnCooldownEnded(object? state)
        {
            dispatcher.BeginInvoke(() =>
            {
                if (key.AutoDetectCooldown)
                {
                    ProcessAutoDetectCooldown();
                }
                else
                {
                    ProcessCooldown();
                }
            });
        }

        private void ProcessAutoDetectCooldown()
        {
            var pixel = screen.GetPixelColor(key.DetectX, key.DetectY);
            
            var isAvailable = IsSkillAvailable(pixel);
            var isOnCooldown = IsSkillOnCooldown(pixel);

            if (isAvailable && buttonState == CooldownButtonState.OnCooldown)
            {
                OnButtonAvailable();
            }
            else if (isOnCooldown && buttonState is CooldownButtonState.Up)
            {
                OnButtonOnCooldown();
            }
            else if (!isAvailable && !isOnCooldown && buttonState is CooldownButtonState.Up)
            {
                log.Debug($"{key.Label} has been toggled on {pixel}");
                OnButtonOnCooldown();
            }
        }

        private void ProcessCooldown()
        {
            if (buttonState == CooldownButtonState.AutoCasting)
            {
                keyboard.PressKey(autocastKeyCode);
            }
            else
            {
                OnButtonAvailable();
            }
        }

        void OnButtonOnCooldown()
        {
            log.Debug($"Button {key.Label} on cooldown");
            UnloadOneShotTimer();

            OnButtonStateChanged(CooldownButtonState.OnCooldown);
        }

        void OnButtonAvailable()
        {
            log.Debug($"Button {key.Label} back up");
            UnloadOneShotTimer();

            OnButtonStateChanged(CooldownButtonState.Up);
        }

        private void UnloadOneShotTimer()
        {
            if (key.AutoDetectCooldown) return;
            UnloadTimer();
        }

        public void UnloadTimer()
        {
            timer?.Dispose();
            timer = null;
        }
    }
}