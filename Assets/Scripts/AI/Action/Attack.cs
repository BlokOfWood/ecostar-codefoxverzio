using UnityEngine;

public class Attack : AIAction
{
    Entity _AIStats;
    float _cooldown = -1;
    int _damage = -1;

    public void initialize(Entity AIStats)
    {
        _AIStats = AIStats;
    }

    public void execute(AIController controller, float deltaTime)
    {
        if (_cooldown <= 0)
        {
            if (_AIStats.AttackCooldown == -1)
            {
                Debug.LogError("No attack cooldown set in the Entity file given!");
            }
            else
            {
                _cooldown = _AIStats.AttackCooldown;
            }
        }
        if(_damage == -1)
        {

            if (controller.AIStats.Damage == -1)
            {
                Debug.LogError("No attack damge set in the Entity file given!");
            }
            else
            {
                _damage = _AIStats.Damage;
            }
        }
    }
}
