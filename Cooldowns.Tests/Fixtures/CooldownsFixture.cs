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
        protected readonly IDispatcher Dispatcher = new FakeDispatcher();

        protected KeyConfig KeyConfig = new();

        protected CooldownButtonState ExpectedState = CooldownButtonState.Ready;

        protected void AssertButtonCooldownState(object? _, CooldownButtonState actualState)
        {
            Assert.Equal(ExpectedState, actualState);
        }

        protected void SetScreenPixel(Color color)
        {
            Screen.GetPixelColor(KeyConfig.DetectX, KeyConfig.DetectY).Returns(color);
        }
    }
}