using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CustomSplineExtruder))]
public class CustomSplineExtruderEditor : Editor
{ 
    public override void OnInspectorGUI()
    {
        CustomSplineExtruder extruder = (CustomSplineExtruder)target;
 
        DrawDefaultInspector();
 
        if (GUI.changed)
        {
            extruder.Extrude();
        }
 
        if (GUILayout.Button("Extrude"))
        {
            extruder.Extrude();
        }
    }
}
