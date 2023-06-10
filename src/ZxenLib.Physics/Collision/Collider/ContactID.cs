namespace ZxenLib.Physics.Collision.Collider;

using System.Runtime.InteropServices;

/// Contact ids to facilitate warm starting.
[StructLayout(LayoutKind.Explicit, Size = 4)]
public struct ContactId
{
    [FieldOffset(0)]
    public ContactFeature ContactFeature;

    /// Used to quickly compare contact ids.
    [FieldOffset(0)]
    public uint Key;
}