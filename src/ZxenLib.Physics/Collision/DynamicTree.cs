namespace ZxenLib.Physics.Collision;

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using Microsoft.Xna.Framework;
using System.Runtime.CompilerServices;
using Collider;
using Common;
using Dynamics;

public class DynamicTree
{
    public const int NullNode = -1;

    private int _freeList;

    private int _nodeCapacity;

    private int _nodeCount;

    private int _root;

    private TreeNode[] _treeNodes;

    public DynamicTree()
    {
        this._root = NullNode;

        this._nodeCapacity = 16;
        this._nodeCount = 0;
        this._treeNodes = ArrayPool<TreeNode>.Shared.Rent(this._nodeCapacity);

        // Build a linked list for the free list.
        // 节点数组初始化
        for (int i = 0; i < this._nodeCapacity; ++i)
        {
            this._treeNodes[i] = new TreeNode {Next = i + 1, Height = -1};
        }

        // 最后一个节点Next为null
        this._treeNodes[this._nodeCapacity - 1].Next = NullNode;
        this._treeNodes[this._nodeCapacity - 1].Height = -1;

        this._freeList = 0;
    }

    private int AllocateNode()
    {
        // Expand the node pool as needed.
        if (this._freeList == NullNode)
        {
            Debug.Assert(this._nodeCount == this._nodeCapacity);

            // The free list is empty. Rebuild a bigger pool.
            // 剩余节点为0,增加可用节点
            TreeNode[]? oldNodes = this._treeNodes;
            this._nodeCapacity *= 2;

            this._treeNodes = ArrayPool<TreeNode>.Shared.Rent(this._nodeCapacity);
            Array.Copy(oldNodes, this._treeNodes, this._nodeCount);
            Array.Clear(oldNodes, 0, this._nodeCount);
            ArrayPool<TreeNode>.Shared.Return(oldNodes);

            // Build a linked list for the free list. The parent
            // pointer becomes the "next" pointer.
            for (int i = this._nodeCount; i < this._nodeCapacity; ++i)
            {
                this._treeNodes[i] = new TreeNode {Next = i + 1, Height = -1};
            }

            this._treeNodes[this._nodeCapacity - 1].Next = NullNode;
            this._treeNodes[this._nodeCapacity - 1].Height = -1;
            this._freeList = this._nodeCount;
        }

        // Peel a node off the free list.
        int nodeId = this._freeList;
        this._freeList = this._treeNodes[nodeId].Next;
        ref TreeNode newNode = ref this._treeNodes[nodeId];
        newNode.Parent = NullNode;
        newNode.Child1 = NullNode;
        newNode.Child2 = NullNode;
        newNode.Height = 0;
        newNode.UserData = null;
        newNode.Moved = false;
        ++this._nodeCount;
        return nodeId;
    }

    private void FreeNode(int nodeId)
    {
        Debug.Assert(0 <= nodeId && nodeId < this._nodeCapacity);
        Debug.Assert(0 < this._nodeCount);
        ref TreeNode freeNode = ref this._treeNodes[nodeId];
        freeNode.Reset();
        freeNode.Next = this._freeList;
        freeNode.Height = -1;
        this._freeList = nodeId;
        --this._nodeCount;
    }

    /// Create a proxy. Provide a tight fitting AABB and a userData pointer.
    public int CreateProxy(in AABB aabb, object userData)
    {
        int proxyId = this.AllocateNode();
        ref TreeNode proxyNode = ref this._treeNodes[proxyId];

        // Fatten the aabb.
        Vector2 r = new Vector2(Settings.AABBExtension, Settings.AABBExtension);
        proxyNode.AABB.LowerBound = aabb.LowerBound - r;
        proxyNode.AABB.UpperBound = aabb.UpperBound + r;
        proxyNode.UserData = userData;
        proxyNode.Height = 0;
        proxyNode.Moved = true;
        this.InsertLeaf(proxyId);

        return proxyId;
    }

    /// Destroy a proxy. This asserts if the id is invalid.
    public void DestroyProxy(int proxyId)
    {
        Debug.Assert(0 <= proxyId && proxyId < this._nodeCapacity);
        Debug.Assert(this._treeNodes[proxyId].IsLeaf());

        this.RemoveLeaf(proxyId);
        this.FreeNode(proxyId);
    }

