﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//State Machine used by zombie characters
public class AIZombieStateMachine : AIStateMachine
{
    // Inspector parameters aka zombie attributes by defult
    [SerializeField][Range(10.0f,360.0f)] float _fov = 50.0f;
    [SerializeField] [Range(0.0f, 1.0f)] float _sight = 0.5f;
    [SerializeField] [Range(0.0f, 1.0f)] float _hearing = 1.0f;
    [SerializeField] [Range(0.0f, 1.0f)] float _aggression = 0.5f;
    [SerializeField] [Range(0, 100)] int _health = 100;
    [SerializeField] [Range(0.0f, 1.0f)] float _intelligence = 0.5f;
    [SerializeField] [Range(0.0f, 1.0f)] float _satisfaction = 1.0f;
    [SerializeField] float _replenishRate = 0.5f;
    [SerializeField] float _depletionRate = 0.1f;

    // Private
    private int _seeking = 0;
    private bool _feeding = false;
    private bool _crawling = false;
    private int _attackType = 0;
    private float _speed = 0.0f;


    // Hashes
    private int _speedHash = Animator.StringToHash("Speed");
    private int _seekingHash = Animator.StringToHash("Seeking");
    private int _feedingHash = Animator.StringToHash("Feeding");
    private int _attackHash = Animator.StringToHash("Attack");


    //public Properties
    //getters and setters of states
    public float fov { get { return _fov; } }
    public float hearing { get { return _hearing; } }
    public float sight { get { return _sight; } }
    public bool crawling { get { return _crawling; } }
    public float intelligence { get { return _intelligence; } }
    public float satisfaction { get { return _satisfaction; } set { _satisfaction = value; } }
    public float aggression { get { return _aggression; } set { _aggression = value; } }
    public int health { get { return _health; } set { _health = value; } }
    public int attackType { get { return _attackType; } set { _attackType = value; } }
    public bool feeding { get { return _feeding; } set { _feeding = value; } }
    public int seeking { get { return _seeking; } set { _seeking = value; } }
    public float replenishRate { get { return _replenishRate; } }
    public float speed
    {
        get { return _speed; }
        set { _speed = value; }
    }

    /*********************************************************/
    //refesh animators each frame
    protected override void Update()
    {
        base.Update();

        if (_animator)
        {
            //set parameters of animators
            _animator.SetFloat(_speedHash, _speed);
            _animator.SetBool(_feedingHash, _feeding);
            _animator.SetInteger(_seekingHash, _seeking);
            _animator.SetInteger(_attackHash, _attackType);
        }
        _satisfaction = Mathf.Max(0, _satisfaction - ((_depletionRate * Time.deltaTime)/100.0f)*Mathf.Pow(_speed,3.0f));
    }

    /*********************************************************/
   
    //pos--the position of taking dmg, force --incoming dmg velocity
    public override void TakeDamage(Vector3 pos, Vector3 force, int dmg, Rigidbody body, CharacterManager manager, int hitDir = 0)
    {
        if (GameSceneManager.instance && GameSceneManager.instance.bloodParticle)
        {
            //emit particle when hit
            ParticleSystem system = GameSceneManager.instance.bloodParticle;
            system.transform.position = pos;
            var settings = system.main;
            settings.simulationSpace = ParticleSystemSimulationSpace.World;
            system.Emit(60);
        }
        health -= dmg;
        
        float hitStrenth = force.magnitude;
        //if the force of the weapon if larger than 1
        bool isRagdoll = (hitStrenth>1.0f);
        if (health <= 0) isRagdoll = true;
        //stop nav agent
        if(_navAgent) _navAgent.speed = 0;
        if (isRagdoll)
        {
            if (_navAgent) _navAgent.enabled = false;
            if (_animator) _animator.enabled = false;
            if (_collider) _collider.enabled = false;
            //not anymore tracking target, just reset state
            //may go to alerted state

            
            inMeleeRange = false;
            foreach (Rigidbody body_i in _bodyParts)
            {
                //not gravity or other force
                body_i.isKinematic = false;
            }

            if (hitStrenth > 1.0f)
            {
                body.AddForce(force, ForceMode.Impulse);
            }
        }
    }

}
