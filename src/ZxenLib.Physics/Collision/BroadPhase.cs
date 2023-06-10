namespace ZxenLib.Physics.Collision;

using System;
using System.Buffers;
using Microsoft.Xna.Framework;
using Collider;
using Dynamics;

public class BroadPhase : ITreeQueryCallback
{
    public const int NullProxy = -1;

    private DynamicTree _tree;

    private int _proxyCount;

    private int[] _moveBuffer;

    private int _moveCapacity;

    private int _moveCount;

    private Pair[] _pairBuffer;

    private int _pairCapacity;

    private int _pairCount;

    private int _queryProxyId;

    public BroadPhase()
    {
        this._proxyCount = 0;
        this._tree = new DynamicTree();
        this._pairCapacity = 16;
        this._pairCount = 0;
        this._pairBuffer = ArrayPool<Pair>.Shared.Rent(this._pairCapacity);
        this._moveCapacity = 16;
        this._moveCount = 0;
        this._moveBuffer = ArrayPool<int>.Shared.Rent(this._moveCapacity);
    }

    ~BroadPhase()
    {
        if (this._pairBuffer != null)
        {
            ArrayPool<Pair>.Shared.Return(this._pairBuffer, true);
        }

        if (this._moveBuffer != null)
        {
            ArrayPool<int>.Shared.Return(this._moveBuffer, true);
        }
    }

    /// Create a proxy with an initial AABB. Pairs are not reported until
    /// UpdatePairs is called.
    public int CreateProxy(in AABB aabb, FixtureProxy userData)
    {
        int proxyId = this._tree.CreateProxy(aabb, userData);
        ++this._proxyCount;
        this.BufferMove(proxyId);
        return proxyId;
    }

    /// Destroy a proxy. It is up to the client to remove any pairs.
    public void DestroyProxy(int proxyId)
    {
        this.UnBufferMove(proxyId);
        --this._proxyCount;
        this._tree.DestroyProxy(proxyId);
    }

    /// Call MoveProxy as many times as you like, then when you are done
    /// call UpdatePairs to finalized the proxy pairs (for your time step).
    public void MoveProxy(int proxyId, in AABB aabb, in Vector2 displacement)
    {
        bool buffer = this._tree.MoveProxy(proxyId, aabb, displacement);
        if (buffer)
        {
            this.BufferMove(proxyId);
        }
    }

    /// Call to trigger a re-processing of it's pairs on the next call to UpdatePairs.
    public void TouchProxy(int proxyId)
    {
        this.BufferMove(proxyId);
    }

    /// Get the fat AABB for a proxy.
    public AABB GetFatAABB(int proxyId)
    {
        return this._tree.GetFatAABB(proxyId);
    }

    /// Get user data from a proxy. Returns nullptr if the id is invalid.
    internal object GetUserData(int proxyId)
    {
        return this._tree.GetUserData(proxyId);
    }

    /// Test overlap of fat AABBs.
    public bool TestOverlap(int proxyIdA, int proxyIdB)
    {
        return CollisionUtils.TestOverlap(this._tree.GetFatAABB(proxyIdA), this._tree.GetFatAABB(proxyIdB));
    }

    /// Get the number of proxies.
    public int GetProxyCount()
    {
        return this._proxyCount;
    }

    /// Update the pairs. This results in pair callbacks. This can only add pairs.
    internal void UpdatePairs<T>(T callback)
        where T : IAddPairCallback
    {
        // Reset pair buffer
        this._pairCount = 0;

        // Perform tree queries for all moving proxies.
        for (int j = 0; j < this._moveCount; ++j)
        {
            this._queryProxyId = this._moveBuffer[j];
            if (this._queryProxyId == NullProxy)
            {
                continue;
            }

            // We have to query the tree with the fat AABB so that
            // we don't fail to create a pair that may touch later.
            AABB fatAABB = this._tree.GetFatAABB(this._queryProxyId);

            // Query tree, create pairs and add them pair buffer.
            this._tree.Query(this, fatAABB);
        }

        // Send pairs to caller
        for (int i = 0; i < this._pairCount; ++i)
        {
            ref readonly Pair primaryPair = ref this._pairBuffer[i];
            object? userDataA = this._tree.GetUserData(primaryPair.ProxyIdA);
            object? userDataB = this._tree.GetUserData(primaryPair.ProxyIdB);

            callback.AddPairCallback(userDataA, userDataB);
        }

        // Clear move flags
        for (int i = 0; i < this._moveCount; ++i)
        {
            int proxyId = this._moveBuffer[i];
            if (proxyId == NullProxy)
            {
                continue;
            }

            this._tree.ClearMoved(proxyId);
        }

        // Reset move buffer
        this._moveCount = 0;
    }

