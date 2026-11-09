using System;
using System.Collections.Generic;

public abstract class State
{
    public virtual void Enter() { }

    public virtual void Tick(float deltaTime) { }

    public virtual void Exit() { }
}

public class StateMachine
{
    private readonly struct Transition
    {
        public readonly Type TargetType;
        public readonly Func<State, bool> Condition;
        public readonly Func<State> GetTargetState;

        public Transition(Type targetType, Func<State, bool> condition, Func<State> getTargetState)
        {
            TargetType = targetType;
            Condition = condition;
            GetTargetState = getTargetState;
        }
    }

    private readonly Dictionary<Type, List<Transition>> transitions = new();
    private readonly List<Transition> anyTransitions = new();
    private State currentState;

    public void AddTransition<TFrom, TTo>(Func<TFrom, bool> condition, Func<TTo> getTargetState)
        where TFrom : State
        where TTo : State
    {
        if (condition == null)
        {
            throw new ArgumentNullException(nameof(condition),
                $"Transition {typeof(TFrom).Name} -> {typeof(TTo).Name} needs a condition.");
        }
        if (getTargetState == null)
        {
            throw new ArgumentNullException(nameof(getTargetState),
                $"Transition {typeof(TFrom).Name} -> {typeof(TTo).Name} needs a way to get its target state.");
        }

        if (!transitions.TryGetValue(typeof(TFrom), out List<Transition> list))
        {
            list = new List<Transition>();
            transitions[typeof(TFrom)] = list;
        }
        list.Add(new Transition(typeof(TTo), state => condition((TFrom)state), getTargetState));
    }

    public void AddAnyTransition<TTo>(Func<bool> condition, Func<TTo> getTargetState)
        where TTo : State
    {
        if (condition == null)
        {
            throw new ArgumentNullException(nameof(condition),
                $"Any-state transition to {typeof(TTo).Name} needs a condition.");
        }
        if (getTargetState == null)
        {
            throw new ArgumentNullException(nameof(getTargetState),
                $"Any-state transition to {typeof(TTo).Name} needs a way to get its target state.");
        }

        anyTransitions.Add(new Transition(typeof(TTo), _ => condition(), getTargetState));
    }

    public void Tick(float deltaTime)
    {
        if (currentState == null)
        {
            return;
        }

        if (TryFindTransition(out Transition transition))
        {
            State targetState = transition.GetTargetState();
            if (targetState == null)
            {
                throw new InvalidOperationException(
                    $"Transition from {currentState.GetType().Name} to {transition.TargetType.Name} returned a null target state.");
            }

            GoToState(targetState);
        }

        currentState.Tick(deltaTime);
    }

    public void GoToState(State state)
    {
        if (state == null)
        {
            throw new ArgumentNullException(nameof(state), "Cannot go to a null state.");
        }

        currentState?.Exit();
        currentState = state;
        currentState.Enter();
    }

    public void TearDown()
    {
        currentState?.Exit();
        currentState = null;
    }

    private bool TryFindTransition(out Transition found)
    {
        Type currentType = currentState.GetType();

        foreach (Transition transition in anyTransitions)
        {
            if (transition.TargetType != currentType && transition.Condition(currentState))
            {
                found = transition;
                return true;
            }
        }

        if (transitions.TryGetValue(currentType, out List<Transition> list))
        {
            foreach (Transition transition in list)
            {
                if (transition.Condition(currentState))
                {
                    found = transition;
                    return true;
                }
            }
        }

        found = default;
        return false;
    }
}
