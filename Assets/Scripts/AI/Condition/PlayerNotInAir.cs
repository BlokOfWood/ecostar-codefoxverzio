using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerNotInAir : AICondition
{
    public override bool check(AIController controller)
    {
        return !controller.Player.GetComponent<PlayerController>().inAir();
    }
}
