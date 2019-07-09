using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIZombieState_Feeding : AIZombieState
{
    //inspector 
    [SerializeField] float _slerpSpeed = 5.0f;

    //private 
    private int _eatingStateHash = Animator.StringToHash("Feeding State");
    private int _eatingLayerIndex = -1;

    //override
    /*********************************************************/
    public override AIStateType GetStateType()
    {
        return AIStateType.Feeding;
    }


    /*********************************************************/
    public override void OnEnterState()
    {
        Debug.Log("Enter Feeding state");
        base.OnEnterState();
        if (_zombieStateMachine == null) return;
        if (_eatingLayerIndex == -1) _eatingLayerIndex = _zombieStateMachine.animator.GetLayerIndex("Cinematic");

        //set state machine
        _zombieStateMachine.NavAgentControl(true, false);
        _zombieStateMachine.speed = 0.0f;
        _zombieStateMachine.seeking = 0;
        _zombieStateMachine.feeding = true;
        _zombieStateMachine.attackType = 0;
     
        
    }


    /*********************************************************/
    public override void OnExitState()
    {
        if (_zombieStateMachine) _zombieStateMachine.feeding = false ;
    }


    /*********************************************************/
    public override AIStateType OnUpdate()
    {
        if (_zombieStateMachine.satisfaction > 0.9f)
        {
            //set to false => resume path, not increment waypoint index
            _zombieStateMachine.GetWaypointPosition(false);
            return AIStateType.Alerted;
        }
        if (_zombieStateMachine == null) return AIStateType.Idle;

        // If Visual Threat then drop into alert mode
        if (_zombieStateMachine.visualThreat.type != AITargetType.None
            && _zombieStateMachine.visualThreat.type != AITargetType.Visual_Food)
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
        //if we are in "State Feeding state"
        if (_zombieStateMachine.animator.GetCurrentAnimatorStateInfo(_eatingLayerIndex).shortNameHash==_eatingStateHash)
        {
            //calculate satisfaction over time and dont let it exceed 1
            _zombieStateMachine.satisfaction = Mathf.Min(_zombieStateMachine.satisfaction + (Time.deltaTime * _zombieStateMachine.replenishRate), 1.0f);
        }

        // If root rotation is not being used then we are responsible for keeping zombie rotated
        // and facing in the right direction. 
        if (!_zombieStateMachine.useRootRotation)
        {
            //keep zombie facing player
            Vector3 targetPos = _zombieStateMachine.targetPosition;
            targetPos.y = _zombieStateMachine.transform.position.y;
            Quaternion newRot = Quaternion.LookRotation(targetPos - _zombieStateMachine.transform.position);
            _zombieStateMachine.transform.rotation = Quaternion.Slerp(_zombieStateMachine.transform.rotation,newRot, Time.deltaTime * _slerpSpeed);
        }



        return AIStateType.Feeding;
    }

}
