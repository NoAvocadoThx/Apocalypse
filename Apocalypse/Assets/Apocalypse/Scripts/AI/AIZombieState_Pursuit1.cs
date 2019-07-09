using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIZombieState_Pursuit1 : AIZombieState
{
    [SerializeField] [Range(0, 10)] private float _speed = 1.0f;
    [SerializeField] private float _slerpSpeed = 5.0f;
    [SerializeField] private float _repathDistanceMultiplier = 0.035f;
    [SerializeField] private float _repathVisualMinDuration = 0.05f;
    [SerializeField] private float _repathVisualMaxDuration = 5.0f;
    [SerializeField] private float _repathAudioMinDuration = 0.25f;
    [SerializeField] private float _repathAudioMaxDuration = 5.0f;
    [SerializeField] private float _maxDuration = 40.0f;

    // Private Fields
    private float _timer = 0.0f;
    private bool _targetReached = false;
    private float _repathTimer = 0.0f;


    /*********************************************************/
    public override AIStateType GetStateType() { return AIStateType.Pursuit; }


    /*********************************************************/
    public override void OnEnterState()
    {
        Debug.Log("Entering Pursuit State");

        base.OnEnterState();
        if (_zombieStateMachine == null)
            return;

        // Configure State Machine
        _zombieStateMachine.NavAgentControl(true, false);
        _zombieStateMachine.speed = _speed;
        _zombieStateMachine.seeking = 0;
        _zombieStateMachine.feeding = false;
        _zombieStateMachine.attackType = 0;

        // Zombies will only pursue for so long before breaking off
        _timer = 0.0f;
        _repathTimer = 0.0f;
        _targetReached = false;

        // Set path
        _zombieStateMachine.navAgent.SetDestination(_zombieStateMachine.targetPosition);
        _zombieStateMachine.navAgent.Resume();

    }

    /*********************************************************/
    public override AIStateType OnUpdate()
    {
        _timer += Time.deltaTime;
        _repathTimer += Time.deltaTime;

        if (_timer > _maxDuration)
        {
            return AIStateType.Patrol;
        }

        // IF we are chasing the player and have entered the melee trigger then attack
        if (_stateMachine.targetType == AITargetType.Visual_Player && _zombieStateMachine.inMeleeRange)
        {
            return AIStateType.Attack;
        }

        // Otherwise this is navigation to areas of interest so use the standard target threshold
        if (_targetReached)
        {
            switch (_stateMachine.targetType)
            {

                // If we have reached the source
                case AITargetType.Audio:
                case AITargetType.Visual_Light:
                    _stateMachine.ClearTarget();    // Clear the threat
                    return AIStateType.Alerted;     // Become alert and scan for targets

                case AITargetType.Visual_Food:
                    return AIStateType.Feeding;
            }
        }

        // If for any reason the nav agent has lost its path then call then drop into alerted state
        // so it will try to re-aquire the target or eventually giveup and resume patrolling
        if (_zombieStateMachine.navAgent.isPathStale ||
            !_zombieStateMachine.navAgent.hasPath ||
            _zombieStateMachine.navAgent.pathStatus != NavMeshPathStatus.PathComplete)
        {
            return AIStateType.Alerted;
        }

        // If we are close to the target that was a player and we still have the player in our vision then keep facing right at the player
        if (!_zombieStateMachine.useRootRotation && _zombieStateMachine.targetType == AITargetType.Visual_Player
            && _zombieStateMachine.visualThreat.type == AITargetType.Visual_Player
            && _targetReached)
        {
            Vector3 targetPos = _zombieStateMachine.targetPosition;
            targetPos.y = _zombieStateMachine.transform.position.y;
            Quaternion newRot = Quaternion.LookRotation(targetPos - _zombieStateMachine.transform.position);
            _zombieStateMachine.transform.rotation = newRot;
        }
        else if (!_stateMachine.useRootRotation && !_targetReached)
        {
            // SLowly update our rotation to match the nav agents desired rotation BUT only if we are not persuing the player and are really close to him

            // Generate a new Quaternion representing the rotation we should have
            Quaternion newRot = Quaternion.LookRotation(_zombieStateMachine.navAgent.desiredVelocity);

            // Smoothly rotate to that new rotation over time
            _zombieStateMachine.transform.rotation = Quaternion.Slerp(_zombieStateMachine.transform.rotation, newRot, Time.deltaTime * _slerpSpeed);


        }
        else if (_targetReached)
        {
            return AIStateType.Alerted;
        }


        // Do we have a visual threat that is the player
        if (_zombieStateMachine.visualThreat.type == AITargetType.Visual_Player)
        {
            // The position is different - maybe same threat but it has moved so repath periodically
            if (_zombieStateMachine.targetPosition != _zombieStateMachine.visualThreat.position)
            {
                // Repath more frequently as we get closer to the target (try and save some CPU cycles)
                if (Mathf.Clamp(_zombieStateMachine.visualThreat.distance * _repathDistanceMultiplier, _repathVisualMinDuration, _repathVisualMaxDuration) < _repathTimer)
                {
                    // Repath the agent
                    _zombieStateMachine.navAgent.SetDestination(_zombieStateMachine.visualThreat.position);
                    _repathTimer = 0.0f;
                }
            }
            // Make sure this is the current target
            _zombieStateMachine.SetTarget(_zombieStateMachine.visualThreat);

            // Remain in pursuit state
            return AIStateType.Pursuit;
        }

        return AIStateType.Pursuit;
    }
}
