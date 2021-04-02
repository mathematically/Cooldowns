using System.Windows;
using Microsoft.Xaml.Behaviors;

namespace Cooldowns.Behaviours
{
    public class AlwaysOnTopBehavior : Behavior<Window>
    {
        protected override void OnAttached( )
        {
            AssociatedObject.Topmost = true;

            base.OnAttached();
            AssociatedObject.LostFocus += (_, _) => AssociatedObject.Topmost = true;
        }
    }
}