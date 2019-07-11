﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIZombieState_Attack1 : AIZombieState
{
    // Inspector Assigned
    [SerializeField] [Range(0, 10)] float _speed = 0.0f;
    [SerializeField] [Range(0.0f, 1.0f)] float _lookAtWeight = 0.7f;
    [SerializeField] [Range(0.0f, 90.0f)] float _lookAtAngleThreshold = 15.0f;
    [SerializeField] float _stoppingDistance = 1.0f;
    [SerializeField] float _slerpSpeed = 5.0f;

    // private
    private float _currentLookAtWeight = 0.0f;

    /*********************************************************/
    public override AIStateType GetStateType() { return AIStateType.Attack; }


    /*********************************************************/
    public override void OnEnterState()
    {
        Debug.Log("Entering Attack State");

        base.OnEnterState();
        if (_zombieStateMachine == null)
            return;

        // Configure State Machine
        _zombieStateMachine.NavAgentControl(true, false);
        _zombieStateMachine.seeking = 0;
        _zombieStateMachine.feeding = false;
        _zombieStateMachine.attackType = Random.Range(1, 100); ;
        _zombieStateMachine.speed = _speed;
        _currentLookAtWeight = 0.0f;
    }


    /*********************************************************/
    public override void OnExitState()
    {
        _zombieStateMachine.attackType = 0;
    }

    /*********************************************************/
    public override AIStateType OnUpdate()
    {
        Vector3 targetPos;
        Quaternion newRot;

        //if zombie is in melee range, stop walking
        if (Vector3.Distance(_zombieStateMachine.transform.position, _zombieStateMachine.targetPosition) < _stoppingDistance)
        {
            _zombieStateMachine.speed = 0.0f;

        }
        else
        {
            _zombieStateMachine.speed = _speed;
        }

        // Do we have a visual threat that is the player
        if (_zombieStateMachine.visualThreat.type == AITargetType.Visual_Player)
        {
            // Set new target
            _zombieStateMachine.SetTarget(_stateMachine.visualThreat);

            // If we are not in melee range any more than fo back to pursuit mode
            if (!_zombieStateMachine.inMeleeRange) return AIStateType.Pursuit;

            if (!_zombieStateMachine.useRootRotation)
            {
                // Keep the zombie facing the player at all times
                targetPos = _zombieStateMachine.targetPosition;
                targetPos.y = _zombieStateMachine.transform.position.y;
                newRot = Quaternion.LookRotation(targetPos - _zombieStateMachine.transform.position);
                _zombieStateMachine.transform.rotation = Quaternion.Slerp(_zombieStateMachine.transform.rotation, newRot, Time.deltaTime * _slerpSpeed);
            }
           Debug.Log("gen Attack");
            //generate attack int for attack type in animator
            _zombieStateMachine.attackType = Random.Range(1, 100);

            return AIStateType.Attack;
        }

        // PLayer has stepped outside out zombie's FOV or hidden so face in his/her direction and then
        // drop back to Alerted mode to give the AI a chance to re-aquire target
        if (!_zombieStateMachine.useRootRotation)
        {
            targetPos = _zombieStateMachine.targetPosition;
            targetPos.y = _zombieStateMachine.transform.position.y;
            newRot = Quaternion.LookRotation(targetPos - _zombieStateMachine.transform.position);
            _zombieStateMachine.transform.rotation = newRot;
        }

        // Stay in Patrol State
        return AIStateType.Alerted;
    }


    /*********************************************************/
    public override void OnAnimatorIKUpdated()
    {
        if (_zombieStateMachine == null)
            return;

        if (Vector3.Angle(_zombieStateMachine.transform.forward, _zombieStateMachine.targetPosition - _zombieStateMachine.transform.position) < _lookAtAngleThreshold)
        {
            _zombieStateMachine.animator.SetLookAtPosition(_zombieStateMachine.targetPosition + Vector3.up);
            _currentLookAtWeight = Mathf.Lerp(_currentLookAtWeight, _lookAtWeight, Time.deltaTime);
            _zombieStateMachine.animator.SetLookAtWeight(_currentLookAtWeight);
        }
        else
        {
            _currentLookAtWeight = Mathf.Lerp(_currentLookAtWeight, 0.0f, Time.deltaTime);
            _zombieStateMachine.animator.SetLookAtWeight(_currentLookAtWeight);
        }
    }

}
