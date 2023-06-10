namespace ZxenLib.Entities.Components.Interfaces;

/// <summary>
/// Interface representing an updatable object.
/// </summary>
public interface IUpdatableEntityComponent : IEntityComponent
{
    /// <summary>
    /// Update is called every frame.
    /// </summary>
    /// <param name="deltaTime">The time in seconds it took to complete the last frame.</param>
    void Update(float deltaTime);
}