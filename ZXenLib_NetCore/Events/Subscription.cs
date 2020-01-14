namespace ZxenLib.Events
{
    using System;

    /// <summary>
    /// The container class for a subscription action.
    /// </summary>
    public class Subscription
    {
        /// <summary>
        /// Gets or sets the Id of the subscription.
        /// </summary>
        public Guid SubscriptionId { get; set; }

        /// <summary>
        /// Gets or sets the Subscriber of the event.
        /// </summary>
        public object Subscriber { get; set; }

        /// <summary>
        /// Gets or sets the method to be called by the event.
        /// </summary>
        public Action<EventData> Method { get; set; }
    }
}
