namespace ZxenLib.Physics.Dynamics;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Collision;
using Collision.Shapes;
using Common;
using Contacts;

internal class ContactRegister
{
    public readonly IContactFactory Factory;

    public readonly bool Primary;

    public ContactRegister(IContactFactory factory, bool primary)
    {
        this.Primary = primary;

        this.Factory = factory;
    }
}

// Delegate of b2World.
public class ContactManager : IAddPairCallback, IDisposable
{
    public static readonly IContactFilter DefaultContactFilter = new DefaultContactFilter();

    public BroadPhase BroadPhase { get; private set; } = new BroadPhase();

    public IContactFilter ContactFilter = DefaultContactFilter;

    public LinkedList<Contact> ContactList { get; private set; } = new LinkedList<Contact>();

    public IContactListener ContactListener;

    public int ContactCount => this.ContactList.Count;

    private static readonly ContactRegister[,] _registers = new ContactRegister[(int)ShapeType.TypeCount, (int)ShapeType.TypeCount];

    static ContactManager()
    {
        Register(ShapeType.Circle, ShapeType.Circle, new CircleContactFactory());
        Register(ShapeType.Polygon, ShapeType.Circle, new PolygonAndCircleContactFactory());
        Register(ShapeType.Polygon, ShapeType.Polygon, new PolygonContactFactory());
        Register(ShapeType.Edge, ShapeType.Circle, new EdgeAndCircleContactFactory());
        Register(ShapeType.Edge, ShapeType.Polygon, new EdgeAndPolygonContactFactory());
        Register(ShapeType.Chain, ShapeType.Circle, new ChainAndCircleContactFactory());
        Register(ShapeType.Chain, ShapeType.Polygon, new ChainAndPolygonContactFactory());

        void Register(ShapeType type1, ShapeType type2, IContactFactory factory)
        {
            Debug.Assert(0 <= type1 && type1 < ShapeType.TypeCount);
            Debug.Assert(0 <= type2 && type2 < ShapeType.TypeCount);

            _registers[(int)type1, (int)type2] = new ContactRegister(factory, true);
            if (type1 != type2)
            {
                _registers[(int)type2, (int)type1] = new ContactRegister(factory, false);
            }
        }
    }

    private const int DisposedFalse = 0;

    private const int DisposedTrue = 1;

    private int _disposed = DisposedFalse;

    public void Dispose()
    {
        if (Interlocked.Exchange(ref this._disposed, DisposedTrue) == DisposedTrue)
        {
            return;
        }

        this.BroadPhase = null;
        this.ContactList?.Clear();
        this.ContactList = null;

        this.ContactFilter = null;
        this.ContactListener = null;
    }

    private Contact CreateContact(
        Fixture fixtureA,
        int indexA,
        Fixture fixtureB,
        int indexB)
    {
        ShapeType type1 = fixtureA.ShapeType;
        ShapeType type2 = fixtureB.ShapeType;

        Debug.Assert(0 <= type1 && type1 < ShapeType.TypeCount);
        Debug.Assert(0 <= type2 && type2 < ShapeType.TypeCount);

        ContactRegister? reg = _registers[(int)type1, (int)type2]
                               ?? throw new NullReferenceException($"{type1.ToString()} can not contact to {type2.ToString()}");
        if (reg.Primary)
        {
            return reg.Factory.Create(fixtureA, indexA, fixtureB, indexB);
        }

        return reg.Factory.Create(fixtureB, indexB, fixtureA, indexA);
    }

    private void DestroyContact(Contact contact)
    {
        Fixture? fixtureA = contact.FixtureA;
        Fixture? fixtureB = contact.FixtureB;

        if (contact.Manifold.PointCount > 0 && fixtureA.IsSensor == false && fixtureB.IsSensor == false)
        {
            fixtureA.Body.IsAwake = true;
            fixtureB.Body.IsAwake = true;
        }

        ShapeType typeA = fixtureA.ShapeType;
        ShapeType typeB = fixtureB.ShapeType;

        Debug.Assert(0 <= typeA && typeB < ShapeType.TypeCount);
        Debug.Assert(0 <= typeA && typeB < ShapeType.TypeCount);
        ContactRegister? reg = _registers[(int)typeA, (int)typeB];
        reg.Factory.Destroy(contact);
    }

