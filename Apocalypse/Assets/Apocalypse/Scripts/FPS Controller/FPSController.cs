using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public enum PlayerMoveStatus { NotMoving,Walking,Running,NotGround,Landing}

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

    protected void Start()
    {
        
    }


}
