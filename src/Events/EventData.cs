namespace ZxenLib.Events;

using System;

/// <summary>
/// Container class for <see cref="EventDispatcher"/> event data.
/// </summary>
public class EventData
{
    /// <summary>
    /// Gets or sets the ID of the event.
    /// </summary>
    public string EventId { get; set; }

    /// <summary>
    /// Gets or sets the sender of the event.
    /// </summary>
    public object Sender { get; set; }

    /// <summary>
    /// Gets or sets the target object Id.
    /// </summary>
    public uint TargetObjectId { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="EventArgs"/> of the event.
    /// </summary>
    public EventArgs EventArguments { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the event is handled.
    /// </summary>
    public bool Handled { get; set; } = false;
}