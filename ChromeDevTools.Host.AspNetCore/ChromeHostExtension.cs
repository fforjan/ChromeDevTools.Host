namespace ChromeDevTools.Host.AspNetCore
{
    using Microsoft.AspNetCore.Builder;
    public static class ChromeHostExtension
    {
        public static void HostChromeProtocol(this IApplicationBuilder app, I)
        {
           app.UseMiddleware<ChromeHostMiddleware>();
        }

        public static ChromeProtocolSessions Sessions = new ChromeProtocolSessions();
    }    
}
