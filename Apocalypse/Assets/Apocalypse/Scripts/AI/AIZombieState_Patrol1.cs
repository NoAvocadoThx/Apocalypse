using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIZombieState_Patrol1 : AIZombieState
{
    //Inspector
    [SerializeField] AIWaypointNetwork _waypointNetwork=null;
    [SerializeField] bool _randomPatrol = false;
    [SerializeField] int _curWaypoint = 0;
    [SerializeField][Range(0.0f,3.0f)] float _speed = 1.0f;
    [SerializeField] float _turnOnSpotThreshold = 80.0f;
    [SerializeField] float _slerpSpeed=5.0f;

    /*********************************************************/
    public override void OnEnterState()
    {
        Debug.Log("Enter Patrol state");
        base.OnEnterState();
        if (_zombieStateMachine == null) return;
       

        //set state machine
        _zombieStateMachine.NavAgentControl(true, false);
        _zombieStateMachine.speed = _speed;
        _zombieStateMachine.seeking = 0;
        _zombieStateMachine.feeding = false;
        _zombieStateMachine.attackType = 0;

        // If the current target is not a waypoint then we need to select
        // a waypoint from te waypoint network and make this the new target
        // and plot a path to it
        if (_zombieStateMachine.targetType != AITargetType.Waypoint)
        {
            //clear previous target
            _zombieStateMachine.ClearTarget();

            //if we have valid waypointNetwork
            if (_waypointNetwork && _waypointNetwork.waypoints.Count > 0)
            {
                //set cur waypoint to random waypoint if randomPatrol
                if (_randomPatrol)
                {
                    _curWaypoint = Random.Range(0, _waypointNetwork.waypoints.Count);
                }

                //if it is a valid index waypoint
                if (_curWaypoint < _waypointNetwork.waypoints.Count)
                {
                    Transform waypoint = _waypointNetwork.waypoints[_curWaypoint];
                    if (waypoint)
                    {
                        
                        _zombieStateMachine.SetTarget(
                            AITargetType.Waypoint, 
                            null, 
                            waypoint.position, 
                            Vector3.Distance(_zombieStateMachine.transform.position, waypoint.position));

                        //let navAgent make a path for the zombie
                        _zombieStateMachine.navAgent.SetDestination(waypoint.position);
                       
                    }
                }
            }
        }
        //resume patrol state if pause?
        _zombieStateMachine.navAgent.Resume();
    }


    /*********************************************************/
    public override AIStateType OnUpdate()
    {
        
        if (_zombieStateMachine == null) return AIStateType.Idle;

        //if it sees player, set state to pursuit
        if (_zombieStateMachine.visualThreat.type == AITargetType.Visual_Player)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.visualThreat);
            return AIStateType.Pursuit;
        }
        //if see light (no sight of player), set state to alerted
        if (_zombieStateMachine.visualThreat.type == AITargetType.Visual_Light)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.visualThreat);
            return AIStateType.Alerted;
        }
        //if audio threat, then got to alerted state
        if (_zombieStateMachine.audioThreat.type == AITargetType.Audio)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.audioThreat);
            return AIStateType.Alerted;
        }
        //if find food, run to food
        if (_zombieStateMachine.visualThreat.type == AITargetType.Visual_Food)
        {
            //if satisfaction > distance to the food position
            if ((1.0f - _zombieStateMachine.satisfaction) > (_zombieStateMachine.visualThreat.distance / _zombieStateMachine.sensorRadius))
            {
                _zombieStateMachine.SetTarget(_zombieStateMachine.visualThreat);
                return AIStateType.Pursuit;
            }
           
        }
        // Calculate angle we need to turn through to be facing our target
        float angle = Vector3.Angle(_zombieStateMachine.transform.forward, (_zombieStateMachine.navAgent.steeringTarget - _zombieStateMachine.transform.position));
        // If its too big then drop out of Patrol and into Altered
        if (Mathf.Abs(angle) > _turnOnSpotThreshold)
        {
            Debug.Log("Alerted????");
            return AIStateType.Alerted;
        }
        // If root rotation is not being used then we are responsible for keeping zombie rotated
        // and facing in the right direction. 
        if (!_zombieStateMachine.useRootRotation)
        {
            // Generate a new Quaternion representing the rotation we should have
            Quaternion newRot = Quaternion.LookRotation(_zombieStateMachine.navAgent.desiredVelocity);
            // Smoothly rotate to that new rotation over time
            _zombieStateMachine.transform.rotation = Quaternion.Slerp(_zombieStateMachine.transform.rotation, newRot, Time.deltaTime * _slerpSpeed);
        }
        //if zombie lost its path or currrent waypoint, we generate a new waypoint for it
        if (_zombieStateMachine.navAgent.isPathStale || !_zombieStateMachine.navAgent.hasPath
            ||_zombieStateMachine.navAgent.pathStatus!=NavMeshPathStatus.PathComplete)
        {
            
            genNextWaypoint();
        }

        return AIStateType.Patrol;
    }


    /*********************************************************/
    //select a new waypoint. Either randomly selects a new
    //waypoint from the waypoint network or increments the current
    //waypoint index (with wrap-around) to visit the waypoints in
    //the network in sequence. Sets the new waypoint as the the
    //target and generates a nav agent path for it
    private void genNextWaypoint()
    {
        // Increase the current waypoint with wrap-around to zero (or choose a random waypoint)
        if (_randomPatrol && _waypointNetwork.waypoints.Count > 1)
        {
            // Keep generating random waypoint until we find one that isn't the current one
            // NOTE: Very important that waypoint networks do not only have one waypoint :)
            int oldWaypoint = _curWaypoint;
            while (_curWaypoint == oldWaypoint)
            {
                _curWaypoint = Random.Range(0, _waypointNetwork.waypoints.Count);
            }
        }
        else
            _curWaypoint = _curWaypoint == _waypointNetwork.waypoints.Count - 1 ? 0 : _curWaypoint + 1;

        // Fetch the new waypoint from the waypoint list
        if (_waypointNetwork.waypoints[_curWaypoint] != null)
        {
            Transform newWaypoint = _waypointNetwork.waypoints[_curWaypoint];

            // This is our new target position
            _zombieStateMachine.SetTarget(AITargetType.Waypoint,
                                            null,
                                            newWaypoint.position,
                                            Vector3.Distance(newWaypoint.position, _zombieStateMachine.transform.position));

            // Set new Path
            _zombieStateMachine.navAgent.SetDestination(newWaypoint.position);
        }
    }

    /*********************************************************/
    //when the zombie has reached its target (entered its target trigger
    public override void OnDesinationReached(bool isReached)
    {
        if (_zombieStateMachine == null||!isReached) return;
        if (_zombieStateMachine.targetType == AITargetType.Waypoint) genNextWaypoint();
        
    }

    /*********************************************************/
    public override AIStateType GetStateType()
    {

        return AIStateType.Patrol;
    }



    /*********************************************************/
    //
    public override void OnAnimatorIKUpdated()
    {
        if (!_zombieStateMachine) return;
        _zombieStateMachine.animator.SetLookAtPosition(_zombieStateMachine.targetPosition+Vector3.up);
        _zombieStateMachine.animator.SetLookAtWeight(0.55f);
    }
}
