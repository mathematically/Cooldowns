# Cooldowns
A simple cooldown tracker and autocaster for Last Epoch. I originally made this because I hate that on large modern screens the bottom toolbars are miles away and added autocast when I found out that was legal.  

1. Shows the main button cooldowns at character mini toolbar height.
2. Disappear when they are on cooldown, come back when they are up.
3. Supports autocast. 
--* Use main key (Q, W, E, R) to toggle autocast on/off.
--* Autocasted key should be set to the in-game secondary binding.
--* By default autocast keys are keypad 1,2,3,4
4. Keys you don't use on cooldown can be hidden by setting them as disabled in the config.

# Config

Config is in appsettings.json and should be obvious.

Autocast keycodes are decimal versions of Windows virtual key codes which you can find at the link below if you want to change them (tenkeyless!) 

https://docs.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes

