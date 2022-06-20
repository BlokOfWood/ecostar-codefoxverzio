using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : AIAction
{
    Entity _AIStats;

    public void initialize(Entity AIStats)
    {
        _AIStats = AIStats;
    }

    public void execute(AIController controller, float deltaTime)
    {
        Vector3 lookAtPosition = controller.Player.transform.position;
        lookAtPosition.y = controller.transform.position.y;
        controller.transform.LookAt(lookAtPosition);

        controller.transform.position 
            += controller.transform.forward * _AIStats.MovementSpeed * deltaTime;
    }
}
