using System;
using System.Collections.Generic;
using Cooldowns.Domain.Config;
using Cooldowns.Domain.Keyboard;
using NLog;
using WindowsInput.Native;
using Color = Cooldowns.Domain.Screen.Color;

namespace Cooldowns.Domain.Buttons
{
    public sealed class CooldownButton : IDisposable
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();

        public static readonly System.Drawing.Color SkillAvailableColor = System.Drawing.Color.FromArgb(255, 255, 255);
        public static readonly System.Drawing.Color SkillActiveColor = System.Drawing.Color.FromArgb(65, 60, 53);
        public static readonly System.Drawing.Color SkillCooldownColor = System.Drawing.Color.FromArgb(19, 19, 23);

        private readonly IScreen screen;
        private readonly IKeyboard keyboard;
        private readonly IDispatcher dispatcher;
        private readonly ICooldownTimer cooldownTimer;

        private readonly KeyConfig config;

        // Default is cooldown so we will get an event when we first detect an available pixel.
        private CooldownButtonState buttonState = CooldownButtonState.Cooldown;
        private CooldownButtonMode buttonMode = CooldownButtonMode.Manual;

        private bool isScreenActive;
        private bool isScreenAvailable;
        private bool isScreenCooldown;

        private static bool IsSkillAvailable(System.Drawing.Color p)
        {
            return Color.IsExactMatch(p, SkillAvailableColor);
        }

        private static bool IsSkillActive(System.Drawing.Color p)
        {
            return Color.IsMatch(p, SkillActiveColor);
        }

        private static bool IsSkillOnCooldown(System.Drawing.Color p)
        {
            return Color.IsMatch(p, SkillCooldownColor);
        }

        public event EventHandler<CooldownButtonState>? ButtonStateChanged;
        public event EventHandler<CooldownButtonMode>? ButtonModeChanged;

        private VirtualKeyCode ActionKeyCode { get; }
        public VirtualKeyCode ModeKeyCode { get; }

        public CooldownButton(IScreen screen, IKeyboard keyboard, IDispatcher dispatcher, ICooldownTimer cooldownTimer, KeyConfig config)
        {
            this.screen = screen ?? throw new ArgumentNullException(nameof(screen));
            this.keyboard = keyboard ?? throw new ArgumentNullException(nameof(keyboard));
            this.dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            this.cooldownTimer = cooldownTimer ?? throw new ArgumentNullException(nameof(cooldownTimer));

            this.config = config;

            ActionKeyCode = Enum.Parse<VirtualKeyCode>(config.ActionKey);
            ModeKeyCode = Enum.Parse<VirtualKeyCode>(config.ModeKey);

            cooldownTimer.Ticked += OnTimerTicked;

            OnButtonModeChanged(buttonMode);
            OnButtonStateChanged(buttonState);
        }

        private void OnButtonStateChanged(CooldownButtonState state)
        {
            if (buttonState == state) return;
            log.Debug($"{config.ActionKey} {state}");
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
            log.Debug($"Manual mode enabled for {config.ActionKey}");
            OnButtonModeChanged(CooldownButtonMode.Manual);
            OnButtonStateChanged(CooldownButtonState.Ready);
        }

        private void SetAutocastMode()
        {
            log.Debug($"Autocast enabled for {config.ActionKey}");
            OnButtonModeChanged(CooldownButtonMode.AutoCast);
            OnButtonStateChanged(CooldownButtonState.Ready);
        }

        private void SetDisabledMode()
        {
            log.Debug($"{config.ActionKey} disabled.");
            OnButtonModeChanged(CooldownButtonMode.Disabled);
        }

        private void OnTimerTicked(object? o, EventArgs eventArgs)
        {
            if (buttonMode == CooldownButtonMode.Manual)
            {
                ProcessManualCooldown();
            }
            else if (buttonMode == CooldownButtonMode.AutoCast)
            {
                ProcessAutocastCooldown();
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

            if (isScreenAvailable)
            {
                log.Debug($"AUTCASTING {config.ActionKey}");
                keyboard.PressKey(ActionKeyCode);
            }
        }

        private CooldownButtonState lastDetected = CooldownButtonState.Cooldown;
        private readonly List<System.Drawing.Color> detected = new();

        private void GetCurrentButtonScreenState()
        {
            var pixelColor = screen.GetPixelColor(config.DetectX, config.DetectY);

            isScreenAvailable = IsSkillAvailable(pixelColor);
            isScreenCooldown = IsSkillOnCooldown(pixelColor);
            isScreenActive = IsSkillActive(pixelColor);

            if (isScreenAvailable && lastDetected is not CooldownButtonState.Ready)
            {
                log.Debug($"{ActionKeyCode} is now AVAILABLE {pixelColor.R} {pixelColor.G} {pixelColor.B}");
                lastDetected = CooldownButtonState.Ready;
            }
            else if (isScreenCooldown && lastDetected is not CooldownButtonState.Cooldown)
            {
                log.Debug($"{ActionKeyCode} is now on COOLDOWN {pixelColor.R} {pixelColor.G} {pixelColor.B}");
                lastDetected = CooldownButtonState.Cooldown;
            }
            else if (isScreenActive && lastDetected is not CooldownButtonState.Active)
            {
                log.Debug($"{ActionKeyCode} is now ACTIVE {pixelColor.R} {pixelColor.G} {pixelColor.B}");
                lastDetected = CooldownButtonState.Active;
            }

            if (detected.Contains(pixelColor)) return;
            log.Debug($"{ActionKeyCode} UNKNOWN colour detected {pixelColor.R} {pixelColor.G} {pixelColor.B}");
            detected.Add(pixelColor);
        }

        public void Dispose()
        {
            foreach (var pixelColor in detected)
            {
                log.Debug($"{ActionKeyCode} detected colour {pixelColor.R} {pixelColor.G} {pixelColor.B}");
            }

            cooldownTimer.Ticked -= OnTimerTicked;
        }
    }
}