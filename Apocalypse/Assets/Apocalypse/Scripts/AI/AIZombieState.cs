using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIZombieState : AIState
{


    /*********************************************************/
    //  Called by the parent state machine when threats enter/stay/exit the zombie's
    //	sensor trigger, This will be any colliders assigned to the Visual or Audio
    //	Aggravator layers or the player.
    //	It examines the threat and stored it in the parent machine Visual or Audio
    //	threat members if found to be a higher priority threat.
    public override void OnTriggerEvent(AITriggerEventType eventType, Collider other)
    {
        if (!_stateMachine) return;
        // We are not interested in exit events so only step in and process if its an
        // enter or stay.
        if (eventType != AITriggerEventType.Exit)
        {

            AITargetType curType = _stateMachine.visualThreat.type;
            // Is the collider that has entered our sensor a player
            if (other.CompareTag("Player"))
            {
                // Get distance from the sensor origin to the collider
                float distance = Vector3.Distance(_stateMachine.sensorPosition, other.transform.position);
                // If the currently stored threat is not a player or if this player is closer than a player
                // previously stored as the visual threat...this could be more important
                if (curType != AITargetType.Visual_Player || (curType == AITargetType.Visual_Player && distance<_stateMachine.visualThreat.distance))
                {

                }
            }
        }
    }
}