    /// Move a proxy with a swepted AABB. If the proxy has moved outside of its fattened AABB,
    /// then the proxy is removed from the tree and re-inserted. Otherwise
    /// the function returns immediately.
    /// @return true if the proxy was re-inserted.
    public bool MoveProxy(int proxyId, in AABB aabb, in Vector2 displacement)
    {
        Debug.Assert(0 <= proxyId && proxyId < this._nodeCapacity);

        Debug.Assert(this._treeNodes[proxyId].IsLeaf());

        // Extend AABB
        Vector2 r = new Vector2(Settings.AABBExtension, Settings.AABBExtension);
        AABB fatAABB = new AABB
        {
            LowerBound = aabb.LowerBound - r,
            UpperBound = aabb.UpperBound + r
        };

        // Predict AABB movement
        Vector2 d = Settings.AABBMultiplier * displacement;

        if (d.X < 0.0f)
        {
            fatAABB.LowerBound.X += d.X;
        }
        else
        {
            fatAABB.UpperBound.X += d.X;
        }

        if (d.Y < 0.0f)
        {
            fatAABB.LowerBound.Y += d.Y;
        }
        else
        {
            fatAABB.UpperBound.Y += d.Y;
        }

        ref TreeNode proxyNode = ref this._treeNodes[proxyId];
        ref AABB treeAABB = ref proxyNode.AABB;
        if (treeAABB.Contains(aabb))
        {
            // The tree AABB still contains the object, but it might be too large.
            // Perhaps the object was moving fast but has since gone to sleep.
            // The huge AABB is larger than the new fat AABB.
            AABB hugeAABB = new AABB
            {
                LowerBound = fatAABB.LowerBound - 4.0f * r,
                UpperBound = fatAABB.UpperBound + 4.0f * r
            };

            if (hugeAABB.Contains(treeAABB))
            {
                // The tree AABB contains the object AABB and the tree AABB is
                // not too large. No tree update needed.
                return false;
            }

            // Otherwise the tree AABB is huge and needs to be shrunk
        }

        this.RemoveLeaf(proxyId);

        proxyNode.AABB = fatAABB;

        this.InsertLeaf(proxyId);

        proxyNode.Moved = true;

        return true;
    }

    /// Get proxy user data.
    /// @return the proxy user data or 0 if the id is invalid.
    public object GetUserData(int proxyId)
    {
        Debug.Assert(0 <= proxyId && proxyId < this._nodeCapacity);
        return this._treeNodes[proxyId].UserData;
    }

    public bool WasMoved(int proxyId)
    {
        Debug.Assert(0 <= proxyId && proxyId < this._nodeCapacity);
        return this._treeNodes[proxyId].Moved;
    }

    public void ClearMoved(int proxyId)
    {
        Debug.Assert(0 <= proxyId && proxyId < this._nodeCapacity);
        this._treeNodes[proxyId].Moved = false;
    }

    /// Get the fat AABB for a proxy.
    public AABB GetFatAABB(int proxyId)
    {
        Debug.Assert(0 <= proxyId && proxyId < this._nodeCapacity);
        return this._treeNodes[proxyId].AABB;
    }

    private readonly Stack<int> _queryStack = new Stack<int>(256);

    /// Query an AABB for overlapping proxies. The callback class
    /// is called for each proxy that overlaps the supplied AABB.
    public void Query(in ITreeQueryCallback callback, in AABB aabb)
    {
        Stack<int>? stack = this._queryStack;
        stack.Clear();
        stack.Push(this._root);

        while (stack.Count > 0)
        {
            int nodeId = stack.Pop();
            if (nodeId == NullNode)
            {
                continue;
            }

            ref readonly TreeNode node = ref this._treeNodes[nodeId];

            if (CollisionUtils.TestOverlap(node.AABB, aabb))
            {
                if (node.IsLeaf())
                {
                    bool proceed = callback.QueryCallback(nodeId);
                    if (proceed == false)
                    {
                        return;
                    }
                }
                else
                {
                    stack.Push(node.Child1);
                    stack.Push(node.Child2);
                }
            }
        }

        stack.Clear();
    }

    private readonly Stack<int> _rayCastStack = new Stack<int>(256);

