using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AIObject : ScriptableObject
{
    public AIState[] StateMachine;
    public UINode[] EditorInfo;

    public AIObject(List<UINode> nodes)
    {
        EditorInfo = nodes.ToArray();
        StateMachine = nodes.ConvertAll(x => x.ContainedState).ToArray();
    }
}
