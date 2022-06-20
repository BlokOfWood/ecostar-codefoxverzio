using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueBuilderCamera : MonoBehaviour
{
    ///<summary>Objects that while active the Camera shouldn't move.</summary>
    public GameObject[] blockerObjects;
    public float CameraSpeed;

    private void Update()
    {
        foreach(GameObject i in blockerObjects)
        {
            if (i.activeInHierarchy) return;
        }
        transform.position += (transform.up * Input.GetAxis("Vertical") + transform.right * Input.GetAxis("Horizontal")) * Time.deltaTime * CameraSpeed;
    }
}