    /// Ray-cast against the proxies in the tree. This relies on the callback
    /// to perform a exact ray-cast in the case were the proxy contains a shape.
    /// The callback also performs the any collision filtering. This has performance
    /// roughly equal to k * log(n), where k is the number of collisions and n is the
    /// number of proxies in the tree.
    /// @param input the ray-cast input data. The ray extends from p1 to p1 + maxFraction * (p2 - p1).
    /// @param callback a callback class that is called for each proxy that is hit by the ray.
    public void RayCast(in ITreeRayCastCallback inputCallback, in RayCastInput input)
    {
        Vector2 p1 = input.P1;
        Vector2 p2 = input.P2;
        Vector2 r = p2 - p1;
        Debug.Assert(r.LengthSquared() > 0.0f);
        r.Normalize();

        // v is perpendicular to the segment.
        Vector2 v = MathUtils.Cross(1.0f, r);
        Vector2 abs_v = MathExtensions.Vector2Abs(v);

        // Separating axis for segment (Gino, p80).
        // |dot(v, p1 - c)| > dot(|v|, h)

        float maxFraction = input.MaxFraction;

        // Build a bounding box for the segment.
        AABB segmentAABB = new AABB();
        {
            Vector2 t = p1 + maxFraction * (p2 - p1);
            segmentAABB.LowerBound = Vector2.Min(p1, t);
            segmentAABB.UpperBound = Vector2.Max(p1, t);
        }

        Stack<int>? stack = this._rayCastStack;
        stack.Clear();
        stack.Push(this._root);

        while (stack.Count > 0)
        {
            int nodeId = stack.Pop();
            if (nodeId == NullNode)
            {
                continue;
            }

            ref readonly TreeNode node = ref this._treeNodes[nodeId];

            if (CollisionUtils.TestOverlap(node.AABB, segmentAABB) == false)
            {
                continue;
            }

            // Separating axis for segment (Gino, p80).
            // |dot(v, p1 - c)| > dot(|v|, h)
            Vector2 c = node.AABB.GetCenter();
            Vector2 h = node.AABB.GetExtents();
            float separation = Math.Abs(Vector2.Dot(v, p1 - c)) - Vector2.Dot(abs_v, h);
            if (separation > 0.0f)
            {
                continue;
            }

            if (node.IsLeaf())
            {
                RayCastInput subInput = new RayCastInput
                {
                    P1 = input.P1,
                    P2 = input.P2,
                    MaxFraction = maxFraction
                };

                float value = inputCallback.RayCastCallback(subInput, nodeId);

                if (value.Equals(0.0f))
                {
                    // The client has terminated the ray cast.
                    return;
                }

                if (value > 0.0f)
                {
                    // Update segment bounding box.
                    maxFraction = value;
                    Vector2 t = p1 + maxFraction * (p2 - p1);
                    segmentAABB.LowerBound = Vector2.Min(p1, t);
                    segmentAABB.UpperBound = Vector2.Max(p1, t);
                }
            }
            else
            {
                stack.Push(node.Child1);
                stack.Push(node.Child2);
            }
        }
    }

    /// Validate this tree. For testing.
    public void Validate()
    {
        #if b2DEBUG
	ValidateStructure(m_root);
	ValidateMetrics(m_root);

	int freeCount = 0;
	int freeIndex = m_freeList;
	while (freeIndex != b2_nullNode)
	{
		Debug.Assert(0 <= freeIndex && freeIndex < m_nodeCapacity);
		freeIndex = m_nodes[freeIndex].next;
		++freeCount;
	}

	Debug.Assert(GetHeight() == ComputeHeight());

	Debug.Assert(m_nodeCount + freeCount == m_nodeCapacity);
        #endif
    }

    /// Compute the height of the binary tree in O(N) time. Should not be
    /// called often.
    public int GetHeight()
    {
        if (this._root == NullNode)
        {
            return 0;
        }

        return this._treeNodes[this._root].Height;
    }

    /// Get the maximum balance of an node in the tree. The balance is the difference
    /// in height of the two children of a node.
    public int GetMaxBalance()
    {
        int maxBalance = 0;
        for (int i = 0; i < this._nodeCapacity; ++i)
        {
            ref readonly TreeNode node = ref this._treeNodes[i];
            if (node.Height <= 1)
            {
                continue;
            }

            Debug.Assert(node.IsLeaf() == false);

            int child1 = node.Child1;
            int child2 = node.Child2;
            int balance = Math.Abs(this._treeNodes[child2].Height - this._treeNodes[child1].Height);
            maxBalance = Math.Max(maxBalance, balance);
        }

        return maxBalance;
    }

