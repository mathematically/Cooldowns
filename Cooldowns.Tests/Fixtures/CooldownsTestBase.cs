using System.Drawing;
using Cooldowns.Domain;
using Cooldowns.Domain.Buttons;
using Cooldowns.Domain.Config;
using Cooldowns.Domain.Keyboard;
using Cooldowns.Domain.Timer;
using NSubstitute;
using Xunit;

namespace Cooldowns.Tests.Fixtures
{
    public class CooldownsTestBase
    {
        protected readonly IKeyboard Keyboard = Substitute.For<IKeyboard>();
        protected readonly IScreen Screen = Substitute.For<IScreen>();
        protected readonly ICooldownTimer CooldownTimer = Substitute.For<ICooldownTimer>();
        protected readonly IDispatcher Dispatcher = new FakeDispatcher();

        protected KeyConfig KeyConfig = new();

        protected CooldownButtonState ExpectedState = CooldownButtonState.Ready;
        protected CooldownButtonMode ExpectedMode = CooldownButtonMode.Manual;

        protected void Configure()
        {
            KeyConfig = new KeyConfig
            {
                Label = "Q",
                ActionKey = "VK_Q",
                ModeKey = "F5",
                
                DetectX = 100,
                DetectY = 200,
            };
        }

        protected void AssertButtonCooldownState(object? _, ButtonStateEventArgs args)
        {
            Assert.Equal(KeyConfig.Label, args.Name);
            Assert.Equal(ExpectedState, args.State);
        }

        protected void AssertButtonCooldownMode(object? _, ButtonModeEventArgs args)
        {
            Assert.Equal(KeyConfig.Label, args.Name);
            Assert.Equal(ExpectedMode, args.Mode);
        }

        private void SetScreenPixel(Color color, int x, int y)
        {
            Screen.GetPixelColor(x, y).Returns(color);
        }

        protected void SetButtonPixel(Color color)
        {
            SetScreenPixel(color, KeyConfig.DetectX, KeyConfig.DetectY);
        }
    }
}