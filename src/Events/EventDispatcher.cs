namespace ZxenLib.Events
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides a means for subscribing and broadcasting events to a set of registered listeners.
    /// </summary>
    public class EventDispatcher : IEventDispatcher
    {
        /// <summary>
        /// The task factory used internally.
        /// </summary>
        private readonly TaskFactory taskFactory =
            new TaskFactory(
                CancellationToken.None,
                TaskCreationOptions.None,
                TaskContinuationOptions.None,
                TaskScheduler.Default);

        /// <summary>
        /// The cache of subscriptions, indexed by event Id.
        /// </summary>
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<Guid, Subscription>> subscriptionDictionary =
            new ConcurrentDictionary<string, ConcurrentDictionary<Guid, Subscription>>();

        /// <summary>
        /// Publishes event data to the event dispatcher.
        /// </summary>
        /// <param name="eventData">The <see cref="EventData"/> to publish.</param>
        public void Publish(EventData eventData)
        {
            if (eventData == null)
            {
                throw new ArgumentNullException(nameof(eventData));
            }

            if (eventData.Sender == null)
            {
                throw new ArgumentNullException(nameof(EventData.Sender));
            }

            if (string.IsNullOrWhiteSpace(eventData.EventId))
            {
                throw new ArgumentNullException(nameof(EventData.EventId));
            }

            if (!this.subscriptionDictionary.TryGetValue(eventData.EventId, out ConcurrentDictionary<Guid, Subscription> subscriptions))
            {
                return;
            }

            ConcurrentDictionary<Guid, Subscription> cachedSubs;
            lock (subscriptions)
            {
                cachedSubs = subscriptions;
            }

            foreach (Guid key in cachedSubs.Keys)
            {
                Subscription currentSub = cachedSubs[key];
                if (currentSub == null)
                {
                    bool wasRemoved = false;
                    while (!wasRemoved)
                    {
                        wasRemoved = subscriptions.TryRemove(key, out _);
                    }

                    continue;
                }

                if (currentSub.Subscriber != null && currentSub.Method != null)
                {
                    try
                    {
                        currentSub.Method.Invoke(eventData);
                    }
                    catch
                    {
                        // Ignore the exception for now.
                        // TODO: Not ignore the exception.
                    }
                }
            }
        }

        /// <summary>
        /// Asynchronously publishes event data to the event dispatcher.
        /// </summary>
        /// <param name="eventData">The <see cref="EventData"/> to publish.</param>
        /// <returns>The <see cref="Task"/> for the asynchronous work.</returns>
        public async Task PublishAsync(EventData eventData)
        {
            await this.taskFactory.StartNew(() => this.Publish(eventData));
        }

        /// <summary>
        /// Subscribes to an event.
        /// </summary>
        /// <param name="eventId">The event ID.</param>
        /// <param name="action">The <see cref="Action{T}"/> to call when the event is published.</param>
        /// <param name="subscriber">The subscriber of the event.</param>
        public void Subscribe(string eventId, Action<EventData> action, object subscriber)
        {
            if (string.IsNullOrWhiteSpace(eventId))
            {
                throw new ArgumentNullException(nameof(eventId));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (subscriber == null)
            {
                throw new ArgumentNullException(nameof(subscriber));
            }

            Guid subscriptionKey = Guid.NewGuid();

            ConcurrentDictionary<Guid, Subscription> subscriptions =
                this.subscriptionDictionary.GetOrAdd(eventId, (key) => new ConcurrentDictionary<Guid, Subscription>());

            subscriptions.TryAdd(subscriptionKey, new Subscription()
            {
                SubscriptionId = subscriptionKey,
                Subscriber = subscriber,
                Method = action
            });
        }

        /// <summary>
        /// Unsubscribes from an event.
        /// </summary>
        /// <param name="eventId">The event ID.</param>
        /// <param name="subscriber">The subscriber of the event.</param>
        /// <param name="action">The <see cref="Action{T}"/> to call when the event is published.</param>
        public void Unsubscribe(string eventId, object subscriber, Action<EventData> action)
        {
            if (!this.subscriptionDictionary.TryGetValue(eventId, out ConcurrentDictionary<Guid, Subscription> subscriptions))
            {
                return;
            }

            lock (subscriptions)
            {
                Subscription sub = subscriptions.FirstOrDefault(x =>
                {
                    return x.Value.Subscriber == subscriber
                        && x.Value.Method == action;
                }).Value;

                bool wasRemoved = false;
                while (!wasRemoved)
                {
                    wasRemoved = subscriptions.TryRemove(sub.SubscriptionId, out sub);
                }
            }
        }
    }
}
