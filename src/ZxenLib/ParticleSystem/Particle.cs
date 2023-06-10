namespace ZxenLib.ParticleSystem;

using System;
using Graphics;
using Graphics.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class Particle
    {
        /// <summary>
        /// Sprite for particle.
        /// </summary>
        public Sprite Sprite { get; set; }

        /// <summary>
        /// Atlas for particle.
        /// </summary>
        public Atlas Atlas { get; set; }

        /// <summary>
        /// Particle's position.
        /// </summary>
        public Vector2 Position { get; set; }

        /// <summary>
        /// Particle's current velocity.
        /// </summary>
        public Vector2 Velocity { get; set; }

        /// <summary>
        /// Particle's current angle.
        /// </summary>
        public float Angle { get; set; }

        /// <summary>
        /// Particle's current velocity.
        /// </summary>
        public float AngularVelocity { get; set; }

        /// <summary>
        /// Time that particle will remain on the screen in seconds.
        /// </summary>
        public float TimeToLive { get; set; }

        /// <summary>
        /// The current scale size of the particle.
        /// </summary>
        public float Size { get; set; }

        /// <summary>
        /// Apply gravity to the particle.
        /// </summary>
        public bool ApplyGravity { get; set; }

        /// <summary>
        /// Value of gravity's acceleration.
        /// </summary>
        public float GravityAcceleration { get; set; }

        /// <summary>
        /// Maximum speed at which the particle can move
        /// </summary>
        public float MaxSpeed { get; set; }
        private bool _doUpdate = true;

        public Particle(
            Sprite sprite,
            Atlas atlas,
            Vector2 position,
            Vector2 velocity,
            float angle,
            float angularVelocity,
            Color color,
            float size,
            float timeToLive,
            bool gravity = false,
            float gravityAcceleration = 0f,
            float maxFallSpeed = 0f)
        {
            this.Sprite = sprite;
            this.Position = position;
            this.Velocity = velocity;
            this.Angle = angle;
            this.AngularVelocity = angularVelocity;
            this.Sprite.DrawColor = color;
            this.Size = size;
            this.TimeToLive = timeToLive;
            this.ApplyGravity = gravity;
            this.GravityAcceleration = gravityAcceleration;
            this.MaxSpeed = maxFallSpeed;
        }

        public void Update(float deltaTime)
        {
            if (!this._doUpdate)
            {
                return;
            }

            if (this.ApplyGravity)
            {
                this.Velocity = new(this.Velocity.X,
                    MathHelper.Clamp(this.Velocity.Y + this.GravityAcceleration * deltaTime, -this.MaxSpeed, this.MaxSpeed));
            }

            Vector2 clampedVelocity = new(
                MathHelper.Clamp(this.Velocity.X, -this.MaxSpeed, this.MaxSpeed),
                MathHelper.Clamp(this.Velocity.Y, -this.MaxSpeed, this.MaxSpeed));

            this.Position += clampedVelocity * deltaTime;
            this.Angle += this.AngularVelocity * deltaTime;
            this.TimeToLive -= deltaTime;
        }

        public void Draw(SpriteBatch sb)
        {
            sb.Draw(
                this.Atlas.TextureAtlas,
                this.Position,
                this.Sprite.SourceRect,
                this.Sprite.DrawColor,
                this.Angle,
                this.Sprite.Center,
                this.Size,
                SpriteEffects.None,
                1f);
        }
    }