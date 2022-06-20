using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AICondition
{
    public abstract bool check(AIController controller);
}