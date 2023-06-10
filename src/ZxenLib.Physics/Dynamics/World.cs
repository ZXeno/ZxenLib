namespace ZxenLib.Physics.Dynamics;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using System.Threading;
using Collision;
using Collision.Collider;
using Collision.Shapes;
using Common;
using Contacts;
using Joints;
using Color = Common.Color;

public class World : IDisposable
{
    /// <summary>
    /// This is used to compute the time step ratio to
    /// support a variable time step.
    /// 时间步倍率
    /// </summary>
    private float _invDt0;

    /// <summary>
    /// 时间步完成
    /// </summary>
    private bool _stepComplete;

    /// <summary>
    /// 存在新接触点
    /// </summary>
    public bool HasNewContacts;

    /// <summary>
    /// Register a destruction listener. The listener is owned by you and must
    /// remain in scope.
    /// 析构监听器
    /// </summary>
    public IDestructionListener DestructionListener { get; set; }

    /// <summary>
    /// Debug Drawer
    /// 调试绘制
    /// </summary>
    public IDrawer Drawer { get; set; }

    /// <summary>
    /// 是否启用连续碰撞
    /// </summary>
    public bool ContinuousPhysics { get; set; }

    /// <summary>
    /// 重力常数
    /// </summary>
    public Vector2 Gravity { get; set; }

    /// <summary>
    /// 清除受力
    /// </summary>
    public bool IsAutoClearForces { get; set; }

    /// <summary>
    /// 锁定世界
    /// </summary>
    public bool IsLocked { get; private set; }

    /// <summary>
    /// 世界是否允许休眠
    /// </summary>
    public bool AllowSleep
    {
        get => this._allowSleep;
        set
        {
            if (this._allowSleep == value)
            {
                return;
            }

            this._allowSleep = value;
            if (this._allowSleep)
            {
                return;
            }

            LinkedListNode<Body>? node = this.BodyList.First;
            while (node != null)
            {
                node.Value.IsAwake = true;
                node = node.Next;
            }
        }
    }

    private bool _allowSleep;

    /// <summary>
    /// Enable/disable single stepped continuous physics. For testing.
    /// 子步进
    /// </summary>
    public bool SubStepping { get; set; }

    /// <summary>
    /// These are for debugging the solver.
    /// Enable/disable warm starting. For testing.
    /// 热启动,用于调试求解器
    /// </summary>
    public bool WarmStarting { get; set; }

    /// <summary>
    /// 性能统计
    /// </summary>
    public Profile Profile;

    public ToiProfile ToiProfile { get; set; } = null;

    public GJkProfile GJkProfile { get; set; } = null;

    /// <summary>
    /// 接触点管理器
    /// </summary>
    public ContactManager ContactManager { get; private set; } = new ContactManager();

    /// <summary>
    /// 物体链表
    /// </summary>
    public LinkedList<Body> BodyList { get; private set; } = new LinkedList<Body>();

    /// <summary>
    /// 关节链表
    /// </summary>
    public LinkedList<Joint> JointList { get; private set; } = new LinkedList<Joint>();

    /// Get the number of broad-phase proxies.
    public int ProxyCount => this.ContactManager.BroadPhase.GetProxyCount();

    /// Get the number of bodies.
    public int BodyCount => this.BodyList.Count;

    /// Get the number of joints.
    public int JointCount => this.JointList.Count;

    /// Get the number of contacts (each may have 0 or more contact points).
    public int ContactCount => this.ContactManager.ContactList.Count;

    /// Get the height of the dynamic tree.
    public int TreeHeight => this.ContactManager.BroadPhase.GetTreeHeight();

    /// Get the balance of the dynamic tree.
    public int TreeBalance => this.ContactManager.BroadPhase.GetTreeBalance();

    /// Get the quality metric of the dynamic tree. The smaller the better.
    /// The minimum is 1.
    public float TreeQuality => this.ContactManager.BroadPhase.GetTreeQuality();

    public World()
        : this(new Vector2(0, -10))
    { }

    public World(in Vector2 gravity)
    {
        this.Gravity = gravity;

        this.WarmStarting = true;
        this.ContinuousPhysics = true;
        this.SubStepping = false;
        this._stepComplete = true;

        this.AllowSleep = true;
        this.IsAutoClearForces = true;
        this._invDt0 = 0.0f;
        this.Profile = default;
    }

    ~World()
    {
        this.Dispose();
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

        this.BodyList?.Clear();
        this.BodyList = null;
        this.JointList?.Clear();
        this.JointList = null;
        this.ContactManager?.Dispose();
        this.ContactManager = null;

        this.DestructionListener = null;
        this.Drawer = null;

        this.Profile = default;
        this.ToiProfile = null;
        this.GJkProfile = null;
    }

    /// <summary>
    /// Register a contact filter to provide specific control over collision.
    /// Otherwise the default filter is used (b2_defaultFilter). The listener is
    /// owned by you and must remain in scope.
    /// 注册碰撞过滤器,用于在碰撞过程中执行自定义过滤
    /// </summary>
    /// <param name="filter"></param>
    public void SetContactFilter(IContactFilter filter)
    {
        this.ContactManager.ContactFilter = filter;
    }

    /// <summary>
    /// Register a contact event listener. The listener is owned by you and must
    /// remain in scope.
    /// 注册接触监听器
    /// </summary>
    /// <param name="listener"></param>
    public void SetContactListener(IContactListener listener)
    {
        this.ContactManager.ContactListener = listener;
    }

    /// <summary>
    /// Create a rigid body given a definition. No reference to the definition
    /// is retained.
    /// @warning This function is locked during callbacks.
    /// 创建一个物体(刚体)
    /// </summary>
    /// <param name="def"></param>
    /// <returns></returns>
    public Body CreateBody(in BodyDef def)
    {
        Debug.Assert(this.IsLocked == false);
        if (this.IsLocked) // 世界锁定时无法创建物体
        {
            return null;
        }

        // 创建物体并关联到本世界
        Body? body = new Body(def, this);

        // Add to world doubly linked list.
        // 添加物体到物体链表头部
        body.Node = this.BodyList.AddFirst(body);
        return body;
    }

