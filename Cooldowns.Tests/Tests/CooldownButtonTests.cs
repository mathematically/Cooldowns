using System;
using System.Drawing;
using Cooldowns.Domain.Buttons;
using Cooldowns.Tests.Fixtures;
using NSubstitute;
using WindowsInput.Native;
using Xunit;

namespace Cooldowns.Tests.Tests
{
    public class CooldownButtonTests : CooldownsTestBase, IDisposable
    {
        private readonly CooldownButton cooldownButton;

        public CooldownButtonTests()
        {
            Configure();

            SetButtonPixel(CooldownButton.SkillAvailableColor);

            cooldownButton = new CooldownButton(Screen, Keyboard, Dispatcher, CooldownTimer, KeyConfig);
            cooldownButton.ButtonStateChanged += AssertButtonCooldownState;
            cooldownButton.ButtonModeChanged += AssertButtonCooldownMode;
        }

        [Fact]
        public void When_screen_shows_cooldown_button_is_on_cooldown()
        {
            SetButtonPixel(CooldownButton.SkillCooldownColor);
            ExpectedState = CooldownButtonState.Cooldown;

            CooldownTimer.Ticked += Raise.Event();
        }

        [Fact]
        public void When_screen_shows_available_button_is_available()
        {
            SetButtonPixel(CooldownButton.SkillAvailableColor);
            ExpectedState = CooldownButtonState.Ready;

            CooldownTimer.Ticked += Raise.Event();
        }

        [Fact]
        public void When_screen_shows_active_button_is_on_active()
        {
            SetButtonPixel(CooldownButton.SkillActiveColor);
            ExpectedState = CooldownButtonState.Active;

            CooldownTimer.Ticked += Raise.Event();
        }

        [Fact]
        public void When_screen_colour_unknown_button_is_assumed_to_be_on_cooldown()
        {
            SetButtonPixel(Color.DarkSalmon);
            ExpectedState = CooldownButtonState.Cooldown;

            CooldownTimer.Ticked += Raise.Event();
        }

        [Fact]
        public void When_cooldown_expires_autocast_buttons_press_action_key()
        {
            cooldownButton.Init(ButtonMode.AutoCast);
            SetButtonPixel(CooldownButton.SkillCooldownColor);
            CooldownTimer.Ticked += Raise.Event();

            SetButtonPixel(CooldownButton.SkillAvailableColor);
            CooldownTimer.Ticked += Raise.Event();

            Keyboard.Received(1).PressKey(VirtualKeyCode.VK_Q);
        }

        public void Dispose()
        {
            cooldownButton.ButtonStateChanged -= AssertButtonCooldownState;
            cooldownButton.Dispose();
        }
    }
}