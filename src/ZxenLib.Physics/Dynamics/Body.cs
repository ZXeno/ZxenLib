namespace ZxenLib.Physics.Dynamics;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using System.Runtime.CompilerServices;
using Collision;
using Collision.Shapes;
using Common;
using Contacts;
using Joints;

/// The body type.
/// static: zero mass, zero velocity, may be manually moved
/// kinematic: zero mass, non-zero velocity set by user, moved by solver
/// dynamic: positive mass, non-zero velocity determined by forces, moved by solver
public enum BodyType
{
    StaticBody = 0,

    KinematicBody = 1,

    DynamicBody = 2
}

/// A body definition holds all the data needed to construct a rigid body.
/// You can safely re-use body definitions. Shapes are added to a body after construction.
public struct BodyDef
{
    private bool? _enabled;

    /// Does this body start out enabled?
    public bool Enabled
    {
        get => this._enabled ?? true;
        set => this._enabled = value;
    }

    private bool? _allowSleep;

    /// Set this flag to false if this body should never fall asleep. Note that
    /// this increases CPU usage.
    public bool AllowSleep
    {
        get => this._allowSleep ?? true;
        set => this._allowSleep = value;
    }

    /// The world angle of the body in radians.
    public float Angle;

    /// Angular damping is use to reduce the angular velocity. The damping parameter
    /// can be larger than 1.0f but the damping effect becomes sensitive to the
    /// time step when the damping parameter is large.
    /// Units are 1/time
    public float AngularDamping;

    /// The angular velocity of the body.
    public float AngularVelocity;

    private bool? _awake;

    /// Is this body initially awake or sleeping?
    public bool Awake
    {
        get => this._awake ?? true;
        set => this._awake = value;
    }

    /// The body type: static, kinematic, or dynamic.
    /// Note: if a dynamic body would have zero mass, the mass is set to one.
    public BodyType BodyType;

    /// Is this a fast moving body that should be prevented from tunneling through
    /// other moving bodies? Note that all bodies are prevented from tunneling through
    /// kinematic and static bodies. This setting is only considered on dynamic bodies.
    /// @warning You should use this flag sparingly since it increases processing time.
    public bool Bullet;

    /// Should this body be prevented from rotating? Useful for characters.
    public bool FixedRotation;

    private float? _gravityScale;

    /// Scale the gravity applied to this body.
    public float GravityScale
    {
        get => this._gravityScale ?? 1.0f;
        set => this._gravityScale = value;
    }

    /// Linear damping is use to reduce the linear velocity. The damping parameter
    /// can be larger than 1.0f but the damping effect becomes sensitive to the
    /// time step when the damping parameter is large.
    /// Units are 1/time
    public float LinearDamping;

    /// The linear velocity of the body's origin in world co-ordinates.
    public Vector2 LinearVelocity;

    /// The world position of the body. Avoid creating bodies at the origin
    /// since this can lead to many overlapping shapes.
    public Vector2 Position;

    /// Use this to store application specific body data.
    public object UserData;
}

/// A rigid body. These are created via b2World::CreateBody.
public class Body : IDisposable
{
    /// <summary>
    /// 接触边缘列表
    /// </summary>
    internal readonly LinkedList<ContactEdge> ContactEdges;

    /// <summary>
    /// 夹具列表
    /// </summary>
    public IReadOnlyList<Fixture> FixtureList => this.Fixtures;

    /// <summary>
    /// 夹具列表
    /// </summary>
    internal readonly List<Fixture> Fixtures;

    /// <summary>
    /// 关节边缘列表
    /// </summary>
    internal readonly LinkedList<JointEdge> JointEdges;

    /// <summary>
    /// Get/Set the angular damping of the body.
    /// 角阻尼
    /// </summary>
    private float _angularDamping;

    /// <summary>
    /// 质心的转动惯量
    /// </summary>
    private float _inertia;

    /// <summary>
    /// 线性阻尼
    /// </summary>
    private float _linearDamping;

    /// <summary>
    /// Get the total mass of the body.
    /// @return the mass, usually in kilograms (kg).
    /// 质量
    /// </summary>
    private float _mass;

    /// <summary>
    /// 物体类型
    /// </summary>
    private BodyType _type;

    /// <summary>
    /// 所属世界
    /// </summary>
    internal World _world;

    /// <summary>
    /// 物体标志
    /// </summary>
    internal BodyFlags Flags;

    /// <summary>
    /// 受力
    /// </summary>
    internal Vector2 Force;

    /// <summary>
    /// 重力系数
    /// </summary>
    internal float GravityScale;