    /// Get the ratio of the sum of the node areas to the root area.
    public float GetAreaRatio()
    {
        if (this._root == NullNode)
        {
            return 0.0f;
        }

        ref readonly TreeNode root = ref this._treeNodes[this._root];
        float rootArea = root.AABB.GetPerimeter();

        float totalArea = 0.0f;
        for (int i = 0; i < this._nodeCapacity; ++i)
        {
            ref readonly TreeNode node = ref this._treeNodes[i];
            if (node.Height < 0)
            {
                // Free node in pool
                continue;
            }

            totalArea += node.AABB.GetPerimeter();
        }

        return totalArea / rootArea;
    }

    /// Build an optimal tree. Very expensive. For testing.
    public void RebuildBottomUp()
    {
        int[]? nodes = new int[this._nodeCount];
        int count = 0;

        // Build array of leaves. Free the rest.
        for (int i = 0; i < this._nodeCapacity; ++i)
        {
            ref TreeNode nodeI = ref this._treeNodes[i];
            if (nodeI.Height < 0)
            {
                // free node in pool
                continue;
            }

            if (nodeI.IsLeaf())
            {
                nodeI.Parent = NullNode;
                nodes[count] = i;
                ++count;
            }
            else
            {
                this.FreeNode(i);
            }
        }

        while (count > 1)
        {
            float minCost = Settings.MaxFloat;
            int iMin = -1, jMin = -1;
            for (int i = 0; i < count; ++i)
            {
                ref readonly AABB aabbi = ref this._treeNodes[nodes[i]].AABB;

                for (int j = i + 1; j < count; ++j)
                {
                    AABB aabbj = this._treeNodes[nodes[j]].AABB;
                    AABB.Combine(aabbi, aabbj, out AABB b);
                    float cost = b.GetPerimeter();
                    if (cost < minCost)
                    {
                        iMin = i;
                        jMin = j;
                        minCost = cost;
                    }
                }
            }

            int index1 = nodes[iMin];
            int index2 = nodes[jMin];
            ref TreeNode child1 = ref this._treeNodes[index1];
            ref TreeNode child2 = ref this._treeNodes[index2];

            int parentIndex = this.AllocateNode();
            ref TreeNode parent = ref this._treeNodes[parentIndex];
            parent.Child1 = index1;
            parent.Child2 = index2;
            parent.Height = 1 + Math.Max(child1.Height, child2.Height);
            parent.AABB.Combine(child1.AABB, child2.AABB);
            parent.Parent = NullNode;

            child1.Parent = parentIndex;
            child2.Parent = parentIndex;

            nodes[jMin] = nodes[count - 1];
            nodes[iMin] = parentIndex;
            --count;
        }

        this._root = nodes[0];
        this.Validate();
    }

    /// Shift the world origin. Useful for large worlds.
    /// The shift formula is: position -= newOrigin
    /// @param newOrigin the new origin with respect to the old origin
    public void ShiftOrigin(in Vector2 newOrigin)
    {
        // Build array of leaves. Free the rest.
        for (int i = 0; i < this._nodeCapacity; ++i)
        {
            this._treeNodes[i].AABB.LowerBound -= newOrigin;
            this._treeNodes[i].AABB.UpperBound -= newOrigin;
        }
    }

