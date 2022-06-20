using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "Abilities/Create Run Ability")]
public class Dash : Ability
{
    ///<summary>The speed multiplier while <c>Dashing</c>.</summary>
    public float RunSpeedMultiplier;
    ///<summary>The amount the sensitivity is divided with to reduce it.</summary>
    public float SensitivityDivisor;

    public override void initialize(GameObject gameObject)
    {
        base.initialize(gameObject);

        //Sets the trigger statement.
        _triggerStatement = () =>
        {
            if (_player_ref.Controls.Player.Dash.phase == InputActionPhase.Started && !_player_ref.RestrictedMovement) 
                return true;
            return false;
        };
    }
    protected override byte activate()
    {
        //Handles the error code of the base object.
        byte _activation_result = base.activate();
        if (_activation_result != 0) return _activation_result;

        //Speeds up the character and reduces look sensitivity.
        _player_ref.PlayerEntity.MovementSpeed *= RunSpeedMultiplier;
        _camera_ref.LookSensitivity /= SensitivityDivisor;

        return 0;
    }

    protected override byte deactivate()
    {
        //Handles the error code of the base object.
        byte _deactivation_result = base.deactivate();
        if (_deactivation_result != 0) return _deactivation_result;

        //Reverses the effects of the Activate function.
        _player_ref.PlayerEntity.MovementSpeed /= RunSpeedMultiplier;
        _camera_ref.LookSensitivity *= SensitivityDivisor;

        return 0;
    }

    public override void tick(float deltaTime) 
    {
        base.tick(deltaTime);

        if (Active)
        {
            _player_ref.HorAxis = 0;
            _player_ref.VerAxis = 1;
        }
    }
}