    /// <summary>
    /// 质心的转动惯量倒数
    /// </summary>
    internal float InverseInertia;

    /// <summary>
    /// 质量倒数
    /// </summary>
    internal float InvMass;

    /// <summary>
    /// 岛屿索引
    /// </summary>
    internal int IslandIndex;

    /// <summary>
    /// 链表节点物体
    /// </summary>
    internal LinkedListNode<Body> Node;

    /// <summary>
    /// 扫描
    /// </summary>
    internal Sweep Sweep; // the swept motion for CCD

    /// <summary>
    /// 扭矩
    /// </summary>
    internal float Torque;

    /// <summary>
    /// 物体位置
    /// </summary>
    internal Transform Transform; // the body origin transform

    internal Body(in BodyDef def, World world)
    {
        Debug.Assert(def.Position.IsValid());
        Debug.Assert(def.LinearVelocity.IsValid());
        Debug.Assert(def.Angle.IsValid());
        Debug.Assert(def.AngularVelocity.IsValid());
        Debug.Assert(def.AngularDamping.IsValid() && def.AngularDamping >= 0.0f);
        Debug.Assert(def.LinearDamping.IsValid() && def.LinearDamping >= 0.0f);

        this.Flags = 0;

        if (def.Bullet)
        {
            this.Flags |= BodyFlags.IsBullet;
        }

        if (def.FixedRotation)
        {
            this.Flags |= BodyFlags.FixedRotation;
        }

        if (def.AllowSleep)
        {
            this.Flags |= BodyFlags.AutoSleep;
        }

        if (def.Awake && def.BodyType != BodyType.StaticBody)
        {
            this.Flags |= BodyFlags.IsAwake;
        }

        if (def.Enabled)
        {
            this.Flags |= BodyFlags.IsEnabled;
        }

        this._world = world;

        this.Transform.Position = def.Position;
        this.Transform.Rotation.Set(def.Angle);

        this.Sweep = new Sweep
        {
            LocalCenter = Vector2.Zero,
            C0 = this.Transform.Position,
            C = this.Transform.Position,
            A0 = def.Angle,
            A = def.Angle,
            Alpha0 = 0.0f
        };

        this.JointEdges = new LinkedList<JointEdge>();
        this.ContactEdges = new LinkedList<ContactEdge>();
        this.Fixtures = new List<Fixture>();
        this.Node = null;

        this.LinearVelocity = def.LinearVelocity;
        this.AngularVelocity = def.AngularVelocity;

        this._linearDamping = def.LinearDamping;
        this.AngularDamping = def.AngularDamping;
        this.GravityScale = def.GravityScale;

        this.Force.SetZero();
        this.Torque = 0.0f;

        this.SleepTime = 0.0f;

        this._type = def.BodyType;

        this._mass = 0.0f;
        this.InvMass = 0.0f;

        this._inertia = 0.0f;
        this.InverseInertia = 0.0f;

        this.UserData = def.UserData;
    }

    public float AngularDamping
    {
        get => this._angularDamping;
        set => this._angularDamping = value;
    }

    /// <summary>
    /// Get/Set the angular velocity.
    /// the new angular velocity in radians/second.
    /// 角速度
    /// </summary>
    public float AngularVelocity { get; internal set; }

    /// Get the rotational inertia of the body about the local origin.
    /// @return the rotational inertia, usually in kg-m^2.
    public float Inertia => this._inertia + this._mass * Vector2.Dot(this.Sweep.LocalCenter, this.Sweep.LocalCenter);

    /// Get/Set the linear damping of the body.
    public float LinearDamping
    {
        get => this._linearDamping;
        set => this._linearDamping = value;
    }

    /// <summary>
    /// 线速度
    /// </summary>
    /// Set the linear velocity of the center of mass.
    /// @param v the new linear velocity of the center of mass.
    /// Get the linear velocity of the center of mass.
    /// @return the linear velocity of the center of mass.
    public Vector2 LinearVelocity { get; internal set; }

    public float Mass => this._mass;

    /// <summary>
    /// 休眠时间
    /// </summary>
    internal float SleepTime { get; set; }

