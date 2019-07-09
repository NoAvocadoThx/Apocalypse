using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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


        // Set path
        _zombieStateMachine.navAgent.SetDestination(_zombieStateMachine.targetPosition);
        _zombieStateMachine.navAgent.Resume();

    }
}
