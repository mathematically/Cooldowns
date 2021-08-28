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
        private static bool IsSkillCooldown(Color p) => p.ToArgb().Equals(SkillCooldownColor.ToArgb());
        
        private readonly IScreen screen;
        private readonly IKeyboard keyboard;
        private readonly ICooldownTimer cooldownTimer;

        private readonly Key key;

        public VirtualKeyCode ActionKeyCode { get; }
        public VirtualKeyCode ModeKeyCode { get; }

        private bool isAvailable;
        private bool isCooldown;

        private CooldownButtonState buttonState = CooldownButtonState.Ready;
        private CooldownButtonMode buttonMode = CooldownButtonMode.Manual;

        public event EventHandler<CooldownButtonState>? ButtonStateChanged;
        public event EventHandler<CooldownButtonMode>? ButtonModeChanged;

        public CooldownButton(IScreen screen, IKeyboard keyboard, ICooldownTimer cooldownTimer, Key key)
        {
            this.screen = screen;
            this.keyboard = keyboard;
            this.cooldownTimer = cooldownTimer;
            this.key = key;

            ActionKeyCode = Enum.Parse<VirtualKeyCode>(key.ActionKey);
            ModeKeyCode = Enum.Parse<VirtualKeyCode>(key.ModeKey);

            cooldownTimer.Ticked += OnTimerTicked;
           
            OnButtonModeChanged(buttonMode);
            OnButtonStateChanged(buttonState);
        }

        private void OnButtonStateChanged(CooldownButtonState state)
        {
            if (buttonState == state) return;
            buttonState = state;
            ButtonStateChanged?.Invoke(this, buttonState);
        }

        private void OnButtonModeChanged(CooldownButtonMode mode)
        {
            if (buttonMode == mode) return;
            buttonMode = mode;
            ButtonModeChanged?.Invoke(this, mode);
        }

        public void ChangeMode()
        {
            // Mode just cycles through the various options in a fixed sequence to keep the
            // required UI just a simple button.
            switch (buttonMode)
            {
                case CooldownButtonMode.Disabled:
                    EnterManualMode();
                    break;
                case CooldownButtonMode.Manual:
                    EnterAutocastMode();
                    break;
                case CooldownButtonMode.AutoCast:
                    EnterDisabledMode();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            void EnterManualMode()
            {
                OnButtonModeChanged(CooldownButtonMode.Manual);
                OnButtonStateChanged(CooldownButtonState.Ready);

                // On entering manual mode stop any timer.
                // A new one will start on the next button press.
                cooldownTimer.Stop();
            }

            void EnterAutocastMode()
            {
                OnButtonModeChanged(CooldownButtonMode.AutoCast);
                OnButtonStateChanged(CooldownButtonState.Ready);

                // Autocast mode so start the timer which will read
                // the screen to check button state and to autocast.
                cooldownTimer.Start();
            }

            void EnterDisabledMode()
            {
                // Disabled is all off.
                OnButtonModeChanged(CooldownButtonMode.Disabled);
                OnButtonStateChanged(CooldownButtonState.Disabled);
                cooldownTimer.Stop();
            }
        }

        public void Press()
        {
            switch (buttonMode)
            {
                // Disabled or AutoCasting nothing to do on button press.
                case CooldownButtonMode.Disabled or CooldownButtonMode.AutoCast:
                    return;
                // Manual mode we need to start the timer so we can check for cooldown end
                case CooldownButtonMode.Manual when buttonState == CooldownButtonState.Ready:
                    log.Debug($"Starting one shot timer for {key.Label} keypress.");
                    OnButtonStateChanged(CooldownButtonState.Cooldown);
                    cooldownTimer.Start();
                    break;
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
            GetButtonStateFromScreen();

            if (isAvailable && buttonState == CooldownButtonState.Cooldown)
            {
                OnButtonStateChanged(CooldownButtonState.Ready);
                cooldownTimer.Stop();
            }
            else if (isCooldown && buttonState is CooldownButtonState.Ready)
            {
                OnButtonStateChanged(CooldownButtonState.Cooldown);
                cooldownTimer.Start();
            }
        }

        private void ProcessAutocastCooldown()
        {
            GetButtonStateFromScreen();

            if (isAvailable)
            {
                keyboard.PressKey(ActionKeyCode);
            }
        }

        private void GetButtonStateFromScreen()
        {
            Color pixelColor = screen.GetPixelColor(key.DetectX, key.DetectY);

            isAvailable = IsSkillAvailable(pixelColor);
            isCooldown = IsSkillCooldown(pixelColor);
        }

        public void Dispose()
        {
            cooldownTimer.Ticked -= OnTimerTicked;
            cooldownTimer.Dispose();
        }
    }
}