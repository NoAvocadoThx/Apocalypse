using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum AIStateType { None, Idle, Alerted, Patrol, Attack, Feeding, Pursuit, Dead}
public enum AITargetType { None, Waypoint, Visual_Player,Visual_Light, Visual_Food,Audio}
public enum AITriggerEventType { Enter,Exit,Stay}

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
    protected Dictionary<AIStateType, AIState> _states = new Dictionary<AIStateType, AIState>();
    protected AITarget _target = new AITarget();
    protected AIState _curState = null;

    [SerializeField] protected AIStateType _curStateType = AIStateType.Idle;
    [SerializeField] protected SphereCollider _targetTrigger = null;
    [SerializeField] protected SphereCollider _sensorTrigger = null;

    [SerializeField] [Range(0, 15)] protected float _stoppingDistance = 1.0f; 

    //Component cache
    protected Animator _animator = null;
    protected NavMeshAgent _navAgent = null;
    protected Collider _collider = null;
    protected Transform _transform = null;

    //public properties
    public Animator animator { get { return _animator; } }
    public NavMeshAgent navAgent { get { return _navAgent; } }

    //cache reference to gameObject
    protected virtual void Awake()
    {
        _transform = transform;
        _animator = GetComponent<Animator>();
        _navAgent = GetComponent<NavMeshAgent>();
        _collider = GetComponent<Collider>();
    }

    protected virtual void Start()
    {
        //fetch all states on this game object
        AIState[] states = GetComponents<AIState>();
        //loop through all states and add them to the state dictionary
        foreach (AIState state in states)
        {
            if (state != null && !_states.ContainsKey(state.GetStateType()))
            {
                _states[state.GetStateType()] = state;
                state.SetStateMachine(this);
            }
        }

        if (_states.ContainsKey(_curStateType))
        {
            _curState = _states[_curStateType];
            //intialize curState after fetch
            _curState.OnEnterState();
        }
        else
        {
            _curState = null;
        }
    }

  // Set Target parameters

    public void SetTarget(AITargetType t,Collider c,Vector3 p, float d)
    {
        _target.Set(t, c, p, d);
        //
        if (_targetTrigger != null)
        {
            _targetTrigger.radius = _stoppingDistance;
            _targetTrigger.transform.position = _target.position;
            _targetTrigger.enabled = true;
        }
    }

    // Set Target parameters
    public void SetTarget(AITargetType t, Collider c, Vector3 p, float d,float s)
    {
        _target.Set(t, c, p, d);
        if (_targetTrigger != null)
        {
            _targetTrigger.radius = s;
            _targetTrigger.transform.position = _target.position;
            _targetTrigger.enabled = true;
        }
    }

    // Set Target parameters
    public void SetTarget(AITarget t)
    {
        _target = t;
        if (_targetTrigger != null)
        {
            _targetTrigger.radius = _stoppingDistance;
            _targetTrigger.transform.position = _target.position;
            _targetTrigger.enabled = true;
        }
    }

    //Clear target
    public void ClearTarget()
    {
        _target.Clear();
        if (_targetTrigger != null)
        {
            _targetTrigger.enabled = false;
        }
    }

    //clear visual and audio target each frame so we re-calculate distance to the curTarget
    protected virtual void FixedUpdate()
    {
        visualThreat.Clear();
        audioThreat.Clear();
        if (_target.type != AITargetType.None)
        {
            _target.distance = Vector3.Distance(_transform.position, _target.position);
        }
    }



    //Update each frame. Gives current State achance to update itself and perform transitions
    protected virtual void Update()
    {
        if (_curState == null) return;
        AIStateType newStateType= _curState.OnUpdate();
        if (newStateType != _curStateType)
        {
            AIState newState = null;
            //if newStateType exits, populate newState with actual AI state reference
            if(_states.TryGetValue(newStateType,out newState))
            {
                _curState.OnExitState();
                newState.OnEnterState();
                _curState = newState;
            }
            //if we do not find the state, we return to AIStateType Idle state by default
            else
            if (_states.TryGetValue(AIStateType.Idle, out newState))
            {
                _curState.OnExitState();
                newState.OnEnterState();
                _curState = newState;
            }
            _curStateType = newStateType;
        }
    }
}
