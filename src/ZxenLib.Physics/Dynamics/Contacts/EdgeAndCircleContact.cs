namespace ZxenLib.Physics.Dynamics.Contacts;

using System.Diagnostics;
using System.Runtime.CompilerServices;
using Collision;
using Collision.Collider;
using Collision.Shapes;
using Common;

/// <summary>
///     边缘与圆接触
/// </summary>
public class EdgeAndCircleContact : Contact
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void Evaluate(ref Manifold manifold, in Transform xfA, Transform xfB)
    {
        CollisionUtils.CollideEdgeAndCircle(
            ref manifold,
            (EdgeShape)this.FixtureA.Shape,
            xfA,
            (CircleShape)this.FixtureB.Shape,
            xfB);
    }
}

internal class EdgeAndCircleContactFactory : IContactFactory
{
    private readonly ContactPool<EdgeAndCircleContact> _pool = new ContactPool<EdgeAndCircleContact>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Contact Create(Fixture fixtureA, int indexA, Fixture fixtureB, int indexB)
    {
        Debug.Assert(fixtureA.ShapeType == ShapeType.Edge);
        Debug.Assert(fixtureB.ShapeType == ShapeType.Circle);
        EdgeAndCircleContact? contact = this._pool.Get();
        contact.Initialize(fixtureA, 0, fixtureB, 0);
        return contact;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Destroy(Contact contact)
    {
        this._pool.Return((EdgeAndCircleContact)contact);
    }
}