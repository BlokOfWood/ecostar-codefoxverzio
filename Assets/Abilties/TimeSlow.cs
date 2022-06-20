using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Create Time Slow Ability")]
public class TimeSlow : Ability
{
    protected List<(ObjectProperties, float)> SlowedObjects;
    public float Multiplier;

    public override void initialize(GameObject gameObject)
    {
        base.initialize(gameObject);
        _triggerStatement = () =>
        {
            if (!Active && _player_ref.Controls.Player.TimeSlow.triggered)
            {
                return true;
            }
            return false;
        };

        SlowedObjects = new List<(ObjectProperties, float)>();
    }

    protected override byte activate()
    {
        bool _hit = Physics.Raycast(_player_ref.CameraPivot.transform.position, _player_ref.CameraPivot.transform.forward, out RaycastHit _raycastHit, Range, ~(1 << 8));

        Debug.Log(_hit);
        if (_hit && _raycastHit.transform.GetComponent<ObjectProperties>())
        {
            var inst = SlowedObjects.Find((x) => x.Item1 == _raycastHit.transform.GetComponent<ObjectProperties>());
            if (inst.Item1)
            {
                inst.Item2 = BaseDuration;
                SlowedObjects[SlowedObjects.FindIndex((x) => inst.Item1 == x.Item1)] = inst;
            }
            else
            {
                SlowedObjects.Add((_raycastHit.transform.GetComponent<ObjectProperties>(), BaseDuration));
                SlowedObjects[SlowedObjects.Count - 1].Item1.Speed *= Multiplier;
            }
        }

        return base.activate();
    }

    protected override byte deactivate()
    {
        return base.deactivate();
    }

    public override void tick(float deltaTime)
    {
        for (int i = 0; i < SlowedObjects.Count; i++)
        {
            var curr = SlowedObjects[i];

            if(curr.Item2 <= 0)
            {
                SlowedObjects.Remove(curr);
                curr.Item1.Speed /= Multiplier;
                continue;
            }

            curr.Item2 -= deltaTime;
            SlowedObjects[i] = curr;
        }
        base.tick(deltaTime);
    }
}