    /// <summary>
    /// Destroy a rigid body given a definition. No reference to the definition
    /// is retained. This function is locked during callbacks.
    /// @warning This automatically deletes all associated shapes and joints.
    /// @warning This function is locked during callbacks.
    /// 删除一个物体(刚体)
    /// </summary>
    /// <param name="body"></param>
    /// <returns></returns>
    public bool DestroyBody(Body body)
    {
        Debug.Assert(this.BodyList.Count > 0);
        Debug.Assert(this.IsLocked == false);
        if (this.IsLocked)
        {
            return false;
        }

        // Delete the attached joints.
        // 删除所有挂载的关节
        LinkedListNode<JointEdge>? jointEdgePointer = body.JointEdges.First;
        while (jointEdgePointer != default)
        {
            LinkedListNode<JointEdge>? next = jointEdgePointer.Next;
            this.DestructionListener?.SayGoodbye(jointEdgePointer.Value.Joint);
            this.DestroyJoint(jointEdgePointer.Value.Joint);
            jointEdgePointer = next;
        }

        // Delete the attached contacts.
        // 删除所有挂载的接触点
        LinkedListNode<ContactEdge>? contactEdge = body.ContactEdges.First;
        while (contactEdge != default)
        {
            LinkedListNode<ContactEdge>? next = contactEdge.Next;
            this.ContactManager.Destroy(contactEdge.Value.Contact);
            contactEdge = next;
        }

        // Delete the attached fixtures. This destroys broad-phase proxies.
        // 删除所有挂载的夹具,同时会删除对应的粗检测代理
        foreach (Fixture? fixture in body.Fixtures)
        {
            this.DestructionListener?.SayGoodbye(fixture);
            fixture.DestroyProxies(this.ContactManager.BroadPhase);
        }

        // Remove world body list.
        this.BodyList.Remove(body.Node);
        body.Dispose();
        return true;
    }

    /// <summary>
    /// Create a joint to constrain bodies together. No reference to the definition
    /// is retained. This may cause the connected bodies to cease colliding.
    /// @warning This function is locked during callbacks.
    /// 创建关节,用于把两个物体连接在一起,在回调中不可调用
    /// </summary>
    /// <param name="def"></param>
    /// <returns></returns>
    public Joint CreateJoint(JointDef def)
    {
        Debug.Assert(this.IsLocked == false);
        if (this.IsLocked)
        {
            return null;
        }

        Joint? j = Joint.Create(def);

        // Connect to the world list.
        // 添加到关节列表头部
        j.Node = this.JointList.AddFirst(j);

        // Connect to the bodies' doubly linked lists.
        // 连接到物体的双向链表中
        j.EdgeA.Joint = j;
        j.EdgeA.Other = j.BodyB;
        j.EdgeA.Node = j.BodyA.JointEdges.AddFirst(j.EdgeA);

        j.EdgeB.Joint = j;
        j.EdgeB.Other = j.BodyA;
        j.EdgeB.Node = j.BodyB.JointEdges.AddFirst(j.EdgeB);

        Body? bodyA = def.BodyA;
        Body? bodyB = def.BodyB;

        // If the joint prevents collisions, then flag any contacts for filtering.
        if (def.CollideConnected == false)
        {
            LinkedListNode<ContactEdge>? node = bodyB.ContactEdges.First;
            while (node != null)
            {
                ContactEdge? contactEdge = node.Value;
                node = node.Next;
                if (contactEdge.Other == bodyA)
                {
                    // Flag the contact for filtering at the next time step (where either
                    // body is awake).
                    contactEdge.Contact.FlagForFiltering();
                }
            }
        }

        // Note: creating a joint doesn't wake the bodies.

        return j;
    }

    /// Destroy a joint. This may cause the connected bodies to begin colliding.
    /// @warning This function is locked during callbacks.
    public void DestroyJoint(Joint joint)
    {
        Debug.Assert(this.IsLocked == false);
        Debug.Assert(this.JointList.Count > 0);
        if (this.IsLocked)
        {
            return;
        }

        bool collideConnected = joint.CollideConnected;

        // Remove from the doubly linked list.
        this.JointList.Remove(joint.Node);

        // Disconnect from island graph.
        // Wake up connected bodies.
        Body? bodyA = joint.BodyA;
        bodyA.IsAwake = true;
        Debug.Assert(bodyA.JointEdges.Count > 0);
        bodyA.JointEdges.Remove(joint.EdgeA.Node);
        joint.EdgeA.Dispose();

        Body? bodyB = joint.BodyB;
        bodyB.IsAwake = true;
        Debug.Assert(bodyB.JointEdges.Count > 0);
        bodyB.JointEdges.Remove(joint.EdgeB.Node);
        joint.EdgeB.Dispose();

        // If the joint prevents collisions, then flag any contacts for filtering.
        if (collideConnected == false)
        {
            LinkedListNode<ContactEdge>? node = bodyB.ContactEdges.First;
            while (node != null)
            {
                ContactEdge? contactEdge = node.Value;
                node = node.Next;
                if (contactEdge.Other == bodyA)
                {
                    // Flag the contact for filtering at the next time step (where either
                    // body is awake).
                    contactEdge.Contact.FlagForFiltering();
                }
            }
        }
    }

    private readonly Stopwatch _stepTimer = new Stopwatch();

    private readonly Stopwatch _timer = new Stopwatch();

