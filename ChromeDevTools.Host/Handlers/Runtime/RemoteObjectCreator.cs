
namespace ChromeDevTools.Host.Handlers.Runtime
{
    using ChromeDevTools.Host.Runtime.Runtime;

    /// <summary>
    /// Allows to create a remote object for the specific value type.
    /// </summary>
    public static class RemoteObjectCreator
    {
        /// <summary>
        /// Create remote object from a string
        /// </summary>
        public static RemoteObject Create(string value)
        {
            return new RemoteObject
            {
                Type = "string",
                Value = value
            };
        }

        /// <summary>
        /// Create remote object from a double
        /// </summary>
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
