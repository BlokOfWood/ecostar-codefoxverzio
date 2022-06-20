using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyToPlayer : AIAction
{
    public void initialize(Entity AIStats)
    {
        throw new System.NotImplementedException();
    }

    public void execute(AIController controller, float deltaTime)
    {
        Vector3 lookAtPosition = controller.Player.transform.position;
        controller.transform.LookAt(lookAtPosition);

        controller.transform.position
            += controller.transform.forward * controller.AIStats.MovementSpeed * deltaTime;
    }
}
