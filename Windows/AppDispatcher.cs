using System;
using System.Windows;
using Cooldowns.Domain;

namespace Cooldowns.Windows
{
    public class AppDispatcher: IDispatcher
    {
        public void BeginInvoke(Action action)
        {
            Application.Current.Dispatcher.BeginInvoke(action);
        }
    }
}