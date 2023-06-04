namespace ZxenLib.Physics.Collisions;

using System.Collections.Generic;
using Microsoft.Xna.Framework;

public class CollisionManifold
{
    private Vector2 normal;
    private IEnumerable<Vector2> contactPoints;
    private float depth;
    private bool isColliding;
    private uint key = 0;

    public CollisionManifold()
    {
        this.depth = 0;
        this.normal = new Vector2();
        this.contactPoints = new List<Vector2>();
    }

    public CollisionManifold(Vector2 normal, IEnumerable<Vector2> contactPoints, float depth)
    {
        this.normal = normal;
        this.depth = depth;
        this.contactPoints = contactPoints;
        this.isColliding = true;
        this.key = Ids.GetNewId();
    }

    public Vector2 Normal
    {
        get => this.normal;
        set => this.normal = value;
    }

    public uint Key => this.key;

    public IEnumerable<Vector2> ContactPoints
    {
        get => this.contactPoints;
        set => this.contactPoints = value;
    }

    public float Depth
    {
        get => this.depth;
        set => this.depth = value;
    }

    public bool IsColliding
    {
        get => this.isColliding;
        set => this.isColliding = value;
    }
}