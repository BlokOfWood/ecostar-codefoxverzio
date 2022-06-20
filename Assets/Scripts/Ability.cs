using System;
using UnityEngine;
public abstract class Ability : ScriptableObject
{
    ///<summary>The attached player game object</summary>
    protected GameObject _attachedPlayer;
    ///<summary>The lambda function that decides if the ability is activated</summary>
    protected Func<bool> _triggerStatement = null;
    ///<summary>Reference to the PlayerController component for easy access</summary>
    protected PlayerController _player_ref;
    ///<summary>Reference to the PlayerController component for easy access</summary>
    protected CinemachineController _camera_ref;
    ///<summary>Determines if the ability is currently active, should be set forever to false if the ability is not duration based.</summary>
    [HideInInspector] public bool Active = false;
    [Tooltip("The range of the ability")]
    public float Range;
    [Tooltip("The base cooldown of the ability")]
    public float BaseCooldown = 0;
    [Tooltip("The base duration of the ability")]
    public float BaseDuration = 0;
    ///<summary>The time left from the cooldown</summary>
    float _currCooldown = -5000;
    ///<summary>The time left from the duration</summary>
    float _currDuration = -5000;

    ///<summary>Initializes the required variables and checks for being on an incorrect object</summary>
    ///<param name="gameObject">The game object that the ability is on.</param>
    public virtual void initialize(GameObject gameObject)
    {
        _attachedPlayer = gameObject;
        if(!_attachedPlayer.GetComponent<PlayerController>())
        {
            Debug.LogError("Ability called from a non-native character! VERY BAD");
            return;
        }
        _player_ref = _attachedPlayer.GetComponent<PlayerController>();
        _camera_ref = _player_ref.CameraPivot.transform.parent.GetComponent<CinemachineController>();
    }

    ///<summary>The function for activating a given ability.</summary> 
    protected virtual byte activate()
    {
        /*
         Returns error codes.
         1: Cooldown is not down.
         2: The ability is already active, it cannot be reactivated.
         */
        if (_currCooldown > 0)
        {
            return 1;
        }
        if (Active)
        {
            Debug.LogError("Ability is already active, but Activate was called!");
            return 2;
        }

        Active = true;

        return 0;
    }

    ///<summary>The function for deactivating a given ability.</summary> 
    protected virtual byte deactivate()
    {
        /*
         Returns error codes.
         2: The ability is already active, it cannot be reactivated.
         */
        if (!Active)
        {
            Debug.LogError("Ability is not active, but Deactivate was called!");
            return 2;
        }
        Active = false;

        return 0;
    }

    ///<summary>The internal tick function of the Ability.</summary>
    ///<param name="deltaTime">The amount of time that has passed since the last activation, used for durations and cooldowns.</param>
    public virtual void tick(float deltaTime)
    {
        //Progresses the internal timers of the ability object.
        if (_currCooldown > 0) _currCooldown -= deltaTime;
        if (_currDuration != -5000)
        {
            if (_currDuration > 0) _currDuration -= deltaTime;
            else deactivate();
        }

        if (_triggerStatement != null)
        {
            bool trigger = _triggerStatement.Invoke();

            if (trigger && !Active)
            {
                activate();
            }
            else if (!trigger && Active)
            {
                deactivate();
            }
        }
    }
}   