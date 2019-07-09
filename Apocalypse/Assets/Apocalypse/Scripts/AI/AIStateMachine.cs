
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

    /*********************************************************/
    public void Set(AITargetType t,Collider c,Vector3 p, float d)
    {
        _type = t;
        _collider = c;
        _position = p;
        _distance = d;
        _time = Time.time;
    }

    /*********************************************************/
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
    //no root motion by default
    protected int _rootPositionRefCount = 0;
    protected int _rootRotationRedCount = 0;
    protected bool _isTargetReached = false;

    //[SerializeField] so we can see from inspector
    [SerializeField] protected AIStateType _curStateType = AIStateType.Idle;
    [SerializeField] protected SphereCollider _targetTrigger = null;
    [SerializeField] protected SphereCollider _sensorTrigger = null;

    [SerializeField] [Range(0, 15)] protected float _stoppingDistance = 1.0f; 

    //Component cache
    protected Animator _animator = null;
    protected NavMeshAgent _navAgent = null;
    protected Collider _collider = null;
    protected Transform _transform = null;

    /*********************************************************/
    // Public properties
    public bool inMeleeRange { get; set; }
    public bool isTargetReached { get { return _isTargetReached; } }
    public Animator animator { get { return _animator; } }
    public NavMeshAgent navAgent { get { return _navAgent; } }
    public Vector3 sensorPosition
    {
        get
        {
            //get correctly scaled sensor center of AI Entity
            if (_sensorTrigger == null) return Vector3.zero;
            Vector3 point = _sensorTrigger.transform.position;
            point.x += _sensorTrigger.center.x * _sensorTrigger.transform.lossyScale.x;
            point.y += _sensorTrigger.center.y * _sensorTrigger.transform.lossyScale.y;
            point.z += _sensorTrigger.center.z * _sensorTrigger.transform.lossyScale.z;
            return point;
        }
    }

    public float sensorRadius
    {
        get
        {
            //get correctly scaled sensor radius of AI Entity
            if (_sensorTrigger == null) return 0.0f;
            float radius = Mathf.Max(_sensorTrigger.radius * _sensorTrigger.transform.lossyScale.x,
                                     _sensorTrigger.radius * _sensorTrigger.transform.lossyScale.y);
            return Mathf.Max(radius, _sensorTrigger.radius * _sensorTrigger.transform.lossyScale.z);
        }
    }

    /*********************************************************/
    public bool useRootPosition { get { return _rootPositionRefCount > 0; } }
    public bool useRootRotation { get { return _rootRotationRedCount > 0; } }
    public AITargetType targetType { get { return _target.type; } }
    public Vector3 targetPosition { get { return _target.position; } }
    public int targetColliderID
    {
        get
        {
            if (_target.collider)
                return _target.collider.GetInstanceID();
            else
                return -1; 
        }
    }

    

    /*********************************************************/
    /// <summary>
    /// cache reference to gameObject
    /// </summary>
    protected virtual void Awake()
    {
        _transform = transform;
        _animator = GetComponent<Animator>();
        _navAgent = GetComponent<NavMeshAgent>();
        _collider = GetComponent<Collider>();

        if (GameSceneManager.instance != null)
        {
            //register State Mahcines with Scene database
            if (_collider) GameSceneManager.instance.RegisterAIStateMachine(_collider.GetInstanceID(), this);
            if (_sensorTrigger) GameSceneManager.instance.RegisterAIStateMachine(_sensorTrigger.GetInstanceID(), this);

        }
    }

    /*********************************************************/
    /// <summary>
    /// 
    /// </summary>
    protected virtual void Start()
    {

        if (_sensorTrigger)
        {
            AISensor script = _sensorTrigger.GetComponent<AISensor>();
            if (script)
            {
                script.parentStateMachine = this;
            }
        }
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


        // Fetch all AIStateMachineLink derived behaviours from the animator
        // and set their State Machine references to this state machine
        if (_animator)
        {
            AIStateMachineLink[] _scripts = _animator.GetBehaviours<AIStateMachineLink>();
            foreach(AIStateMachineLink script in _scripts)
            {
                script.stateMachine = this;
            }
        }

    }

    /*********************************************************/
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
    /*********************************************************/
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
    /*********************************************************/
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
    /*********************************************************/
    //Clear target
    public void ClearTarget()
    {
        _target.Clear();
        if (_targetTrigger != null)
        {
            _targetTrigger.enabled = false;
        }
    }

    /*********************************************************/
    //clear visual and audio target each frame so we re-calculate distance to the curTarget
    protected virtual void FixedUpdate()
    {
        visualThreat.Clear();
        audioThreat.Clear();
        if (_target.type != AITargetType.None)
        {
            _target.distance = Vector3.Distance(_transform.position, _target.position);
        }
        _isTargetReached = false;
    }


    /*********************************************************/
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

    /*********************************************************/
    //Called by Physic system when the AI's main collider enters its trigger. This allows
    //the child state to know when it has entered the sphere of influence of a waypoint or last player
    //sighted position
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (_targetTrigger == null || other != _targetTrigger) return;

        _isTargetReached = true;
        //notify child state
        if (_curState)
        {
            _curState.OnDesinationReached(true);
        }
    }

    /*********************************************************/
    protected virtual void OnTriggerStay(Collider other)
    {
        if (_targetTrigger == null || other != _targetTrigger) return;

        _isTargetReached = true;
        
    }



    /*********************************************************/
    //Informs the child state that the AI entity is no longer at
    //its desination (Typically true when a new target has been)
    //set by the child
    protected void OnTriggerExit(Collider other)
    {
        if (_targetTrigger == null || other != _targetTrigger) return;

        _isTargetReached = false;
        //notify child state
        if (_curState)
        {
            _curState.OnDesinationReached(false);
        }
    }


    /*********************************************************/
    // Called by our AISensor component when an AI Aggravator
    //has entered/exited the sensor trigger.
    public virtual void OnTriggerEvent(AITriggerEventType type, Collider other)
    {
        if (_curState)
        {
            _curState.OnTriggerEvent(type, other);
        }
    }

    /*********************************************************/
    // -----------------------------------------------------------
    // Name	:	OnAnimatorMove
    // Desc	:	Called by Unity after root motion has been
    //			evaluated but not applied to the object.
    //			This allows us to determine via code what to do
    //			with the root motion information
    // -----------------------------------------------------------
    protected virtual void OnAnimatorMove()
    {
        if (_curState)
        {
            _curState.OnAnimatorUpdated();
        }
    }

    /*********************************************************/
    // ----------------------------------------------------------
    // Name	: OnAnimatorIK
    // Desc	: Called by Unity just prior to the IK system being
    //		  updated giving us a chance to setup up IK Targets
    //		  and weights.
    // ----------------------------------------------------------
    protected virtual void OnAnimatorIK(int layerIndex)
    {
        if (_curState)
        {
            _curState.OnAnimatorIKUpdated();
        }
    }

    /*********************************************************/
    //Configure the NavMeshAgent to enable/disable auto
    //updates of position/rotation to our transform
    public void NavAgentControl(bool positionUpdate,bool rotationUpdate)
    {
        if (_navAgent)
        {
            _navAgent.updatePosition = positionUpdate;
            _navAgent.updateRotation = rotationUpdate;
        }
    }

    /*********************************************************/
    //Called by the State Machine Behaviours to
    //Enable/Disable root motion
    public void AddRootMotionRequest(int rootPosition,int rootRotation)
    {
        _rootPositionRefCount += rootPosition;
        _rootRotationRedCount += rootRotation;
    }

}
