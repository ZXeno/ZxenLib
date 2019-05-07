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
        private readonly ConcurrentDictionary<string, SynchronizedCollection<Subscription>> subscriptionDictionary =
            new ConcurrentDictionary<string, SynchronizedCollection<Subscription>>();

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

            if (!this.subscriptionDictionary.TryGetValue(eventData.EventId, out SynchronizedCollection<Subscription> subscriptions))
            {
                return;
            }

            List<Subscription> cachedSubs;
            lock (subscriptions.SyncRoot)
            {
                cachedSubs = subscriptions.ToList();
            }

            for (int x = 0; x < cachedSubs.Count; x++)
            {
                Subscription currentSub = cachedSubs[x];

                if (currentSub == null)
                {
                    subscriptions.Remove(currentSub);
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
                else
                {
                    subscriptions.Remove(currentSub);
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

            SynchronizedCollection<Subscription> subscriptions =
                this.subscriptionDictionary.GetOrAdd(eventId, (key) => new SynchronizedCollection<Subscription>());

            subscriptions.Add(new Subscription()
            {
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
            if (!this.subscriptionDictionary.TryGetValue(eventId, out SynchronizedCollection<Subscription> subscriptions))
            {
                return;
            }

            lock (subscriptions.SyncRoot)
            {
                Subscription sub = subscriptions.FirstOrDefault(x =>
                {
                    return x.Subscriber == subscriber
                        && x.Method == action;
                });

                subscriptions.Remove(sub);
            }
        }
    }
}
