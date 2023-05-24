namespace ZxenLib.Physics;

using Entities.Components;
using Microsoft.Xna.Framework;

public class Rigidbody2D : EntityComponent
{
    private Vector2 position = new Vector2();
    private float rotation = 0f;

    public Rigidbody2D()
    {
        this.Id = Ids.GetNewId();
        this.IsEnabled = true;
    }

    public float Rotation
    {
        get => this.rotation;
        set => this.rotation = value;
    }

    public Vector2 Position
    {
        get => this.position;
        set => this.position = value;
    }
}