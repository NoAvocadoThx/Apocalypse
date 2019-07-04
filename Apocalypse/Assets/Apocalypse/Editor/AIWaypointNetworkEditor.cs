using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

[CustomEditor(typeof(AIWaypointNetwork))]
public class NewBehaviourScript : Editor
{

    public override void OnInspectorGUI()
    {
        //inspector custom UI
        AIWaypointNetwork network = (AIWaypointNetwork)target;
       
        network.DisplayMode = (PathDisplayMode)EditorGUILayout.EnumPopup("Display Mode", network.DisplayMode);
        if (network.DisplayMode == PathDisplayMode.Paths)
        {
            network.UIStart = EditorGUILayout.IntSlider("Waypoint Start", network.UIStart, 0, network.waypoints.Count - 1);
            network.UIEnd = EditorGUILayout.IntSlider("Waypoint End", network.UIEnd, 0, network.waypoints.Count - 1);
        }
        //base.OnInspectorGUI();
        DrawDefaultInspector();
    }
    void OnSceneGUI()
    {
        //display waypoint labels
        AIWaypointNetwork network = (AIWaypointNetwork)target;
        GUIStyle style = new GUIStyle();
        for (int i = 0; i < network.waypoints.Count; i++)
        {
            style.normal.textColor = Color.white;
            if (network.waypoints[i] != null)
            {
                Handles.Label(network.waypoints[i].position, "Waypoint " + i.ToString(), style);
            }
        }
        //if its in connection mode
        if (network.DisplayMode == PathDisplayMode.Connections)
        {
            Vector3[] vertices = new Vector3[network.waypoints.Count + 1];

            for (int i = 0; i <= vertices.Length - 1; i++)
            {
                //when i==last waypoint, i=first waypoint
                int index = i != network.waypoints.Count ? i : 0;
                if (network.waypoints[index] != null)
                    vertices[i] = network.waypoints[index].position;
                else
                    vertices[i] = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
            }
            Handles.color = Color.cyan;
            Handles.DrawPolyLine(vertices);
        }
        //if in paths mode,show actually paths
        else if(network.DisplayMode == PathDisplayMode.Paths)
        {
            NavMeshPath path = new NavMeshPath();

            if (network.waypoints[network.UIStart] && network.waypoints[network.UIEnd])
            {
                Vector3 fromPoint = network.waypoints[network.UIStart].position;
                Vector3 toPoint = network.waypoints[network.UIEnd].position;
                NavMesh.CalculatePath(fromPoint, toPoint, NavMesh.AllAreas, path);
                Handles.color = Color.yellow;
                //draw line between path corners aka turning direction
                Handles.DrawPolyLine(path.corners);
            }
        }
        //if none of these ode selected
    }
}
