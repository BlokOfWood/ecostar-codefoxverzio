using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectTest : MonoBehaviour
{
    void FixedUpdate()
    {
        transform.eulerAngles += new Vector3(0, GetComponent<ObjectProperties>().Speed*25);
    }
}
