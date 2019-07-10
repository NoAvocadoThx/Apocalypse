using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIZombieState_Attack1 : AIZombieState
{
    // Inspector Assigned
    [SerializeField] [Range(0, 10)] float _speed = 0.0f;
    [SerializeField] [Range(0.0f, 1.0f)] float _lookAtWeight = 0.7f;
    [SerializeField] [Range(0.0f, 90.0f)] float _lookAtAngleThreshold = 15.0f;
    [SerializeField] float _slerpSpeed = 5.0f;
}