    public void AddPairCallback(object proxyUserDataA, object proxyUserDataB)
    {
        FixtureProxy? proxyA = (FixtureProxy)proxyUserDataA;
        FixtureProxy? proxyB = (FixtureProxy)proxyUserDataB;
        Fixture? fixtureA = proxyA.Fixture;
        Fixture? fixtureB = proxyB.Fixture;

        int indexA = proxyA.ChildIndex;
        int indexB = proxyB.ChildIndex;

        Body? bodyA = fixtureA.Body;
        Body? bodyB = fixtureB.Body;

        // Are the fixtures on the same body?
        if (bodyA == bodyB)
        {
            return;
        }

        // TODO_ERIN use a hash table to remove a potential bottleneck when both
        // bodies have a lot of contacts.
        // Does a contact already exist?
        LinkedListNode<ContactEdge>? node1 = bodyB.ContactEdges.First;
        while (node1 != null)
        {
            ContactEdge? contactEdge = node1.Value;
            node1 = node1.Next;

            if ((contactEdge.Contact.FixtureA == fixtureA
                 && contactEdge.Contact.FixtureB == fixtureB
                 && contactEdge.Contact.ChildIndexA == indexA
                 && contactEdge.Contact.ChildIndexB == indexB)
                || (contactEdge.Contact.FixtureA == fixtureB
                    && contactEdge.Contact.FixtureB == fixtureA
                    && contactEdge.Contact.ChildIndexA == indexB
                    && contactEdge.Contact.ChildIndexB == indexA))
            {
                // A contact already exists.
                return;
            }
        }

        if (bodyB.ShouldCollide(bodyA) == false                        // Does a joint override collision? Is at least one body dynamic?
            || this.ContactFilter?.ShouldCollide(fixtureA, fixtureB) == false) // Check user filtering.
        {
            return;
        }

        // Call the factory.
        Contact? c = this.CreateContact(fixtureA, indexA, fixtureB, indexB);
        Debug.Assert(c != default, "Get null contact!");

        // Contact creation may swap fixtures.
        fixtureA = c.FixtureA;
        fixtureB = c.FixtureB;
        bodyA = fixtureA.Body;
        bodyB = fixtureB.Body;

        // Insert into the world.
        c.Node.Value = c;
        this.ContactList.AddFirst(c.Node);

        // Connect to island graph.

        // Connect to body A
        c.NodeA.Contact = c;
        c.NodeA.Other = bodyB;
        c.NodeA.Node.Value = c.NodeA;
        bodyA.ContactEdges.AddFirst(c.NodeA.Node);

        // Connect to body B
        c.NodeB.Contact = c;
        c.NodeB.Other = bodyA;
        c.NodeB.Node.Value = c.NodeB;
        bodyB.ContactEdges.AddFirst(c.NodeB.Node);
    }

    public void FindNewContacts()
    {
        this.BroadPhase.UpdatePairs(this);
    }

    public void Destroy(Contact c)
    {
        Debug.Assert(this.ContactCount > 0);
        Fixture? fixtureA = c.FixtureA;
        Fixture? fixtureB = c.FixtureB;
        Body? bodyA = fixtureA.Body;
        Body? bodyB = fixtureB.Body;

        if (c.IsTouching) // 存在接触监听器且当前接触点接触,则触发结束接触
        {
            this.ContactListener?.EndContact(c);
        }

        // Remove from the world.
        this.ContactList.Remove(c.Node);

        // Remove from body 1
        bodyA.ContactEdges.Remove(c.NodeA.Node);

        // Remove from body 2
        bodyB.ContactEdges.Remove(c.NodeB.Node);

        // Call the factory.
        this.DestroyContact(c);
    }

    public void Collide()
    {
        LinkedListNode<Contact>? node = this.ContactList.First;

        // Update awake contacts.
        while (node != default)
        {
            Contact? c = node.Value;
            node = node.Next;
            Fixture? fixtureA = c.FixtureA;
            Fixture? fixtureB = c.FixtureB;
            int indexA = c.ChildIndexA;
            int indexB = c.ChildIndexB;
            Body? bodyA = fixtureA.Body;
            Body? bodyB = fixtureB.Body;

            // Is this contact flagged for filtering?
            if (c.Flags.IsSet(Contact.ContactFlag.FilterFlag))
            {
                // Should these bodies collide?
                if (bodyB.ShouldCollide(bodyA) == false)
                {
                    this.Destroy(c);
                    continue;
                }

                // Check user filtering.
                if (this.ContactFilter?.ShouldCollide(fixtureA, fixtureB) == false)
                {
                    this.Destroy(c);
                    continue;
                }

                // Clear the filtering flag.
                c.Flags &= ~Contact.ContactFlag.FilterFlag;
            }

            bool activeA = bodyA.IsAwake && bodyA.BodyType != BodyType.StaticBody;
            bool activeB = bodyB.IsAwake && bodyB.BodyType != BodyType.StaticBody;

            // At least one body must be awake and it must be dynamic or kinematic.
            if (activeA == false && activeB == false)
            {
                continue;
            }

            int proxyIdA = fixtureA.Proxies[indexA].ProxyId;
            int proxyIdB = fixtureB.Proxies[indexB].ProxyId;
            bool overlap = this.BroadPhase.TestOverlap(proxyIdA, proxyIdB);

            // Here we destroy contacts that cease to overlap in the broad-phase.
            if (overlap == false)
            {
                this.Destroy(c);
                continue;
            }

            // The contact persists.
            c.Update(this.ContactListener);
        }
    }
}