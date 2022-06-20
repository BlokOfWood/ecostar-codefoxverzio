using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public Weapon weaponStats;

    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<AIController>())
        {
            other.GetComponent<AIController>().ApplyDamage(weaponStats.damage);
        }
    }
}