    /// Set the type of this body. This may alter the mass and velocity.
    public BodyType BodyType
    {
        get => this._type;
        set
        {
            Debug.Assert(this._world.IsLocked == false);
            if (this._world.IsLocked)
            {
                return;
            }

            if (this._type == value)
            {
                return;
            }

            this._type = value;

            this.ResetMassData();

            if (this._type == BodyType.StaticBody)
            {
                this.LinearVelocity = Vector2.Zero;
                this.AngularVelocity = 0.0f;
                this.Sweep.A0 = this.Sweep.A;
                this.Sweep.C0 = this.Sweep.C;
                this.UnsetFlag(BodyFlags.IsAwake);
                this.SynchronizeFixtures();
            }

            this.IsAwake = true;

            this.Force.SetZero();
            this.Torque = 0.0f;

            // Delete the attached contacts.
            // 删除所有接触点

            LinkedListNode<ContactEdge>? node = this.ContactEdges.First;
            while (node != null)
            {
                ContactEdge? c = node.Value;
                node = node.Next;
                this._world.ContactManager.Destroy(c.Contact);
            }

            this.ContactEdges.Clear();

            // Touch the proxies so that new contacts will be created (when appropriate)
            BroadPhase? broadPhase = this._world.ContactManager.BroadPhase;
            foreach (Fixture? f in this.Fixtures)
            {
                int proxyCount = f.ProxyCount;
                for (int i = 0; i < proxyCount; ++i)
                {
                    broadPhase.TouchProxy(f.Proxies[i].ProxyId);
                }
            }
        }
    }

    /// Should this body be treated like a bullet for continuous collision detection?
    /// Is this body treated like a bullet for continuous collision detection?
    public bool IsBullet
    {
        get => this.Flags.IsSet(BodyFlags.IsBullet);
        set
        {
            if (value)
            {
                this.Flags |= BodyFlags.IsBullet;
            }
            else
            {
                this.Flags &= ~BodyFlags.IsBullet;
            }
        }
    }

    /// You can disable sleeping on this body. If you disable sleeping, the
    /// body will be woken.
    /// Is this body allowed to sleep
    public bool IsSleepingAllowed
    {
        get => this.Flags.IsSet(BodyFlags.AutoSleep);
        set
        {
            if (value)
            {
                this.Flags |= BodyFlags.AutoSleep;
            }
            else
            {
                this.Flags &= ~BodyFlags.AutoSleep;
                this.IsAwake = true;
            }
        }
    }

    /// <summary>
    /// Set the sleep state of the body. A sleeping body has very
    /// low CPU cost.
    /// @param flag set to true to wake the body, false to put it to sleep.
    /// Get the sleeping state of this body.
    /// @return true if the body is awake.
    /// </summary>
    public bool IsAwake
    {
        get => this.Flags.IsSet(BodyFlags.IsAwake);
        set
        {
            if (this.BodyType == BodyType.StaticBody)
            {
                return;
            }

            if (value)
            {
                this.Flags |= BodyFlags.IsAwake;
                this.SleepTime = 0.0f;
            }
            else
            {
                this.Flags &= ~BodyFlags.IsAwake;
                this.SleepTime = 0.0f;
                this.LinearVelocity = Vector2.Zero;
                this.AngularVelocity = 0.0f;
                this.Force.SetZero();
                this.Torque = 0.0f;
            }
        }
    }

    /// <summary>
    /// Set the active state of the body. An inactive body is not
    /// simulated and cannot be collided with or woken up.
    /// If you pass a flag of true, all fixtures will be added to the
    /// broad-phase.
    /// If you pass a flag of false, all fixtures will be removed from
    /// the broad-phase and all contacts will be destroyed.
    /// Fixtures and joints are otherwise unaffected. You may continue
    /// to create/destroy fixtures and joints on inactive bodies.
    /// Fixtures on an inactive body are implicitly inactive and will
    /// not participate in collisions, ray-casts, or queries.
    /// Joints connected to an inactive body are implicitly inactive.
    /// An inactive body is still owned by a b2World object and remains
    /// in the body list.
    /// Get the active state of the body.
    /// </summary>
    public bool IsEnabled

    {
        get => this.Flags.IsSet(BodyFlags.IsEnabled);
        set
        {
            Debug.Assert(this._world.IsLocked == false);

            if (value == this.IsEnabled)
            {
                return;
            }

            if (value)
            {
                this.Flags |= BodyFlags.IsEnabled;

                // Create all proxies.
                // 激活时创建粗检测代理
                BroadPhase? broadPhase = this._world.ContactManager.BroadPhase;
                foreach (Fixture? f in this.Fixtures)
                {
                    f.CreateProxies(broadPhase, this.Transform);
                }

                // Contacts are created at the beginning of the next
                this.World.HasNewContacts = true;
            }
            else
            {
                this.Flags &= ~BodyFlags.IsEnabled;

                // Destroy all proxies.
                // 休眠时销毁粗检测代理
                BroadPhase? broadPhase = this._world.ContactManager.BroadPhase;
                foreach (Fixture? f in this.Fixtures)
                {
                    f.DestroyProxies(broadPhase);
                }

                // Destroy the attached contacts.
                // 销毁接触点
                LinkedListNode<ContactEdge>? node = this.ContactEdges.First;
                while (node != null)
                {
                    ContactEdge? c = node.Value;
                    node = node.Next;
                    this._world.ContactManager.Destroy(c.Contact);
                }

                this.ContactEdges.Clear();
            }
        }
    }

