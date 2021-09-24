using System;
using Cooldowns.Domain;

namespace Cooldowns.Tests.Fixtures
{
    public class FakeDispatcher : IDispatcher
    {
        public void BeginInvoke(Action action)
        {
            action();
        }

        public void Invoke(Action action)
        {
            throw new NotImplementedException();
        }
    }
}