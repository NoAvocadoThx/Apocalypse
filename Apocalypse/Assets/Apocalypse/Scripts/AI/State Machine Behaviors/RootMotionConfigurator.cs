using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// ------------------------------------------------------------
// CLASS	:	RootMotionConfigurator
// DESC		:	A State Machine Behaviour that communicates
//				with an AIStateMachine derived class to
//				allow for enabling/disabling root motion on
//				a per animation state basis.
// ------------------------------------------------------------
public class RootMotionConfigurator : AIStateMachineLink
{
    [SerializeField] private int _rootPostion = 0;
    [SerializeField] private int _rootRotation = 0;

    private bool _rootMotionProcessed = false;

    // Called prior to the first frame the
    // animation assigned to this state.
    public override void OnStateEnter(Animator animator, AnimatorStateInfo animStateInfo, int layerIndex)
    {
        if (_stateMachine)
        {
            //Debug.Log(_stateMachine.GetType().ToString());
            _stateMachine.AddRootMotionRequest(_rootPostion, _rootRotation);
            _rootMotionProcessed = true;
        }
    }

    // Called on the last frame of the animation prior
    // to leaving the state.
    public override void OnStateExit(Animator animator, AnimatorStateInfo animStateInfo, int layerIndex)
    {
        if (_stateMachine&&_rootMotionProcessed)
        {
            _stateMachine.AddRootMotionRequest(-_rootPostion, -_rootRotation);
            _rootMotionProcessed = false;
        }
    }
}
