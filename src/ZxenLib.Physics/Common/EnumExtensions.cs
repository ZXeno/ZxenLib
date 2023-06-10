namespace ZxenLib.Physics.Common;

using Dynamics;
using Dynamics.Contacts;

public static class EnumExtensions
{
    public static bool IsSet(this BodyFlags self, BodyFlags flag) => (self & flag) == flag;

    public static bool IsSet(this DrawFlag self, DrawFlag flag) => (self & flag) == flag;

    public static bool IsSet(this Contact.ContactFlag self, Contact.ContactFlag flag) => (self & flag) == flag;
}