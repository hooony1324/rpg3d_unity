using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public abstract class MonoStateMachine<EntityType> : MonoBehaviour
{
    public delegate void StateChangedHandler(StateMachine<EntityType> stateMachine,
        State<EntityType> newState,
        State<EntityType> prevState,
        int layer);

    private readonly StateMachine<EntityType> stateMachine = new();

    public event StateChangedHandler onStateChanged;

    public EntityType Owner => stateMachine.Owner;

    private void Update()
    {
        if (Owner != null)
            stateMachine.Update();
    }

    public void Setup(EntityType owner)
    {
        stateMachine.Setup(owner);

        AddStates();
        MakeTransitions();

        stateMachine.SetupLayers();

        stateMachine.onStateChanged += (_, newState, prevState, layer)
            => onStateChanged?.Invoke(stateMachine, newState, prevState, layer);
    }

    public void AddState<T>(int layer = 0)
        where T : State<EntityType>
        => stateMachine.AddState<T>(layer);

    public void MakeTransition<FromStateType, ToStateType>(int transitionCommand,
        Func<State<EntityType>, bool> transitionCondition,
        int layer = 0)
        where FromStateType : State<EntityType>
        where ToStateType : State<EntityType>
        => stateMachine.MakeTransition<FromStateType, ToStateType>(transitionCommand, transitionCondition, layer);

    public void MakeTransition<FromStateType, ToStateType>(Enum transitionCommand,
        Func<State<EntityType>, bool> transitionCondition,
        int layer = 0)
        where FromStateType : State<EntityType>
        where ToStateType : State<EntityType>
        => stateMachine.MakeTransition<FromStateType, ToStateType>(transitionCommand, transitionCondition, layer);

    public void MakeTransition<FromStateType, ToStateType>(
        Func<State<EntityType>, bool> transitionCondition, int layer = 0)
        where FromStateType : State<EntityType>
        where ToStateType : State<EntityType>
        => stateMachine.MakeTransition<FromStateType, ToStateType>(int.MinValue, transitionCondition, layer);

    public void MakeTransition<FromStateType, ToStateType>(int transitionCommand, int layer = 0)
        where FromStateType : State<EntityType>
        where ToStateType : State<EntityType>
        => stateMachine.MakeTransition<FromStateType, ToStateType>(transitionCommand, null, layer);

    public void MakeTransition<FromStateType, ToStateType>(Enum transitionCommand, int layer = 0)
        where FromStateType : State<EntityType>
        where ToStateType : State<EntityType>
        => stateMachine.MakeTransition<FromStateType, ToStateType>(transitionCommand, null, layer);

    public void MakeAnyTransition<ToStateType>(int transitionCommand,
        Func<State<EntityType>, bool> transitionCondition, int layer = 0, bool canTransitionToSelf = false)
        where ToStateType : State<EntityType>
        => stateMachine.MakeAnyTransition<ToStateType>(transitionCommand, transitionCondition, layer, canTransitionToSelf);

    public void MakeAnyTransition<ToStateType>(Enum transitionCommand,
        Func<State<EntityType>, bool> transitionCondition, int layer = 0, bool canTransitionToSelf = false)
        where ToStateType : State<EntityType>
        => stateMachine.MakeAnyTransition<ToStateType>(transitionCommand, transitionCondition, layer, canTransitionToSelf);

    public void MakeAnyTransition<ToStateType>(Func<State<EntityType>, bool> transitionCondition,
        int layer = 0, bool canTransitionToSelf = false)
        where ToStateType : State<EntityType>
        => stateMachine.MakeAnyTransition<ToStateType>(int.MinValue, transitionCondition, layer, canTransitionToSelf);

    public void MakeAnyTransition<ToStateType>(Enum transitionCommand, int layer = 0, bool canTransitionToSelf = false)
        where ToStateType : State<EntityType>
        => stateMachine.MakeAnyTransition<ToStateType>(transitionCommand, null, layer, canTransitionToSelf);

    public void MakeAnyTransition<ToStateType>(int transitionCommand, int layer = 0, bool canTransitionToSelf = false)
    where ToStateType : State<EntityType>
        => stateMachine.MakeAnyTransition<ToStateType>(transitionCommand, null, layer, canTransitionToSelf);

    public bool ExecuteCommand(int transitionCommand, int layer)
        => stateMachine.ExecuteCommand(transitionCommand, layer);

    public bool ExecuteCommand(Enum transitionCommand, int layer)
        => stateMachine.ExecuteCommand(transitionCommand, layer);

    public bool ExecuteCommand(int transitionCommand)
        => stateMachine.ExecuteCommand(transitionCommand);

    public bool ExecuteCommand(Enum transitionCommand)
        => stateMachine.ExecuteCommand(transitionCommand);

    public bool SendMessage(int message, int layer, object extraData = null)
        => stateMachine.SendMessage(message, layer, extraData);

    public bool SendMessage(Enum message, int layer, object extraData = null)
        => stateMachine.SendMessage(message, layer, extraData);

    public bool SendMessage(int message, object extraData = null)
        => stateMachine.SendMessage(message, extraData);

    public bool SendMessage(Enum message, object extraData = null)
        => stateMachine.SendMessage(message, extraData);

    public bool IsInState<T>() where T : State<EntityType>
        => stateMachine.IsInState<T>();

    public bool IsInState<T>(int layer = 0) where T : State<EntityType>
        => stateMachine.IsInState<T>(layer);

    public State<EntityType> GetCurrentState(int layer = 0) => stateMachine.GetCurrentState(layer);

    public Type GetCurrentStateType(int layer = 0) => stateMachine.GetCurrentStateType(layer);

    protected abstract void AddStates();
    protected abstract void MakeTransitions();
}
