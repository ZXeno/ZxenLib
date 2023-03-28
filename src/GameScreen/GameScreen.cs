namespace ZxenLib.GameScreen;

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using ZxenLib.Events;

/// <summary>
/// Defines the implemenation of a GameScreen.
/// </summary>
public abstract partial class GameScreen
{
    private readonly IEventDispatcher eventDispatcher;
    private List<GameScreen> childStates;

    /// <summary>
    /// Initializes a new instance of the <see cref="GameScreen"/> class.
    /// </summary>
    public GameScreen(GameScreenManager gameScreenManager, IEventDispatcher eventDispatcher)
    {
        this.Id = Guid.NewGuid().ToString();
        this.eventDispatcher = eventDispatcher;
        this.StateManager = gameScreenManager;
        this.childStates = new List<GameScreen>();
        this.Tag = this;
        this.IsInitialized = false;
    }

    /// <summary>
    /// Gets or sets the Id of this object.
    /// </summary>
    public string Id { get; protected set; }

    /// <summary>
    /// Gets this <see cref="GameScreen"/>'s child states.
    /// </summary>
    public IList<GameScreen> ChildScreens { get => this.childStates; }

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="GameScreen"/> has been initialilized.
    /// </summary>
    public bool IsInitialized { get; protected set; }

    /// <summary>
    /// Gets or sets the GameScreen tag.
    /// </summary>
    public GameScreen Tag { get; protected set; }

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="GameScreen"/> is enabled.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="GameScreen"/> is visible.
    /// </summary>
    public bool Visible { get; set; }

    /// <summary>
    /// Gets or sets the draw order for this state.
    /// </summary>
    public int DrawOrder { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="GameScreenManager"/> for this <see cref="GameScreen"/>.
    /// </summary>
    protected GameScreenManager StateManager { get; set; }

    /// <summary>
    /// Performs initialization.
    /// </summary>
    public virtual void Initialize()
    {
        this.eventDispatcher.Subscribe(GameScreenManager.StateChangeEventId, this.StateChange, this);
        this.IsInitialized = true;
    }

    /// <summary>
    /// Loads any content.
    /// </summary>
    public virtual void LoadContent()
    {
    }

    /// <summary>
    /// Performs updates for this <see cref="GameScreen"/>. Called every frame.
    /// </summary>
    /// <param name="deltaTime">The elapsed time of the previous frame.</param>
    public virtual void Update(float deltaTime)
    {
        foreach (GameScreen child in this.childStates)
        {
            if (child.IsEnabled)
            {
                child.Update(deltaTime);
            }
        }
    }

    /// <summary>
    /// Performs draw call batching for this <see cref="GameScreen"/>. Called every frame.
    /// </summary>
    /// <param name="sb">The <see cref="SpriteBatch"/> used for batching draw calls.</param>
    public virtual void Draw(SpriteBatch sb)
    {
        foreach (GameScreen childState in this.childStates)
        {
            if (childState.Visible)
            {
                childState.Draw(sb);
            }
        }
    }

    /// <summary>
    /// Disposes of this object.
    /// </summary>
    public virtual void ClearState()
    {
        this.eventDispatcher.Unsubscribe(GameScreenManager.StateChangeEventId, this, this.StateChange);
        this.childStates.Clear();
        this.childStates = null;
        this.StateManager = null;
        this.Tag = null;
    }

    /// <summary>
    /// Handles StateChange events for this <see cref="GameScreen"/>.
    /// </summary>
    /// <param name="eventData">The <see cref="EventData"/> for this event.</param>
    protected internal virtual void StateChange(EventData eventData)
    {
        if (this.StateManager.CurrentState == this.Tag)
        {
            if (!this.IsInitialized)
            {
                this.Initialize();
            }

            this.Show();
        }
        else
        {
            this.Hide();
        }
    }

    /// <summary>
    /// Shows the state.
    /// </summary>
    protected virtual void Show()
    {
        this.Visible = true;
        this.IsEnabled = true;
        foreach (GameScreen state in this.childStates)
        {
            state.IsEnabled = true;
            state.Visible = true;
        }
    }

    /// <summary>
    /// Hides the state.
    /// </summary>
    protected virtual void Hide()
    {
        this.Visible = false;
        this.IsEnabled = false;
        foreach (GameScreen state in this.childStates)
        {
            state.IsEnabled = false;
            state.Visible = false;
        }
    }
}