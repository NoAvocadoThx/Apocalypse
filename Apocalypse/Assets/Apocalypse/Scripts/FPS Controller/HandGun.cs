using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class HandGun : MonoBehaviour
{
    
   // [SerializeField] private AISoundEmitter _soundEmitter = null;
    [SerializeField] float _fireRate = 1.0f;

    private FPSController _fPSController = null;
    private CharacterManager _charactermanager = null;
    private Animator _animator = null;
    public float _nextTimeToShoot=0.0f;
    private float _timer = 0.0f;

    public AudioSource audioSource = new AudioSource();
    public bool canShoot = false;
    //Hashes
    // Hashes
    private int _isWalkingHash = Animator.StringToHash("isWalking");
    private int _isRunningHash = Animator.StringToHash("isRunning");
    private int _isIdleHash = Animator.StringToHash("isIdle");
    private int _isShootingHash = Animator.StringToHash("isShooting");
    private int _isCrouchingHash = Animator.StringToHash("isCrouching");
    private int _isAimingHash = Animator.StringToHash("isAiming");


   
    /*********************************************************/
    // Start is called before the first frame update
    void Start()
    {
        _fPSController = GetComponentInParent<FPSController>();
        _charactermanager = GetComponentInParent<CharacterManager>();
        _animator = GetComponent<Animator>();
        if (_fPSController == null) { Debug.Log("no fps controller"); return; }
        if (_charactermanager == null) { Debug.Log("no Character manager"); return; }
        if (_animator == null) { Debug.Log("No animator"); return; }

    }

    /*********************************************************/
    // Update is called once per frame
    void Update()
    {
        _timer += Time.deltaTime;
        shoot();
        _animator.SetBool(_isIdleHash, (_fPSController.movementStatus == PlayerMoveStatus.NotMoving));
        _animator.SetBool(_isWalkingHash, (_fPSController.movementStatus == PlayerMoveStatus.Walking));
       
        _animator.SetBool(_isRunningHash, (_fPSController.movementStatus == PlayerMoveStatus.Running));
        _animator.SetBool(_isCrouchingHash, (_fPSController.movementStatus == PlayerMoveStatus.Crouching));
        
    }

    /*********************************************************/
    //shoot
    public void shoot()
    {

        //shoot the weapon if press and hold the mouse button
        if (Input.GetMouseButtonDown(0) && Time.time > _nextTimeToShoot && _fPSController.movementStatus != PlayerMoveStatus.Running)
        {
            canShoot = true;
            _nextTimeToShoot = Time.time + 1.0f / _fireRate;
            _animator.SetBool(_isShootingHash, true);
           
                audioSource.Play();
            

           


        }
        else
        {
            canShoot = false;
            _animator.SetBool(_isShootingHash, false);
        }


    }
}
