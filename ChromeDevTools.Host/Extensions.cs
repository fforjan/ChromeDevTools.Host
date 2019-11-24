namespace ChromeDevTools.Host
{
    using System;

    public static class Extensions
    {
        // basic extension for IServiceProvider
        public static T GetService<T>(this IServiceProvider serviceProvider) { return (T)serviceProvider.GetService(typeof(T)); }
    }
}