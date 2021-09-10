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
        public static readonly Color SkillActiveColor = Color.FromArgb(65, 60, 53);
        public static readonly Color SkillCooldownColor = Color.FromArgb(19, 19, 23);

        // Some screen colors differ it seems, presumably due to lighting or aliasing or something.
        // 255,255,255 is always white though.
        private static int tolerance = 5;

        private static bool IsSkillAvailable(Color p) => p.ToArgb().Equals(SkillAvailableColor.ToArgb());
        //private static bool IsSkillActive(Color p) => p.ToArgb().Equals(SkillActiveColor.ToArgb());
        private static bool IsSkillActive(Color p)
        {
            return p.R > SkillActiveColor.R - tolerance && p.R < SkillActiveColor.R + tolerance &&
                   p.G > SkillActiveColor.G - tolerance && p.G < SkillActiveColor.G + tolerance &&
                   p.B > SkillActiveColor.B - tolerance && p.B < SkillActiveColor.B + tolerance;
        }

//        private static bool IsSkillOnCooldown(Color p) => p.ToArgb().Equals(SkillCooldownColor.ToArgb());
        private static bool IsSkillOnCooldown(Color p)
        {
            return p.R > SkillCooldownColor.R - tolerance && p.R < SkillCooldownColor.R + tolerance &&
                   p.G > SkillCooldownColor.G - tolerance && p.G < SkillCooldownColor.G + tolerance &&
                   p.B > SkillCooldownColor.B - tolerance && p.B < SkillCooldownColor.B + tolerance;
        }

        private readonly IScreen screen;
        private readonly IKeyboard keyboard;
        private readonly IDispatcher dispatcher;
        private readonly ICooldownTimer cooldownTimer;

        private readonly Key key;

        private readonly VirtualKeyCode actionKeyCode;
        private bool isScreenAvailable;
        private bool isScreenActive;
        private bool isScreenCooldown;

        // default is cooldown so we will get an event when we first detect an available pixel.
        private CooldownButtonState buttonState = CooldownButtonState.Cooldown;
        private CooldownButtonMode buttonMode = CooldownButtonMode.Manual;

        public event EventHandler<CooldownButtonState>? ButtonStateChanged;
        public event EventHandler<CooldownButtonMode>? ButtonModeChanged;

        public CooldownButton(IScreen screen, IKeyboard keyboard, IDispatcher dispatcher, ICooldownTimer cooldownTimer, Key key)
        {
            this.screen = screen ?? throw new ArgumentNullException(nameof(screen));
            this.keyboard = keyboard ?? throw new ArgumentNullException(nameof(keyboard));
            this.dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));;
            this.cooldownTimer = cooldownTimer ?? throw new ArgumentNullException(nameof(cooldownTimer));
            this.key = key;

            // todo should check this really.
            actionKeyCode = Enum.Parse<VirtualKeyCode>(key.ActionKey);

            cooldownTimer.Ticked += OnTimerTicked;
           
            OnButtonModeChanged(buttonMode);
            OnButtonStateChanged(buttonState);
        }

        private void OnButtonStateChanged(CooldownButtonState state)
        {
            if (buttonState == state) return;
            buttonState = state;
            dispatcher.BeginInvoke(() => ButtonStateChanged?.Invoke(this, buttonState));
        }

        private void OnButtonModeChanged(CooldownButtonMode mode)
        {
            if (buttonMode == mode) return;
            buttonMode = mode;
            dispatcher.BeginInvoke(() => ButtonModeChanged?.Invoke(this, mode));
        }

        public void ChangeMode()
        {
            // Mode just cycles through the various options in a fixed sequence to keep the
            // required UI to just a simple button. disabled -> manual -> autocast.
            switch (buttonMode)
            {
                case CooldownButtonMode.Disabled:
                    SetManualMode();
                    break;
                case CooldownButtonMode.Manual:
                    SetAutocastMode();
                    break;
                case CooldownButtonMode.AutoCast:
                    SetDisabledMode();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            void SetManualMode()
            {
                log.Debug($"Manual mode enabled for {key.ActionKey}");
                OnButtonModeChanged(CooldownButtonMode.Manual);
                OnButtonStateChanged(CooldownButtonState.Ready);
            }

            void SetAutocastMode()
            {
                log.Debug($"Autocast enabled for {key.ActionKey}");
                OnButtonModeChanged(CooldownButtonMode.AutoCast);
                OnButtonStateChanged(CooldownButtonState.Ready);
            }

            void SetDisabledMode()
            {
                log.Debug($"{key.ActionKey} disabled.");
                OnButtonModeChanged(CooldownButtonMode.Disabled);
            }
        }

        private void OnTimerTicked(object? o, EventArgs eventArgs)
        {
            switch (buttonMode)
            {
                case CooldownButtonMode.Disabled:
                    return;
                case CooldownButtonMode.Manual:
                    ProcessManualCooldown();
                    break;
                case CooldownButtonMode.AutoCast:
                    ProcessAutocastCooldown();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ProcessManualCooldown()
        {
            GetCurrentButtonScreenState();

            if (isScreenAvailable && buttonState is not CooldownButtonState.Ready)
            {
                OnButtonStateChanged(CooldownButtonState.Ready);
            }
            else if (isScreenCooldown && buttonState is not CooldownButtonState.Cooldown)
            {
                OnButtonStateChanged(CooldownButtonState.Cooldown);
            }
            else if (isScreenActive && buttonState is not CooldownButtonState.Active)
            {
                OnButtonStateChanged(CooldownButtonState.Active);
            }
        }

        private void ProcessAutocastCooldown()
        {
            GetCurrentButtonScreenState();

            if (isScreenAvailable)
            {
                log.Debug($"Autocasting {key.ActionKey}");
                keyboard.PressKey(actionKeyCode);
            }
        }

        private void GetCurrentButtonScreenState()
        {
            Color pixelColor = screen.GetPixelColor(key.DetectX, key.DetectY);

            isScreenAvailable = IsSkillAvailable(pixelColor);
            isScreenCooldown = IsSkillOnCooldown(pixelColor);
            isScreenActive = IsSkillActive(pixelColor);

#if DEBUG
            if (isScreenAvailable && buttonState is not CooldownButtonState.Ready)
            {
                log.Debug($"{actionKeyCode} is now AVAILABLE {pixelColor.R} {pixelColor.G} {pixelColor.B}");
            }

            if (isScreenCooldown && buttonState is not CooldownButtonState.Cooldown)
            {
                log.Debug($"{actionKeyCode} is now on COOLDOWN {pixelColor.R} {pixelColor.G} {pixelColor.B}");
            }

            if (isScreenActive && buttonState is not CooldownButtonState.Active)
            {
                log.Debug($"{actionKeyCode} is currently ACTIVE {pixelColor.R} {pixelColor.G} {pixelColor.B}");
            }

            if (!isScreenAvailable && !isScreenCooldown && !isScreenActive)
            {
                log.Debug($"{actionKeyCode} {key.DetectX} {key.DetectY} Unknown colour detected {pixelColor.R} {pixelColor.G} {pixelColor.B}");
            }
#endif

        }

        public void Dispose()
        {
            cooldownTimer.Ticked -= OnTimerTicked;
        }
    }
}