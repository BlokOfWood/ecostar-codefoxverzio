using System;
using System.Linq;
using UnityEngine;

[Serializable]
public class AITransition
{
    public byte ToID;
    public int Condition;
    public AICondition ConditionInstance;

    public AITransition(byte _ToID, int _Condition)
    {
        ToID = _ToID;
        Condition = _Condition;
    }

    public void Initialize()
    {
        ConditionInstance = (AICondition)Activator.CreateInstance
            (
                Type.GetType
                (
                    ClassRefs.AIConditions.Values.ToArray()[Condition]
                )
            );
    }
}
 
           