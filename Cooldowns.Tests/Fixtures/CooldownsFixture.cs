using System.Drawing;
using Cooldowns.Domain;
using Cooldowns.Domain.Buttons;
using Cooldowns.Domain.Config;
using Cooldowns.Domain.Keyboard;
using NSubstitute;
using Xunit;

namespace Cooldowns.Tests.Fixtures
{
    public class CooldownsFixture
    {
        protected readonly IKeyboard Keyboard = Substitute.For<IKeyboard>();
        protected readonly IScreen Screen = Substitute.For<IScreen>();
        protected readonly ICooldownTimer CooldownTimer = Substitute.For<ICooldownTimer>();

        protected Key Config = new();

        //protected readonly FakeCooldownTimer CooldownTimer = new();

        protected CooldownButtonState ExpectedState = CooldownButtonState.Disabled;

        protected void AssertButtonCooldownState(object? _, CooldownButtonState actualState)
        {
            Assert.Equal(ExpectedState, actualState);
        }

        protected void SetScreenPixel(Color color)
        {
            Screen.GetPixelColor(Config.DetectX, Config.DetectY).Returns(color);
        }
    }
}