namespace ZxenLib.ParticleSystem;

using System;
using System.Collections.Generic;
using System.Linq;
using Entities;
using Entities.Components;
using Entities.Components.Interfaces;
using Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class ParticleEmitter : IUpdatableEntityComponent
{
    private readonly ISpriteManager spriteManager;

    private bool hasPlayed = false;
    private List<Particle> particles;
    private string[] spriteIds;
    private string atlasId;
    private Random random;

    public ParticleEmitter() { }

    public ParticleEmitter(IEntity parent, ISpriteManager spriteManager, IEnumerable<string> spriteIdList, string atlasId, Random random = null!, Vector2? location = null, int maxParticles = 0)
    {
        this.Parent = parent;
        this.Transform = parent.GetComponent<Transform>()!;
        this.spriteManager = spriteManager;
        this.spriteIds = spriteIdList.ToArray();
        this.atlasId = atlasId;
        this.particles = new();
        this.Id = Ids.GetNewId();
        this.random = random;
        this.ApplyGravity = false;
        this.GravityAcceleration = 3400.0f;
        this.MaxParticleSpeed = 750.0f;
        this.MaxParticles = maxParticles;
        this.Loop = true;
        this.AngularVelocity = 0;
        this.ParticleMinVelocityX = -500;
        this.ParticleMaxVelocityX = 500;
        this.ParticleMinVelocityY = -1000;
        this.ParticleMaxVelocityY = 1000;
        this.ParticleDrawColor = Color.White;
        this.ParticleSize = 1f;
        this.ParticleTimeToLive = 1f;
    }

    public uint Id { get; }

    public bool IsEnabled { get; set; }

    public IEntity Parent { get; private set; }

    public Transform Transform { get; set; }

    public void Register(IEntity parent)
    {
        this.Parent = parent;
        this.Transform = parent.GetComponent<Transform>()!;
    }

    public void Unregister()
    {
        this.Parent.UnregisterComponent(this.Id);
    }

    /// <summary>
    /// Determines if gravity is applied to the particles of this emitter.
    /// </summary>
    public bool ApplyGravity { get; set; }

    /// <summary>
    /// Rate of gravity acceleration of particles of this emitter.
    /// </summary>
    public float GravityAcceleration { get; set; }

    /// <summary>
    /// Maximum speed at which particles of this emotter can travel.
    /// </summary>
    public float MaxParticleSpeed { get; set; }

    /// <summary>
    /// Maximum number of particles this emtter can eminate.
    /// </summary>
    public int MaxParticles { get; set; }

    /// <summary>
    /// Does this particle emitter loop, or is it a single-shot?
    /// </summary>
    public bool Loop { get; set; }

    /// <summary>
    /// Angular Velocity of particles emitted.
    /// </summary>
    public float AngularVelocity { get; set; }

    /// <summary>
    /// The minimum x-axis velocity of particles emitted by this emitter.
    /// </summary>
    public int ParticleMinVelocityX { get; set; }

    /// <summary>
    /// The maximum x-axis velocity of particles emitted by this emitter.
    /// </summary>
    public int ParticleMaxVelocityX { get; set; }

    /// <summary>
    /// The minimm Y velocity of particles emitted by this emitter.
    /// </summary>
    public int ParticleMinVelocityY { get; set; }

    /// <summary>
    /// Maximum Y velocity of particles emitted by this emitter.
    /// </summary>
    public int ParticleMaxVelocityY { get; set; }

    /// <summary>
    /// The default draw color of particles emitted by this emitter.
    /// </summary>
    public Color ParticleDrawColor { get; set; }

    /// <summary>
    /// Scalar size of particles emitted by this emitter.
    /// </summary>
    public float ParticleSize { get; set; }

    /// <summary>
    /// Lifetime in seconds of particles emitted by this emitter.
    /// </summary>
    public float ParticleTimeToLive { get; set; }

    public void Update(float deltaTime)
    {
        if (!this.hasPlayed && this.particles.Count < this.MaxParticles)
        {
            for (int i = 0; i < this.MaxParticles; i++)
            {
                this.particles.Add(this.GenerateNewParticle());
            }
        }

        for (int particle = 0; particle < this.particles.Count; particle++)
        {
            this.particles[particle].Update(deltaTime);

            if (this.particles[particle].TimeToLive > 0)
            {
                continue;
            }

            this.particles.RemoveAt(particle);
            particle--;
        }

        this.hasPlayed = true;
        if (this.Loop)
        {
            this.hasPlayed = false;
        }
    }

    /// <summary>
    /// Plays the current effect. Does not enable loop.
    /// </summary>
    public void Play()
    {
        this.hasPlayed = false;
    }

    /// <summary>
    /// Stops the currently playing particle effect. If it loops, it stops the loop.
    /// </summary>
    public void Stop()
    {
        this.hasPlayed = true;
    }

    /// <summary>
    /// Toggles the "Loop" property of the particle emitter.
    /// </summary>
    public void ToggleLoop()
    {
        this.Loop = !this.Loop;
    }

    /// <summary>
    /// Generates particles based on the criteria of properties in this emitter.
    /// </summary>
    /// <returns></returns>
    private Particle GenerateNewParticle()
    {
        Vector2 position = this.Transform.Position;
        Vector2 velocity = new Vector2(
            this.random.Next(this.ParticleMinVelocityX, this.ParticleMaxVelocityX),
            this.random.Next(this.ParticleMinVelocityY, this.ParticleMaxVelocityY));
        float angle = 0;
        float angularVelocity = this.AngularVelocity;
        Color color = this.ParticleDrawColor;
        float size = this.ParticleSize;
        float ttl = this.ParticleTimeToLive;

        Sprite sprite = this.spriteManager.GetSprite(
            this.atlasId,
            this.spriteIds[this.random.Next(this.spriteIds.Length)]);

        Atlas atlas = this.spriteManager.GetAtlas(this.atlasId);

        return new Particle(
            sprite,
            atlas,
            position,
            velocity,
            angle,
            angularVelocity,
            color,
            size,
            ttl, this.ApplyGravity, this.GravityAcceleration, this.MaxParticleSpeed);
    }

    /// <summary>
    /// Draws all particles in this emitter.
    /// </summary>
    /// <param name="sb"></param>
    public void Draw(SpriteBatch sb)
    {
        if (!this.IsEnabled)
        {
            return;
        }

        Matrix transformMatrix = Camera2D.Main.GetCurrentTransformMatrix();
        sb.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            DepthStencilState.Default,
            null,
            null,
            transformMatrix);

        for (int index = 0; index < this.particles.Count; index++)
        {
            this.particles[index].Draw(sb);
        }

        sb.End();
    }
}