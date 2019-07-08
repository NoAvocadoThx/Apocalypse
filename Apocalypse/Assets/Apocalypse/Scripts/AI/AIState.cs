using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIState : MonoBehaviour
{
    //protected fields
    protected AIStateMachine _stateMachine;

    //public
    //call by parent state machine to assign its reference
    public void SetStateMachine(AIStateMachine stateMachine) { _stateMachine = stateMachine; }

    //default handlers
    public virtual void OnEnterState() { }
    public virtual void OnExitState() { }

    // Called by the parent state machine to allow root motion processing
    public virtual void OnAnimatorUpdated()
    {
        // Get the number of meters the root motion has updated for this update and
        // divide by deltaTime to get meters per second. We then assign this to
        // the nav agent's velocity.
        if (_stateMachine.useRootPosition)
            _stateMachine.navAgent.velocity = _stateMachine.animator.deltaPosition/Time.deltaTime;

        // Grab the root rotation from the animator and assign as our transform's rotation.
        if (_stateMachine.useRootRotation)
            _stateMachine.transform.rotation = _stateMachine.animator.rootRotation;
    }
    public virtual void OnAnimatorIKUpdated() { }
    public virtual void OnTriggerEvent(AITriggerEventType eventType, Collider other) { }
    public virtual void OnDesinationReached(bool isReached) { }

    public abstract AIStateType GetStateType();
    public abstract AIStateType OnUpdate();



}
