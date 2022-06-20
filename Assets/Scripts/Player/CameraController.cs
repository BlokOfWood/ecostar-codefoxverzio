using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Tooltip("Used for dodging, when the player moves a great distance in a short time. This gives the speed multiplier of the player to the camera as well, so the camera doesn't lag behind.")]
    [HideInInspector] public float LerpTimeMultiplier = 1f;
    public float LookSensitivity = 50;
    [Tooltip("The upper rotation limit of the X axis. Should be above 360 probably.")]
    public float UpperXRotLimit;
    [Tooltip("The bottom rotation limit of the X axis.")]
    public float BottomXRotLimit;
    [Tooltip("The time value of the linear interpolation of the camera towards the player.")]
    public float PositionLerpTime;
    [Tooltip("Reference to the Player.")]
    public GameObject Player;
    [Tooltip("Reference to the Camera.")]
    public GameObject Camera;
    InputSettings _controls;

    void Start()
    {
        _controls = Player.GetComponent<PlayerController>().Controls;
        transform.position = Player.transform.position;
    }

    void FixedUpdate()
    {
        //Changes the movement by the multiplier given from another script. (probably when dodging).
        PositionLerpTime *= LerpTimeMultiplier;

        float _xDeltaRot = -_controls.Player.CameraRotation.ReadValue<Vector2>().y * Time.fixedDeltaTime * LookSensitivity;
        float _yDeltaRot = _controls.Player.CameraRotation.ReadValue<Vector2>().x * Time.fixedDeltaTime * LookSensitivity;

        if(Cursor.lockState == CursorLockMode.None)
        {
            _xDeltaRot = 0;
            _yDeltaRot = 0;
        }

        /* Camera Rotation */
        Vector3 _deltaRot = Vector3.up * _yDeltaRot;

        float _xNewRot = transform.localEulerAngles.x + _xDeltaRot;
        //Because Unity at 360 switches to 0 by adding 360 when the value is surely too small makes it easy to check if it is in the allowed range.
        if (_xNewRot < 90) _xNewRot += 360;

        //Checks if it is in the allowed range because if the rotation goes too far then the camera can flip over.
        if (_xNewRot < UpperXRotLimit && _xNewRot > BottomXRotLimit)
            _deltaRot += Vector3.right * _xDeltaRot;

        //Adds the calculated delta rotation to the pivot of the camera.
        transform.localEulerAngles += _deltaRot;

        //Camera Movement
        transform.position = Vector3.Lerp(transform.position, Player.transform.position, PositionLerpTime);

        //Resets the multiplier on the first frame.
        if(LerpTimeMultiplier != 1f)
        {
            PositionLerpTime /= LerpTimeMultiplier;
            LerpTimeMultiplier = 1f;
        }
    }
}
