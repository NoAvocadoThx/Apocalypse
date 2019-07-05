using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DoorState { Open, Animating, Closed };

public class SlidingDoor : MonoBehaviour
{

    public float slidingDistance = 4.0f;
    public float duration = 1.5f;
    public AnimationCurve slideCurve = new AnimationCurve();

    private Transform _transform = null;
    private Vector3 _openPos = Vector3.zero;
    private Vector3 _closedPos = Vector3.zero;
    private DoorState _doorState = DoorState.Closed;
    // Start is called before the first frame update
    void Start()
    {
        _transform = transform;
        _closedPos = transform.position;
        _openPos = _closedPos + (_transform.right * slidingDistance);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && _doorState!=DoorState.Animating)
        {
            //wait until coroutine finishes
            StartCoroutine(animateDoor((_doorState == DoorState.Open) ? DoorState.Closed : DoorState.Open));
        }
    }

    //newState is the state we wish to move to
    IEnumerator animateDoor(DoorState newState)
    {
        _doorState = DoorState.Animating;
        float time = 0.0f;
        //if door open then we want the door to close
        Vector3 startPos = (newState == DoorState.Open) ? _closedPos : _openPos;
        Vector3 endPos = (newState == DoorState.Open) ? _openPos : _closedPos;

        while (time <= duration)
        {
            float t = time / duration;
            _transform.position = Vector3.Lerp(startPos, endPos, slideCurve.Evaluate(t));
            time += Time.deltaTime;
            yield return null;
        }
        //if time >duration for frame reasons
        _transform.position = endPos;
        _doorState = newState;
    }
}
