namespace ZxenLib.Physics;

using System;

public static class PhysicsSettings
{
    public const float LengthUnitsPerMeter = 1.0f;

    /// A small length used as a collision and constraint tolerance. Usually it is
    /// chosen to be numerically significant, but visually insignificant.
    public const float LinearSlop = 0.005f * LengthUnitsPerMeter;

    /// The radius of the polygon/edge shape skin. This should not be modified. Making
    /// this smaller means polygons will have an insufficient buffer for continuous collision.
    /// Making it larger may create artifacts for vertex collision.
    public const float PolygonRadius = 2.0f * LinearSlop;
}