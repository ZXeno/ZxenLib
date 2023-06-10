namespace ZxenLib.Physics.Dynamics.Contacts;

using System.Diagnostics;
using System.Runtime.CompilerServices;
using Collision;
using Collision.Collider;
using Collision.Shapes;
using Common;

public class ChainAndCircleContact : Contact
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void Evaluate(ref Manifold manifold, in Transform xfA, Transform xfB)
    {
        ChainShape? chain = (ChainShape)this.FixtureA.Shape;

        chain.GetChildEdge(out EdgeShape? edge, this.ChildIndexA);
        CollisionUtils.CollideEdgeAndCircle(
            ref manifold,
            edge,
            xfA,
            (CircleShape)this.FixtureB.Shape,
            xfB);
    }
}

internal class ChainAndCircleContactFactory : IContactFactory
{
    private readonly ContactPool<ChainAndCircleContact> _pool = new ContactPool<ChainAndCircleContact>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Contact Create(Fixture fixtureA, int indexA, Fixture fixtureB, int indexB)
    {
        Debug.Assert(fixtureA.ShapeType == ShapeType.Chain);
        Debug.Assert(fixtureB.ShapeType == ShapeType.Circle);
        ChainAndCircleContact? contact = this._pool.Get();
        contact.Initialize(fixtureA, indexA, fixtureB, indexB);
        return contact;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Destroy(Contact contact)
    {
        this._pool.Return((ChainAndCircleContact)contact);
    }
}