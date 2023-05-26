namespace ZxenLib.Physics.Primitives;

using Microsoft.Xna.Framework;

public class RaycastResult
{
    private Vector2 point;
    private Vector2 normal;
    private float t;
    private bool hit;

    public RaycastResult()
    {
        this.point = new Vector2();
        this.normal = new Vector2();
        this.t = -1;
    }

    public Vector2 Point => this.point;

    public Vector2 Normal => this.normal;

    public float T => this.t;

    public bool Hit => this.hit;

    public void Init(Vector2 point, Vector2 normal, float t, bool hit)
    {
        this.point.X = point.X;
        this.point.Y = point.Y;
        this.normal.X = normal.X;
        this.normal.Y = normal.Y;
        this.t = t;
        this.hit = hit;
    }

    public void Reset()
    {
        this.point.X = 0;
        this.point.Y = 0;
        this.normal.X = 0;
        this.normal.Y = 0;
        this.t = -1;
        this.hit = false;
    }
}