using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class navAgentExample : MonoBehaviour
{
    //inspector assigned vars
    public AIWaypointNetwork WaypointNetwork = null;
    public int curIndex = 0;
    public bool hasPath = false;
    public bool pathPending = false;
    public bool pathStale = false;
    public NavMeshPathStatus pathStatus = NavMeshPathStatus.PathInvalid;
    private NavMeshAgent _navAgent = null;
    public AnimationCurve jumpCurve = new AnimationCurve();

    // Start is called before the first frame update
    void Start()
    {
        //NavMeshAgent referrence
        _navAgent = GetComponent<NavMeshAgent>();

        //only NavMeshAgent moving but model not moving

        //_navAgent.updatePosition = false;
       // _navAgent.updateRotation = false;

        if (WaypointNetwork == null) return;
        //move agent to destination (hard coded location)
        /* if (WaypointNetwork.waypoints[curIndex])
         {
             _navAgent.destination = WaypointNetwork.waypoints[curIndex].position;
         }*/



        //move agent to desination (automatic)
        setNextDestination(false);

    }

    void setNextDestination (bool increment)
    {
        //no network then return
        if (!WaypointNetwork) return;
        int incStep = increment ? 1 : 0;
        Transform nextWaypointTransform = null;


        //if next waypoint reachable +1 or go to 0
        // Calculate index of next waypoint factoring in the increment with wrap-around and fetch waypoint 
        int nextWaypoint = (curIndex + incStep >= WaypointNetwork.waypoints.Count)?0:curIndex+incStep;
        nextWaypointTransform = WaypointNetwork.waypoints[nextWaypoint];
        if (nextWaypointTransform)
        {
            //update 
            curIndex = nextWaypoint;
            _navAgent.destination = nextWaypointTransform.position;
            return;
        }
            
        
       curIndex++;
    }
    // Update is called once per frame
    void Update()
    {
        hasPath = _navAgent.hasPath;
        pathPending = _navAgent.pathPending;
        pathStale = _navAgent.isPathStale;
        pathStatus = _navAgent.pathStatus;

        //if agent is on offMeshLink, do "jump" for 2 seconds and then return back to current bahavior
        if (_navAgent.isOnOffMeshLink)
        {
            StartCoroutine(Jump(1.0f));
            return;
        }
        //if the agent has path and it is in a path
        //if the next waypoint is completely not reachable or partially reachable, skip next waypoint and go to next one
        if (!hasPath && !pathPending||pathStatus==NavMeshPathStatus.PathInvalid/*||pathStatus==NavMeshPathStatus.PathPartial*/)
        {
            setNextDestination(true);
        }
        else
        {
            //if next path destination changes to a unreachable or cost-increment..
            //regenerate new path
            if (pathStale)
            {
                setNextDestination(false);
            }
        }
    }

    //jump method
    IEnumerator Jump(float duration)
    {
        OffMeshLinkData data = _navAgent.currentOffMeshLinkData;
        // Start Position is agent current position
        Vector3 startPos = _navAgent.transform.position;
        // End position is fetched from OffMeshLink data and adjusted for baseoffset of agent
        Vector3 endPos = data.endPos + (_navAgent.baseOffset * Vector3.up);
        float time = 0.0f;
        while (time <= duration)
        {
            float t = time / duration;
            //interplote between startPos and endPos plus the height offset of the animation curve
            _navAgent.transform.position = Vector3.Lerp(startPos, endPos, t)+(jumpCurve.Evaluate(t)*Vector3.up);
            time += Time.deltaTime;
            // Accumulate time and yield each frame
            yield return null;
        }
        _navAgent.CompleteOffMeshLink();
    }
}
