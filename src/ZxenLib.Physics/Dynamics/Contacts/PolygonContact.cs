namespace ZxenLib.Physics.Dynamics.Contacts;

using System.Diagnostics;
using System.Runtime.CompilerServices;
using Collision;
using Collision.Collider;
using Collision.Shapes;
using Common;

public class PolygonContact : Contact
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void Evaluate(ref Manifold manifold, in Transform xfA, Transform xfB)
    {
        CollisionUtils.CollidePolygons(
            ref manifold,
            (PolygonShape)this.FixtureA.Shape,
            xfA,
            (PolygonShape)this.FixtureB.Shape,
            xfB);
    }
}

internal class PolygonContactFactory : IContactFactory
{
    private readonly ContactPool<PolygonContact> _pool = new ContactPool<PolygonContact>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Contact Create(Fixture fixtureA, int indexA, Fixture fixtureB, int indexB)
    {
        Debug.Assert(fixtureA.ShapeType == ShapeType.Polygon);
        Debug.Assert(fixtureB.ShapeType == ShapeType.Polygon);
        PolygonContact? contact = this._pool.Get();
        contact.Initialize(fixtureA, 0, fixtureB, 0);
        return contact;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Destroy(Contact contact)
    {
        this._pool.Return((PolygonContact)contact);
    }
}