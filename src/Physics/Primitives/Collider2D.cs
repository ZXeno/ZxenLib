namespace ZxenLib.Physics.Primitives;

using Entities.Components;
using Microsoft.Xna.Framework;

public class Collider2D : EntityComponent
{
    public Collider2D()
    {
        this.Id = Ids.GetNewId();
        this.IsEnabled = true;
    }

    public Vector2 Offset { get; set; }
}