    /// <summary>
    /// Take a time step. This performs collision detection, integration, and constraint solution.
    /// </summary>
    /// <param name="timeStep">the amount of time to simulate, this should not vary.</param>
    /// <param name="velocityIterations">for the velocity constraint solver.</param>
    /// <param name="positionIterations">for the position constraint solver.</param>
    public void Step(float timeStep, int velocityIterations, int positionIterations)
    {
        // profile 计时
        this._stepTimer.Restart();

        // If new fixtures were added, we need to find the new contacts.
        // 如果存在新增夹具,则需要找到新接触点
        if (this.HasNewContacts)
        {
            // 寻找新接触点
            this.ContactManager.FindNewContacts();

            // 去除新增夹具标志
            this.HasNewContacts = false;
        }

        // 锁定世界
        this.IsLocked = true;

        // 时间间隔与迭代次数
        TimeStep step = new TimeStep
        {
            Dt = timeStep, VelocityIterations = velocityIterations,
            PositionIterations = positionIterations
        };

        // 计算时间间隔倒数
        if (timeStep > 0.0f)
        {
            step.InvDt = 1.0f / timeStep;
        }
        else
        {
            step.InvDt = 0.0f;
        }

        step.DtRatio = this._invDt0 * timeStep;

        step.WarmStarting = this.WarmStarting;
        this._timer.Restart();

        // Update contacts. This is where some contacts are destroyed.
        // 更新接触点
        {
            this.ContactManager.Collide();
            this._timer.Stop();
            this.Profile.Collide = this._timer.ElapsedMilliseconds;
        }

        // Integrate velocities, solve velocity constraints, and integrate positions.
        // 对速度进行积分，求解速度约束，整合位置
        if (this._stepComplete && step.Dt > 0.0f)
        {
            this._timer.Restart();
            this.Solve(step);
            this._timer.Stop();
            this.Profile.Solve = this._timer.ElapsedMilliseconds;
        }

        // Handle TOI events.
        // 处理碰撞时间
        if (this.ContinuousPhysics && step.Dt > 0.0f)
        {
            this._timer.Restart();
            this.SolveTOI(step);
            this._timer.Stop();
            this.Profile.SolveTOI = this._timer.ElapsedMilliseconds;
        }

        if (step.Dt > 0.0f)
        {
            this._invDt0 = step.InvDt;
        }

        // 启用受力清理
        if (this.IsAutoClearForces)
        {
            this.ClearForces();
        }

        // 时间步完成,解锁世界
        this.IsLocked = false;
        this._stepTimer.Stop();
        this.Profile.Step = this._stepTimer.ElapsedMilliseconds;
    }

    /// Manually clear the force buffer on all bodies. By default, forces are cleared automatically
    /// after each call to Step. The default behavior is modified by calling SetAutoClearForces.
    /// The purpose of this function is to support sub-stepping. Sub-stepping is often used to maintain
    /// a fixed sized time step under a variable frame-rate.
    /// When you perform sub-stepping you will disable auto clearing of forces and instead call
    /// ClearForces after all sub-steps are complete in one pass of your game loop.
    /// @see SetAutoClearForces
    public void ClearForces()
    {
        LinkedListNode<Body>? node = this.BodyList.First;
        while (node != null)
        {
            Body? body = node.Value;
            node = node.Next;
            body.Force.SetZero();
            body.Torque = 0.0f;
        }
    }

    private class TreeQueryCallback : ITreeQueryCallback
    {
        public ContactManager ContactManager { get; private set; }

        public IQueryCallback Callback { get; private set; }

        public void Set(ContactManager contactManager, in IQueryCallback callback)
        {
            this.ContactManager = contactManager;
            this.Callback = callback;
        }

        public void Reset()
        {
            this.ContactManager = default;
            this.Callback = default;
        }

        /// <inheritdoc />
        public bool QueryCallback(int proxyId)
        {
            FixtureProxy? proxy = (FixtureProxy)this.ContactManager
                .BroadPhase.GetUserData(proxyId);
            return this.Callback.QueryCallback(proxy.Fixture);
        }
    }

    private readonly TreeQueryCallback _treeQueryCallback = new TreeQueryCallback();

    /// Query the world for all fixtures that potentially overlap the
    /// provided AABB.
    /// @param callback a user implemented callback class.
    /// @param aabb the query box.
    public void QueryAABB(in IQueryCallback callback, in AABB aabb)
    {
        this._treeQueryCallback.Set(this.ContactManager, in callback);
        this.ContactManager.BroadPhase.Query(this._treeQueryCallback, aabb);
    }

    private class InternalRayCastCallback : ITreeRayCastCallback
    {
        public ContactManager ContactManager { get; private set; }

        public IRayCastCallback Callback { get; private set; }

        public void Set(ContactManager contactManager, in IRayCastCallback callback)
        {
            this.ContactManager = contactManager;
            this.Callback = callback;
        }

        public void Reset()
        {
            this.ContactManager = default;
            this.Callback = default;
        }

        public float RayCastCallback(in RayCastInput input, int proxyId)
        {
            object? userData = this.ContactManager.BroadPhase.GetUserData(proxyId);
            FixtureProxy? proxy = (FixtureProxy)userData;
            Fixture? fixture = proxy.Fixture;
            int index = proxy.ChildIndex;

            bool hit = fixture.RayCast(out RayCastOutput output, input, index);

            if (!hit)
            {
                return input.MaxFraction;
            }

            float fraction = output.Fraction;
            Vector2 point = (1.0f - fraction) * input.P1 + fraction * input.P2;
            return this.Callback.RayCastCallback(fixture, point, output.Normal, fraction);
        }
    }

    private readonly InternalRayCastCallback _rayCastCallback = new InternalRayCastCallback();

