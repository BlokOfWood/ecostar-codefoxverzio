using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCondition : AICondition
{
    public override bool check(AIController owner)
    {
        //THIS IS ONLY A TEST
        return Input.GetKeyDown(KeyCode.W);
        
    }
}