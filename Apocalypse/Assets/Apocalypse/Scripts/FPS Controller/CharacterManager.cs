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
    [SerializeField] private PlayerHUD _playerHUD = null;

    //private 
    private Collider _collider = null;
    private FPSController _fpsController = null;
    private CharacterController _characterController = null;
    private GameSceneManager _gameSceneManager = null;
    private int _AIBodypartLayer = -1;
    private int _interactiveMask = 0;
    // private bool canDoDmg = false;




    public float health { get { return _health; } }
    public float stamina { get { return _fpsController != null ? _fpsController.stamina : 0.0f; } }
    /*********************************************************/


    // Start is called before the first frame update
    void Start()
    {
        _collider = GetComponent<Collider>();
        _fpsController = GetComponent<FPSController>();
        _characterController = GetComponent<CharacterController>();
        _gameSceneManager = GameSceneManager.instance;
        _AIBodypartLayer = LayerMask.NameToLayer("AI Body Part");
        _interactiveMask = 1 << LayerMask.NameToLayer("Interactive");
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

        // Get rid of really annoying mouse cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        // Start fading in
        if (_playerHUD) _playerHUD.Fade(2.0f, ScreenFadeType.FadeIn);
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
        //Interactive stuff
        Ray ray;
        RaycastHit hit;
        RaycastHit[] hits;
        ray = _cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        // Calculate Ray Length
        //when look up or down the length will increase
        float rayLength = Mathf.Lerp(1.0f, 1.8f, Mathf.Abs(Vector3.Dot(_cam.transform.forward, Vector3.up)));
        // Cast Ray and collect ALL hits
        hits = Physics.RaycastAll(ray, rayLength, _interactiveMask);
        //Process the hits for the one with the highest priorty
        //when it has hits
        if (hits.Length > 0)
        {
            // Used to record the index of the highest priorty
            int highestPriority = int.MinValue;
            InteractiveItem priorityObject = null;

            // Iterate through each hit
            for (int i = 0; i < hits.Length; i++)
            {
                // Process next hit
                hit = hits[i];

                // Fetch its InteractiveItem script from the database
                InteractiveItem interactiveObject = _gameSceneManager.GetInteractiveItem(hit.collider.GetInstanceID());

                // If this is the highest priority object so far then remember it
                if (interactiveObject != null && interactiveObject.priority > highestPriority)
                {
                    priorityObject = interactiveObject;
                    highestPriority = priorityObject.priority;
                }
            }

            // If we found an object then display its text and process any possible activation
            if (priorityObject != null)
            {
                if (_playerHUD)
                    _playerHUD.SetInteractionText(priorityObject.GetText());
                //use the item
                if (Input.GetButtonDown("Use"))
                {
                    priorityObject.Activate(this);
                }
            }
        }
        else
        {
            if (_playerHUD)
                _playerHUD.SetInteractionText(null);
        }

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

        if (_playerHUD) _playerHUD.Invalidate(this);

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