    /// Set this body to have fixed rotation. This causes the mass
    /// to be reset.
    public bool IsFixedRotation
    {
        get => this.Flags.IsSet(BodyFlags.FixedRotation);
        set
        {
            // 物体已经有固定旋转,不需要设置
            if (this.Flags.IsSet(BodyFlags.FixedRotation) && value)
            {
                return;
            }

            if (value)
            {
                this.Flags |= BodyFlags.FixedRotation;
            }
            else
            {
                this.Flags &= ~BodyFlags.FixedRotation;
            }

            this.AngularVelocity = 0.0f;

            this.ResetMassData();
        }
    }

    /// <summary>
    /// Get/Set the user data pointer that was provided in the body definition.
    /// 用户信息
    /// </summary>
    public object UserData { get; set; }

    /// Get the parent world of this body.
    public World World => this._world;

    /// <inheritdoc />
    public void Dispose()
    {
        this._world = null;
        Debug.Assert(this.ContactEdges.Count == 0, "ContactEdges.Count == 0");
        Debug.Assert(this.JointEdges.Count == 0, "JointEdges.Count == 0");
        this.ContactEdges?.Clear();
        this.JointEdges?.Clear();
        this.Fixtures?.Clear();
        GC.SuppressFinalize(this);
    }

    public void SetAngularVelocity(float value)
    {
        if (this._type == BodyType.StaticBody) // 静态物体无角速度
        {
            return;
        }

        if (value * value > 0.0f)
        {
            this.IsAwake = true;
        }

        this.AngularVelocity = value;
    }

    public void SetLinearVelocity(in Vector2 value)
    {
        if (this._type == BodyType.StaticBody) // 静态物体无加速度
        {
            return;
        }

        if (Vector2.Dot(value, value) > 0.0f) // 点积大于0时唤醒本物体
        {
            this.IsAwake = true;
        }

        this.LinearVelocity = value;
    }

    /// <summary>
    /// Creates a fixture and attach it to this body. Use this function if you need
    /// to set some fixture parameters, like friction. Otherwise you can create the
    /// fixture directly from a shape.
    /// If the density is non-zero, this function automatically updates the mass of the body.
    /// Contacts are not created until the next time step.
    /// @param def the fixture definition.
    /// @warning This function is locked during callbacks.
    /// 创建夹具
    /// </summary>
    /// <param name="def"></param>
    /// <returns></returns>
    public Fixture CreateFixture(FixtureDef def)
    {
        Debug.Assert(this._world.IsLocked == false);
        if (this._world.IsLocked)
        {
            return null;
        }

        Fixture? fixture = Fixture.Create(this, def);

        if (this.Flags.IsSet(BodyFlags.IsEnabled))
        {
            BroadPhase? broadPhase = this._world.ContactManager.BroadPhase;
            fixture.CreateProxies(broadPhase, this.Transform);
        }

        fixture.Body = this;
        this.Fixtures.Add(fixture);

        // Adjust mass properties if needed.
        if (fixture.Density > 0.0f)
        {
            this.ResetMassData();
        }

        // Let the world know we have a new fixture. This will cause new contacts
        // to be created at the beginning of the next time step.
        // 通知世界存在新增夹具,在下一个时间步中将自动创建新夹具的接触点
        this._world.HasNewContacts = true;

        return fixture;
    }

    /// Creates a fixture from a shape and attach it to this body.
    /// This is a convenience function. Use b2FixtureDef if you need to set parameters
    /// like friction, restitution, user data, or filtering.
    /// If the density is non-zero, this function automatically updates the mass of the body.
    /// @param shape the shape to be cloned.
    /// @param density the shape density (set to zero for static bodies).
    /// @warning This function is locked during callbacks.
    /// 创建夹具
    public Fixture CreateFixture(Shape shape, float density)
    {
        FixtureDef def = new FixtureDef { Shape = shape, Density = density };

        return this.CreateFixture(def);
    }

