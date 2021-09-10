using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Cooldowns
{
    public sealed class ToolbarViewModel: INotifyPropertyChanged
    {
        private string statusText = string.Empty;

        [NotNull]
        public string StatusText
        {
            get => statusText;
            set
            {
                if (value == statusText) return;
                statusText = value ?? throw new ArgumentNullException(nameof(value));
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}