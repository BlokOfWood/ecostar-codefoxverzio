using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class AIState
{
    public List<int> EnterActions = new List<int>();
    public List<int> TickActions = new List<int>();
    public List<int> ExitActions = new List<int>();
    public List<AITransition> Transitions = new List<AITransition>();

    private AIAction[] _enterActions;
    private AIAction[] _tickActions;
    private AIAction[] _exitActions;
    private AIController _owner;

    public byte ID;
    public string Name = "";

    public AIState(string name, byte id)
    {
        Name = name;
        ID = id;
    }

    public void Initialize(AIController owner)
    {
        //Gets a local reference to the owner component so it doesn't have to be passed through every function call.
        _owner = owner;
        
        _enterActions = EnterActions.Count > 0 ? (new AIAction[EnterActions.Count]) : null;
        _tickActions = TickActions.Count > 0 ? (new AIAction[TickActions.Count]) : null;
        _exitActions = ExitActions.Count > 0 ? (new AIAction[ExitActions.Count]) : null;

        if (_enterActions != null)
        {
           
            for (int i = 0; i < _enterActions.Length; i++)
            {
                //The 0th Action is an empty one used for the purposes of there being a no selection state for the dropdown menu.
                //As it is empty, it cannot be cast to a class.
                if (EnterActions[i] == 0) continue;
                _enterActions[i] = (AIAction)Activator.CreateInstance
                (
                    Type.GetType
                    (
                        ClassRefs.AIActions.Values.ToArray()[EnterActions[i]]
                    )
                );
            }
        }

        if (_tickActions != null)
        {
            //Refer to the comment above
            for (int i = 0; i < _tickActions.Length; i++)
            {
                if (TickActions[i] == 0) continue;
                _tickActions[i] = (AIAction)Activator.CreateInstance
                (
                    Type.GetType
                    (
                        ClassRefs.AIActions.Values.ToArray()[TickActions[i]]
                    )
                );
            }
        }

        if (_exitActions != null)
        {
            //Refer to the comment above
            for (int i = 0; i < _exitActions.Length; i++)
            {
                if (ExitActions[i] == 0) continue;
                _exitActions[i] = (AIAction)Activator.CreateInstance
                (
                    Type.GetType
                    (
                        ClassRefs.AIActions.Values.ToArray()[ExitActions[i]]
                    )
                );
            }
        }

        for (int x = 0; x < Transitions.Count; x++)
        {
            Transitions[x].Initialize();
        }
    }

    public void Enter()
    {
        if (_enterActions == null) return;
        foreach (AIAction i in _enterActions)
        {
            i.execute(_owner, 0);
        }
    }

    public void Tick(float deltaTime)
    {
        if (_tickActions == null) return;
        foreach (AIAction i in _tickActions)
        {
            i.execute(_owner, deltaTime);
        }
    }

    public void Exit()
    {
        if (_exitActions == null) return;
        foreach (AIAction i in _exitActions)
        {
            i.execute(_owner, 0);
        }
    }

    public int CheckTransitions()
    {
        for (int i = 0; i < Transitions.Count; i++)
        {
            if (Transitions[i].ConditionInstance.check(_owner))
            {
                return Transitions[i].ToID;
            }
        }
        return -1;
    }
}