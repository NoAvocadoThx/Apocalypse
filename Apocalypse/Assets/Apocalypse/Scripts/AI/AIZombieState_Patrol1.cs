using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIZombieState_Patrol1 : AIZombieState
{
    //Inspector
    
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
        
        _zombieStateMachine.seeking = 0;
        _zombieStateMachine.feeding = false;
        _zombieStateMachine.attackType = 0;

        // set Destination
        _zombieStateMachine.navAgent.SetDestination(_zombieStateMachine.GetWaypointPosition(false));
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
            Debug.Log("I hear this!");
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

        if (_zombieStateMachine.navAgent.pathPending)
        {
            _zombieStateMachine.speed = 0.0f;
            return AIStateType.Patrol;
        }
        else
            _zombieStateMachine.speed = _speed;
        

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

            _zombieStateMachine.navAgent.SetDestination(_zombieStateMachine.GetWaypointPosition(true));
        }

        return AIStateType.Patrol;
    }


    

    /*********************************************************/
    //when the zombie has reached its target (entered its target trigger
    public override void OnDesinationReached(bool isReached)
    {
        if (_zombieStateMachine == null||!isReached) return;
        if (_zombieStateMachine.targetType == AITargetType.Waypoint) _zombieStateMachine.GetWaypointPosition(true);

    }

    /*********************************************************/
    public override AIStateType GetStateType()
    {

        return AIStateType.Patrol;
    }



    /*********************************************************/
    //
   /* public override void OnAnimatorIKUpdated()
    {
        if (!_zombieStateMachine) return;
        _zombieStateMachine.animator.SetLookAtPosition(_zombieStateMachine.targetPosition+Vector3.up);
        _zombieStateMachine.animator.SetLookAtWeight(0.55f);
    }*/
}
