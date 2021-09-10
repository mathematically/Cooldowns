using System.Drawing;
using Cooldowns.Domain.Buttons;
using Cooldowns.Domain.Config;
using Cooldowns.Tests.Fixtures;
using NSubstitute;
using Xunit;

namespace Cooldowns.Tests
{
    public class CooldownButtonTests : CooldownsFixture
    {
        public CooldownButtonTests()
        {
            Config = new Key
            {
                Label = "Q",
                ActionKey = "VK_Q",
                ModeKey = "F5",
                DetectX = 100,
                DetectY = 200,
            };

            SetScreenPixel(CooldownButton.SkillAvailableColor);
        }

        private void CreateSut()
        {
            var sut = new CooldownButton(Screen, Keyboard, Dispatcher, CooldownTimer, Config);
            sut.ButtonStateChanged += AssertButtonCooldownState;
        }

        [Fact]
        public void When_button_pressed_button_is_on_cooldown()
        {
            CreateSut();

            SetScreenPixel(CooldownButton.SkillCooldownColor);
            ExpectedState = CooldownButtonState.Cooldown;

            CooldownTimer.Ticked += Raise.Event();
        }

        [Fact]
        public void When_cooldown_ends_button_is_available()
        {
            CreateSut();

            SetScreenPixel(CooldownButton.SkillAvailableColor);
            ExpectedState = CooldownButtonState.Ready;

            CooldownTimer.Ticked += Raise.Event();
        }

        [Fact]
        public void If_color_unknown_button_is_on_cooldown()
        {
            CreateSut();

            SetScreenPixel(Color.DarkSalmon);
            ExpectedState = CooldownButtonState.Cooldown;

            CooldownTimer.Ticked += Raise.Event();
        }
    }
}
