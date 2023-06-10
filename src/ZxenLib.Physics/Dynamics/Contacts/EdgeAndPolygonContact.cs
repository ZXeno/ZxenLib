namespace ZxenLib.Physics.Dynamics.Contacts;

using System.Diagnostics;
using System.Runtime.CompilerServices;
using Collision;
using Collision.Collider;
using Collision.Shapes;
using Common;

/// <summary>
///     边缘与多边形接触
/// </summary>
public class EdgeAndPolygonContact : Contact
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void Evaluate(ref Manifold manifold, in Transform xfA, Transform xfB)
    {
        CollisionUtils.CollideEdgeAndPolygon(
            ref manifold,
            (EdgeShape)this.FixtureA.Shape,
            xfA,
            (PolygonShape)this.FixtureB.Shape,
            xfB);
    }
}

internal class EdgeAndPolygonContactFactory : IContactFactory
{
    private readonly ContactPool<EdgeAndPolygonContact> _pool = new ContactPool<EdgeAndPolygonContact>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Contact Create(Fixture fixtureA, int indexA, Fixture fixtureB, int indexB)
    {
        Debug.Assert(fixtureA.ShapeType == ShapeType.Edge);
        Debug.Assert(fixtureB.ShapeType == ShapeType.Polygon);
        EdgeAndPolygonContact? contact = this._pool.Get();
        contact.Initialize(fixtureA, 0, fixtureB, 0);
        return contact;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Destroy(Contact contact)
    {
        this._pool.Return((EdgeAndPolygonContact)contact);
    }
}