    /// Ray-cast the world for all fixtures in the path of the ray. Your callback
    /// controls whether you get the closest point, any point, or n-points.
    /// The ray-cast ignores shapes that contain the starting point.
    /// @param callback a user implemented callback class.
    /// @param point1 the ray starting point
    /// @param point2 the ray ending point
    public void RayCast(in IRayCastCallback callback, in Vector2 point1, in Vector2 point2)
    {
        RayCastInput input = new RayCastInput
        {
            MaxFraction = 1.0f, P1 = point1,
            P2 = point2
        };
        this._rayCastCallback.Set(this.ContactManager, in callback);
        this.ContactManager.BroadPhase.RayCast(this._rayCastCallback, input);
        this._rayCastCallback.Reset();
    }

    /// Shift the world origin. Useful for large worlds.
    /// The body shift formula is: position -= newOrigin
    /// @param newOrigin the new origin with respect to the old origin
    public void ShiftOrigin(in Vector2 newOrigin)
    {
        Debug.Assert(!this.IsLocked);
        if (this.IsLocked)
        {
            return;
        }

        LinkedListNode<Body>? bodyNode = this.BodyList.First;
        while (bodyNode != null)
        {
            Body? b = bodyNode.Value;
            bodyNode = bodyNode.Next;
            b.Transform.Position -= newOrigin;
            b.Sweep.C0 -= newOrigin;
            b.Sweep.C -= newOrigin;
        }

        LinkedListNode<Joint>? jointNode = this.JointList.First;
        while (jointNode != null)
        {
            jointNode.Value.ShiftOrigin(newOrigin);
            jointNode = jointNode.Next;
        }

        this.ContactManager.BroadPhase.ShiftOrigin(newOrigin);
    }

    private readonly Stack<Body> _solveStack = new Stack<Body>(256);

    private readonly Stopwatch _solveTimer = new Stopwatch();

    private readonly Island _solveIsland = new Island();

    /// <summary>
    /// Find islands, integrate and solve constraints, solve position constraints
    /// 找出岛屿,迭代求解约束,求解位置约束(岛屿用来对物理空间进行物体分组求解,提高效率)
    /// </summary>
    /// <param name="step"></param>
    private void Solve(in TimeStep step)
    {
        this.Profile.SolveInit = 0.0f;
        this.Profile.SolveVelocity = 0.0f;
        this.Profile.SolvePosition = 0.0f;

        // Size the island for the worst case.
        // 最坏情况岛屿容量,即全世界在同一个岛屿
        Island? island = this._solveIsland;
        island.Setup(
            this.BodyList.Count,
            this.ContactManager.ContactList.Count,
            this.JointList.Count,
            this.ContactManager.ContactListener);

        // Clear all the island flags.
        // 清除所有岛屿标志
        LinkedListNode<Body>? bodyNode = this.BodyList.First;
        while (bodyNode != null)
        {
            bodyNode.Value.UnsetFlag(BodyFlags.Island);
            bodyNode = bodyNode.Next;
        }

        LinkedListNode<Contact>? contactNode = this.ContactManager.ContactList.First;
        while (contactNode != null)
        {
            contactNode.Value.Flags &= ~Contact.ContactFlag.IslandFlag;
            contactNode = contactNode.Next;
        }

        LinkedListNode<Joint>? jointNode = this.JointList.First;
        while (jointNode != null)
        {
            jointNode.Value.IslandFlag = false;
            jointNode = jointNode.Next;
        }

        // Build and simulate all awake islands.
        Stack<Body>? stack = this._solveStack;
        stack.Clear();

        bodyNode = this.BodyList.First;
        while (bodyNode != null)
        {
            Body? body = bodyNode.Value;
            bodyNode = bodyNode.Next;
            if (body.Flags.IsSet(BodyFlags.Island)) // 已经分配到岛屿则跳过
            {
                continue;
            }

            if (body.IsAwake == false || body.IsEnabled == false) // 跳过休眠物体
            {
                continue;
            }

            // The seed can be dynamic or kinematic.
            if (body.BodyType == BodyType.StaticBody) // 跳过静态物体
            {
                continue;
            }

            // Reset island and stack.
            island.Clear();

            //var stackCount = 0;
            stack.Push(body);

            //stackCount++;
            body.SetFlag(BodyFlags.Island);

            // Perform a depth first search (DFS) on the constraint graph.
            while (stack.Count > 0)
            {
                // Grab the next body off the stack and add it to the island.
                //--stackCount;
                Body? b = stack.Pop();
                Debug.Assert(b.IsEnabled);
                island.Add(b);

                // To keep islands as small as possible, we don't
                // propagate islands across static bodies.
                if (b.BodyType == BodyType.StaticBody)
                {
                    continue;
                }

                // Make sure the body is awake (without resetting sleep timer).
                b.SetFlag(BodyFlags.IsAwake);

                // Search all contacts connected to this body.
                // 查找该物体所有接触点
                LinkedListNode<ContactEdge>? node = b.ContactEdges.First;
                while (node != null)
                {
                    ContactEdge? contactEdge = node.Value;
                    node = node.Next;

                    Contact? contact = contactEdge.Contact;

                    // Has this contact already been added to an island?
                    // 接触点已经标记岛屿,跳过
                    if (contact.Flags.IsSet(Contact.ContactFlag.IslandFlag))
                    {
                        continue;
                    }

                    // Is this contact solid and touching?
                    // 接触点未启用或未接触,跳过
                    if (contact.IsEnabled == false || contact.IsTouching == false)
                    {
                        continue;
                    }

                    // Skip sensors.
                    // 跳过传感器
                    if (contact.FixtureA.IsSensor || contact.FixtureB.IsSensor)
                    {
                        continue;
                    }

                    // 将该接触点添加到岛屿中,并添加岛屿标志
                    island.Add(contact);
                    contact.Flags |= Contact.ContactFlag.IslandFlag;

                    Body? other = contactEdge.Other;

                    // Was the other body already added to this island?
                    // 如果接触边缘的另一个物体已经添加到岛屿则跳过
                    if (other.Flags.IsSet(BodyFlags.Island))
                    {
                        continue;
                    }

                    // 否则将另一边的物体也添加到岛屿
                    //Debug.Assert(stackCount < stackSize);
                    stack.Push(other);
                    other.SetFlag(BodyFlags.Island);
                }

                // Search all joints connect to this body.
                // 将该物体的关节所关联的物体也加入到岛屿中
                LinkedListNode<JointEdge>? jointEdgeNode = b.JointEdges.First;
                while (jointEdgeNode != null)
                {
                    JointEdge je = jointEdgeNode.Value;
                    jointEdgeNode = jointEdgeNode.Next;
                    if (je.Joint.IslandFlag)
                    {
                        continue;
                    }

                    Body? other = je.Other;

                    // Don't simulate joints connected to inactive bodies.
                    // 跳过闲置物体
                    if (other.IsEnabled == false)
                    {
                        continue;
                    }

                    island.Add(je.Joint);
                    je.Joint.IslandFlag = true;

                    if (other.Flags.IsSet(BodyFlags.Island))
                    {
                        continue;
                    }

                    //Debug.Assert(stackCount < stackSize);
                    stack.Push(other);
                    other.SetFlag(BodyFlags.Island);
                }
            }

            // 岛屿碰撞求解
            island.Solve(out Profile profile, step, this.Gravity, this.AllowSleep);
            this.Profile.SolveInit += profile.SolveInit;
            this.Profile.SolveVelocity += profile.SolveVelocity;
            this.Profile.SolvePosition += profile.SolvePosition;

            // Post solve cleanup.
            for (int i = 0; i < island.BodyCount; ++i)
            {
                // Allow static bodies to participate in other islands.
                Body? b = island.Bodies[i];
                if (b.BodyType == BodyType.StaticBody)
                {
                    b.UnsetFlag(BodyFlags.Island);
                }
            }
        }

        {
            this._solveTimer.Restart();

            // Synchronize fixtures, check for out of range bodies.
            bodyNode = this.BodyList.First;
            while (bodyNode != null)
            {
                Body? b = bodyNode.Value;
                bodyNode = bodyNode.Next;

                // If a body was not in an island then it did not move.
                if (!b.Flags.IsSet(BodyFlags.Island))
                {
                    continue;
                }

                if (b.BodyType == BodyType.StaticBody)
                {
                    continue;
                }

                // Update fixtures (for broad-phase).
                b.SynchronizeFixtures();
            }

            // Look for new contacts.
            this.ContactManager.FindNewContacts();
            this._solveTimer.Stop();
            this.Profile.Broadphase = this._solveTimer.ElapsedMilliseconds;
        }
        island.Reset();
    }

