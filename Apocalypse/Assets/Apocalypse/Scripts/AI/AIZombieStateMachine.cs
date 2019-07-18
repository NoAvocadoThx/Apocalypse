using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum AIBoneControlType { Animated,Ragdoll, RagdollToAnim}
//at start of reanimation, take snapshot of the zombie
public class BodyPartSnapshot
{
    public Transform transform;
    public Vector3 position;
    public Quaternion rotation;
    public Quaternion localRotation;

}

//State Machine used by zombie characters
public class AIZombieStateMachine : AIStateMachine
{
    // Inspector parameters aka zombie attributes by defult
    [SerializeField][Range(10.0f,360.0f)] float _fov = 50.0f;
    [SerializeField] [Range(0.0f, 1.0f)] float _sight = 0.5f;
    [SerializeField] [Range(0.0f, 1.0f)] float _hearing = 1.0f;
    [SerializeField] [Range(0.0f, 1.0f)] float _aggression = 0.5f;
    [SerializeField] [Range(0, 100)] int _health = 100;
    [SerializeField] [Range(0, 100)] int _lowerBodyDmg = 0;
    [SerializeField] [Range(0, 100)] int _upperBodyDmg = 0;
    //body dmg animation
    [SerializeField] [Range(0, 100)] int _upperThreshold = 30;
    [SerializeField] [Range(0, 100)] int _limpThreshold = 60;
    [SerializeField] [Range(0, 100)] int _crawlThreshold = 85;

    [SerializeField] [Range(0.0f, 1.0f)] float _intelligence = 0.5f;
    [SerializeField] [Range(0.0f, 1.0f)] float _satisfaction = 1.0f;
    [SerializeField] float _replenishRate = 0.5f;
    [SerializeField] float _depletionRate = 0.1f;
    [SerializeField] float _reanimationBlendTime = 1.5f;
    [SerializeField] float _reanimationWaitTime = 3.0f;
    [SerializeField] LayerMask _geometryLayers = 0;
    [SerializeField] AISoundEmitter _screamPrefab = null;

    // Private
    private int _seeking = 0;
    private bool _feeding = false;
    private bool _crawling = false;
    private int _attackType = 0;
    private float _speed = 0.0f;
    

    //Ragdoll
    private AIBoneControlType _boneControlType = AIBoneControlType.Animated;
    private float _ragdollEndTime = float.MinValue;
    private Vector3 _ragdollHipPosition;
    private Vector3 _ragdollFeetPosition;
    private Vector3 _ragdollHeadPosition;
    private IEnumerator _reanimationCoroutine = null;
    private float _mecanimTransitionTime = 0.1f;
    private List<BodyPartSnapshot> _bodyPartSnapShots = new List<BodyPartSnapshot>();


    // Hashes
    private int _speedHash = Animator.StringToHash("Speed");
    private int _seekingHash = Animator.StringToHash("Seeking");
    private int _feedingHash = Animator.StringToHash("Feeding");
    private int _attackHash = Animator.StringToHash("Attack");
    private int _crawlHash = Animator.StringToHash("Crawling");
    private int _hitTypeHash = Animator.StringToHash("HitType");
    private int _hitTriggerHash = Animator.StringToHash("Hit");
    private int _lowerBodyDamageHash = Animator.StringToHash("Low Body Dmg");
    private int _upperBodyDamageHash = Animator.StringToHash("High Body Dmg");
    private int _reanimateFromBackHash = Animator.StringToHash("Reanimate From Back");
    private int _reanimateFromFrontHash = Animator.StringToHash("Reanimate From Front");
    private int _stateHash = Animator.StringToHash("State");
    private int _upperBodyLayer = -1;
    private int _lowerBodyLayer = -1;


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
    public bool isCrawling { get { return (_lowerBodyDmg >= _crawlThreshold); } }
  
    public float speed
    {
        get { return _speed; }
        set { _speed = value; }
    }



