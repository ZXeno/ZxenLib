namespace ZxenLib.Physics.Dynamics.Contacts;

internal interface IContactFactory
{
    Contact Create(Fixture fixtureA, int indexA, Fixture fixtureB, int indexB);

    void Destroy(Contact contact);
}