    private readonly Island _solveToiIsland = new Island();

    /// <summary>
    /// Find TOI contacts and solve them.
    /// 求解碰撞时间
    /// </summary>
    /// <param name="step"></param>
    private void SolveTOI(in TimeStep step)
    {
        Island? island = this._solveToiIsland;
        island.Setup(
            2 * Settings.MaxToiContacts,
            Settings.MaxToiContacts,
            0,
            this.ContactManager.ContactListener);

        if (this._stepComplete)
        {
            LinkedListNode<Body>? bodyNode = this.BodyList.First;
            while (bodyNode != null)
            {
                Body? b = bodyNode.Value;
                bodyNode = bodyNode.Next;
                b.UnsetFlag(BodyFlags.Island);
                b.Sweep.Alpha0 = 0.0f;
            }

            for (LinkedListNode<Contact>? node = this.ContactManager.ContactList.First; node != null; node = node.Next)
            {
                Contact? c = node.Value;

                // Invalidate TOI
                c.Flags &= ~(Contact.ContactFlag.ToiFlag | Contact.ContactFlag.IslandFlag);
                c.ToiCount = 0;
                c.Toi = 1.0f;
            }
        }

        // Find TOI events and solve them.
        for (;;)
        {
            // Find the first TOI.
            Contact minContact = null;
            float minAlpha = 1.0f;

            LinkedListNode<Contact>? contactNode = this.ContactManager.ContactList.First;
            while (contactNode != null)
            {
                Contact? c = contactNode.Value;
                contactNode = contactNode.Next;

                // Is this contact disabled?
                if (c.IsEnabled == false)
                {
                    continue;
                }

                // Prevent excessive sub-stepping.
                if (c.ToiCount > Settings.MaxSubSteps)
                {
                    continue;
                }

                float alpha = 1.0f;
                if (c.Flags.IsSet(Contact.ContactFlag.ToiFlag))
                {
                    // This contact has a valid cached TOI.
                    alpha = c.Toi;
                }
                else
                {
                    Fixture? fA = c.FixtureA;
                    Fixture? fB = c.FixtureB;

                    // Is there a sensor?
                    // 如果接触点的夹具是传感器,不参与TOI计算,跳过
                    if (fA.IsSensor || fB.IsSensor)
                    {
                        continue;
                    }

                    Body? bA = fA.Body;
                    Body? bB = fB.Body;

                    BodyType typeA = bA.BodyType;
                    BodyType typeB = bB.BodyType;
                    Debug.Assert(typeA == BodyType.DynamicBody || typeB == BodyType.DynamicBody);

                    bool activeA = bA.IsAwake && typeA != BodyType.StaticBody;
                    bool activeB = bB.IsAwake && typeB != BodyType.StaticBody;

                    // Is at least one body active (awake and dynamic or kinematic)?
                    if (activeA == false && activeB == false)
                    {
                        continue;
                    }

                    bool collideA = bA.IsBullet || typeA != BodyType.DynamicBody;
                    bool collideB = bB.IsBullet || typeB != BodyType.DynamicBody;

                    // Are these two non-bullet dynamic bodies?
                    if (collideA == false && collideB == false)
                    {
                        continue;
                    }

                    // Compute the TOI for this contact.
                    // Put the sweeps onto the same time interval.
                    float alpha0 = bA.Sweep.Alpha0;

                    if (bA.Sweep.Alpha0 < bB.Sweep.Alpha0)
                    {
                        alpha0 = bB.Sweep.Alpha0;
                        bA.Sweep.Advance(alpha0);
                    }
                    else if (bB.Sweep.Alpha0 < bA.Sweep.Alpha0)
                    {
                        alpha0 = bA.Sweep.Alpha0;
                        bB.Sweep.Advance(alpha0);
                    }

                    Debug.Assert(alpha0 < 1.0f);

                    int indexA = c.ChildIndexA;
                    int indexB = c.ChildIndexB;

                    // Compute the time of impact in interval [0, minTOI]
                    ToiInput input = new ToiInput();
                    input.ProxyA.Set(fA.Shape, indexA);
                    input.ProxyB.Set(fB.Shape, indexB);
                    input.SweepA = bA.Sweep;
                    input.SweepB = bB.Sweep;
                    input.Tmax = 1.0f;

                    TimeOfImpact.ComputeTimeOfImpact(out ToiOutput output, input, this.ToiProfile, this.GJkProfile);

                    // Beta is the fraction of the remaining portion of the .
                    float beta = output.Time;
                    alpha = output.State == ToiOutput.ToiState.Touching ? Math.Min(alpha0 + (1.0f - alpha0) * beta, 1.0f) : 1.0f;

                    c.Toi = alpha;
                    c.Flags |= Contact.ContactFlag.ToiFlag;
                }

                if (alpha < minAlpha)
                {
                    // This is the minimum TOI found so far.
                    minContact = c;
                    minAlpha = alpha;
                }
            }

            if (minContact == default || 1.0f - 10.0f * Settings.Epsilon < minAlpha)
            {
                // No more TOI events. Done!
                this._stepComplete = true;
                break;
            }

            // Advance the bodies to the TOI.
            Fixture? fixtureA = minContact.FixtureA;
            Fixture? fixtureB = minContact.FixtureB;
            Body? bodyA = fixtureA.Body;
            Body? bodyB = fixtureB.Body;

            Sweep backup1 = bodyA.Sweep;
            Sweep backup2 = bodyB.Sweep;

            bodyA.Advance(minAlpha);
            bodyB.Advance(minAlpha);

            // The TOI contact likely has some new contact points.
            minContact.Update(this.ContactManager.ContactListener);
            minContact.Flags &= ~Contact.ContactFlag.ToiFlag;
            ++minContact.ToiCount;

            // Is the contact solid?
            if (minContact.IsEnabled == false || minContact.IsTouching == false)
            {
                // Restore the sweeps.
                minContact.SetEnabled(false);
                bodyA.Sweep = backup1;
                bodyB.Sweep = backup2;
                bodyA.SynchronizeTransform();
                bodyB.SynchronizeTransform();
                continue;
            }

            bodyA.IsAwake = true;
            bodyB.IsAwake = true;

            // Build the island
            island.Clear();
            island.Add(bodyA);
            island.Add(bodyB);
            island.Add(minContact);

            bodyA.SetFlag(BodyFlags.Island);
            bodyB.SetFlag(BodyFlags.Island);
            minContact.Flags |= Contact.ContactFlag.IslandFlag;

            // Get contacts on bodyA and bodyB.
            {
                Body? body = bodyA;
                if (body.BodyType == BodyType.DynamicBody)
                {
                    LinkedListNode<ContactEdge>? node = body.ContactEdges.First;
                    while (node != null)
                    {
                        ContactEdge? contactEdge = node.Value;
                        node = node.Next;

                        if (island.BodyCount == island.Bodies.Length)
                        {
                            break;
                        }

                        if (island.ContactCount == island.Contacts.Length)
                        {
                            break;
                        }

                        Contact? contact = contactEdge.Contact;

                        // Has this contact already been added to the island?
                        if (contact.Flags.IsSet(Contact.ContactFlag.IslandFlag))
                        {
                            continue;
                        }

                        // Only add static, kinematic, or bullet bodies.
                        Body? other = contactEdge.Other;
                        if (other.BodyType == BodyType.DynamicBody
                            && body.IsBullet == false
                            && other.IsBullet == false)
                        {
                            continue;
                        }

                        // Skip sensors.
                        bool sensorA = contact.FixtureA.IsSensor;
                        bool sensorB = contact.FixtureB.IsSensor;
                        if (sensorA || sensorB)
                        {
                            continue;
                        }

                        // Tentatively advance the body to the TOI.
                        Sweep backup = other.Sweep;
                        if (!other.Flags.IsSet(BodyFlags.Island))
                        {
                            other.Advance(minAlpha);
                        }

                        // Update the contact points
                        contact.Update(this.ContactManager.ContactListener);

                        // Was the contact disabled by the user?
                        if (contact.IsEnabled == false)
                        {
                            other.Sweep = backup;
                            other.SynchronizeTransform();
                            continue;
                        }

                        // Are there contact points?
                        if (contact.IsTouching == false)
                        {
                            other.Sweep = backup;
                            other.SynchronizeTransform();
                            continue;
                        }

                        // Add the contact to the island
                        contact.Flags |= Contact.ContactFlag.IslandFlag;
                        island.Add(contact);

                        // Has the other body already been added to the island?
                        if (other.Flags.IsSet(BodyFlags.Island))
                        {
                            continue;
                        }

                        // Add the other body to the island.
                        other.SetFlag(BodyFlags.Island);

                        if (other.BodyType != BodyType.StaticBody)
                        {
                            other.IsAwake = true;
                        }

                        island.Add(other);
                    }
                }
            }
            {
                Body? body = bodyB;
                if (body.BodyType == BodyType.DynamicBody)
                {
                    LinkedListNode<ContactEdge>? node = body.ContactEdges.First;
                    while (node != null)
                    {
                        ContactEdge? contactEdge = node.Value;
                        node = node.Next;

                        if (island.BodyCount == island.Bodies.Length)
                        {
                            break;
                        }

                        if (island.ContactCount == island.Contacts.Length)
                        {
                            break;
                        }

                        Contact? contact = contactEdge.Contact;

                        // Has this contact already been added to the island?
                        if (contact.Flags.IsSet(Contact.ContactFlag.IslandFlag))
                        {
                            continue;
                        }

                        // Only add static, kinematic, or bullet bodies.
                        Body? other = contactEdge.Other;
                        if (other.BodyType == BodyType.DynamicBody
                            && body.IsBullet == false
                            && other.IsBullet == false)
                        {
                            continue;
                        }

                        // Skip sensors.
                        bool sensorA = contact.FixtureA.IsSensor;
                        bool sensorB = contact.FixtureB.IsSensor;
                        if (sensorA || sensorB)
                        {
                            continue;
                        }

                        // Tentatively advance the body to the TOI.
                        Sweep backup = other.Sweep;
                        if (!other.Flags.IsSet(BodyFlags.Island))
                        {
                            other.Advance(minAlpha);
                        }

                        // Update the contact points
                        contact.Update(this.ContactManager.ContactListener);

                        // Was the contact disabled by the user?
                        if (contact.IsEnabled == false)
                        {
                            other.Sweep = backup;
                            other.SynchronizeTransform();
                            continue;
                        }

                        // Are there contact points?
                        if (contact.IsTouching == false)
                        {
                            other.Sweep = backup;
                            other.SynchronizeTransform();
                            continue;
                        }

                        // Add the contact to the island
                        contact.Flags |= Contact.ContactFlag.IslandFlag;
                        island.Add(contact);

                        // Has the other body already been added to the island?
                        if (other.Flags.IsSet(BodyFlags.Island))
                        {
                            continue;
                        }

                        // Add the other body to the island.
                        other.SetFlag(BodyFlags.Island);

                        if (other.BodyType != BodyType.StaticBody)
                        {
                            other.IsAwake = true;
                        }

                        island.Add(other);
                    }
                }
            }

            float dt = (1.0f - minAlpha) * step.Dt;
            TimeStep subStep = new TimeStep
            {
                Dt = dt, InvDt = 1.0f / dt,
                DtRatio = 1.0f, PositionIterations = 20,
                VelocityIterations = step.VelocityIterations, WarmStarting = false
            };

            island.SolveTOI(subStep, bodyA.IslandIndex, bodyB.IslandIndex);

            // Reset island flags and synchronize broad-phase proxies.
            for (int i = 0; i < island.BodyCount; ++i)
            {
                Body? body = island.Bodies[i];
                body.UnsetFlag(BodyFlags.Island);

                if (body.BodyType != BodyType.DynamicBody)
                {
                    continue;
                }

                body.SynchronizeFixtures();

                // Invalidate all contact TOIs on this displaced body.
                LinkedListNode<ContactEdge>? node = bodyB.ContactEdges.First;
                while (node != null)
                {
                    node.Value.Contact.Flags &= ~(Contact.ContactFlag.ToiFlag | Contact.ContactFlag.IslandFlag);
                    node = node.Next;
                }
            }

            // Commit fixture proxy movements to the broad-phase so that new contacts are created.
            // Also, some contacts can be destroyed.
            this.ContactManager.FindNewContacts();

            if (this.SubStepping)
            {
                this._stepComplete = false;
                break;
            }
        }

        island.Reset();
    }

