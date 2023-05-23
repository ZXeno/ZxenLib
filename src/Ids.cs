namespace ZxenLib;

using System;

/// <summary>
/// Used to create or generate IDs of various types.
/// </summary>
public static class Ids
{
    private static uint currIdVal = 1000;
    private static ushort cameraIds = 0;

    /// <summary>
    /// Returns a new unsigned integer ID starting from 1000.
    /// </summary>
    /// <returns><see cref="uint"/></returns>
    public static uint GetNewId()
    {
        return currIdVal++;
    }

    /// <summary>
    /// Returns a new <see cref="Guid"/> ID.
    /// </summary>
    /// <returns><see cref="Guid"/></returns>
    public static Guid GetNewGuidId()
    {
        return Guid.NewGuid();
    }

    /// <summary>
    /// Creates a new string ID by generating a new <see cref="Guid"/> and returning the value as a string.
    /// </summary>
    /// <returns><see cref="string"/></returns>
    public static string GetNewStringId()
    {
        return Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Gets a new camera draw order index starting from 0.
    /// </summary>
    /// <returns><see cref="ushort"/></returns>
    public static ushort GetNewCameraIndex()
    {
        return cameraIds++;
    }
}