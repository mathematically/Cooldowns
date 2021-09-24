using System;
using System.Windows.Controls;
using Cooldowns.Domain;
using Cooldowns.Domain.Buttons;
using Cooldowns.Domain.Config;
using Cooldowns.Domain.Keyboard;

namespace Cooldowns.Factory
{
    public class CooldownButtonFactory: ICooldownButtonFactory
    {
        private readonly IDispatcher dispatcher;
        private readonly IScreen screen;
        private readonly IKeyboard keyboard;

        public CooldownButtonFactory(IDispatcher dispatcher, IScreen screen, IKeyboard keyboard)
        {
            this.dispatcher = dispatcher;
            this.screen = screen;
            this.keyboard = keyboard;
        }

        public CooldownButton Create(Button button, KeyConfig config, ICooldownTimer cooldownTimer, 
            Action<Button, CooldownButtonState> onToolbarButtonStateChanged, Action<Button, CooldownButtonMode> onToolbarButtonModeChanged)
        {
            var cooldownButton = new CooldownButton(screen, keyboard, dispatcher, cooldownTimer, config);

            cooldownButton.ButtonStateChanged += (_, buttonState) => onToolbarButtonStateChanged(button, buttonState);
            cooldownButton.ButtonModeChanged += (_, buttonMode) => onToolbarButtonModeChanged(button, buttonMode);

            return cooldownButton;
        }
    }
}