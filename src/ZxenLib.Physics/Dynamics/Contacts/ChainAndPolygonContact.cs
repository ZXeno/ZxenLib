namespace ZxenLib.Physics.Dynamics.Contacts;

using System.Diagnostics;
using System.Runtime.CompilerServices;
using Collision;
using Collision.Collider;
using Collision.Shapes;
using Common;

public class ChainAndPolygonContact : Contact
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void Evaluate(ref Manifold manifold, in Transform xfA, Transform xfB)
    {
        ChainShape? chain = (ChainShape)this.FixtureA.Shape;

        chain.GetChildEdge(out EdgeShape? edge, this.ChildIndexA);
        CollisionUtils.CollideEdgeAndPolygon(ref manifold, edge, xfA, (PolygonShape)this.FixtureB.Shape, xfB);
    }
}

internal class ChainAndPolygonContactFactory : IContactFactory
{
    private readonly ContactPool<ChainAndPolygonContact> _pool = new ContactPool<ChainAndPolygonContact>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Contact Create(Fixture fixtureA, int indexA, Fixture fixtureB, int indexB)
    {
        Debug.Assert(fixtureA.ShapeType == ShapeType.Chain);
        Debug.Assert(fixtureB.ShapeType == ShapeType.Polygon);
        ChainAndPolygonContact? contact = this._pool.Get();
        contact.Initialize(fixtureA, indexA, fixtureB, indexB);
        return contact;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Destroy(Contact contact)
    {
        this._pool.Return((ChainAndPolygonContact)contact);
    }
}