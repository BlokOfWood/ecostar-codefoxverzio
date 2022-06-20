using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ClassRefs
{
    //Horrible, Right? I know. I know.

    /// <summary>
    /// First string the display name of the type
    /// Second string the value that can be used to generate a reference to the class.
    /// </summary>
    public static IDictionary<string, string> AIActions = new Dictionary<string, string>
    {
        {"", "" },
        { GetDictonaryEntry(typeof(TestAction)).Key,  GetDictonaryEntry(typeof(TestAction)).Value},
        { GetDictonaryEntry(typeof(FollowPlayer)).Key,  GetDictonaryEntry(typeof(FollowPlayer)).Value},
        { GetDictonaryEntry(typeof(FlyToPlayer)).Key,  GetDictonaryEntry(typeof(FlyToPlayer)).Value},
        { "Add New Entry", "" }
    };
    /// <summary>
    /// First string the display name of the type
    /// Second string the value that can be used to generate a reference to the class.
    /// </summary>
    public static IDictionary<string, string> AIConditions = new Dictionary<string, string>
    {
        {"","" },
        {GetDictonaryEntry(typeof(PlayerInAir)).Key, GetDictonaryEntry(typeof(PlayerInAir)).Value },
        {GetDictonaryEntry(typeof(PlayerNotInAir)).Key, GetDictonaryEntry(typeof(PlayerNotInAir)).Value }
    };

    //Generates a key-value pair from the type provided.
    static KeyValuePair<string, string> GetDictonaryEntry(Type type) =>
        new KeyValuePair<string, string>(type.Name, $"{type.AssemblyQualifiedName}.{type.Name}, {type.Assembly.GetName()}");
}

public class AIController : MonoBehaviour
{
    public AIObject FSM;
    public Entity AIStats;
    public GameObject Player;
    private Entity _entityInst;
    private AIState[] _stateMachine;
    int _currStateIndex = 0;

    private void Start()
    {
        _entityInst = Instantiate(AIStats);
        _stateMachine = FSM.StateMachine;
        for(int i = 0; i < _stateMachine.Length; i++)
        {
            _stateMachine[i].Initialize(GetComponent<AIController>());
        }
    }

    void Update()
    {
        if (_stateMachine == null || _stateMachine.Length == 0)
            return;
        _stateMachine[_currStateIndex].Tick(Time.deltaTime);
        int toTransition = _stateMachine[_currStateIndex].CheckTransitions();
        if (toTransition != -1)
        {
            int index = _stateMachine.ToList().FindIndex(x => 
            {
                return x.ID == toTransition;
            });
            if(index == -1)
            {
                Debug.LogError($"Couldn't find the Node to transition to! ToID: {toTransition}");
            }
            else
            {
                _stateMachine[_currStateIndex].Exit();
                _stateMachine[index].Enter();
                _currStateIndex = index;
            }
        }
    }

    public void ApplyDamage(int amount)
    {
        _entityInst.Health -= amount;
    }
}