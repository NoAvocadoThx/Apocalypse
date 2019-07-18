using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIDamageTrigger : MonoBehaviour
{

    [SerializeField] string _parameter = "";
    [SerializeField] int _bloodParticleAmount = 10;
    [SerializeField] float _dmgAmount = 1.0f;
    [SerializeField] bool _doDamageSound = true;
    [SerializeField] bool _doPainSound = true;

    //private
    AIStateMachine _stateMachine = null;
    Animator _animator = null;
    int _parameterHash = -1;
    GameSceneManager _gameSceneManager = null;
    private bool _firstContact = false;

    /*********************************************************/
    private void Start()
    {
        _stateMachine = transform.root.GetComponentInChildren<AIStateMachine>();
        if (_stateMachine != null)
        {
            _animator = _stateMachine.animator;
        }
        _parameterHash = Animator.StringToHash(_parameter);
        _gameSceneManager = GameSceneManager.instance;
    }


    /*********************************************************/
    void OnTriggerEnter(Collider col)
    {
        if (!_animator)
            return;
        //if target is player and taking dmg from player
        if (col.gameObject.CompareTag("Player") && _animator.GetFloat(_parameterHash) > 0.9f)
            _firstContact = true;
    }
    /*********************************************************/
    void OnTriggerStay(Collider other)
    {
        if (!_animator) return;
        //if the curve we set in the animation curve is larger than 0.9f(to be safe but it is actually 1)
        //we do damage
        if (other.gameObject.CompareTag("Player") && _animator.GetFloat(_parameterHash) > 0.9f)
        {
            if (GameSceneManager.instance && GameSceneManager.instance.bloodParticle)
            {
                ParticleSystem system = GameSceneManager.instance.bloodParticle;

                system.transform.position = transform.position;
                system.transform.rotation = Camera.main.transform.rotation;
                var settings = system.main;
                settings.simulationSpace = ParticleSystemSimulationSpace.World;
                system.Emit(_bloodParticleAmount);
                Debug.Log("Attacked Player!");
            }

            if (_gameSceneManager)
            {
                PlayerInfo info = _gameSceneManager.GetPlayerInfo(other.GetInstanceID());
                if (info != null && info.characterManager != null)
                {
                    //Debug.Log("sd Player!");
                    info.characterManager.TakeDamage(_dmgAmount, _doDamageSound && _firstContact, _doPainSound);
                }
            }
            _firstContact = false;

        }

     
    }
}