    /// Destroy a fixture. This removes the fixture from the broad-phase and
    /// destroys all contacts associated with this fixture. This will
    /// automatically adjust the mass of the body if the body is dynamic and the
    /// fixture has positive density.
    /// All fixtures attached to a body are implicitly destroyed when the body is destroyed.
    /// @param fixture the fixture to be removed.
    /// @warning This function is locked during callbacks.
    /// 删除夹具
    public void DestroyFixture(Fixture fixture)
    {
        if (fixture == default)
        {
            return;
        }

        // 世界锁定时不能删除夹具
        Debug.Assert(this._world.IsLocked == false);
        if (this._world.IsLocked)
        {
            return;
        }

        // 断言夹具所属物体
        Debug.Assert(fixture.Body == this);

        // Remove the fixture from this body's singly linked list.
        Debug.Assert(this.Fixtures.Count > 0);

        // You tried to remove a shape that is not attached to this body.
        // 确定该夹具存在于物体的夹具列表中
        Debug.Assert(this.Fixtures.Any(e => e == fixture));
        float density = fixture.Density;

        // Destroy any contacts associated with the fixture.
        // 销毁关联在夹具上的接触点
        LinkedListNode<ContactEdge>? node = this.ContactEdges.First;
        while (node != null)
        {
            ContactEdge? contactEdge = node.Value;
            node = node.Next;
            if (contactEdge.Contact.FixtureA == fixture || contactEdge.Contact.FixtureB == fixture)
            {
                // This destroys the contact and removes it from
                // this body's contact list.
                this._world.ContactManager.Destroy(contactEdge.Contact);
            }
        }

        // 如果物体处于活跃状态,销毁夹具的粗检测代理对象
        if (this.Flags.IsSet(BodyFlags.IsEnabled))
        {
            BroadPhase? broadPhase = this._world.ContactManager.BroadPhase;
            fixture.DestroyProxies(broadPhase);
        }

        this.Fixtures.Remove(fixture);
        fixture.Body = null;
        Fixture.Destroy(fixture);

        // Reset the mass data.
        // 夹具销毁后重新计算物体质量
        if (density > 0.0f)
        {
            this.ResetMassData();
        }
    }

    /// Set the position of the body's origin and rotation.
    /// Manipulating a body's transform may cause non-physical behavior.
    /// Note: contacts are updated on the next call to b2World::Step.
    /// @param position the world position of the body's local origin.
    /// @param angle the world rotation in radians.
    public void SetTransform(in Vector2 position, float angle)
    {
        Debug.Assert(this._world.IsLocked == false);
        if (this._world.IsLocked)
        {
            return;
        }

        this.Transform.Rotation.Set(angle);
        this.Transform.Position = position;

        this.Sweep.C = MathUtils.Mul(this.Transform, this.Sweep.LocalCenter);
        this.Sweep.A = angle;

        this.Sweep.C0 = this.Sweep.C;
        this.Sweep.A0 = angle;

        BroadPhase? broadPhase = this._world.ContactManager.BroadPhase;
        foreach (Fixture? f in this.Fixtures)
        {
            f.Synchronize(broadPhase, this.Transform, this.Transform);
        }

        // Check for new contacts the next step
        this.World.HasNewContacts = true;
    }

    /// Get the body transform for the body's origin.
    /// @return the world transform of the body's origin.
    public Transform GetTransform()
    {
        return this.Transform;
    }

    /// Get the world body origin position.
    /// @return the world position of the body's origin.
    public Vector2 GetPosition()
    {
        return this.Transform.Position;
    }

    /// Get the angle in radians.
    /// @return the current world rotation angle in radians.
    public float GetAngle()
    {
        return this.Sweep.A;
    }

    /// Get the world position of the center of mass.
    public Vector2 GetWorldCenter()
    {
        return this.Sweep.C;
    }

    /// Get the local position of the center of mass.
    public Vector2 GetLocalCenter()
    {
        return this.Sweep.LocalCenter;
    }

    /// <summary>
    /// Apply a force at a world point. If the force is not
    /// applied at the center of mass, it will generate a torque and
    /// affect the angular velocity. This wakes up the body.
    /// @param force the world force vector, usually in Newtons (N).
    /// @param point the world position of the point of application.
    /// @param wake also wake up the body
    /// 在指定位置施加作用力
    /// </summary>
    /// <param name="force"></param>
    /// <param name="point"></param>
    /// <param name="wake"></param>
    public void ApplyForce(in Vector2 force, in Vector2 point, bool wake)
    {
        if (this._type != BodyType.DynamicBody)
        {
            return;
        }

        if (wake && !this.Flags.IsSet(BodyFlags.IsAwake))
        {
            this.IsAwake = true;
        }

        // Don't accumulate a force if the body is sleeping.
        if (this.Flags.IsSet(BodyFlags.IsAwake))
        {
            this.Force += force;
            this.Torque += MathUtils.Cross(point - this.Sweep.C, force);
        }
    }

