using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{

    //inspector
    [SerializeField] private CapsuleCollider _meleeTrigger = null;
    [SerializeField] private CameraScreenEffect _cameraScreenBlood = null;
    [SerializeField] private Camera _cam = null;
    [SerializeField] private float _health = 100.0f;
    [SerializeField] private AISoundEmitter _soundEmitter = null;
    [SerializeField] private float _walkRadius = 0.5f;
    [SerializeField] private float _runRadius = 7.0f;
    [SerializeField] private float _landingRadius = 12.0f;

    //Pain Damage audio
    [SerializeField] private AudioCollection _dmgCollection = null;
    [SerializeField] private AudioCollection _painCollection = null;
    [SerializeField] private float _nextPainSoundTime = 0.0f;
    [SerializeField] private float _painSoundOffset = 0.35f;

    //private 
    private Collider _collider = null;
    private FPSController _fpsController = null;
    private CharacterController _characterController = null;
    private GameSceneManager _gameSceneManager = null;
    private int _AIBodypartLayer = -1;
    // private bool canDoDmg = false;


    /*********************************************************/


    // Start is called before the first frame update
    void Start()
    {
        _collider = GetComponent<Collider>();
        _fpsController = GetComponent<FPSController>();
        _characterController = GetComponent<CharacterController>();
        _gameSceneManager = GameSceneManager.instance;
        _AIBodypartLayer = LayerMask.NameToLayer("AI Body Part");
        if (_gameSceneManager)
        {
            PlayerInfo info = new PlayerInfo();
            info.camera = _cam;
            info.characterManager = this;
            info.collider = _collider;
            info.meleeTrigger = _meleeTrigger;
            //register playerInfo to game scene manager
            _gameSceneManager.RegisterPlayerInfo(_collider.GetInstanceID(), info);
        }
    }
    /*********************************************************/


    public void TakeDamage(float amount, bool doDamage, bool doPain)
    {
        _health = Mathf.Max(_health - (amount * Time.deltaTime), 0.0f);
        if (_fpsController)
        {
            _fpsController.dragMultiplier = 0.0f;
        }
        if (_cameraScreenBlood)
        {
            _cameraScreenBlood.minBloodAmount = 1.0f - (_health / 100.0f);
            _cameraScreenBlood.bloodAmount = Mathf.Min(_cameraScreenBlood.minBloodAmount + 0.3f, 1);
        }
        //sounds
        if (AudioManager.instance)
        {
            if (doDamage && _dmgCollection)
            {
                AudioManager.instance.PlayOneShotSound(_dmgCollection.audioGroup, _dmgCollection.audioClip, transform.position,
                                                       _dmgCollection.volume, _dmgCollection.spatialBlend, _dmgCollection.priority);
            }
            if (doPain && _painCollection != null && _nextPainSoundTime < Time.time)
            {
                AudioClip painClip = _painCollection.audioClip;
                if (painClip)
                {
                    _nextPainSoundTime = Time.time + painClip.length;
                    StartCoroutine(AudioManager.instance.PlayOneShotSoundDelayed(_painCollection.audioGroup, painClip,
                                                                                      transform.position,
                                                                                      _painCollection.volume,
                                                                                      _painCollection.spatialBlend,
                                                                                      _painSoundOffset,
                                                                                      _painCollection.priority));
                }
            }
        }
    }



    /*********************************************************/
    public void Update()
    {

        //when press left key
        if (Input.GetMouseButtonDown(0) && _fpsController.movementStatus != PlayerMoveStatus.Running)
        {
            DoDamage();



        }
        if (_fpsController || _soundEmitter != null)
        {
            //if player is in low health
            //blood will attract zombies
            float newRadius = Mathf.Max((100.0f - _health) / 10.0f, _walkRadius);
            switch (_fpsController.movementStatus)
            {
                //set sound radius
                case PlayerMoveStatus.Landing: newRadius = Mathf.Max(newRadius, _landingRadius); break;
                case PlayerMoveStatus.Running: newRadius = Mathf.Max(newRadius, _runRadius); break;
                    //case PlayerMoveStatus.Walking:newRadius = Mathf.Max(newRadius, _walkRadius);break;


            }
            _soundEmitter.SetRadius(newRadius);
            _fpsController.dragMultiplierLimit = Mathf.Max(_health / 100.0f, 0.25f);
        }



    }


    /*********************************************************/

    public void DoDamage(int hitDir = 0)
    {

        if (!_cam || !_gameSceneManager)
        {
            Debug.Log("no CAM or no gameSceneManager");
        }
        //ray
        Ray ray;
        RaycastHit hit;
        bool isHit = false;
        //shoot ray from center of screen
        ray = _cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        //hit-- the collider we hit
        //range -- <1000m
        //only take mask for fourth parameter
        isHit = Physics.Raycast(ray, out hit, 1000.0f, 1 << _AIBodypartLayer);
        if (isHit)
        {
            AIStateMachine stateMachine = _gameSceneManager.GetAIStateMachine(hit.rigidbody.GetInstanceID());
            if (stateMachine)
            {
                //take damage
                stateMachine.TakeDamage(hit.point, ray.direction * 1.0f, 15, hit.rigidbody, this, 0);
            }
        }
    }
}
