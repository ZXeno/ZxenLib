namespace ZxenLib.Physics.Dynamics;

/// Implement this class to provide collision filtering. In other words, you can implement
/// this class if you want finer control over contact creation.
public sealed class DefaultContactFilter : IContactFilter
{
    /// Return true if contact calculations should be performed between these two shapes.
    /// @warning for performance reasons this is only called when the AABBs begin to overlap.
    public bool ShouldCollide(Fixture fixtureA, Fixture fixtureB)
    {
        Filter filterA = fixtureA.Filter;
        Filter filterB = fixtureB.Filter;

        if (filterA.GroupIndex == filterB.GroupIndex && filterA.GroupIndex != 0)
        {
            return filterA.GroupIndex > 0;
        }

        bool collide = (filterA.MaskBits & filterB.CategoryBits) != 0
                       && (filterA.CategoryBits & filterB.MaskBits) != 0;
        return collide;
    }
}