    /// <summary>
    /// Apply a force to the center of mass. This wakes up the body.
    /// @param force the world force vector, usually in Newtons (N).
    /// @param wake also wake up the body
    /// 在质心施加作用力
    /// </summary>
    /// <param name="force"></param>
    /// <param name="wake"></param>
    public void ApplyForceToCenter(in Vector2 force, bool wake)
    {
        if (this._type != BodyType.DynamicBody)
        {
            return;
        }

        if (wake && !this.Flags.IsSet(BodyFlags.IsAwake))
        {
            this.IsAwake = true;
        }

        // Don't accumulate a force if the body is sleeping
        if (this.Flags.IsSet(BodyFlags.IsAwake))
        {
            this.Force += force;
        }
    }

    /// <summary>
    /// Apply a torque. This affects the angular velocity
    /// without affecting the linear velocity of the center of mass.
    /// @param torque about the z-axis (out of the screen), usually in N-m.
    /// @param wake also wake up the body
    /// 施加扭矩
    /// </summary>
    /// <param name="torque"></param>
    /// <param name="wake"></param>
    public void ApplyTorque(float torque, bool wake)
    {
        if (this._type != BodyType.DynamicBody)
        {
            return;
        }

        if (wake && !this.Flags.IsSet(BodyFlags.IsAwake))
        {
            this.IsAwake = true;
        }

        // Don't accumulate a force if the body is sleeping
        if (this.Flags.IsSet(BodyFlags.IsAwake))
        {
            this.Torque += torque;
        }
    }

    /// <summary>
    /// Apply an impulse at a point. This immediately modifies the velocity.
    /// It also modifies the angular velocity if the point of application
    /// is not at the center of mass. This wakes up the body.
    /// @param impulse the world impulse vector, usually in N-seconds or kg-m/s.
    /// @param point the world position of the point of application.
    /// @param wake also wake up the body
    /// 在物体指定位置施加线性冲量
    /// </summary>
    /// <param name="impulse"></param>
    /// <param name="point"></param>
    /// <param name="wake"></param>
    public void ApplyLinearImpulse(in Vector2 impulse, in Vector2 point, bool wake)
    {
        if (this._type != BodyType.DynamicBody)
        {
            return;
        }

        if (wake && !this.Flags.IsSet(BodyFlags.IsAwake))
        {
            this.IsAwake = true;
        }

        // Don't accumulate velocity if the body is sleeping
        if (this.Flags.IsSet(BodyFlags.IsAwake))
        {
            this.LinearVelocity += this.InvMass * impulse;
            this.AngularVelocity += this.InverseInertia * MathUtils.Cross(point - this.Sweep.C, impulse);
        }
    }

    /// <summary>
    /// Apply an impulse to the center of mass. This immediately modifies the velocity.
    /// @param impulse the world impulse vector, usually in N-seconds or kg-m/s.
    /// @param wake also wake up the body
    /// 在质心施加线性冲量
    /// </summary>
    /// <param name="impulse"></param>
    /// <param name="wake"></param>
    public void ApplyLinearImpulseToCenter(in Vector2 impulse, bool wake)
    {
        if (this._type != BodyType.DynamicBody)
        {
            return;
        }

        if (wake && !this.Flags.IsSet(BodyFlags.IsAwake))
        {
            this.IsAwake = true;
        }

        // Don't accumulate velocity if the body is sleeping
        if (this.Flags.IsSet(BodyFlags.IsAwake))
        {
            this.LinearVelocity += this.InvMass * impulse;
        }
    }

    /// <summary>
    /// Apply an angular impulse.
    /// @param impulse the angular impulse in units of kg*m*m/s
    /// @param wake also wake up the body
    /// 施加角冲量
    /// </summary>
    /// <param name="impulse"></param>
    /// <param name="wake"></param>
    public void ApplyAngularImpulse(float impulse, bool wake)
    {
        if (this._type != BodyType.DynamicBody)
        {
            return;
        }

        if (wake && !this.Flags.IsSet(BodyFlags.IsAwake))
        {
            this.IsAwake = true;
        }

        // Don't accumulate velocity if the body is sleeping
        if ((this.Flags & BodyFlags.IsAwake) != 0)
        {
            this.AngularVelocity += this.InverseInertia * impulse;
        }
    }

    /// Get the mass data of the body.
    /// @return a struct containing the mass, inertia and center of the body.
    public MassData GetMassData()
    {
        return new MassData
        {
            Mass = this._mass,
            RotationInertia = this._inertia + this._mass * Vector2.Dot(this.Sweep.LocalCenter, this.Sweep.LocalCenter),
            Center = this.Sweep.LocalCenter
        };
    }

