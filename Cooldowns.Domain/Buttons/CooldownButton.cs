using System;
using System.Drawing;
using WindowsInput.Native;
using Cooldowns.Domain.Config;
using Cooldowns.Domain.Keyboard;
using NLog;

namespace Cooldowns.Domain.Buttons
{
    public sealed class CooldownButton: IDisposable
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();

        public static readonly Color SkillAvailableColor = Color.FromArgb(255, 255, 255);
        public static readonly Color SkillCooldownColor = Color.FromArgb(17, 17, 21);

        private static bool IsSkillAvailable(Color p) => p.ToArgb().Equals(SkillAvailableColor.ToArgb());
        private static bool IsSkillOnCooldown(Color p) => p.ToArgb().Equals(SkillCooldownColor.ToArgb());
        
        private readonly IScreen screen;
        private readonly IKeyboard keyboard;
        private readonly ICooldownTimer cooldownTimer;
        private readonly Key key;
        private readonly VirtualKeyCode autocastKeyCode;

        private CooldownButtonState buttonState = CooldownButtonState.Disabled;

        public event EventHandler<CooldownButtonState>? ButtonStateChanged;

        private void OnButtonStateChanged(CooldownButtonState state)
        {
            if (buttonState == state) return;
            buttonState = state;
            ButtonStateChanged?.Invoke(this, buttonState);
        }

        public CooldownButton(IScreen screen, IKeyboard keyboard, ICooldownTimer cooldownTimer, Key key)
        {
            this.screen = screen;
            this.keyboard = keyboard;
            this.cooldownTimer = cooldownTimer;
            this.key = key;

            cooldownTimer.CooldownEnded += OnCooldownEnded;
            autocastKeyCode = Enum.Parse<VirtualKeyCode>(key.AutocastKey);
           
            // todo what are the rules for these settings? What combos are ok? Do we need cooldown at all now?

            if (key.Autocast)
            {
                log.Debug($"Key {key.Label} will auto cast as {autocastKeyCode}");
            }

            if (key.AutoDetectCooldown)
            {
                log.Debug($"Key {key.Label} will auto detect cooldonws based on screen pixels");
                cooldownTimer.StartRepeating();
            }

            OnButtonStateChanged(key.Enabled ? CooldownButtonState.Ready : CooldownButtonState.Disabled);
        }

        public void Press()
        {
            if (buttonState == CooldownButtonState.Disabled || key.AutoDetectCooldown) return;

            switch (key.Autocast)
            {
                case true when buttonState == CooldownButtonState.AutoCasting:
                    log.Debug($"Auto cast stopped for {key.Label} will be disabled on next timer end.");
                    OnButtonStateChanged(CooldownButtonState.Cooldown);
                    
                    break;
                
                case true when buttonState == CooldownButtonState.Ready:
                    log.Debug($"Enabling auto cast for {key.Label} as {autocastKeyCode}.");
                    cooldownTimer.StartRepeating();
                    OnButtonStateChanged(CooldownButtonState.AutoCasting);
                    
                    break;
                
                default:
                    if (buttonState == CooldownButtonState.Ready)
                    {
                        log.Debug($"Creating one shot timer for {key.Label}.");
                        cooldownTimer.StartOnce(key.Cooldown);
                        OnButtonStateChanged(CooldownButtonState.Cooldown);
                    }

                    break;
            }
        }

        private void OnCooldownEnded(object? o, EventArgs eventArgs)
        {
            if (key.AutoDetectCooldown)
            {
                ProcessAutoDetectCooldown();
            }
            else
            {
                ProcessCooldown();
            }
        }

        private void ProcessAutoDetectCooldown()
        {
            var pixel = screen.GetPixelColor(key.DetectX, key.DetectY);
            var isAvailable = IsSkillAvailable(pixel);
            var isOnCooldown = IsSkillOnCooldown(pixel);

            if (isAvailable && buttonState == CooldownButtonState.Cooldown)
            {
                OnButtonAvailable();
            }
            else if (isOnCooldown && buttonState is CooldownButtonState.Ready)
            {
                OnButtonOnCooldown();
            }
            else if (!isAvailable && !isOnCooldown && buttonState is CooldownButtonState.Ready)
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
            OnButtonStateChanged(CooldownButtonState.Cooldown);
        }

        void OnButtonAvailable()
        {
            log.Debug($"Button {key.Label} back up");
            OnButtonStateChanged(CooldownButtonState.Ready);
        }

        public void Dispose()
        {
            cooldownTimer.CooldownEnded -= OnCooldownEnded;
            cooldownTimer.Dispose();
        }
    }
}