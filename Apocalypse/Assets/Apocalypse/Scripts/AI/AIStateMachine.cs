﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum AIStateType { None, Idle, Alerted, Patrol, Attack, Feeding, Pursuit, Dead}
public enum AITargetType { None, Waypoint, Visual_Player,Visual_Light, Visual_Food,Audio}

public struct AITarget
{
    private AITargetType _type; //type of target
    private Collider _collider; //
    private Vector3 _position;//current world pos
    private float _distance;//distance from target
    private float _time;//time the target was last ping'd

    public AITargetType type { get { return _type; } }
    public Collider collider { get { return _collider; } }
    public Vector3 position { get { return _position; } }
    public float distance { get{ return _distance;  }  set { _distance = value; } }
    public float time { get { return _time; } }

    public void Set(AITargetType t,Collider c,Vector3 p, float d)
    {
        _type = t;
        _collider = c;
        _position = p;
        _distance = d;
        _time = Time.time;
    }

    public void Clear()
    {
        _type = AITargetType.None;
        _collider = null;
        _position = Vector3.zero;
        _distance = Mathf.Infinity;
        _time = 0;
     
    }
}


public abstract class AIStateMachine : MonoBehaviour
{
    //public
    public AITarget visualThreat = new AITarget();
    public AITarget audioThreat = new AITarget();

    //protected
    protected Dictionary<AIStateType, AIState> _state = new Dictionary<AIStateType, AIState>();
    protected AITarget _target = new AITarget();

    [SerializeField] protected SphereCollider _targetTrigger = null;
    [SerializeField] protected SphereCollider _sensorTrigger = null;

    //Component cache
    protected Animator _animator = null;
    protected NavMeshAgent _navAgent = null;
    protected Collider _collider = null;
    protected Transform _transform = null;

    //public properties
    public Animator animator { get { return _animator; } }
    public NavMeshAgent navAgent { get { return _navAgent; } }


    protected virtual void Start()
    {
        //fetch all states on this game object
        AIState[] states = GetComponents<AIState>();
        //loop through all states and add them to the state dictionary
        foreach (AIState state in states)
        {
            if (state != null && !_state.ContainsKey(state.GetStateType()))
            {
                _state[state.GetStateType()] = state;
            }
        }
    }
}
