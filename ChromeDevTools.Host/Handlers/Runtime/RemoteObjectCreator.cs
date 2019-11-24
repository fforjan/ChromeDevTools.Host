using ChromeDevTools.Host.Runtime.Runtime;

namespace ChromeDevTools.Host.Handlers.Runtime
{
    public static class RemoteObjectCreator
    {
        public static RemoteObject Create(string value)
        {
            return new RemoteObject
            {
                Type = "string",
                Value = value
            };
        }

        public static RemoteObject Create(double value)
        {
            return new RemoteObject
            {
                Type = "number",
                Value = value,
                Description = value.ToString()
            };
        }
    }
}
