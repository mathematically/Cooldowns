using System;
using System.Drawing;
using Cooldowns.Domain.Config;
using Cooldowns.Domain.Keyboard;
using NLog;
using WindowsInput.Native;

namespace Cooldowns.Domain.Buttons
{
    public sealed class CooldownButton : IDisposable
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();

        // Some screen colors differ it seems, presumably due to lighting or aliasing or something.
        // 255,255,255 is always white though.
        private static readonly int colourTolerance = 5;

        public static readonly Color SkillAvailableColor = Color.FromArgb(255, 255, 255);
        public static readonly Color SkillActiveColor = Color.FromArgb(65, 60, 53);
        public static readonly Color SkillCooldownColor = Color.FromArgb(19, 19, 23);

        private readonly IScreen screen;
        private readonly IKeyboard keyboard;
        private readonly IDispatcher dispatcher;
        private readonly ICooldownTimer cooldownTimer;

        private readonly KeyConfig keyConfig;

        // Default is cooldown so we will get an event when we first detect an available pixel.
        private CooldownButtonState buttonState = CooldownButtonState.Cooldown;
        private CooldownButtonMode buttonMode = CooldownButtonMode.Manual;

        private bool isScreenActive;
        private bool isScreenAvailable;
        private bool isScreenCooldown;
        private bool hasAutocastedThisCycle = false;

        private static bool IsSkillAvailable(Color p)
        {
            return p.ToArgb().Equals(SkillAvailableColor.ToArgb());
        }

        private static bool IsSkillActive(Color p)
        {
            return p.R > SkillActiveColor.R - colourTolerance && p.R < SkillActiveColor.R + colourTolerance &&
                   p.G > SkillActiveColor.G - colourTolerance && p.G < SkillActiveColor.G + colourTolerance &&
                   p.B > SkillActiveColor.B - colourTolerance && p.B < SkillActiveColor.B + colourTolerance;
        }

        private static bool IsSkillOnCooldown(Color p)
        {
            return p.R > SkillCooldownColor.R - colourTolerance && p.R < SkillCooldownColor.R + colourTolerance &&
                   p.G > SkillCooldownColor.G - colourTolerance && p.G < SkillCooldownColor.G + colourTolerance &&
                   p.B > SkillCooldownColor.B - colourTolerance && p.B < SkillCooldownColor.B + colourTolerance;
        }

        public event EventHandler<CooldownButtonState>? ButtonStateChanged;
        public event EventHandler<CooldownButtonMode>? ButtonModeChanged;

        public VirtualKeyCode ActionKeyCode { get; }
        public VirtualKeyCode ModeKeyCode { get; }

        public CooldownButton(IScreen screen, IKeyboard keyboard, IDispatcher dispatcher, ICooldownTimer cooldownTimer, KeyConfig keyConfig)
        {
            this.screen = screen ?? throw new ArgumentNullException(nameof(screen));
            this.keyboard = keyboard ?? throw new ArgumentNullException(nameof(keyboard));
            this.dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            this.cooldownTimer = cooldownTimer ?? throw new ArgumentNullException(nameof(cooldownTimer));

            this.keyConfig = keyConfig;

            ActionKeyCode = Enum.Parse<VirtualKeyCode>(keyConfig.ActionKey);
            ModeKeyCode = Enum.Parse<VirtualKeyCode>(keyConfig.ModeKey);

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
        }

        private void SetManualMode()
        {
            log.Debug($"Manual mode enabled for {keyConfig.ActionKey}");
            OnButtonModeChanged(CooldownButtonMode.Manual);
            OnButtonStateChanged(CooldownButtonState.Ready);
        }

        private void SetAutocastMode()
        {
            log.Debug($"Autocast enabled for {keyConfig.ActionKey}");
            hasAutocastedThisCycle = false;
            OnButtonModeChanged(CooldownButtonMode.AutoCast);
            OnButtonStateChanged(CooldownButtonState.Ready);
        }

        private void SetDisabledMode()
        {
            log.Debug($"{keyConfig.ActionKey} disabled.");
            OnButtonModeChanged(CooldownButtonMode.Disabled);
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
            }
        }

        private void ProcessManualCooldown()
        {
            GetCurrentButtonScreenState();

            if (isScreenAvailable && buttonState is not CooldownButtonState.Ready)
                OnButtonStateChanged(CooldownButtonState.Ready);

            if (isScreenCooldown && buttonState is not CooldownButtonState.Cooldown)
                OnButtonStateChanged(CooldownButtonState.Cooldown);

            if (isScreenActive && buttonState is not CooldownButtonState.Active)
                OnButtonStateChanged(CooldownButtonState.Active);
        }

        private void ProcessAutocastCooldown()
        {
            GetCurrentButtonScreenState();

            if (isScreenCooldown && hasAutocastedThisCycle)
            {
                hasAutocastedThisCycle = false;
            }
            else if (isScreenAvailable && !hasAutocastedThisCycle)
            {
                log.Debug($"Autocasting {keyConfig.ActionKey}");
                keyboard.PressKey(ActionKeyCode);
                hasAutocastedThisCycle = true;
            }
            
        }

        private void GetCurrentButtonScreenState()
        {
            var pixelColor = screen.GetPixelColor(keyConfig.DetectX, keyConfig.DetectY);

            isScreenAvailable = IsSkillAvailable(pixelColor);
            isScreenCooldown = IsSkillOnCooldown(pixelColor);
            isScreenActive = IsSkillActive(pixelColor);


#if DEBUG
            if (isScreenAvailable && buttonState is not CooldownButtonState.Ready)
                log.Debug($"{ActionKeyCode} is now AVAILABLE {pixelColor.R} {pixelColor.G} {pixelColor.B}");

            if (isScreenCooldown && buttonState is not CooldownButtonState.Cooldown)
                log.Debug($"{ActionKeyCode} is now on COOLDOWN {pixelColor.R} {pixelColor.G} {pixelColor.B}");

            if (isScreenActive && buttonState is not CooldownButtonState.Active)
                log.Debug($"{ActionKeyCode} is currently ACTIVE {pixelColor.R} {pixelColor.G} {pixelColor.B}");

            if (!isScreenAvailable && !isScreenCooldown && !isScreenActive)
                log.Debug(
                    $"{ActionKeyCode} {keyConfig.DetectX} {keyConfig.DetectY} UNKNOWN colour detected {pixelColor.R} {pixelColor.G} {pixelColor.B}");
#endif
        }

        public void Dispose()
        {
            cooldownTimer.Ticked -= OnTimerTicked;
        }
    }
}