using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Create Double Jump Ability")]
public class DoubleJump : Ability
{
    public override void initialize(GameObject gameObject)
    {
        base.initialize(gameObject);

        _triggerStatement = () =>
        {
            if (!Active && _player_ref.Jumped && _player_ref.Controls.Player.Jump.triggered)
            {
                return true;
            }
            return false;
        };
    }

    public override void tick(float deltaTime)
    {
        if (_triggerStatement != null)
        {
            if (!_player_ref.inAir())
            {
                Active = false;
            }
            else
            {
                bool trigger = _triggerStatement.Invoke();
                if (trigger)
                {
                    activate();
                }
            }
        }
    }

    protected override byte activate()
    {
        _player_ref.jump();
        Active = true;

        return 0;
    }
}