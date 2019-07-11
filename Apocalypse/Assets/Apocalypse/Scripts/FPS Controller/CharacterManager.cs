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

    //private 
    private Collider _collider = null;
    private FPSController _fpsController = null;
    private CharacterController _characterController = null;
    private GameSceneManager _gameSceneManager = null;

    // Start is called before the first frame update
    void Start()
    {
        _collider = GetComponent<Collider>();
        _fpsController = GetComponent<FPSController>();
        _characterController = GetComponent<CharacterController>();
        _gameSceneManager = GameSceneManager.instance;
        if (_gameSceneManager)
        {
            PlayerInfo info = new PlayerInfo();
            info.camera = _cam;
            info.characterManager = this;
            info.collider = _collider;
            info.meleeTrigger = _meleeTrigger;
            //register playerInfo to game scene manager
            _gameSceneManager.RegisterPlayerInfo(_collider.GetInstanceID(),info);
        }
    }

   public void TakeDamage(float amount)
    {
        _health = Mathf.Max(_health - (amount*Time.deltaTime), 0.0f);
        if (_cameraScreenBlood)
        {
            _cameraScreenBlood.minBloodAmount = 1.0f - (_health / 100.0f);
            _cameraScreenBlood.bloodAmount = Mathf.Min(_cameraScreenBlood.minBloodAmount + 0.3f,1);
        }
    }
}
