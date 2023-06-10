﻿namespace ZxenLib.GameScreen;

using System.Collections.Generic;
using System.Linq;
using DependencyInjection;
using Microsoft.Xna.Framework.Graphics;
using ZxenLib.Events;

/// <summary>
/// Manages the current game screen/state.
/// </summary>
public class GameScreenManager
{
    /// <summary>
    /// Defines the event id of the state changed event.
    /// </summary>
    public const string StateChangeEventId = "OnStateChange";

    private const int StartDrawOrder = 1000;
    private const int DrawOrderInc = 100;
    private readonly Stack<GameScreen> gameStates;
    private readonly IEventDispatcher eventDispatcher;
    private int drawOrder;

    public readonly DependencyContainer ServiceContainer;

    /// <summary>
    /// Initializes a new instance of the <see cref="GameScreenManager"/> class.
    /// </summary>
    /// <param name="eventDispatcher">The <see cref="IEventDispatcher"/> dependency.</param>
    /// <param name="serviceContainer">The <see cref="DependencyContainer"/> dependency.</param>
    public GameScreenManager(IEventDispatcher eventDispatcher, DependencyContainer serviceContainer)
    {
        this.drawOrder = StartDrawOrder;
        this.gameStates = new();
        this.eventDispatcher = eventDispatcher;
        this.ServiceContainer = serviceContainer;
    }

    /// <summary>
    /// Gets the current state.
    /// </summary>
    public GameScreen CurrentState
    {
        get => this.gameStates.Peek();
    }

    /// <summary>
    /// Performs initialization.
    /// </summary>
    public virtual void Initialize()
    {
    }

    public void RegisterScreenType<T>() where T : GameScreen
    {
        this.ServiceContainer.Register<T>();
    }

    /// <summary>
    /// Checks for the top-most state and removes it.
    /// </summary>
    public void PopState()
    {
        if (this.gameStates.Count > 0)
        {
            GameScreen removedState = this.RemoveState();
            this.drawOrder -= DrawOrderInc;
            this.eventDispatcher.Publish(new EventData()
            {
                EventId = StateChangeEventId,
                Sender = this,
                TargetObjectId = removedState.Id,
                EventArguments = null
            });
        }
    }

    /// <summary>
    /// Adds a new state.
    /// </summary>
    /// <param name="newState">New state being added.</param>
    public void PushState(GameScreen newState)
    {
        this.drawOrder += DrawOrderInc;
        newState.DrawOrder = this.drawOrder;
        this.AddState(newState);
    }

    /// <summary>
    /// Updates the currently active state.
    /// </summary>
    /// <param name="deltaTime">The elapsed time of the previous frame.</param>
    public void UpdateStates(float deltaTime)
    {
        foreach (GameScreen state in this.gameStates)
        {
            if (state.IsInitialized && state.IsEnabled)
            {
                state.Update(deltaTime);
            }
        }
    }

    /// <summary>
    /// Performs draw call batching for currently active state. Called every frame.
    /// </summary>
    /// <param name="sb">The <see cref="SpriteBatch"/> used for batching draw calls.</param>
    public void DrawStates()
    {
        List<GameScreen> drawOrderedList = this.gameStates.OrderBy(x => x.DrawOrder).ToList();

        foreach (GameScreen state in drawOrderedList)
        {
            if (state.IsInitialized && state.IsEnabled && state.Visible)
            {
                state.Draw();
            }
        }
    }

    /// <summary>
    /// Changes the game state to the provided new state.
    /// </summary>
    /// <param name="newState"><see cref="GameScreen"/> being added to the manager.</param>
    public void ChangeState(GameScreen newState, bool registerWithContainer = false)
    {
        if (registerWithContainer && newState != null)
        {
            this.ServiceContainer.Register(newState);
        }

        while (this.gameStates.Count > 0)
        {
            this.RemoveState();
        }

        newState.DrawOrder = StartDrawOrder;
        this.drawOrder = StartDrawOrder;
        this.AddState(newState);
    }

    public void ChangeState<T>() where T : GameScreen
    {
        GameScreen resolvedGameScreen = this.ServiceContainer.Resolve<T>();
        this.ChangeState(resolvedGameScreen);
    }

    /// <summary>
    /// Removes state from the top of the stack, unregisters state from listener.
    /// </summary>
    /// <returns><see cref="GameScreen"/>.</returns>
    private GameScreen RemoveState()
    {
        GameScreen state = this.gameStates.Peek();
        state.ClearState();
        this.gameStates.Pop();

        return state;
    }

    /// <summary>
    /// Adds new state to the top of the stack.
    /// </summary>
    /// <param name="newState"><see cref="GameScreen"/> being added to the manager.</param>
    private void AddState(GameScreen newState)
    {
        this.gameStates.Push(newState);
        if (!newState.IsInitialized)
        {
            newState.Initialize();
        }

        if (!newState.IsEnabled)
        {
            newState.IsEnabled = true;
        }

        this.eventDispatcher.Publish(new EventData()
        {
            EventId = StateChangeEventId,
            Sender = this,
            TargetObjectId = newState.Id,
            EventArguments = null
        });
    }
}