using System;
using Cooldowns.Domain.Buttons;
using Cooldowns.Domain.Config;
using Cooldowns.Domain.Keyboard;
using Cooldowns.Domain.Timer;

namespace Cooldowns.Domain.Factory
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

        public CooldownButton? Create(KeyConfig config, ICooldownTimer cooldownTimer, Action<ButtonStateEventArgs> onToolbarButtonStateChanged, 
            Action<ButtonModeEventArgs> onToolbarButtonModeChanged)
        {
            var cooldownButton = new CooldownButton(screen, keyboard, dispatcher, cooldownTimer, config);

            cooldownButton.ButtonStateChanged += (_, args) => onToolbarButtonStateChanged(args);
            cooldownButton.ButtonModeChanged += (_, args) => onToolbarButtonModeChanged(args);

            return cooldownButton;
        }
    }
}