    private void InsertLeaf(int leaf)
    {
        if (this._root == NullNode)
        {
            this._root = leaf;
            this._treeNodes[this._root].Parent = NullNode;
            return;
        }

        // Find the best sibling for this node
        AABB leafAABB = this._treeNodes[leaf].AABB;
        int index = this._root;
        while (this._treeNodes[index].IsLeaf() == false)
        {
            ref TreeNode indexNode = ref this._treeNodes[index];
            int child1 = indexNode.Child1;
            int child2 = indexNode.Child2;

            float area = indexNode.AABB.GetPerimeter();

            AABB.Combine(indexNode.AABB, leafAABB, out AABB combinedAABB);
            float combinedArea = combinedAABB.GetPerimeter();

            // Cost of creating a new parent for this node and the new leaf
            float cost = 2.0f * combinedArea;

            // Minimum cost of pushing the leaf further down the tree
            float inheritanceCost = 2.0f * (combinedArea - area);

            // Cost of descending into child1
            float cost1;
            if (this._treeNodes[child1].IsLeaf())
            {
                AABB.Combine(leafAABB, this._treeNodes[child1].AABB, out AABB aabb);
                cost1 = aabb.GetPerimeter() + inheritanceCost;
            }
            else
            {
                AABB.Combine(leafAABB, this._treeNodes[child1].AABB, out AABB aabb);
                float oldArea = this._treeNodes[child1].AABB.GetPerimeter();
                float newArea = aabb.GetPerimeter();
                cost1 = newArea - oldArea + inheritanceCost;
            }

            // Cost of descending into child2
            float cost2;
            if (this._treeNodes[child2].IsLeaf())
            {
                AABB.Combine(leafAABB, this._treeNodes[child2].AABB, out AABB aabb);
                cost2 = aabb.GetPerimeter() + inheritanceCost;
            }
            else
            {
                AABB.Combine(leafAABB, this._treeNodes[child2].AABB, out AABB aabb);
                float oldArea = this._treeNodes[child2].AABB.GetPerimeter();
                float newArea = aabb.GetPerimeter();
                cost2 = newArea - oldArea + inheritanceCost;
            }

            // Descend according to the minimum cost.
            if (cost < cost1 && cost < cost2)
            {
                break;
            }

            // Descend
            if (cost1 < cost2)
            {
                index = child1;
            }
            else
            {
                index = child2;
            }
        }

        int sibling = index;

        // Create a new parent.
        ref readonly TreeNode oldNode = ref this._treeNodes[sibling];

        int oldParent = oldNode.Parent;
        int newParent = this.AllocateNode();
        ref TreeNode newParentNode = ref this._treeNodes[newParent];
        newParentNode.Parent = oldParent;
        newParentNode.UserData = null;
        newParentNode.AABB.Combine(leafAABB, oldNode.AABB);
        newParentNode.Height = oldNode.Height + 1;

        if (oldParent != NullNode)
        {
            ref TreeNode oldParentNode = ref this._treeNodes[oldParent];

            // The sibling was not the root.
            if (oldParentNode.Child1 == sibling)
            {
                oldParentNode.Child1 = newParent;
            }
            else
            {
                oldParentNode.Child2 = newParent;
            }

            newParentNode.Child1 = sibling;
            newParentNode.Child2 = leaf;
            this._treeNodes[sibling].Parent = newParent;
            this._treeNodes[leaf].Parent = newParent;
        }
        else
        {
            // The sibling was the root.
            newParentNode.Child1 = sibling;
            newParentNode.Child2 = leaf;
            this._treeNodes[sibling].Parent = newParent;
            this._treeNodes[leaf].Parent = newParent;
            this._root = newParent;
        }

        // Walk back up the tree fixing heights and AABBs
        index = this._treeNodes[leaf].Parent;
        while (index != NullNode)
        {
            index = this.Balance(index);
            ref TreeNode indexNode = ref this._treeNodes[index];
            Debug.Assert(indexNode.Child1 != NullNode);
            Debug.Assert(indexNode.Child2 != NullNode);
            ref TreeNode child1 = ref this._treeNodes[indexNode.Child1];
            ref TreeNode child2 = ref this._treeNodes[indexNode.Child2];
            indexNode.Height = 1 + Math.Max(child1.Height, child2.Height);
            indexNode.AABB.Combine(child1.AABB, child2.AABB);

            index = indexNode.Parent;
        }

        //Validate();
    }