    /*********************************************************/
    protected override void Start()
    {
        base.Start();
        _lowerBodyLayer = _animator.GetLayerIndex("Lower Body");
        _upperBodyLayer = _animator.GetLayerIndex("Upper Body");
        // Create BodyPartSnapShot List
        //get reference to zombie
        if (_rootBone != null)
        {
            //get all children
            Transform[] transforms = _rootBone.GetComponentsInChildren<Transform>();
            foreach (Transform trans in transforms)
            {
                BodyPartSnapshot snapShot = new BodyPartSnapshot();
                snapShot.transform = trans;
                _bodyPartSnapShots.Add(snapShot);
            }
        }
        UpdatedAnimationDmg();
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
            _animator.SetInteger(_stateHash,(int) _curStateType);
        }
        _satisfaction = Mathf.Max(0, _satisfaction - ((_depletionRate * Time.deltaTime)/100.0f)*Mathf.Pow(_speed,3.0f));
    }


    /*********************************************************/
    protected virtual void LateUpdate()
    {
        if (_boneControlType == AIBoneControlType.RagdollToAnim)
        {
            //if in wait time to avoid wierd blend animation
            if (Time.time <= _ragdollEndTime + _mecanimTransitionTime)
            {
                Vector3 animatedToRagdoll = _ragdollHipPosition - _rootBone.position;
                Vector3 newRootPosition = transform.position + animatedToRagdoll;

                RaycastHit[] hits = Physics.RaycastAll(newRootPosition + (Vector3.up * 0.25f), Vector3.down, float.MaxValue, _geometryLayers);
                newRootPosition.y = float.MinValue;
                foreach (RaycastHit hit in hits)
                {
                    if (!hit.transform.IsChildOf(transform))
                    {
                        newRootPosition.y = Mathf.Max(hit.point.y, newRootPosition.y);
                    }
                }

                NavMeshHit navMeshHit;
                //caluclate the reanimation position plus the baseOffset.
                Vector3 baseOffset = Vector3.zero;
                if (_navAgent) baseOffset.y = _navAgent.baseOffset;
                if (NavMesh.SamplePosition(newRootPosition, out navMeshHit, 25.0f, NavMesh.AllAreas))
                {
                    transform.position = navMeshHit.position+baseOffset;
                }
                else
                {
                    transform.position = newRootPosition+baseOffset;
                }


                Vector3 ragdollDirection = _ragdollHeadPosition - _ragdollFeetPosition;
                ragdollDirection.y = 0.0f;

                Vector3 meanFeetPosition = 0.5f * (_animator.GetBoneTransform(HumanBodyBones.LeftFoot).position + _animator.GetBoneTransform(HumanBodyBones.RightFoot).position);
                Vector3 animatedDirection = _animator.GetBoneTransform(HumanBodyBones.Head).position - meanFeetPosition;
                animatedDirection.y = 0.0f;

                //Try to match the rotations. Note that we can only rotate around Y axis, as the animated characted must stay upright,
                //hence setting the y components of the vectors to zero. 
                transform.rotation *= Quaternion.FromToRotation(animatedDirection.normalized, ragdollDirection.normalized);
            }

            // Calculate Interpolation value
            float blendAmount = Mathf.Clamp01((Time.time - _ragdollEndTime - _mecanimTransitionTime) / _reanimationBlendTime);

            // Calculate blended bone positions by interplating between ragdoll bone snapshots and animated bone positions
            foreach (BodyPartSnapshot snapshot in _bodyPartSnapShots)
            {
                if (snapshot.transform == _rootBone)
                {
                    snapshot.transform.position = Vector3.Lerp(snapshot.position, snapshot.transform.position, blendAmount);
                }

                snapshot.transform.rotation = Quaternion.Slerp(snapshot.rotation, snapshot.transform.rotation, blendAmount);
            }


            // Conditional to exit reanimation mode
            if (blendAmount == 1.0f)
            {
                _boneControlType = AIBoneControlType.Animated;
                if (_navAgent) _navAgent.enabled = true;
                if (_collider) _collider.enabled = true;

                AIState newState = null;
                if (_states.TryGetValue(AIStateType.Alerted, out newState))
                {
                    if (_curState != null) _curState.OnExitState();
                    newState.OnEnterState();
                    _curState = newState;
                    _curStateType = AIStateType.Alerted;
                }
            }
        }
    }

   


    /*********************************************************/

    protected void UpdatedAnimationDmg()
    {
        //max dmg is 100
        if (_upperBodyDmg > 100) _upperBodyDmg = 100;
        if (_lowerBodyDmg > 100) _lowerBodyDmg = 100;
        if (_animator)
        {
            //play lower body limp animation
            if (_lowerBodyLayer != -1)
            {
                _animator.SetLayerWeight(_lowerBodyLayer, (_lowerBodyDmg > _limpThreshold && _lowerBodyDmg < _crawlThreshold) ? 1.0f : 0.0f);

            }
            //play upper body limp animation
            if (_upperBodyLayer != -1)
            {
                _animator.SetLayerWeight(_upperBodyLayer, (_upperBodyDmg > _limpThreshold && _upperBodyDmg < _crawlThreshold) ? 1.0f : 0.0f);

            }
            _animator.SetBool(_crawlHash, isCrawling);
            _animator.SetInteger(_lowerBodyDamageHash, _lowerBodyDmg);
            _animator.SetInteger(_upperBodyDamageHash, _upperBodyDmg);
            //enable layers
            if (_lowerBodyDmg > _limpThreshold && _lowerBodyDmg < _crawlThreshold) SetLayerActive("Lower Body", true);
            else SetLayerActive("Lower Body", false);

            if (_upperBodyDmg > _upperThreshold && _upperBodyDmg < _crawlThreshold)
                SetLayerActive("Upper Body", true);
            else SetLayerActive("Upper Body", false);


        }

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
        float hitStrenth = force.magnitude;
        //health -= dmg;
        //is in ragdoll state
        if (_boneControlType == AIBoneControlType.Ragdoll)
        {
            if (body)
            {
                if (hitStrenth > 1.0f)
                {
                    body.AddForce(force, ForceMode.Impulse);
                }

                //only when player hit head, reduce health of zombie
                if (body.CompareTag("Head"))
                {
                    _health = Mathf.Max(_health - dmg, 0);
                    
                }
                //shooting upper body
                else if (body.CompareTag("Upper Body"))
                {
                    _upperBodyDmg += dmg;
                }
                //shooting lower body
                else if (body.CompareTag("Lower Body"))
                {
                    _lowerBodyDmg += dmg;
                }

                UpdatedAnimationDmg();
                if (_health > 0)
                {
                    if (_reanimationCoroutine != null)
                        StopCoroutine(_reanimationCoroutine);

                    _reanimationCoroutine = Reanimate();
                    StartCoroutine(_reanimationCoroutine);
                }

            }
            return;
        }

        //calculate attacker local pos
        Vector3 attackerPos = transform.TransformPoint(manager.transform.position);
        //local pos where the zombie is hit
        Vector3 hitPos = transform.TransformPoint(pos);

        
        //if the force of the weapon if larger than 1
        bool isRagdoll = (hitStrenth>1.0f);
        //if (health <= 0) isRagdoll = true;
        if (body)
        {
           

            //only when player hit head, reduce health of zombie
            if (body.CompareTag("Head"))
            {
                _health = Mathf.Max(_health - dmg, 0);
                //when zombie die, ragdoll
                if (_health == 0)
                {
                    isRagdoll = true;
                }

            }
            //shooting upper body
            //not ragdoll when shooting upper body
            else if (body.CompareTag("Upper Body"))
            {
                _upperBodyDmg += dmg;
                UpdatedAnimationDmg();
            }
            //shooting lower body
            //ragdoll when shooting lower body
            else if (body.CompareTag("Lower Body"))
            {
                _lowerBodyDmg += dmg;
                UpdatedAnimationDmg();
                isRagdoll = true;
            }

           
          

        }

        //ragdoll to be true
        //if down, crawling, cinematic enabled or attack from behind
        if (_boneControlType != AIBoneControlType.Animated || isCrawling || IsLayerActive("Cinematic") || attackerPos.z < 0)
            isRagdoll = true;

        //not ragdoll
        if (!isRagdoll)
        {
            float angle = 0.0f;
            if (hitDir == 0)
            {
                //calculate angle of incoming attack
                Vector3 vecToHit = (pos - transform.position).normalized;
                angle = AIState.FindSignedAngle(vecToHit, transform.forward);
            }
            int hitType = 0;
            //set hit types according to attack angle
            if (body.gameObject.CompareTag("Head"))
            {
                if (angle < -10 || hitDir == -1) hitType = 1;
                else if (angle > 10 || hitDir== 1) hitType = 3;
                else hitType = 2;
            }
            else
            if (body.gameObject.CompareTag("Upper Body"))
            {
                if (angle < -20 || hitDir== -1) hitType = 4;
                else if (angle > 20 || hitDir == 1) hitType = 6;
                else hitType = 5;
            }

            if (_animator)
            {
                _animator.SetInteger(_hitTypeHash, hitType);
                _animator.SetTrigger(_hitTriggerHash);
            }

            return;
        }
       
        //stop nav agent
        //  if(_navAgent) _navAgent.speed = 0;
       else 
        {
            //clear the state
            if (_curState)
            {
                _curState.OnExitState();
                _curState = null;
                _curStateType = AIStateType.None;
            }
            if (_navAgent) _navAgent.enabled = false;
            if (_animator) _animator.enabled = false;
            if (_collider) _collider.enabled = false;
            //Mute audio roaming
            if (_layeredAudioSource!=null) _layeredAudioSource.Mute(true);


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
            _boneControlType = AIBoneControlType.Ragdoll;
            //reanimation zombies
            if (_health>0)
            {
                if (_reanimationCoroutine != null)
                    StopCoroutine(_reanimationCoroutine);

                _reanimationCoroutine = Reanimate();
                StartCoroutine(_reanimationCoroutine);
            }
        }
    }


   //Starts the reanimation procedure
    private IEnumerator Reanimate()
    {
        if (_boneControlType != AIBoneControlType.Ragdoll || !_animator) yield break;

        //wait time before reanimate
        yield return new WaitForSeconds(_reanimationWaitTime);

        // Record time at start of reanimation procedure
        _ragdollEndTime = Time.time;
        // Set all rigidbodies back to being kinematic
        foreach (Rigidbody body in _bodyParts)
        {
            body.isKinematic = true;
        }
        // Put us in reanimation mode
        _boneControlType = AIBoneControlType.RagdollToAnim;

        // Record postions and rotations of all bones prior to reanimation
        foreach (BodyPartSnapshot snapShot in _bodyPartSnapShots)
        {
            snapShot.position = snapShot.transform.position;
            snapShot.rotation = snapShot.transform.rotation;
        }
        // Record the ragdolls head and feet position
        _ragdollHeadPosition = _animator.GetBoneTransform(HumanBodyBones.Head).position;
        _ragdollFeetPosition = (_animator.GetBoneTransform(HumanBodyBones.LeftFoot).position + _animator.GetBoneTransform(HumanBodyBones.RightFoot).position) * 0.5f;
        _ragdollHipPosition = _rootBone.position;
        // Enable Animator
        _animator.enabled = true;

        if (_rootBone != null)
        {
            //check if the model's hips is facing forward(z as default)
            float forwardTest;

            switch (_rootBoneAlignment)
            {
                case AIBoneAlignmentType.ZAxis:
                    forwardTest = _rootBone.forward.y; break;
                case AIBoneAlignmentType.ZAxisInverted:
                    forwardTest = -_rootBone.forward.y; break;
                case AIBoneAlignmentType.YAxis:
                    forwardTest = _rootBone.up.y; break;
                case AIBoneAlignmentType.YAxisInverted:
                    forwardTest = -_rootBone.up.y; break;
                case AIBoneAlignmentType.XAxis:
                    forwardTest = _rootBone.right.y; break;
                case AIBoneAlignmentType.XAxisInverted:
                    forwardTest = -_rootBone.right.y; break;
                default:
                    forwardTest = _rootBone.forward.y; break;
            }

            // Set the trigger in the animator
            if (forwardTest >= 0)
                _animator.SetTrigger(_reanimateFromBackHash);
            else
                _animator.SetTrigger(_reanimateFromFrontHash);
        }
       
    }
}
