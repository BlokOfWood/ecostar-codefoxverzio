using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAction1 : AIAction
{
    public void initialize(Entity AIStats)
    {
        return;
    }

    public void execute(AIController controller, float deltaTime)
    {
        Debug.Log("TestAction1");
    }
}