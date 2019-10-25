using System;

namespace ChromeDevTools.Host
{
    public static class Extensions
    {
        public static T GetService<T>(this IServiceProvider serviceProvider) { return (T)serviceProvider.GetService(typeof(T)); }
    }
}