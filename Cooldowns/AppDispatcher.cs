using System;
using Cooldowns.Domain;
using static System.Windows.Application;

namespace Cooldowns
{
    public class AppDispatcher : IDispatcher
    {
        public void BeginInvoke(Action action)
        {
            Current.Dispatcher.BeginInvoke(action);
        }

        public void Invoke(Action action)
        {
            Current.Dispatcher.Invoke(action);
        }
    }
}