    /// Set the mass properties to override the mass properties of the fixtures.
    /// Note that this changes the center of mass position.
    /// Note that creating or destroying fixtures can also alter the mass.
    /// This function has no effect if the body isn't dynamic.
    /// @param massData the mass properties.
    public void SetMassData(in MassData massData)
    {
        Debug.Assert(this._world.IsLocked == false);
        if (this._world.IsLocked)
        {
            return;
        }

        if (this._type != BodyType.DynamicBody)
        {
            return;
        }

        this.InvMass = 0.0f;
        this._inertia = 0.0f;
        this.InverseInertia = 0.0f;

        this._mass = massData.Mass;
        if (this._mass <= 0.0f)
        {
            this._mass = 1.0f;
        }

        this.InvMass = 1.0f / this._mass;

        if (massData.RotationInertia > 0.0f && !this.Flags.IsSet(BodyFlags.FixedRotation)) // 存在转动惯量且物体可旋转
        {
            this._inertia = massData.RotationInertia - this._mass * Vector2.Dot(massData.Center, massData.Center);
            Debug.Assert(this._inertia > 0.0f);
            this.InverseInertia = 1.0f / this._inertia;
        }

        // Move center of mass.
        Vector2 oldCenter = this.Sweep.C;
        this.Sweep.LocalCenter = massData.Center;
        this.Sweep.C0 = this.Sweep.C = MathUtils.Mul(this.Transform, this.Sweep.LocalCenter);

        // Update center of mass velocity.
        this.LinearVelocity += MathUtils.Cross(this.AngularVelocity, this.Sweep.C - oldCenter);
    }

    /// This resets the mass properties to the sum of the mass properties of the fixtures.
    /// This normally does not need to be called unless you called SetMassData to override
    /// the mass and you later want to reset the mass.
    /// 重置质量数据
    private void ResetMassData()
    {
        // Compute mass data from shapes. Each shape has its own density.
        // 从所有形状计算质量数据,每个形状都有各自的密度
        this._mass = 0.0f;
        this.InvMass = 0.0f;
        this._inertia = 0.0f;
        this.InverseInertia = 0.0f;
        this.Sweep.LocalCenter.SetZero();

        // Static and kinematic bodies have zero mass.
        if (this._type == BodyType.StaticBody || this._type == BodyType.KinematicBody)
        {
            this.Sweep.C0 = this.Transform.Position;
            this.Sweep.C = this.Transform.Position;
            this.Sweep.A0 = this.Sweep.A;
            return;
        }

        Debug.Assert(this._type == BodyType.DynamicBody);

        // Accumulate mass over all fixtures.
        Vector2 localCenter = Vector2.Zero;
        foreach (Fixture? f in this.Fixtures)
        {
            if (f.Density.Equals(0.0f))
            {
                continue;
            }

            f.GetMassData(out MassData massData);
            this._mass += massData.Mass;
            localCenter += massData.Mass * massData.Center;
            this._inertia += massData.RotationInertia;
        }

        // Compute center of mass.
        if (this._mass > 0.0f)
        {
            this.InvMass = 1.0f / this._mass;
            localCenter *= this.InvMass;
        }

        if (this._inertia > 0.0f && !this.Flags.IsSet(BodyFlags.FixedRotation)) // 存在转动惯量且物体可旋转
        {
            // Center the inertia about the center of mass.
            this._inertia -= this._mass * Vector2.Dot(localCenter, localCenter);
            Debug.Assert(this._inertia > 0.0f);
            this.InverseInertia = 1.0f / this._inertia;
        }
        else
        {
            this._inertia = 0.0f;
            this.InverseInertia = 0.0f;
        }

        // Move center of mass.
        Vector2 oldCenter = this.Sweep.C;
        this.Sweep.LocalCenter = localCenter;
        this.Sweep.C0 = this.Sweep.C = MathUtils.Mul(this.Transform, this.Sweep.LocalCenter);

        // Update center of mass velocity.
        this.LinearVelocity += MathUtils.Cross(this.AngularVelocity, this.Sweep.C - oldCenter);
    }

    /// Get the world coordinates of a point given the local coordinates.
    /// @param localPoint a point on the body measured relative the the body's origin.
    /// @return the same point expressed in world coordinates.
    public Vector2 GetWorldPoint(in Vector2 localPoint)
    {
        return MathUtils.Mul(this.Transform, localPoint);
    }

