using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateTransition<EntityType>
{
    public const int kNullCommand = int.MinValue;

    private Func<State<EntityType>, bool> transitionCondition;
    public bool CanTransitionToSelf {  get; private set; }
    public State<EntityType> FromState { get; private set; }
    public State<EntityType> ToState { get; private set; }
    public int TransitionCommand { get; private set; }
    public bool IsTransferable => transitionCondition == null || transitionCondition.Invoke(FromState);

    public StateTransition(State<EntityType> fromState, 
        State<EntityType> toState,
        int transitionCommand,
        Func<State<EntityType>, bool> transitionCondition,
        bool canTransitionToSelf)
    {
        Debug.Assert(transitionCommand != kNullCommand || transitionCondition != null, "StateTransition - TransitionCommand와 TransitionCondition은 둘 다 null이 될 수 없습니다.");

        FromState = fromState;
        ToState = toState;
        TransitionCommand = transitionCommand;
        this.transitionCondition = transitionCondition;
        CanTransitionToSelf = canTransitionToSelf;
    }
}
