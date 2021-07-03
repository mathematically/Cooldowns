using System.Runtime.Versioning;
using System.Windows;
using Microsoft.Xaml.Behaviors;

namespace Cooldowns.Behaviours
{
    [SupportedOSPlatform("windows")]
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