    /// Get the world coordinates of a vector given the local coordinates.
    /// @param localVector a vector fixed in the body.
    /// @return the same vector expressed in world coordinates.
    public Vector2 GetWorldVector(in Vector2 localVector)
    {
        return MathUtils.Mul(this.Transform.Rotation, localVector);
    }

    /// Gets a local point relative to the body's origin given a world point.
    /// @param a point in world coordinates.
    /// @return the corresponding local point relative to the body's origin.
    public Vector2 GetLocalPoint(in Vector2 worldPoint)
    {
        return MathUtils.MulT(this.Transform, worldPoint);
    }

    /// Gets a local vector given a world vector.
    /// @param a vector in world coordinates.
    /// @return the corresponding local vector.
    public Vector2 GetLocalVector(in Vector2 worldVector)
    {
        return MathUtils.MulT(this.Transform.Rotation, worldVector);
    }

    /// Get the world linear velocity of a world point attached to this body.
    /// @param a point in world coordinates.
    /// @return the world velocity of a point.
    public Vector2 GetLinearVelocityFromWorldPoint(in Vector2 worldPoint)
    {
        return this.LinearVelocity + MathUtils.Cross(this.AngularVelocity, worldPoint - this.Sweep.C);
    }

    /// Get the world velocity of a local point.
    /// @param a point in local coordinates.
    /// @return the world velocity of a point.
    public Vector2 GetLinearVelocityFromLocalPoint(in Vector2 localPoint)
    {
        return this.GetLinearVelocityFromWorldPoint(this.GetWorldPoint(localPoint));
    }

    /// Dump this body to a log file
    public void Dump()
    {
        // Todo
    }

    /// <summary>
    /// 同步夹具
    /// </summary>
    internal void SynchronizeFixtures()
    {
        BroadPhase? broadPhase = this.World.ContactManager.BroadPhase;

        if (this.Flags.IsSet(BodyFlags.IsAwake))
        {
            Transform xf1 = new Transform();
            xf1.Rotation.Set(this.Sweep.A0);
            xf1.Position = this.Sweep.C0 - MathUtils.Mul(xf1.Rotation, this.Sweep.LocalCenter);

            for (int index = 0; index < this.Fixtures.Count; index++)
            {
                this.Fixtures[index].Synchronize(broadPhase, xf1, this.Transform);
            }
        }
        else
        {
            for (int index = 0; index < this.Fixtures.Count; index++)
            {
                this.Fixtures[index].Synchronize(broadPhase, this.Transform, this.Transform);
            }
        }

        // var xf1 = new Transform();
        // xf1.Rotation.Set(Sweep.A0);
        // xf1.Position = Sweep.C0 - MathUtils.Mul(xf1.Rotation, Sweep.LocalCenter);
        //
        // var broadPhase = _world.ContactManager.BroadPhase;
        // for (var index = 0; index < Fixtures.Count; index++)
        // {
        //     Fixtures[index].Synchronize(broadPhase, xf1, Transform);
        // }
    }

    /// <summary>
    /// 同步位置
    /// </summary>
    internal void SynchronizeTransform()
    {
        this.Transform.Rotation.Set(this.Sweep.A);
        this.Transform.Position = this.Sweep.C - MathUtils.Mul(this.Transform.Rotation, this.Sweep.LocalCenter);
    }

    /// <summary>
    /// This is used to prevent connected bodies from colliding.
    /// It may lie, depending on the collideConnected flag.
    /// 判断物体之间是否应该检测碰撞
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    internal bool ShouldCollide(Body other)
    {
        // At least one body should be dynamic.
        if (this._type != BodyType.DynamicBody && other._type != BodyType.DynamicBody)
        {
            return false;
        }

        // Does a joint prevent collision?
        LinkedListNode<JointEdge>? node = this.JointEdges.First;
        while (node != null)
        {
            JointEdge joint = node.Value;
            node = node.Next;
            if (joint.Other == other && joint.Joint.CollideConnected == false)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 在安全时间段内快进,此时不同步粗检测
    /// </summary>
    /// <param name="alpha"></param>
    internal void Advance(float alpha)
    {
        // Advance to the new safe time. This doesn't sync the broad-phase.
        this.Sweep.Advance(alpha);
        this.Sweep.C = this.Sweep.C0;
        this.Sweep.A = this.Sweep.A0;
        this.Transform.Rotation.Set(this.Sweep.A);
        this.Transform.Position = this.Sweep.C - MathUtils.Mul(this.Transform.Rotation, this.Sweep.LocalCenter);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetFlag(BodyFlags flag)
    {
        this.Flags |= flag;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UnsetFlag(BodyFlags flag)
    {
        this.Flags &= ~flag;
    }
}