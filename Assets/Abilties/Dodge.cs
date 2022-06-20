using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Create Dodge Ability")]
public class Dodge : Ability
{
    [Tooltip("The amount that the speed is multiplied by while dodging.")]
    public float DodgeSpeedMultiplier;

    public override void initialize(GameObject gameObject)
    {
        base.initialize(gameObject);

        _triggerStatement = () => {
            if (_player_ref.Controls.Player.Dodge.triggered && !_player_ref.RestrictedMovement)
                return true;
            return false;
        };
    }

    protected override byte activate()
    {
        _player_ref.HorAxis *= DodgeSpeedMultiplier;
        _player_ref.VerAxis *= DodgeSpeedMultiplier;
        return 0;        
    }

    protected override byte deactivate()
    {
        Debug.LogError("Shouldn't be called on this ability");
        return 1;
    }
}