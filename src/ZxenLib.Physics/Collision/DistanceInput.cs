namespace ZxenLib.Physics.Collision;

using Common;

/// Input for b2Distance.
/// You have to option to use the shape radii
/// in the computation. Even
public struct DistanceInput
{
    public DistanceProxy ProxyA;

    public DistanceProxy ProxyB;

    public Transform TransformA;

    public Transform TransformB;

    public bool UseRadii;
}