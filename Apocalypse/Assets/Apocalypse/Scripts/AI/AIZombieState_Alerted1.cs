using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIZombieState_Alerted1 : AIZombieState
{
    [SerializeField][Range(1,60)] float _maxDuration =10.0f;
    [SerializeField] float _waypointAngleThreshold = 90.0f;
    [SerializeField] float _threatAngleThreshhold = 10.0f;
    [SerializeField] float _directionChangeTime = 1.5f;
    [SerializeField] float _slerpSpeed = 45.0f;

    //private 
    float _timer = 0.0f;
    float _directionChangeTimer = 0.0f;


    /*********************************************************/
    public override void OnEnterState()
    {
        Debug.Log("Enter Alerted state");
        base.OnEnterState();
        if (_zombieStateMachine == null) return;
      

        //set state machine
        _zombieStateMachine.NavAgentControl(true, false);
        _zombieStateMachine.speed = 0.0f;
        _zombieStateMachine.seeking = 0;
        _zombieStateMachine.feeding = false;
        _zombieStateMachine.attackType = 0;
        _timer = _maxDuration;
        _directionChangeTimer = 0.0f;
    }



    /*********************************************************/
    public override AIStateType GetStateType()
    {
        return AIStateType.Alerted;
    }



    /*********************************************************/
    public override AIStateType OnUpdate()
    {
        //Debug.Log(_zombieStateMachine.seeking);
        //reduce timer
        _timer -= Time.deltaTime;
        _directionChangeTimer += Time.deltaTime;
        //transition into a patrol state
        if (_timer <= 0.0f)
        {
            _zombieStateMachine.navAgent.SetDestination(_zombieStateMachine.GetWaypointPosition(false));
            _zombieStateMachine.navAgent.Resume();
            _timer = _maxDuration;
        }
        //if it sees player, set state to pursuit
        if (_zombieStateMachine.visualThreat.type == AITargetType.Visual_Player)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.visualThreat);
            return AIStateType.Pursuit;
        }
        //if see light (no sight of player), then set alerted time to maxDuration
        if (_zombieStateMachine.visualThreat.type == AITargetType.Visual_Light)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.visualThreat);
            _timer = _maxDuration;
        }
        //if audio threat, then set alerted time to maxDuration
        if (_zombieStateMachine.audioThreat.type == AITargetType.Audio)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.audioThreat);
            _timer = _maxDuration;
        }
        //if find food and no audio threat, run to food
        if (_zombieStateMachine.audioThreat.type==AITargetType.None&&_zombieStateMachine.visualThreat.type == AITargetType.Visual_Food&&_zombieStateMachine.targetType==AITargetType.None)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.visualThreat);
            return AIStateType.Pursuit;
        }

        float angle;
        //if audio threat and light threat
        if ((_zombieStateMachine.targetType == AITargetType.Audio || _zombieStateMachine.targetType == AITargetType.Visual_Light)&& !_zombieStateMachine.isTargetReached)
        {
            //positive right, negative left
            angle = AIState.FindSignedAngle(_zombieStateMachine.transform.forward, _zombieStateMachine.targetPosition - _zombieStateMachine.transform.position);
            //if deal with audio threat, go into pursuit state and the angle is smaller than the threshhold
            if (_zombieStateMachine.targetType == AITargetType.Audio&&Mathf.Abs(angle)<_threatAngleThreshhold)
            {
                return AIStateType.Pursuit;
            }

            if (_directionChangeTimer > _directionChangeTime)
            {
                //if random number is smaller than the intelligence property
                //the zombie may turn to a random direction instead of turning to the target direction
                if (Random.value < _zombieStateMachine.intelligence)
                {
                    _zombieStateMachine.seeking = (int)Mathf.Sign(angle);
                }
                else
                {
                    _zombieStateMachine.seeking = (int)Mathf.Sign(Random.Range(-1.0f, 1.0f));
                }
                _directionChangeTimer = 0.0f;
            }
           
        }
        //if the target is a waypoint
        else if (_zombieStateMachine.targetType == AITargetType.Waypoint&& !_zombieStateMachine.navAgent.pathPending)
        {
            //angle between zombie forward and from zombie to target
            angle = AIState.FindSignedAngle(_zombieStateMachine.transform.forward,
                                        _zombieStateMachine.navAgent.steeringTarget - _zombieStateMachine.transform.position);
            
            if (Mathf.Abs(angle) < _waypointAngleThreshold) return AIStateType.Patrol;
            _zombieStateMachine.seeking = (int)Mathf.Sign(angle);
        }
        else
        {
            if (_directionChangeTimer > _directionChangeTime)
            {
               
                _zombieStateMachine.seeking = (int)Mathf.Sign(Random.Range(-1.0f, 1.0f));
                
                _directionChangeTimer = 0.0f;
            }
        }

        //if we are not using roor rotation 
        //rotate zombie by ourself
        if (!_zombieStateMachine.useRootRotation)
        {
            _zombieStateMachine.transform.Rotate(new Vector3(0.0f, _slerpSpeed * _zombieStateMachine.seeking * Time.deltaTime, 0.0f));
        }
            return AIStateType.Alerted;
    }
}
