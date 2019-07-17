using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ComChannels { ComChannel1, ComChannel2, ComChannel3, ComChannel4 }

public class AIStateMachineLink : StateMachineBehaviour
{
    protected AIStateMachine _stateMachine;
    public AIStateMachine stateMachine { set { _stateMachine = value; } }
}