    private void RemoveLeaf(int leaf)
    {
        if (leaf == this._root)
        {
            this._root = NullNode;
            return;
        }

        int parent = this._treeNodes[leaf].Parent;
        ref TreeNode parentNode = ref this._treeNodes[parent];
        int grandParent = parentNode.Parent;

        int sibling = parentNode.Child1 == leaf ? parentNode.Child2 : parentNode.Child1;

        if (grandParent != NullNode)
        {
            ref TreeNode grandParentNode = ref this._treeNodes[grandParent];

            // Destroy parent and connect sibling to grandParent.
            if (grandParentNode.Child1 == parent)
            {
                grandParentNode.Child1 = sibling;
            }
            else
            {
                grandParentNode.Child2 = sibling;
            }

            this._treeNodes[sibling].Parent = grandParent;
            this.FreeNode(parent);

            // Adjust ancestor bounds.
            int index = grandParent;
            while (index != NullNode)
            {
                index = this.Balance(index);
                ref TreeNode indexNode = ref this._treeNodes[index];
                ref TreeNode child1 = ref this._treeNodes[indexNode.Child1];
                ref TreeNode child2 = ref this._treeNodes[indexNode.Child2];

                indexNode.AABB.Combine(child1.AABB, child2.AABB);
                indexNode.Height = 1 + Math.Max(child1.Height, child2.Height);

                index = indexNode.Parent;
            }
        }
        else
        {
            this._root = sibling;
            this._treeNodes[sibling].Parent = NullNode;
            this.FreeNode(parent);
        }

        //Validate();
    }

    private int Balance(int iA)
    {
        Debug.Assert(iA != NullNode);

        ref TreeNode A = ref this._treeNodes[iA];
        if (A.IsLeaf() || A.Height < 2)
        {
            return iA;
        }

        int iB = A.Child1;
        int iC = A.Child2;
        Debug.Assert(0 <= iB && iB < this._nodeCapacity);
        Debug.Assert(0 <= iC && iC < this._nodeCapacity);

        ref TreeNode B = ref this._treeNodes[iB];
        ref TreeNode C = ref this._treeNodes[iC];

        int balance = C.Height - B.Height;

        // Rotate C up
        if (balance > 1)
        {
            int iF = C.Child1;
            int iG = C.Child2;
            ref TreeNode F = ref this._treeNodes[iF];
            ref TreeNode G = ref this._treeNodes[iG];
            Debug.Assert(0 <= iF && iF < this._nodeCapacity);
            Debug.Assert(0 <= iG && iG < this._nodeCapacity);

            // Swap A and C
            C.Child1 = iA;
            C.Parent = A.Parent;
            A.Parent = iC;

            // A's old parent should point to C
            if (C.Parent != NullNode)
            {
                ref TreeNode cParentNode = ref this._treeNodes[C.Parent];
                if (cParentNode.Child1 == iA)
                {
                    cParentNode.Child1 = iC;
                }
                else
                {
                    Debug.Assert(this._treeNodes[C.Parent].Child2 == iA);
                    cParentNode.Child2 = iC;
                }
            }
            else
            {
                this._root = iC;
            }

            // Rotate
            if (F.Height > G.Height)
            {
                C.Child2 = iF;
                A.Child2 = iG;
                G.Parent = iA;
                A.AABB.Combine(B.AABB, G.AABB);
                C.AABB.Combine(A.AABB, F.AABB);

                A.Height = 1 + Math.Max(B.Height, G.Height);
                C.Height = 1 + Math.Max(A.Height, F.Height);
            }
            else
            {
                C.Child2 = iG;
                A.Child2 = iF;
                F.Parent = iA;
                A.AABB.Combine(B.AABB, F.AABB);
                C.AABB.Combine(A.AABB, G.AABB);

                A.Height = 1 + Math.Max(B.Height, F.Height);
                C.Height = 1 + Math.Max(A.Height, G.Height);
            }

            return iC;
        }

        // Rotate B up
        if (balance < -1)
        {
            int iD = B.Child1;
            int iE = B.Child2;
            ref TreeNode D = ref this._treeNodes[iD];
            ref TreeNode E = ref this._treeNodes[iE];
            Debug.Assert(0 <= iD && iD < this._nodeCapacity);
            Debug.Assert(0 <= iE && iE < this._nodeCapacity);

            // Swap A and B
            B.Child1 = iA;
            B.Parent = A.Parent;
            A.Parent = iB;

            // A's old parent should point to B
            if (B.Parent != NullNode)
            {
                ref TreeNode bParentNode = ref this._treeNodes[B.Parent];
                if (bParentNode.Child1 == iA)
                {
                    bParentNode.Child1 = iB;
                }
                else
                {
                    Debug.Assert(this._treeNodes[B.Parent].Child2 == iA);
                    bParentNode.Child2 = iB;
                }
            }
            else
            {
                this._root = iB;
            }

            // Rotate
            if (D.Height > E.Height)
            {
                B.Child2 = iD;
                A.Child1 = iE;
                E.Parent = iA;
                A.AABB.Combine(C.AABB, E.AABB);
                B.AABB.Combine(A.AABB, D.AABB);

                A.Height = 1 + Math.Max(C.Height, E.Height);
                B.Height = 1 + Math.Max(A.Height, D.Height);
            }
            else
            {
                B.Child2 = iE;
                A.Child1 = iD;
                D.Parent = iA;
                A.AABB.Combine(C.AABB, D.AABB);
                B.AABB.Combine(A.AABB, E.AABB);

                A.Height = 1 + Math.Max(C.Height, D.Height);
                B.Height = 1 + Math.Max(A.Height, E.Height);
            }

            return iB;
        }

        return iA;
    }

