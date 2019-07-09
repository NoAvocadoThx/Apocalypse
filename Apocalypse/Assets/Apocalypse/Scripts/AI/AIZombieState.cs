using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIZombieState : AIState
{
    //Private 
    protected int _playerLayerMask = -1;
    protected int _bodyPartLayer = -1;
    protected int _visualLayerMask = -1;
    protected AIZombieStateMachine _zombieStateMachine = null;

    void Awake()
    {
        //get a mask for line of sight testing with the player. +1 will include the default layer
        _playerLayerMask = LayerMask.GetMask("Player","AI Body Part")+1;
        _visualLayerMask=LayerMask.GetMask("Player", "AI Body Part","Visual Aggravator") + 1;
        //get layer index of the AI body part
        _bodyPartLayer = LayerMask.NameToLayer("AI Body Part");
    }

    /*********************************************************/
    //Check for type compliance and store reference as derived type
    //cast _stateMachine to _AIZombieStateMachine
    public override void SetStateMachine(AIStateMachine stateMachine) {
        if (stateMachine.GetType() == typeof(AIZombieStateMachine))
        {
            base.SetStateMachine(stateMachine);
            _zombieStateMachine = (AIZombieStateMachine)stateMachine;

        }
        
    }

    /*********************************************************/
    //  Called by the parent state machine when threats enter/stay/exit the zombie's
    //	sensor trigger, This will be any colliders assigned to the Visual or Audio
    //	Aggravator layers or the player.
    //	It examines the threat and stored it in the parent machine Visual or Audio
    //	threat members if found to be a higher priority threat.
    public override void OnTriggerEvent(AITriggerEventType eventType, Collider other)
    {
        if (!_zombieStateMachine) return;
        // We are not interested in exit events so only step in and process if its an
        // enter or stay.
        if (eventType != AITriggerEventType.Exit)
        {
            //Prioprity rank: 1>2>3>...

            AITargetType curType = _zombieStateMachine.visualThreat.type;
            // 1. Is the collider that has entered our sensor a player
            if (other.CompareTag("Player"))
            {
                // Get distance from the sensor origin to the collider
                float distance = Vector3.Distance(_zombieStateMachine.sensorPosition, other.transform.position);
                // If the currently stored threat is not a player or if this player is closer than a player
                // previously stored as the visual threat...this could be more important
                if (curType != AITargetType.Visual_Player || (curType == AITargetType.Visual_Player && distance<_zombieStateMachine.visualThreat.distance))
                {
                    RaycastHit hitInfo;
                    // Is the collider within our view cone and do we have line or sight
                    if (ColliderIsVisible(other,out hitInfo, _playerLayerMask))
                    {
                        // when it's close and in our FOV and we have line of sight so store as the current most dangerous threat
                        _zombieStateMachine.visualThreat.Set(AITargetType.Visual_Player, other, other.transform.position, distance);
                    }
                }
            }
            
            // 2. Is the collider that has entered our sensor a flash light of player
            else if(other.CompareTag("Flash Light") && curType != AITargetType.Visual_Player)
            {
                BoxCollider flashTrigger = (BoxCollider)other;
                //get distance from Zombie sensor position to flashTrigger position
                float distanceToThreat = Vector3.Distance(_zombieStateMachine.sensorPosition, flashTrigger.transform.position);
                //size of flashTrigger in world space
                float zSize = flashTrigger.size.z * flashTrigger.transform.lossyScale.z;
                //aggravate factor 0 means zombie is close to flash light, 1 means zombie is far from flash light
                float aggrFactor = distanceToThreat / zSize;
                //if aggrFactor is smaller than the sight of zombie and it is smaller than the intelligence, other(flash light) will
                //be the current target of zombie
                if (aggrFactor <= _zombieStateMachine.sight && aggrFactor <= _zombieStateMachine.intelligence)
                {
                    _zombieStateMachine.visualThreat.Set(AITargetType.Visual_Light, other, other.transform.position, distanceToThreat);
                }
                   
            }

            // 3. If the collider that has entered our sensor a sound
            else if(other.CompareTag("AI Sound Emitter"))
            {
                SphereCollider soundTrigger = (SphereCollider)other;
                if (!soundTrigger) return;

                //get position of the agent sensor
                Vector3 agentSensorPosition = _zombieStateMachine.sensorPosition;
                Vector3 soundPos;
                float soundRadius;
                AIState.ConvertSphereColliderToWorldSpace(soundTrigger, out soundPos, out soundRadius);

                //how far inside the sound radius are we
                float distanceToTheat = (soundPos - agentSensorPosition).magnitude;
                //distance fator, 1 means at sound radius, 0 means at sound center
                float distanceFactor = (distanceToTheat / soundRadius);
                //take zombie's hearing ability into consideration
                distanceFactor += distanceFactor * (1.0f - _zombieStateMachine.hearing);
                //too far
                if (distanceFactor > 1.0f)
                {
                    return;
                }

                //if zombie can hear it and it is closer than the sound previously heard
                if (distanceFactor < _zombieStateMachine.audioThreat.distance)
                {
                    //we set this sound to most priority target
                    _zombieStateMachine.audioThreat.Set(AITargetType.Audio, other, soundPos, distanceToTheat);
                }
            }

            // 4. If the collider is food
            //if no player, visual and sound target and zombie's satisfaction <= 90%
            else if(other.CompareTag("AI Food") && curType != AITargetType.Visual_Player && curType != AITargetType.Visual_Light
                   && _zombieStateMachine.audioThreat.type==AITargetType.None&& _zombieStateMachine.satisfaction <= 0.9f)
            {
                //distance from target
                float distanceToTheat = Vector3.Distance(other.transform.position, _zombieStateMachine.sensorPosition);

                //if distance is smaller than the target previous stored
                if (distanceToTheat < _zombieStateMachine.visualThreat.distance)
                {
                    //we check that if the target is in FOV and sight of the zombie 
                    RaycastHit hitInfo;
                    if(ColliderIsVisible(other,out hitInfo, _visualLayerMask))
                    {
                        //set new target
                        _zombieStateMachine.visualThreat.Set(AITargetType.Visual_Food, other, other.transform.position, distanceToTheat);
                    }
                }
            }



        }
    }

    /*********************************************************/
    //Test the passed collider against the zombie's FOV and using the passed
    //layer mask for line of sight testing.
    protected virtual bool ColliderIsVisible(Collider other,out RaycastHit hitInfo,int layerMask = -1)
    {
        hitInfo = new RaycastHit();
        if (!_zombieStateMachine) return false;
        // Calculate the angle between the sensor origin and the direction of the collider
        Vector3 head = _stateMachine.sensorPosition;
        Vector3 direction = other.transform.position - head;
        float angle = Vector3.Angle(direction, transform.forward);

        // If thr angle is greater than half our FOV then it is outside the view cone so
        // return false - no visibility
        if (angle > (_zombieStateMachine.fov * 0.5f))
            return false;
        // Now we need to test line of sight. Perform a ray cast from our sensor origin in the direction of the collider for distance
        // of our sensor radius scaled by the zombie's sight ability. This will return ALL hits.
        RaycastHit[] hits = Physics.RaycastAll(head, direction.normalized, _zombieStateMachine.sensorRadius * _zombieStateMachine.sight, layerMask);

        // Find the closest collider that is NOT the AIs own body part. If its not the target then the target is obstructed
        float closestColliderDistance = float.MaxValue;
        Collider closestCollider = null;


        //for each hit
        for(int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];
            // If this hit closer than any we previously have found and stored
            if (hit.distance < closestColliderDistance)
            {
                // If the hit is on the body part layer
                if (hit.transform.gameObject.layer == _bodyPartLayer)
                {
                    // And assuming it is not our own body part
                    if (_stateMachine != GameSceneManager.instance.GetAIStateMachine(hit.rigidbody.GetInstanceID()))
                    {
                        // Store the collider, distance and hit info.
                        closestColliderDistance = hit.distance;
                        closestCollider = hit.collider;
                        hitInfo = hit;
                    }
                }
                else
                {
                    // Its not a body part so simply store this as the new closest hit we have found
                    closestColliderDistance = hit.distance;
                    closestCollider = hit.collider;
                    hitInfo = hit;
                }
            }
        }
        // If the closest hit is the collider we are testing against, it means we have line-of-sight
        // so return true.
        if (closestCollider && closestCollider.gameObject == other.gameObject) return true;
        return false;
    }

}
