using System;
using System.Drawing;
using Cooldowns.Domain.Buttons;
using Cooldowns.Domain.Config;
using Cooldowns.Tests.Fixtures;
using NSubstitute;
using Xunit;

namespace Cooldowns.Tests
{
    public class CooldownButtonTests : CooldownsFixture, IDisposable
    {
        private readonly CooldownButton cooldownButton;

        public CooldownButtonTests()
        {
            KeyConfig = new KeyConfig
            {
                Label = "Q",
                ActionKey = "VK_Q",
                ModeKey = "F5",
                DetectX = 100,
                DetectY = 200,
            };

            SetScreenPixel(CooldownButton.SkillAvailableColor);

            cooldownButton = new CooldownButton(Screen, Keyboard, Dispatcher, CooldownTimer, KeyConfig);
            cooldownButton.ButtonStateChanged += AssertButtonCooldownState;
        }

        [Fact]
        public void When_screen_shows_cooldown_button_is_on_cooldown()
        {
            SetScreenPixel(CooldownButton.SkillCooldownColor);
            ExpectedState = CooldownButtonState.Cooldown;

            CooldownTimer.Ticked += Raise.Event();
        }

        [Fact]
        public void When_screen_shows_available_button_is_available()
        {
            SetScreenPixel(CooldownButton.SkillAvailableColor);
            ExpectedState = CooldownButtonState.Ready;

            CooldownTimer.Ticked += Raise.Event();
        }

        [Fact]
        public void When_screen_shows_active_button_is_on_active()
        {
            SetScreenPixel(CooldownButton.SkillActiveColor);
            ExpectedState = CooldownButtonState.Active;

            CooldownTimer.Ticked += Raise.Event();
        }

        [Fact]
        public void When_screen_colour_unknown_button_is_assumed_to_be_on_cooldown()
        {
            SetScreenPixel(Color.DarkSalmon);
            ExpectedState = CooldownButtonState.Cooldown;

            CooldownTimer.Ticked += Raise.Event();
        }

        public void Dispose()
        {
            cooldownButton.ButtonStateChanged -= AssertButtonCooldownState;
            cooldownButton.Dispose();
        }
    }
}
