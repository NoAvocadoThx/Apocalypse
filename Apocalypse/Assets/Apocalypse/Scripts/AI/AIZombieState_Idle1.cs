using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIZombieState_Idle1 : AIZombieState
{
    //Inspector 
    [SerializeField] Vector2 _idleTimeRange = new Vector2(10.0f, 60.0f);

    //private
    float _idleTime = 0.0f;
    float _timer = 0.0f;

    /*********************************************************/
    public override void OnEnterState()
    {
        Debug.Log("Enter Idle state");
        base.OnEnterState();
        if (_zombieStateMachine == null) return;
        //random idle state time
        _idleTime = Random.Range(_idleTimeRange.x, _idleTimeRange.y);
        _timer = 0.0f;

        //set state machine
        _zombieStateMachine.NavAgentControl(true,false);
        _zombieStateMachine.speed = 0.0f;
        _zombieStateMachine.seeking = 0;
        _zombieStateMachine.feeding = false;
        _zombieStateMachine.attackType = 0;
        _zombieStateMachine.ClearTarget();
    }


    /*********************************************************/
    public override AIStateType GetStateType()
    {
        
        return AIStateType.Idle;
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
            _zombieStateMachine.SetTarget(_zombieStateMachine.visualThreat);
            return AIStateType.Pursuit;
        }

        //if nothing happens, then maintain idle state for random seconds and then go to patrol state
        _timer += Time.deltaTime;
        if (_timer > _idleTime)
        {
            _zombieStateMachine.navAgent.SetDestination(_zombieStateMachine.GetWaypointPosition(false));
            _zombieStateMachine.navAgent.isStopped = false;
            return AIStateType.Alerted;
        }


        return AIStateType.Idle;
    }
}