    /// Query an AABB for overlapping proxies. The callback class
    /// is called for each proxy that overlaps the supplied AABB.
    public void Query(in ITreeQueryCallback callback, in AABB aabb)
    {
        this._tree.Query(callback, aabb);
    }

    /// Ray-cast against the proxies in the tree. This relies on the callback
    /// to perform a exact ray-cast in the case were the proxy contains a shape.
    /// The callback also performs the any collision filtering. This has performance
    /// roughly equal to k * log(n), where k is the number of collisions and n is the
    /// number of proxies in the tree.
    /// @param input the ray-cast input data. The ray extends from p1 to p1 + maxFraction * (p2 - p1).
    /// @param callback a callback class that is called for each proxy that is hit by the ray.
    public void RayCast(in ITreeRayCastCallback callback, in RayCastInput input)
    {
        this._tree.RayCast(callback, input);
    }

    /// Get the height of the embedded tree.
    public int GetTreeHeight()
    {
        return this._tree.GetHeight();
    }

    /// Get the balance of the embedded tree.
    public int GetTreeBalance()
    {
        return this._tree.GetMaxBalance();
    }

    /// Get the quality metric of the embedded tree.
    public float GetTreeQuality()
    {
        return this._tree.GetAreaRatio();
    }

    /// Shift the world origin. Useful for large worlds.
    /// The shift formula is: position -= newOrigin
    /// @param newOrigin the new origin with respect to the old origin
    public void ShiftOrigin(in Vector2 newOrigin)
    {
        this._tree.ShiftOrigin(newOrigin);
    }

    private void BufferMove(int proxyId)
    {
        if (this._moveCount == this._moveCapacity)
        {
            int[]? oldBuffer = this._moveBuffer;
            this._moveCapacity *= 2;
            this._moveBuffer = ArrayPool<int>.Shared.Rent(this._moveCapacity);
            Array.Copy(oldBuffer, this._moveBuffer, this._moveCount);
            Array.Clear(oldBuffer, 0, this._moveCount);
            ArrayPool<int>.Shared.Return(oldBuffer);
        }

        this._moveBuffer[this._moveCount] = proxyId;
        ++this._moveCount;
    }

    private void UnBufferMove(int proxyId)
    {
        for (int i = 0; i < this._moveCount; ++i)
        {
            if (this._moveBuffer[i] == proxyId)
            {
                this._moveBuffer[i] = NullProxy;
            }
        }
    }

    public bool QueryCallback(int proxyId)
    {
        // A proxy cannot form a pair with itself.
        if (proxyId == this._queryProxyId)
        {
            return true;
        }

        bool moved = this._tree.WasMoved(proxyId);
        if (moved && proxyId > this._queryProxyId)
        {
            // Both proxies are moving. Avoid duplicate pairs.
            return true;
        }

        // Grow the pair buffer as needed.
        if (this._pairCount == this._pairCapacity)
        {
            Pair[]? oldBuffer = this._pairBuffer;
            this._pairCapacity += this._pairCapacity >> 1;
            this._pairBuffer = ArrayPool<Pair>.Shared.Rent(this._pairCapacity);
            Array.Copy(oldBuffer, this._pairBuffer, this._pairCount);
            Array.Clear(oldBuffer, 0, this._pairCount);
            ArrayPool<Pair>.Shared.Return(oldBuffer);
        }

        this._pairBuffer[this._pairCount].ProxyIdA = Math.Min(proxyId, this._queryProxyId);
        this._pairBuffer[this._pairCount].ProxyIdB = Math.Max(proxyId, this._queryProxyId);
        ++this._pairCount;

        return true;
    }
}

public struct Pair
{
    public int ProxyIdA;

    public int ProxyIdB;
}