    /// Dump the world into the log file.
    /// @warning this should be called outside of a time step.
    public void Dump()
    {
        if (this.IsLocked)
        {
            return;
        }

        DumpLogger.Log($"gravity = ({this.Gravity.X}, {this.Gravity.Y});");
        DumpLogger.Log($"bodies  = {this.BodyList.Count};");
        DumpLogger.Log($"joints  = {this.JointList.Count};");
        int i = 0;
        foreach (Body? b in this.BodyList)
        {
            b.IslandIndex = i;
            b.Dump();
            ++i;
        }

        i = 0;
        foreach (Joint? j in this.JointList)
        {
            j.Index = i;
            ++i;
        }

        // First pass on joints, skip gear joints.
        foreach (Joint? j in this.JointList)
        {
            if (j.JointType == JointType.GearJoint)
            {
                continue;
            }

            DumpLogger.Log("{");
            j.Dump();
            DumpLogger.Log("}");
        }

        // Second pass on joints, only gear joints.
        foreach (Joint? j in this.JointList)
        {
            if (j.JointType != JointType.GearJoint)
            {
                continue;
            }

            DumpLogger.Log("{");
            j.Dump();
            DumpLogger.Log("}");
        }
    }

    #region Drawer

    /// <summary>
    /// Register a routine for debug drawing. The debug draw functions are called
    /// inside with <see cref="DebugDraw"/> method. The debug draw object is owned
    /// by you and must remain in scope.
    /// 调试绘制,用于绘制物体的图形
    /// </summary>
    /// <param name="drawer"></param>
    public void SetDebugDrawer(IDrawer drawer)
    {
        this.Drawer = drawer;
    }

