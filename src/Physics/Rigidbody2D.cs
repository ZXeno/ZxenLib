namespace ZxenLib.Physics;

using System;
using Entities;
using Entities.Components;
using Microsoft.Xna.Framework;

public class Rigidbody2D : EntityComponent
{
    private Vector2 localPosition = new Vector2();
    private Angle localRotation = new Angle();
    private Transform parentTransform;

    public Rigidbody2D()
    {
        this.Id = Ids.GetNewId();
        this.IsEnabled = true;
    }

    public Angle LocalRotation
    {
        get => this.localRotation;
        set => this.localRotation = value;
    }

    public Vector2 LocalPosition
    {
        get => this.localPosition;
        set => this.localPosition = value;
    }

    /// <summary>
    /// Registers the component with a parent entity.
    /// </summary>
    /// <param name="parent">The <see cref="IEntity"/> object parent of this <see cref="IEntityComponent"/>.</param>
    public override void Register(IEntity parent)
    {
        ArgumentNullException.ThrowIfNull(parent);

        if (this.Parent?.Id != parent.Id)
        {
            this.Parent = parent;
        }

        parent.RegisterComponent(this);

        this.parentTransform = Transform.GetOrAddTransform(parent);
    }
}