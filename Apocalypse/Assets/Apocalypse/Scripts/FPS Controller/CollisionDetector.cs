using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDetector : MonoBehaviour
{

    FPSController _controller = null;
    /*********************************************************/
    // Start is called before the first frame update
    void Start()
    {
        _controller = GetComponentInParent<FPSController>();
    }

    private void OnTriggerStay(Collider other)
    {
        AIStateMachine machine = GameSceneManager.instance.GetAIStateMachine(other.GetInstanceID());
        if (machine && _controller)
        {
            _controller.DoCollision();
            machine.visualThreat.Set(AITargetType.Visual_Player, _controller.CharacterController, _controller.transform.position, Vector3.Distance(machine.transform.position, _controller.transform.position));
            machine.SetState(AIStateType.Attack);
        }
    }
}
