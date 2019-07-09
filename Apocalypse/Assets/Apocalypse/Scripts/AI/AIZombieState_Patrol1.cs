using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIZombieState_Patrol1 : AIZombieState
{
    //Inspector
    [SerializeField] AIWaypointNetwork _waypointNetwork=null;
    [SerializeField] bool _randomPatrol = false;
    [SerializeField] int _curWaypoint = 0;
    [SerializeField][Range(0.0f,3.0f)] float _speed = 1.0f;

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
                    _curWaypoint = Random.Range(0, _waypointNetwork.waypoints.Count - 1);
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
        return AIStateType.Patrol;
    }


    /*********************************************************/
    public override AIStateType GetStateType()
    {

        return AIStateType.Patrol;
    }
}
