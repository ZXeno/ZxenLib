namespace ZxenLib.Entities;

using System;

public static class Ids
{
    private static uint currIdVal = 999;

    public static uint GetNewId()
    {
        return currIdVal++;
    }

    public static Guid GetNewGuidId()
    {
        return Guid.NewGuid();
    }

    public static string GetNewStringId()
    {
        return Guid.NewGuid().ToString();
    }
}