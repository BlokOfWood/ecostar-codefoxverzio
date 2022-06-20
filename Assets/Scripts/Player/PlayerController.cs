using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Entity PlayerEntity;

    public InputSettings Controls;

    [Tooltip("The series of Abilities avaliable to the player.")]
    public List<Ability> UnlockedAbilities;

    ///<summary>Reference to the Animator Component.</summary>
    Animator _animator;
    ///<summary>Reference to the Rigidbody Component.</summary>
    Rigidbody _rigidbody;
    ///<summary>Reference to the dialogue component.</summary>
    DialoguePlayer _dialoguePlayer;

    [Tooltip("The pivot object the camera orbits.")]
    public GameObject CameraPivot;
    ///<summary>Horizontal controls axis, used for movement.</summary>
    [HideInInspector] public float HorAxis;
    ///<summary>Vertical controls axis, used for movement.</summary>
    [HideInInspector] public float VerAxis;

    [Header("Jump")]
    [HideInInspector] public bool RestrictedMovement = false;
    ///<summary>It signifies whether the player has normal jumped yet. Used to determine when the double jump should be triggered.</summary>
    [HideInInspector] public bool Jumped = false;
    ///<summary>The y velocity with which the player can still jump, needed because the y velocity might be very small but still non-zero even when stationary.</summary>
    public float JumpSensitivity;
    [Tooltip("The distance from ground for which the Jump End animation triggers.")]
    public float NearImpact;
    [Tooltip("The amount that the gravity is multiplied by when the player is falling. This is done for better feeling jumping.")]
    public float FallGravMultiplier;
    [Tooltip("The base gravity that the game starts on. Needed to be able to reverse gravity when the player is not falling anymore.")]
    float _baseGravity;

    [Header("Movement")]
    [Tooltip("The length of the raycast that determines if the player is up against a wall.")]
    public float WallSensitivity;
    [Tooltip("Time variable used in linear interpolating the movement.")]
    public float LerpTime;

    [Header("Grab")]
    [Tooltip("The length of the raycast when determining if there is a grabbable ledge.")]
    public float GrabRange;
    ///<summary>The position the player will be after the climb happens.</summary>
    Vector3 _afterClimbPos;


    [Header("Pickup Object")]
    [Tooltip("The range from the camera where the player can pick up an object.")]
    public float PickupRange;
    [Tooltip("Game object that the carried object is parented to.")]
    public GameObject PickupObjectParent;
    ///<summary>The reference to the currently picked up object.</summary>
    GameObject _carriedObject;
    ///<summary>The previous transform of the object that was picked up.</summary>
    Transform _previousTransform = null;

    [Header("Push/Pull")]
    public float MoveRange;
    GameObject _movedObject = null;

    [Header("Combat")]
    public GameObject weapon;

    private void Awake()
    {
        Time.fixedDeltaTime = (float)1 / Screen.currentResolution.refreshRate;
        _baseGravity = Physics.gravity.y;


        //Setting up Input Callbacks.
        Controls = new InputSettings();
        Controls.Enable();


        //If the camera is out of range, then it gets stuck, so setting it to a pre-selected good value makes it so that it doesn't start stuck.
        CameraPivot.transform.eulerAngles = new Vector3(340, 0, 0);


        //Locks the cursor to the window.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;


        //Gets references for components into variables for shorter lines.
        _animator = GetComponent<Animator>();
        _rigidbody =  GetComponent<Rigidbody>();
        _dialoguePlayer = GetComponent<DialoguePlayer>();


        //Initializes each ability currently on the player.
        foreach(Ability i in UnlockedAbilities)
        {
            i.initialize(gameObject);
        }
    }

    private void FixedUpdate()
    {
        //Uses a raycast do determine the object the player wants to interact with. 
        //~(1 << 8) is a bitmask excluding the player from the raycast, because that made actually picking up stuff difficult.
        RaycastHit _raycastHit = new RaycastHit();
        bool _hit = Physics.Raycast(CameraPivot.transform.position, CameraPivot.transform.forward, out _raycastHit, PickupRange, ~(1 << 8));

        /*
         * Interact priority list (Highest Priority to Lowest)
         * Triggering Dialogue
         * Picking Up Object, Push/Pull Object
         */
        if (Controls.Player.Interact.triggered)
        {
            if (!_dialoguePlayer.TriggerDialogue())
            {
                //If there is no carried object we can make the assumption that the player might be trying to pick one up.
                if (!_carriedObject && !_movedObject)
                {
                    ///<summary>Has to only do stuff when the raycast hit, otherwise the thrown null reference exception would kill the frame it occured on.</summary>
                    if (_hit && _raycastHit.transform.gameObject.CompareTag("Grabbable"))
                    {
                        //If the targeted object is tagged as grabbable then they will be moved under a parent game object, where they are stored.

                        _carriedObject = _raycastHit.transform.gameObject;
                        _carriedObject.transform.parent = PickupObjectParent.transform;
                        _carriedObject.transform.localPosition = Vector3.zero;

                        foreach (Collider i in _carriedObject.GetComponents<Collider>())
                        {
                            i.enabled = false;
                        }
                        _carriedObject.GetComponent<Rigidbody>().useGravity = false;
                        _carriedObject.GetComponent<Rigidbody>().isKinematic = true;
                    }

                    _hit = Physics.Raycast(transform.position, transform.forward, out _raycastHit, PickupRange, ~(1 << 8));
                    if(_hit && _raycastHit.transform.CompareTag("Moveable"))
                    {
                        _movedObject = _raycastHit.transform.gameObject;
                        _movedObject.transform.parent = transform;

                        if(_movedObject.GetComponent<Rigidbody>())
                        {
                            _movedObject.GetComponent<Rigidbody>().isKinematic = true;
                        }

                        foreach (Collider i in _movedObject.GetComponentsInChildren<Collider>(true))
                        {
                            i.enabled = false;
                        }

                        RestrictedMovement = true;
                    }
                }
                //If there is no reason to suspect otherwise, when the player presses the interact button while carrying an object we can assume that he is trying to put it down.
                else
                {
                    if (_carriedObject != null)
                    {
                        _carriedObject.transform.parent = _previousTransform;
                        foreach (Collider i in _carriedObject.GetComponentsInChildren<Collider>(true))
                        {
                            i.enabled = true;
                        }
                        _carriedObject.GetComponent<Rigidbody>().useGravity = true;
                        _carriedObject.GetComponent<Rigidbody>().isKinematic = false;
                        _carriedObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
                        _carriedObject = null;
                    }
                    else if(_movedObject != null)
                    {
                        _movedObject.transform.parent = null;

                        foreach (Collider i in _movedObject.GetComponents<Collider>())
                        {
                            i.enabled = true;
                        }

                        if (_movedObject.GetComponent<Rigidbody>())
                        {
                            _movedObject.GetComponent<Rigidbody>().isKinematic = false;
                        }

                        _movedObject = null;
                        RestrictedMovement = false;
                    }
                }
            }
        }

        /*Handling exiting*/
        if (Controls.Player.Exit.triggered)
        {
            GetComponent<DialoguePlayer>().LeaveBox();
        }

        /*Gets Input*/
        bool _pressedJump = Controls.Player.Jump.triggered;
        HorAxis = Controls.Player.Movement.ReadValue<Vector2>().x;
        VerAxis = Controls.Player.Movement.ReadValue<Vector2>().y;


        /* Changing gravity when falling */
        if (_rigidbody.velocity.y < 0)
        {
            Physics.gravity = new Vector3(0, _baseGravity * FallGravMultiplier, 0);
        }
        else if(_rigidbody.velocity.y >= 0)
        {
            Physics.gravity = new Vector3(0, _baseGravity, 0);
        }


        /* Grab */
        //Checks if the character is falling, because that is when we want the character to actually grab.
        if (_rigidbody.velocity.y > 0)
        {
            RaycastHit[] _grabRaycastHits = Physics.RaycastAll(transform.position, transform.forward, GrabRange);
            foreach (RaycastHit i in _grabRaycastHits)
            {
                if (i.collider.isTrigger && !i.collider.gameObject.CompareTag("NPC"))
                {
                    if (_animator.GetCurrentAnimatorClipInfo(0)[0].clip.name != "Climbing")
                    {
                        _afterClimbPos = i.collider.ClosestPoint(transform.position);
                        _afterClimbPos.y = i.transform.position.y;
                        _afterClimbPos += i.collider.gameObject.GetComponents<BoxCollider>()[0].bounds.extents.y * Vector3.up;
                        _afterClimbPos += GetComponent<Collider>().bounds.extents.y * Vector3.up;
                    }
                    transform.position = _afterClimbPos+Vector3.down*3f;
                    _afterClimbPos = GameObject.Find("mixamorig:Hips").transform.position;

                    _rigidbody.velocity = Vector3.zero;
                    _rigidbody.useGravity = false;

                    _animator.SetTrigger("Climb");
                }
            }
        }
        else if(_animator.GetAnimatorTransitionInfo(0).IsUserName("ClimbLeave") && !_rigidbody.useGravity)
        {
            _rigidbody.useGravity = true;

            transform.position += GameObject.Find("mixamorig:Hips").transform.position - _afterClimbPos + transform.forward;
        }


        /*Abilities*/
        //Calls the tick function of every ability on this object.
        foreach (Ability ability in UnlockedAbilities)
        {
            ability.tick(Time.fixedDeltaTime);
        }


        /* Jump */
        if (_pressedJump && !inAir())
        {
            jump();
            Jumped = true;
        }
        else if (!inAir() && _rigidbody.velocity.y < JumpSensitivity)
        {
            Jumped = false;
        }


        /*Forward Movement Calculation*/
        Vector3 _deltaForward = new Vector3(CameraPivot.transform.forward.x, 0, CameraPivot.transform.forward.z) * VerAxis;
        Vector3 _deltaRight = new Vector3(CameraPivot.transform.right.x, 0, CameraPivot.transform.right.z) * HorAxis;


        /* Player Movement */
        if(!_animator.GetAnimatorTransitionInfo(0).IsUserName("ClimbLeave") && _animator.GetCurrentAnimatorClipInfo(0)[0].clip.name != "Climbing")
        move(_deltaForward + _deltaRight);
    

        /*Animation triggers*/
        _animator.SetFloat("Movement", Vector3.Distance(Vector3.zero, new Vector3(HorAxis, VerAxis)));
        if (_pressedJump && !RestrictedMovement)
            _animator.SetBool("Jump", true);
        else if (Physics.RaycastAll(transform.position, transform.up * -1, NearImpact).Where(x => x.transform != transform).Count() != 0 && _rigidbody.velocity.y < 0 )
            _animator.SetBool("Jump", false);

        /* TEST ATTACK */
        if(Controls.Player.Attack.triggered)
        {
            weapon.GetComponent<Animator>().Play("swing");
        }
    }

    /// <summary>Detects if the player is up against the wall in a direction. (Used to avoid actually being able to start walking into walls.)</summary>
    /// <param name="direction">The direction that we want to check if the player is up against a wall. (usually the direction the player is facing)</param>
    /// <returns>Is the player up to a wall.</returns>
    bool detectWall(Vector3 direction)
    {
        RaycastHit[] _hits = Physics.RaycastAll(transform.position, direction, GetComponent<CapsuleCollider>().radius + WallSensitivity);
        foreach (RaycastHit i in _hits)
        {
            if (i.transform == transform) continue;
            if (i.collider.isTrigger == true) continue;
            return true;
        }

        return false;
    }

    public void jump()
    {
        if (RestrictedMovement) return;
        //Clears out the y axis velocity, for use with the double jump, normally because the jump is launched from the ground it doesn't mess up anything.
        _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, PlayerEntity.JumpForce, _rigidbody.velocity.z);
    }

    public void move(Vector3 _direction)
    {
        /* Changing the forward direction of the player */
        if (HorAxis != 0 || VerAxis != 0)
        {
           _rigidbody.MoveRotation(Quaternion.Euler(new Vector3(0, Quaternion.FromToRotation(Vector3.forward, _direction).eulerAngles.y, 0)));
        }

        //Calculates the delta of the movement.
        Vector3 _deltaMovement = _direction;
        _deltaMovement *= PlayerEntity.MovementSpeed * Time.fixedDeltaTime;
        
        Vector3 _movePosition = Vector3.Lerp(transform.position, transform.position + _deltaMovement, LerpTime);

        if (!detectWall(_direction))
            _rigidbody.MovePosition(_movePosition);

        if (!_movedObject) return;
        _movePosition = Vector3.Lerp(_movedObject.transform.position, _movedObject.transform.position + _deltaMovement, LerpTime);
        _movedObject.GetComponent<Rigidbody>().MovePosition(_movePosition);
    }

    public bool inAir()
    {
        bool inAir = Mathf.Abs(_rigidbody.velocity.y) > JumpSensitivity;
        return inAir;
    }
}