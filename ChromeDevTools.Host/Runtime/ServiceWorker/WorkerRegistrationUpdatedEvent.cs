namespace ChromeDevTools.Host.Runtime.ServiceWorker
{
    using Newtonsoft.Json;

    /// <summary>
    /// WorkerRegistrationUpdated
    /// </summary>
    public sealed class WorkerRegistrationUpdatedEvent : IEvent
    {
        /// <summary>
        /// Gets or sets the registrations
        /// </summary>
        [JsonProperty("registrations")]
        public ServiceWorkerRegistration[] Registrations
        {
            get;
            set;
        }
    }
}