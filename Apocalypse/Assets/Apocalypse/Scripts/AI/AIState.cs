using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIState : MonoBehaviour
{
    //protected fields
    protected AIStateMachine _stateMachine;

    //public
    /*********************************************************/
    //call by parent state machine to assign its reference
    public virtual void SetStateMachine(AIStateMachine stateMachine) { _stateMachine = stateMachine; }

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

    /*********************************************************/
    // Converts the passed sphere collider's pos and radius into world space and take hiearchical scale into account
    public static void ConvertSphereColliderToWorldSpace(SphereCollider col,out Vector3 pos,out float radius)
    {
        //default
        pos = Vector3.zero;
        radius = 0.0f;

        //if no valid collider, return
        if (col == null) return;

        //calculate world space position of sphere center
        pos = col.transform.position;
        pos.x += col.center.x * col.transform.lossyScale.x;
        pos.y += col.center.y * col.transform.lossyScale.y;
        pos.z += col.center.z * col.transform.lossyScale.z;

        //for radius center
        radius = Mathf.Max(col.radius * col.transform.lossyScale.x,
                           col.radius * col.transform.lossyScale.y);
        radius = Mathf.Max(radius, col.radius * col.transform.lossyScale.z);
    }



    public virtual void OnAnimatorIKUpdated() { }
    public virtual void OnTriggerEvent(AITriggerEventType eventType, Collider other) { }
    public virtual void OnDesinationReached(bool isReached) { }

    public abstract AIStateType GetStateType();
    public abstract AIStateType OnUpdate();



}
