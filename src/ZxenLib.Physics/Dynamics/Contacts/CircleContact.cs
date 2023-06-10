namespace ZxenLib.Physics.Dynamics.Contacts;

using System.Diagnostics;
using System.Runtime.CompilerServices;
using Collision;
using Collision.Collider;
using Collision.Shapes;
using Common;

internal class CircleContact : Contact
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void Evaluate(ref Manifold manifold, in Transform xfA, Transform xfB)
    {
        CollisionUtils.CollideCircles(
            ref manifold,
            (CircleShape)this.FixtureA.Shape,
            xfA,
            (CircleShape)this.FixtureB.Shape,
            xfB);
    }
}

internal class CircleContactFactory : IContactFactory
{
    private readonly ContactPool<CircleContact> _pool = new ContactPool<CircleContact>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Contact Create(Fixture fixtureA, int indexA, Fixture fixtureB, int indexB)
    {
        Debug.Assert(fixtureA.ShapeType == ShapeType.Circle);
        Debug.Assert(fixtureB.ShapeType == ShapeType.Circle);
        CircleContact? contact = this._pool.Get();
        contact.Initialize(fixtureA, 0, fixtureB, 0);
        return contact;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Destroy(Contact contact)
    {
        this._pool.Return((CircleContact)contact);
    }
}