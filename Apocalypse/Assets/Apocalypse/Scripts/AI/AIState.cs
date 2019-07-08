using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIState : MonoBehaviour
{
    //protected fields
    protected AIStateMachine _stateMachine;

    //public
    public void SetStateMachine(AIStateMachine stateMachine) { _stateMachine = stateMachine; }

    //default handlers
    public virtual void OnEnterState() { }
    public virtual void OnExitState() { }
    public virtual void OnAnimatorUpdated() { }
    public virtual void OnAnimatorIKUpdated() { }
    public virtual void OnTriggerEvent(AITriggerEventType eventType, Collider other) { }
    public virtual void OnDesinationReached(bool isReached) { }

    public abstract AIStateType GetStateType();
    public abstract AIStateType OnUpdate();



}
