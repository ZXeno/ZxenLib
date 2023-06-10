namespace ZxenLib.Physics.Dynamics.Contacts;

using System.Diagnostics;
using System.Runtime.CompilerServices;
using Collision;
using Collision.Collider;
using Collision.Shapes;
using Common;

/// <summary>
///     多边形与圆接触
/// </summary>
public class PolygonAndCircleContact : Contact
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void Evaluate(ref Manifold manifold, in Transform xfA, Transform xfB)
    {
        CollisionUtils.CollidePolygonAndCircle(
            ref manifold,
            (PolygonShape)this.FixtureA.Shape,
            xfA,
            (CircleShape)this.FixtureB.Shape,
            xfB);
    }
}

internal class PolygonAndCircleContactFactory : IContactFactory
{
    private readonly ContactPool<PolygonAndCircleContact> _pool = new ContactPool<PolygonAndCircleContact>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Contact Create(Fixture fixtureA, int indexA, Fixture fixtureB, int indexB)
    {
        Debug.Assert(fixtureA.ShapeType == ShapeType.Polygon);
        Debug.Assert(fixtureB.ShapeType == ShapeType.Circle);
        PolygonAndCircleContact? contact = this._pool.Get();
        contact.Initialize(fixtureA, 0, fixtureB, 0);
        return contact;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Destroy(Contact contact)
    {
        this._pool.Return((PolygonAndCircleContact)contact);
    }
}