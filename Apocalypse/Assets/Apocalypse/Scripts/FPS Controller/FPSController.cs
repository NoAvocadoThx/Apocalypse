using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public enum PlayerMoveStatus { NotMoving,Walking,Running,NotGrounded,Landing}

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    [SerializeField] private float _walkSpeed = 1.0f;
    [SerializeField] private float _runSpeed = 4.5f;
    [SerializeField] private float _jumpSpeed = 7.5f;
    [SerializeField] private float _stickToGroundForce = 5.0f;
    [SerializeField] private float _gravityMultiplier = 2.5f;
    [SerializeField] private UnityStandardAssets.Characters.FirstPerson.MouseLook _mouseLook;

    //Private

    private Camera _camera = null;
    private bool _jumpButtonPressed = false;
    private Vector2 _inputVector = Vector2.zero;
    private Vector3 _moveDirection = Vector3.zero;
    private bool _previouslyGrounded = false;
    private bool _isWalking = false;
    private bool _isJumping = false;
    private float _fallingTimer = 0.0f;

    private CharacterController _characterController = null;
    private PlayerMoveStatus _movementStatus = PlayerMoveStatus.NotMoving;

    //getters
    public PlayerMoveStatus movementStatus { get { return _movementStatus; } }
    public float walkSpeed { get { return _walkSpeed; } }
    public float runSpeed { get { return _runSpeed; } }


    /*********************************************************/
    protected void Start()
    {
        //initialize compennents
        _characterController = GetComponent<CharacterController>();
        _camera = Camera.main;
        _movementStatus = PlayerMoveStatus.NotMoving;
        _fallingTimer = 0.0f;
        _mouseLook.Init(transform, _camera.transform);
    }


    /*********************************************************/
    protected void FixedUpdate()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float verical = Input.GetAxis("Vertical");
        bool wasWalking = _isWalking;
        //hold left shift to run
        _isWalking = !Input.GetKey(KeyCode.LeftShift);
        //set desired speed
        float speed = _isWalking ? _walkSpeed : _runSpeed;
        _inputVector = new Vector2(horizontal, verical);
        // normalize input if it exceeds 1 in combined length:
        if (_inputVector.sqrMagnitude > 1) _inputVector.Normalize();

        // Always move along the camera forward as it is the direction that it being aimed at
        Vector3 desiredMove = transform.forward * _inputVector.y + transform.right * _inputVector.x;

        // Incase we encounter a slope 
        // Get a normal for the surface that is being touched to move along it
        RaycastHit hitInfo;
        if (Physics.SphereCast(transform.position, _characterController.radius, Vector3.down, out hitInfo, _characterController.height / 2f, 1))
            desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;
        // Scale movement by our current speed (walking value or running value)
        _moveDirection.x = desiredMove.x * speed;
        _moveDirection.z = desiredMove.z * speed;

        // If character is on ground
        if (_characterController.isGrounded)
        {
            // Apply severe down force to keep control sticking to floor
            _moveDirection.y = -_stickToGroundForce;

            // If the jump button was pressed then apply speed in up direction
            // and set isJumping to true. Reset jump button status
            if (_jumpButtonPressed)
            {
                _moveDirection.y = _jumpSpeed;
                _jumpButtonPressed = false;
                _isJumping = true;
                // TODO: Play Jumping Sound
            }
        }
        else
        {
            // Otherwise we are not on the ground so apply standard system gravity multiplied
            // by our gravity modifier
            _moveDirection += Physics.gravity * _gravityMultiplier * Time.fixedDeltaTime;
        }

        // Move the Character Controller
        _characterController.Move(_moveDirection * Time.fixedDeltaTime);
    }

    /*********************************************************/
    protected void Update()
    {
        // If we are falling increment timer
        if (_characterController.isGrounded) _fallingTimer = 0.0f;
        else _fallingTimer += Time.deltaTime;

        // Allow Mouse Look a chance to process mouse and rotate camera
        if (Time.timeScale > Mathf.Epsilon)
            _mouseLook.LookRotation(transform, _camera.transform);

        // Process the Jump Button
        // the jump state needs to read here to make sure it is not missed
        if (!_jumpButtonPressed)
            _jumpButtonPressed = Input.GetButtonDown("Jump");

        // Calculate Character Status
        if (!_previouslyGrounded && _characterController.isGrounded)
        {
            //not falling if falling time is smaller than 0.5s
            if (_fallingTimer > 0.5f)
            {
                // TODO: Play Landing Sound
            }

            _moveDirection.y = 0f;
            _isJumping = false;
            _movementStatus = PlayerMoveStatus.Landing;
        }
        //set player movement status
        else
            if (!_characterController.isGrounded)
            _movementStatus = PlayerMoveStatus.NotGrounded;
        else
            if (_characterController.velocity.sqrMagnitude < 0.01f)
            _movementStatus = PlayerMoveStatus.NotMoving;
        else
            if (_isWalking)
            _movementStatus = PlayerMoveStatus.Walking;
        else
            _movementStatus = PlayerMoveStatus.Running;

        _previouslyGrounded = _characterController.isGrounded;
    }

}
