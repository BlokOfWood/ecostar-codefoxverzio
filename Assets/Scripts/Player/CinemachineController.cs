using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinemachineController : MonoBehaviour
{
    public float LookSensitivity = 50;
    [Tooltip("The upper rotation limit of the X axis. Should be above 360 probably.")]
    public GameObject Player;
    InputSettings _controls;

    void Start()
    {
        _controls = Player.GetComponent<PlayerController>().Controls;
        transform.position = Player.transform.position;
    }

    void FixedUpdate()
    {
        float _xDeltaRot = -_controls.Player.CameraRotation.ReadValue<Vector2>().y * Time.fixedDeltaTime * LookSensitivity;
        float _yDeltaRot = _controls.Player.CameraRotation.ReadValue<Vector2>().x * Time.fixedDeltaTime * LookSensitivity;

        if (Cursor.lockState == CursorLockMode.None)
        {
            _xDeltaRot = 0;
            _yDeltaRot = 0;
        }

        GetComponent<Cinemachine.CinemachineFreeLook>().m_XAxis.Value += _yDeltaRot;
        GetComponent<Cinemachine.CinemachineFreeLook>().m_YAxis.Value += _xDeltaRot / 180;

    }
}
