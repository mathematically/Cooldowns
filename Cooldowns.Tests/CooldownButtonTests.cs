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
                Autocast = false,
                AutocastKey = "F5",
                AutoDetectCooldown = false,
                Cooldown = 1000,
                DetectX = 100,
                DetectY = 200,
                Enabled = true,
            };
        }

        [Fact]
        public void When_button_pressed_button_on_cooldown()
        {
            var sut = new CooldownButton(Screen, Keyboard, CooldownTimer, Config);

            sut.ButtonStateChanged += AssertButtonCooldownState;
            SetScreenPixel(CooldownButton.SkillCooldownColor);
            ExpectedState = CooldownButtonState.Cooldown;

            sut.Press();
        }

        [Fact]
        public void When_cooldown_ends_button_is_available()
        {
            var sut = new CooldownButton(Screen, Keyboard, CooldownTimer, Config);
            sut.Press();

            sut.ButtonStateChanged += AssertButtonCooldownState;
            SetScreenPixel(CooldownButton.SkillAvailableColor);
            ExpectedState = CooldownButtonState.Ready;

            CooldownTimer.CooldownEnded += Raise.Event();
        }
    }
}
