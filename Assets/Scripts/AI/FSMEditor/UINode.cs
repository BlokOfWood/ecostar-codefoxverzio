using System;
using UnityEngine;

[Serializable]
public class UINode
{
    public AIState ContainedState;
    public Rect Rect;
    public Vector2 Offset = Vector2.zero;

    public UINode(AIState containedState, Rect rect)
    {
        ContainedState = containedState;
        Rect = rect;
    }
}