    private int ComputeHeight()
    {
        int height = this.ComputeHeight(this._root);
        return height;
    }

    private int ComputeHeight(int nodeId)
    {
        Debug.Assert(0 <= nodeId && nodeId < this._nodeCapacity);
        ref readonly TreeNode node = ref this._treeNodes[nodeId];

        if (node.IsLeaf())
        {
            return 0;
        }

        int height1 = this.ComputeHeight(node.Child1);
        int height2 = this.ComputeHeight(node.Child2);
        return 1 + Math.Max(height1, height2);
    }

    private void ValidateStructure(int index)
    {
        if (index == NullNode)
        {
            return;
        }

        if (index == this._root)
        {
            Debug.Assert(this._treeNodes[index].Parent == NullNode);
        }

        ref readonly TreeNode node = ref this._treeNodes[index];

        int child1 = node.Child1;
        int child2 = node.Child2;

        if (node.IsLeaf())
        {
            Debug.Assert(child1 == NullNode);
            Debug.Assert(child2 == NullNode);
            Debug.Assert(node.Height == 0);
            return;
        }

        Debug.Assert(0 <= child1 && child1 < this._nodeCapacity);
        Debug.Assert(0 <= child2 && child2 < this._nodeCapacity);

        Debug.Assert(this._treeNodes[child1].Parent == index);
        Debug.Assert(this._treeNodes[child2].Parent == index);

        this.ValidateStructure(child1);
        this.ValidateStructure(child2);
    }

    private void ValidateMetrics(int index)
    {
        if (index == NullNode)
        {
            return;
        }

        ref readonly TreeNode node = ref this._treeNodes[index];

        int child1 = node.Child1;
        int child2 = node.Child2;

        if (node.IsLeaf())
        {
            Debug.Assert(child1 == NullNode);
            Debug.Assert(child2 == NullNode);
            Debug.Assert(node.Height == 0);
            return;
        }

        Debug.Assert(0 <= child1 && child1 < this._nodeCapacity);
        Debug.Assert(0 <= child2 && child2 < this._nodeCapacity);

        int height1 = this._treeNodes[child1].Height;
        int height2 = this._treeNodes[child2].Height;
        int height = 1 + Math.Max(height1, height2);
        Debug.Assert(node.Height == height);

        AABB.Combine(this._treeNodes[child1].AABB, this._treeNodes[child2].AABB, out AABB aabb);

        Debug.Assert(aabb.LowerBound == node.AABB.LowerBound);
        Debug.Assert(aabb.UpperBound == node.AABB.UpperBound);

        this.ValidateMetrics(child1);
        this.ValidateMetrics(child2);
    }
}

public struct TreeNode
{
    /// Enlarged AABB
    public AABB AABB;

    public int Child1;

    public int Child2;

    // leaf = 0, free node = -1
    public int Height;

    public object UserData;

    // union next
    public int Parent { get; set; }

    // union parent
    public int Next
    {
        get => this.Parent;
        set => this.Parent = value;
    }

    public bool Moved;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Pure]
    public bool IsLeaf()
    {
        return this.Child1 == DynamicTree.NullNode;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Reset()
    {
        this.AABB = default;
        this.Child1 = default;
        this.Child2 = default;
        this.Height = default;
        this.UserData = default;
        this.Parent = default;
    }
}