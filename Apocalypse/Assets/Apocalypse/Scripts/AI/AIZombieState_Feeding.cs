using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIZombieState_Feeding : AIZombieState
{
    //inspector 
    [SerializeField] float _slerpSpeed = 5.0f;
    [SerializeField] Transform _bloodParticleMount = null;
    [SerializeField] [Range(0.01f, 1.0f)] float _bloodParticleBurstTime = 0.1f;
    [SerializeField] [Range(1, 100)] int _bloodParticleBurstAmount = 10;

    //private 
    private int _eatingStateHash = Animator.StringToHash("Feeding State");
    private int _crawlFeedingHash = Animator.StringToHash("Crawl Feeding");
    private int _eatingLayerIndex = -1;
    private float _timer = 0.0f;
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
        _timer = 0.0f;
        
    }


    /*********************************************************/
    public override void OnExitState()
    {
        if (_zombieStateMachine) _zombieStateMachine.feeding = false ;
    }


    /*********************************************************/
    public override AIStateType OnUpdate()
    {
        _timer += Time.deltaTime;
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
        int curHash = _zombieStateMachine.animator.GetCurrentAnimatorStateInfo(_eatingLayerIndex).shortNameHash;
        if (curHash==_eatingStateHash||curHash==_crawlFeedingHash)
        {
            //calculate satisfaction over time and dont let it exceed 1
            _zombieStateMachine.satisfaction = Mathf.Min(_zombieStateMachine.satisfaction + (Time.deltaTime * _zombieStateMachine.replenishRate)/100.0f, 1.0f);
            if (GameSceneManager.instance && GameSceneManager.instance.bloodParticle && _bloodParticleMount)
            {
               
                    //bind particle to the blood mount
                    ParticleSystem system = GameSceneManager.instance.bloodParticle;
                    system.transform.position = _bloodParticleMount.transform.position;
                    system.transform.rotation = _bloodParticleMount.transform.rotation;
                    var settings = system.main;
                    settings.simulationSpace = ParticleSystemSimulationSpace.World;
                    system.Emit(_bloodParticleBurstAmount);
                    _timer = 0.0f;
                
            }
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

        //tranlate zombie's head to actual target when feeding slowly
        Vector3 headToTarget = _zombieStateMachine.targetPosition - _zombieStateMachine.animator.GetBoneTransform(HumanBodyBones.Head).position;
        _zombieStateMachine.transform.position = Vector3.Lerp(_zombieStateMachine.transform.position, _zombieStateMachine.transform.position + headToTarget,Time.deltaTime);

        return AIStateType.Feeding;
    }

}
