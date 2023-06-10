#pragma warning disable CS8600
namespace ZxenLib.Events;

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Extensions;

/// <summary>
/// Provides a means for subscribing and broadcasting events to a set of registered listeners.
/// </summary>
public class EventDispatcher : IEventDispatcher
{
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
        ArgumentNullException.ThrowIfNull(eventData, nameof(eventData));
        ArgumentNullException.ThrowIfNull(eventData.Sender, nameof(eventData.Sender));
        eventData.EventId.ThrowIfNullOrWhitespace();

        if (!this.subscriptionDictionary.TryGetValue(eventData.EventId, out ConcurrentDictionary<Guid, Subscription> subscriptions))
        {
            return;
        }

        ConcurrentDictionary<Guid, Subscription> cachedSubs = new (subscriptions);

        foreach (Guid key in cachedSubs.Keys)
        {
            if (cachedSubs[key] is not Subscription currentSub)
            {
                subscriptions.TryRemove(key, out _);
                continue;
            }

            if (currentSub.Subscriber == null || currentSub.Method == null)
            {
                continue;
            }

            try
            {
                currentSub.Method.Invoke(eventData);
            }
            catch
            {
                // Handle exceptions if needed
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
        await Task.Run(() => this.Publish(eventData));
    }

    /// <summary>
    /// Subscribes to an event.
    /// </summary>
    /// <param name="eventId">The event ID.</param>
    /// <param name="action">The <see cref="Action{T}"/> to call when the event is published.</param>
    /// <param name="subscriber">The subscriber of the event.</param>
    public void Subscribe(string eventId, Action<EventData> action, object subscriber)
    {
        eventId.ThrowIfNullOrWhitespace();
        ArgumentNullException.ThrowIfNull(action);
        ArgumentNullException.ThrowIfNull(subscriber);

        Guid subscriptionKey = Guid.NewGuid();
        ConcurrentDictionary<Guid, Subscription> subscriptions = this.subscriptionDictionary.GetOrAdd(eventId, key => new ConcurrentDictionary<Guid, Subscription>());
        subscriptions.TryAdd(subscriptionKey, new Subscription { SubscriptionId = subscriptionKey, Subscriber = subscriber, Method = action });
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

        Subscription sub = subscriptions.FirstOrDefault(x => x.Value.Subscriber == subscriber && x.Value.Method == action).Value;

        if (sub != null)
        {
            subscriptions.TryRemove(sub.SubscriptionId, out _);
        }
    }
}