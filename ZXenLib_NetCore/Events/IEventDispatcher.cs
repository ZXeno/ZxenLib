namespace ZxenLib.Events
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides a means for subscribing and broadcasting events to a set of registered listeners.
    /// </summary>
    public interface IEventDispatcher
    {
        /// <summary>
        /// Publishes event data to the event dispatcher.
        /// </summary>
        /// <param name="eventData">The <see cref="EventData"/> to publish.</param>
        void Publish(EventData eventData);

        /// <summary>
        /// Asynchronously publishes event data to the event dispatcher.
        /// </summary>
        /// <param name="eventData">The <see cref="EventData"/> to publish.</param>
        /// <returns>The <see cref="Task"/> for the asynchronous work.</returns>
        Task PublishAsync(EventData eventData);

        /// <summary>
        /// Subscribes to an event.
        /// </summary>
        /// <param name="eventId">The event ID.</param>
        /// <param name="action">The <see cref="Action{T}"/> to call when the event is published.</param>
        /// <param name="subscriber">The subscriber of the event.</param>
        void Subscribe(string eventId, Action<EventData> action, object subscriber);

        /// <summary>
        /// Unsubscribes from an event.
        /// </summary>
        /// <param name="eventId">The event ID.</param>
        /// <param name="subscriber">The subscriber of the event.</param>
        /// <param name="action">The <see cref="Action{T}"/> to call when the event is published.</param>
        void Unsubscribe(string eventId, object subscriber, Action<EventData> action);
    }
}
