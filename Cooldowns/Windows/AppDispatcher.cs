using System;
using Cooldowns.Domain;
using static System.Windows.Application;

namespace Cooldowns.Windows
{
    public class AppDispatcher : IDispatcher
    {
        public void BeginInvoke(Action action)
        {
            Current.Dispatcher.BeginInvoke(action);
        }
    }
}