    /// Call this to draw shapes and other debug draw data. This is intentionally non-const.
    /// 绘制调试数据
    public void DebugDraw()
    {
        if (this.Drawer == null)
        {
            return;
        }

        Color inactiveColor = Color.FromArgb(128, 128, 77);
        Color staticBodyColor = Color.FromArgb(127, 230, 127);
        Color kinematicBodyColor = Color.FromArgb(127, 127, 230);
        Color sleepColor = Color.FromArgb(153, 153, 153);
        Color lastColor = Color.FromArgb(230, 179, 179);
        DrawFlag flags = this.Drawer.Flags;

        if (flags.IsSet(DrawFlag.DrawShape))
        {
            for (LinkedListNode<Body>? node = this.BodyList.First; node != null; node = node.Next)
            {
                Body? b = node.Value;
                Transform xf = b.GetTransform();
                bool isEnabled = b.IsEnabled;
                bool isAwake = b.IsAwake;
                foreach (Fixture? f in b.Fixtures)
                {
                    if (b.BodyType == BodyType.DynamicBody && b.Mass.Equals(0))
                    {
                        // Bad body
                        this.DrawShape(f, xf, Color.FromArgb(1.0f, 0.0f, 0.0f));
                    }
                    else if (isEnabled == false)
                    {
                        this.DrawShape(f, xf, inactiveColor);
                    }
                    else if (b.BodyType == BodyType.StaticBody)
                    {
                        this.DrawShape(f, xf, staticBodyColor);
                    }
                    else if (b.BodyType == BodyType.KinematicBody)
                    {
                        this.DrawShape(f, xf, kinematicBodyColor);
                    }
                    else if (isAwake == false)
                    {
                        this.DrawShape(f, xf, sleepColor);
                    }
                    else
                    {
                        this.DrawShape(f, xf, lastColor);
                    }
                }
            }
        }

        if (flags.IsSet(DrawFlag.DrawJoint))
        {
            LinkedListNode<Joint>? node = this.JointList.First;
            while (node != null)
            {
                node.Value.Draw(this.Drawer);
                node = node.Next;
            }
        }

        if (flags.IsSet(DrawFlag.DrawPair))
        {
            Color color = Color.FromArgb(77, 230, 230);
            for (LinkedListNode<Contact>? node = this.ContactManager.ContactList.First; node != null; node = node.Next)
            {
                Contact? c = node.Value;
                Fixture? fixtureA = c.FixtureA;
                Fixture? fixtureB = c.FixtureB;

                Vector2 cA = fixtureA.GetAABB(c.ChildIndexA).GetCenter();
                Vector2 cB = fixtureB.GetAABB(c.ChildIndexB).GetCenter();

                this.Drawer.DrawSegment(cA, cB, color);
            }
        }

        if (flags.IsSet(DrawFlag.DrawAABB))
        {
            Color color = Color.FromArgb(230, 77, 230);
            BroadPhase? bp = this.ContactManager.BroadPhase;

            LinkedListNode<Body>? node = this.BodyList.First;
            Span<Vector2> vs = stackalloc Vector2[4];
            while (node != null)
            {
                Body? b = node.Value;
                node = node.Next;
                if (b.IsEnabled == false)
                {
                    continue;
                }

                foreach (Fixture? f in b.Fixtures)
                {
                    foreach (FixtureProxy? proxy in f.Proxies)
                    {
                        AABB aabb = bp.GetFatAABB(proxy.ProxyId);
                        vs[0] = new Vector2(aabb.LowerBound.X, aabb.LowerBound.Y);
                        vs[1] = new Vector2(aabb.UpperBound.X, aabb.LowerBound.Y);
                        vs[2] = new Vector2(aabb.UpperBound.X, aabb.UpperBound.Y);
                        vs[3] = new Vector2(aabb.LowerBound.X, aabb.UpperBound.Y);
                        this.Drawer.DrawPolygon(vs, 4, color);
                    }
                }
            }
        }

        if (flags.IsSet(DrawFlag.DrawCenterOfMass))
        {
            LinkedListNode<Body>? node = this.BodyList.First;
            while (node != null)
            {
                Body? b = node.Value;
                node = node.Next;
                Transform xf = b.GetTransform();
                xf.Position = b.GetWorldCenter();
                this.Drawer.DrawTransform(xf);
            }
        }
    }

    /// <summary>
    /// 绘制形状
    /// </summary>
    /// <param name="fixture"></param>
    /// <param name="xf"></param>
    /// <param name="color"></param>
    private void DrawShape(Fixture fixture, in Transform xf, in Color color)
    {
        switch (fixture.Shape)
        {
            case CircleShape circle:
            {
                Vector2 center = MathUtils.Mul(xf, circle.Position);
                float radius = circle.Radius;
                Vector2 axis = MathUtils.Mul(xf.Rotation, new Vector2(1.0f, 0.0f));

                this.Drawer.DrawSolidCircle(center, radius, axis, color);
            }
                break;

            case EdgeShape edge:
            {
                Vector2 v1 = MathUtils.Mul(xf, edge.Vertex1);
                Vector2 v2 = MathUtils.Mul(xf, edge.Vertex2);
                this.Drawer.DrawSegment(v1, v2, color);

                if (edge.OneSided == false)
                {
                    this.Drawer.DrawPoint(v1, 4.0f, color);
                    this.Drawer.DrawPoint(v2, 4.0f, color);
                }
            }
                break;

            case ChainShape chain:
            {
                int count = chain.Count;
                Vector2[]? vertices = chain.Vertices;

                Vector2 v1 = MathUtils.Mul(xf, vertices[0]);
                for (int i = 1; i < count; ++i)
                {
                    Vector2 v2 = MathUtils.Mul(xf, vertices[i]);
                    this.Drawer.DrawSegment(v1, v2, color);
                    v1 = v2;
                }
            }
                break;

            case PolygonShape poly:
            {
                int vertexCount = poly.Count;
                Debug.Assert(vertexCount <= Settings.MaxPolygonVertices);
                Span<Vector2> vertices = stackalloc Vector2[vertexCount];

                for (int i = 0; i < vertexCount; ++i)
                {
                    vertices[i] = MathUtils.Mul(xf, poly.Vertices[i]);
                }

                this.Drawer.DrawSolidPolygon(vertices, vertexCount, color);
            }
                break;
        }
    }

    #endregion
}